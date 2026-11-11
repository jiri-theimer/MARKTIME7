// Návrhář sloupců tabulky (Views/TheGridDesigner/Index.cshtml)
// Strom katalogu sloupců a seznam vybraných sloupců – vlastní řešení bez Kendo.
// Data: <script type="application/json" id="tgdTreeData"> (strom z controlleru, inhale_tree) a id="tgdConfig" (texty, nastavení).
// Výsledek: skryté pole #Rec_j72Columns = UniqueName sloupců oddělené čárkou (beze změny proti původnímu návrháři).
(function () {
    "use strict";

    const byId = (id) => document.getElementById(id);
    const readJson = (id, def) => {
        const e = byId(id);
        if (!e) return def;
        try { return JSON.parse(e.textContent) || def; } catch (err) { return def; }
    };

    const cfg = readJson("tgdConfig", {});
    const T = cfg.texts || {};
    const tree = readJson("tgdTreeData", []);

    const elTree = byId("tree1");
    const elList = byId("list1");
    const elHidden = byId("Rec_j72Columns");
    const elSearch = byId("tgdSearch");
    const elCols = byId("tgdCols");

    const leavesById = new Map();   // UniqueName sloupce -> uzly stromu (list)
    const allLeaves = [];
    const allGroups = [];
    let sels = [];                  // vybrané sloupce v pořadí
    let activeField = null;         // označený řádek v seznamu vybraných
    let drag = null;                // právě přetahovaný sloupec { field, from: "list" | "tree" }

    // ------------------------------------------------------------------ pomocné

    function norm(s) {     // hledání bez ohledu na velikost písmen a diakritiku
        return (s || "").toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g, "");
    }

    function el(tag, cls, text) {
        const e = document.createElement(tag);
        if (cls) e.className = cls;
        if (text != null) e.textContent = text;
        return e;
    }

    function icon(name, cls) {
        return el("span", "material-icons-outlined-nosize" + (cls ? " " + cls : ""), name);
    }

    function isGroup(n) {
        return Array.isArray(n.items) || n.customvalue3 === "tree_group";
    }

    // ------------------------------------------------------------------ strom katalogu

    function buildNode(n, parent, level) {
        n._parent = parent;
        n._norm = norm(n.text);

        if (isGroup(n)) {
            n._group = true;
            n._open = n.expanded === true;
            allGroups.push(n);

            const box = el("div", "tgd-g tgd-l" + level);
            const head = el("div", "tgd-g-head");
            head.appendChild(icon("chevron_right", "tgd-chev"));
            if (n.customvalue2) {
                const img = el("img");
                img.src = n.customvalue2; img.alt = "";
                head.appendChild(img);
            }
            head.appendChild(el("span", "tgd-g-text", n.text));
            n._count = el("span", "tgd-badge tgd-g-count");
            head.appendChild(n._count);
            const body = el("div", "tgd-g-body");
            (n.items || []).forEach((ch) => body.appendChild(buildNode(ch, n, level + 1)));
            box.appendChild(head);
            box.appendChild(body);
            head._node = n;
            n._el = box;
            box.classList.toggle("open", n._open);
            return box;
        }

        allLeaves.push(n);
        if (!leavesById.has(n.id)) leavesById.set(n.id, []);
        leavesById.get(n.id).push(n);

        const row = el("div", "tgd-leaf " + (n.customvalue3 || ""));
        row.title = n.tooltip || "";
        row.draggable = true;
        if (n.customvalue2) {
            const img = el("img");
            img.src = n.customvalue2; img.alt = "";
            row.appendChild(img);
        }
        row.appendChild(el("span", "tgd-leaf-text", n.text));
        row.appendChild(icon("add", "tgd-leaf-act"));
        row._node = n;
        n._el = row;
        return row;
    }

    function renderTree() {
        const frag = document.createDocumentFragment();
        tree.forEach((n) => frag.appendChild(buildNode(n, null, 0)));
        elTree.appendChild(frag);
        elTree.appendChild(el("div", "tgd-empty tgd-notfound", T.notFound || ""));
    }

    function setOpen(g, open) {
        g._open = open;
        g._el.classList.toggle("open", open);
    }

    function refreshToggleAll() {
        const btn = byId("tgdToggleAll");
        if (!btn) return;
        const anyOpen = allGroups.some((g) => g._open);
        btn.title = anyOpen ? (T.collapseAll || "") : (T.expandAll || "");
        btn.querySelector("span").textContent = anyOpen ? "unfold_less" : "unfold_more";
    }

    function applySearch() {
        const words = norm(elSearch.value).split(/\s+/).filter((w) => w !== "");
        const info = byId("tgdSearchInfo");
        elTree.classList.toggle("tgd-searching", words.length > 0);

        if (words.length === 0) {
            allLeaves.forEach((n) => { n._el.hidden = false; });
            allGroups.forEach((g) => { g._el.hidden = false; });
            elTree.classList.remove("tgd-nomatch");
            if (info) info.textContent = "";
            return;
        }

        let found = 0;
        allGroups.forEach((g) => { g._hit = 0; });
        allLeaves.forEach((n) => {
            // slovo se hledá v názvu sloupce i v názvech nadřazených skupin („klient název“ najde Název pod Klientem)
            let hay = n._norm;
            for (let p = n._parent; p; p = p._parent) hay += " " + p._norm;
            const ok = words.every((w) => hay.indexOf(w) > -1);
            n._el.hidden = !ok;
            if (ok) {
                found++;
                for (let p = n._parent; p; p = p._parent) p._hit++;
            }
        });
        allGroups.forEach((g) => { g._el.hidden = g._hit === 0; });
        elTree.classList.toggle("tgd-nomatch", found === 0);
        if (info) info.textContent = found > 0 ? found + " " + (T.found || "") : "";
    }

    // ------------------------------------------------------------------ vybrané sloupce

    function leafOf(field) {
        const arr = leavesById.get(field);
        return arr ? arr[0] : null;
    }

    function labelParts(n) {    // "Skupina -> Sloupec" z controlleru (text_plus_group)
        const s = n.text_plus_group || n.text || "";
        const i = s.indexOf(" -> ");
        return i > -1 ? [s.substring(0, i), s.substring(i + 4)] : ["", s];
    }

    function renderList() {
        elList.textContent = "";
        if (sels.length === 0) {
            elList.appendChild(el("div", "tgd-empty", T.emptyList || ""));
        }
        sels.forEach((field, i) => {
            const n = leafOf(field);
            const parts = labelParts(n);
            const row = el("div", "tgd-sel list1_item" + (field === activeField ? " active" : ""));
            row.dataset.field = field;
            row.draggable = true;
            row.title = n.tooltip || "";
            row.appendChild(icon("drag_indicator", "tgd-grip"));
            row.appendChild(el("span", "tgd-sel-no", String(i + 1)));
            const txt = el("span", "tgd-sel-text");
            if (parts[0] !== "") txt.appendChild(el("small", null, parts[0] + " → "));
            txt.appendChild(document.createTextNode(parts[1]));
            row.appendChild(txt);
            const x = el("button", "tgd-sel-x");
            x.type = "button";
            x.title = T.remove || "";
            x.appendChild(icon("close"));
            row.appendChild(x);
            elList.appendChild(row);
        });
    }

    function refresh() {
        if (elHidden) elHidden.value = sels.join(",");

        const set = new Set(sels);
        allGroups.forEach((g) => { g._sel = 0; });
        allLeaves.forEach((n) => {
            const on = set.has(n.id);
            n._el.classList.toggle("is-sel", on);
            n._el.lastChild.textContent = on ? "check" : "add";
            if (on) for (let p = n._parent; p; p = p._parent) p._sel++;
        });
        allGroups.forEach((g) => {
            g._count.textContent = g._sel > 0 ? String(g._sel) : "";
            g._count.classList.toggle("tgd-badge-on", g._sel > 0);
        });

        if (activeField !== null && !set.has(activeField)) activeField = null;
        renderList();

        ["tgdSelCount", "tgdTabColsCount"].forEach((id) => { if (byId(id)) byId(id).textContent = String(sels.length); });
        refreshButtons();
    }

    function refreshButtons() {
        const i = sels.indexOf(activeField);
        if (byId("cmdRemove")) byId("cmdRemove").disabled = i < 0;
        if (byId("cmdUp")) byId("cmdUp").disabled = i < 1;
        if (byId("cmdDown")) byId("cmdDown").disabled = i < 0 || i >= sels.length - 1;
    }

    function setActive(field) {     // jen označení řádku – seznam se nepřekresluje (jinak by nefungoval dvojklik)
        activeField = field;
        elList.querySelectorAll(".tgd-sel").forEach((r) => r.classList.toggle("active", r.dataset.field === field));
        refreshButtons();
    }

    function showActive() {
        const row = elList.querySelector(".tgd-sel.active");
        if (row) row.scrollIntoView({ block: "nearest" });
    }

    function addField(field, index) {      // index: pozice vložení, bez něj na konec
        if (!leafOf(field)) return;
        const cur = sels.indexOf(field);
        if (cur > -1) {
            if (index == null) { setActive(field); showActive(); return; }   // už je vybraný: jen ho označit
            sels.splice(cur, 1);
            if (cur < index) index--;
        }
        if (index == null || index > sels.length) index = sels.length;
        sels.splice(index, 0, field);
        activeField = field;
        refresh();
        showActive();
    }

    function removeField(field) {
        const i = sels.indexOf(field);
        if (i < 0) return;
        sels.splice(i, 1);
        activeField = sels.length > 0 ? sels[Math.min(i, sels.length - 1)] : null;
        refresh();
    }

    function moveActive(delta) {
        const i = sels.indexOf(activeField);
        const j = i + delta;
        if (i < 0 || j < 0 || j >= sels.length) return;
        sels.splice(i, 1);
        sels.splice(j, 0, activeField);
        refresh();
        showActive();
    }

    // ------------------------------------------------------------------ přetahování (pořadí, přidání ze stromu)

    function dropIndex(y) {     // pozice v seznamu podle svislé souřadnice myši
        const rows = Array.from(elList.querySelectorAll(".tgd-sel"));
        for (let i = 0; i < rows.length; i++) {
            const r = rows[i].getBoundingClientRect();
            if (y < r.top + r.height / 2) return i;
        }
        return rows.length;
    }

    function clearDropMark() {
        elList.querySelectorAll(".tgd-drop-before, .tgd-drop-after").forEach((e) => e.classList.remove("tgd-drop-before", "tgd-drop-after"));
        elList.classList.remove("tgd-drop-empty");
    }

    function bindDrag() {
        elList.addEventListener("dragstart", (e) => {
            const row = e.target.closest(".tgd-sel");
            if (!row) return;
            drag = { field: row.dataset.field, from: "list" };
            e.dataTransfer.effectAllowed = "move";
            e.dataTransfer.setData("text/plain", drag.field);
            row.classList.add("tgd-dragging");
        });
        elTree.addEventListener("dragstart", (e) => {
            const row = e.target.closest(".tgd-leaf");
            if (!row) return;
            drag = { field: row._node.id, from: "tree" };
            e.dataTransfer.effectAllowed = "copyMove";
            e.dataTransfer.setData("text/plain", drag.field);
        });
        elList.addEventListener("dragover", (e) => {
            if (!drag) return;
            e.preventDefault();
            e.dataTransfer.dropEffect = drag.from === "list" ? "move" : "copy";
            clearDropMark();
            const rows = elList.querySelectorAll(".tgd-sel");
            if (rows.length === 0) { elList.classList.add("tgd-drop-empty"); return; }
            const i = dropIndex(e.clientY);
            if (i < rows.length) rows[i].classList.add("tgd-drop-before"); else rows[rows.length - 1].classList.add("tgd-drop-after");
        });
        elList.addEventListener("dragleave", (e) => {
            if (!elList.contains(e.relatedTarget)) clearDropMark();
        });
        elList.addEventListener("drop", (e) => {
            if (!drag) return;
            e.preventDefault();
            const i = dropIndex(e.clientY);
            const field = drag.field;
            drag = null;
            clearDropMark();
            addField(field, i);
        });
        document.addEventListener("dragend", () => {
            drag = null;
            clearDropMark();
            elList.querySelectorAll(".tgd-dragging").forEach((e) => e.classList.remove("tgd-dragging"));
        });
    }

    // ------------------------------------------------------------------ události

    function bindEvents() {
        elTree.addEventListener("click", (e) => {
            const head = e.target.closest(".tgd-g-head");
            if (head) {
                if (elTree.classList.contains("tgd-searching")) return;     // při hledání jsou skupiny s výsledky rozbalené vždy
                setOpen(head._node, !head._node._open);
                refreshToggleAll();
                return;
            }
            // sloupec se přidává dvojklikem (nebo ikonou + na konci řádku); klik na už vybraný sloupec ho jen označí vpravo
            const leaf = e.target.closest(".tgd-leaf");
            if (!leaf) return;
            if (e.target.closest(".tgd-leaf-act") || sels.indexOf(leaf._node.id) > -1) addField(leaf._node.id);
        });
        elTree.addEventListener("dblclick", (e) => {
            const leaf = e.target.closest(".tgd-leaf");
            if (leaf && !e.target.closest(".tgd-leaf-act")) addField(leaf._node.id);
        });

        elList.addEventListener("click", (e) => {
            const row = e.target.closest(".tgd-sel");
            if (!row) return;
            if (e.target.closest(".tgd-sel-x")) { removeField(row.dataset.field); return; }
            setActive(row.dataset.field);
        });
        elList.addEventListener("dblclick", (e) => {
            const row = e.target.closest(".tgd-sel");
            if (row && !e.target.closest(".tgd-sel-x")) removeField(row.dataset.field);
        });

        if (byId("cmdUp")) byId("cmdUp").addEventListener("click", () => moveActive(-1));
        if (byId("cmdDown")) byId("cmdDown").addEventListener("click", () => moveActive(1));
        if (byId("cmdRemove")) byId("cmdRemove").addEventListener("click", () => { if (activeField !== null) removeField(activeField); });

        if (elSearch) {
            let timer = null;
            elSearch.addEventListener("input", () => {
                clearTimeout(timer);
                timer = setTimeout(applySearch, 120);
            });
        }
        if (byId("tgdToggleAll")) {
            byId("tgdToggleAll").addEventListener("click", () => {
                const open = !allGroups.some((g) => g._open);
                allGroups.forEach((g) => setOpen(g, open));
                refreshToggleAll();
            });
        }

        window.addEventListener("resize", fitHeight);
    }

    // ------------------------------------------------------------------ rozvržení: panely vyplní zbytek okna, posouvají se uvnitř

    function fitHeight() {
        if (!elCols || elCols.offsetParent === null) return;
        const top = elCols.getBoundingClientRect().top + window.scrollY;
        elCols.style.height = Math.max(280, window.innerHeight - top - 10) + "px";
    }

    // ------------------------------------------------------------------ záložky (Sloupce / Vnitřní filtr / Sdílení / Výpis katalogu)

    function showTab(name) {
        const panes = document.querySelectorAll(".tgd-pane");
        if (!document.querySelector(".tgd-pane[data-pane='" + name + "']")) name = "cols";
        panes.forEach((p) => { p.hidden = p.dataset.pane !== name; });
        document.querySelectorAll(".tgd-tab").forEach((b) => b.classList.toggle("active", b.dataset.tab === name));
        fitHeight();
    }

    function bindTabs() {
        document.querySelectorAll(".tgd-tab").forEach((b) => b.addEventListener("click", () => showTab(b.dataset.tab)));
        showTab(cfg.tab || "cols");
    }

    // ------------------------------------------------------------------ název tabulky: Uložit jako / Přejmenovat

    let nameOper = null;

    function nameBarOpen(oper, label, value) {
        nameOper = oper;
        byId("tgdNameLabel").textContent = label || "";
        const inp = byId("tgdNameInput");
        inp.value = value || "";
        byId("tgdNameBar").hidden = false;
        fitHeight();
        inp.focus();
        inp.select();
    }

    function nameBarClose() {
        nameOper = null;
        byId("tgdNameBar").hidden = true;
        fitHeight();
    }

    function nameBarOk() {
        const s = byId("tgdNameInput").value.trim();
        if (s === "") {
            _notify_message(T.nameRequired || "");
            byId("tgdNameInput").focus();
            return;
        }
        _postback(nameOper, "j72name=" + encodeURIComponent(s));
    }

    function bindNameBar() {
        if (!byId("tgdNameBar")) return;
        byId("tgdNameOk").addEventListener("click", nameBarOk);
        byId("tgdNameCancel").addEventListener("click", nameBarClose);
        byId("tgdNameInput").addEventListener("keydown", (e) => {
            if (e.key === "Enter") { e.preventDefault(); nameBarOk(); }
            if (e.key === "Escape") { e.preventDefault(); e.stopPropagation(); nameBarClose(); }
        });
    }

    // dvoukrokové potvrzení místo confirm(): první klik tlačítko „nabije“ (text z data-confirm), druhý do 4 s akci provede
    function confirmStep(btn, fn) {
        if (!btn || !btn.dataset || !btn.dataset.confirm) { fn(); return; }
        if (btn.classList.contains("tgd-armed")) { fn(); return; }
        const orig = btn.innerHTML;
        btn.classList.add("tgd-armed");
        btn.textContent = btn.dataset.confirm;
        setTimeout(() => {
            if (btn.classList.contains("tgd-armed") && !btn.disabled) { btn.classList.remove("tgd-armed"); btn.innerHTML = orig; }
        }, 4000);
    }

    // ------------------------------------------------------------------ funkce volané z tlačítek stránky (názvy zachované)

    window.savechanges = function () {
        if (sels.length === 0) {
            _notify_message(T.minOne || "");
            return;
        }
        _submit();
    };
    window.saveas = function () { nameBarOpen("saveas", T.saveAsLabel, ""); };
    window.rename = function () { nameBarOpen("rename", T.renameLabel, cfg.name || ""); };
    window.delete_record = function (btn) { confirmStep(btn, () => _postback("delete")); };
    window.restore2factory = function (btn) { confirmStep(btn, () => _postback("restore2factory")); };
    window.restore2global = function (btn) { confirmStep(btn, () => _postback("restore2global")); };
    window.update2allusers = function (btn) { confirmStep(btn, () => _postback("update2allusers")); };
    window.catalog = function () { _postback("catalog"); };

    window.handle_j73_append = function () { _postback("add_j73"); };
    window.handle_delete_j73 = function (guid) { _postback("delete_j73", "guid=" + guid); };
    window.handle_queryfield_change = function (cbx) { _postback("changefield", "guid=" + cbx.getAttribute("data-value")); };
    window.handle_queryoperator_change = function () { _postback(); };
    window.handle_j73_clear = function () { _postback("clear_j73"); };
    window.handle_j73_saveas = function () { _postback("saveas_j73"); };

    // ------------------------------------------------------------------ start

    function init() {
        if (!elTree || !elList) return;
        renderTree();

        // obnovit výběr z uloženého seznamu; sloupce, které v katalogu nejsou (např. bez oprávnění), se vynechají
        const saved = elHidden && elHidden.value ? elHidden.value.split(",") : [];
        sels = saved.filter((f, i) => leavesById.has(f) && saved.indexOf(f) === i);
        activeField = sels.length > 0 ? sels[0] : null;

        bindEvents();
        bindDrag();
        bindNameBar();
        bindTabs();
        refresh();
        refreshToggleAll();
        fitHeight();

        if (cfg.forceSaveAs === true && byId("tgdNameBar")) window.saveas();
    }

    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init); else init();
})();
