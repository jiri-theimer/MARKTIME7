// Administrace: vlastní výrazy zákazníka pro překlad tiskových sestav (Views/ReportLang/Index.cshtml)
// Dvě záložky: vlastní výrazy (editace) a společný slovník (jen k nahlédnutí, id="rlShared").
// Data: <script type="application/json" id="rlData"> = řádky [{cs,en,de,sk}] (stejný tvar jako soubor reports.lang.json), id="rlConfig" = texty.
// Uložení: řádky jako JSON do skrytého pole #RowsJson a odeslání form1 (ReportLangController.Index).
(function () {
    "use strict";

    const byId = (id) => document.getElementById(id);
    const readJson = (id, def) => {
        const e = byId(id);
        if (!e) return def;
        try { return JSON.parse(e.textContent) || def; } catch (err) { return def; }
    };

    const cfg = readJson("rlConfig", {});
    const T = cfg.texts || {};
    const COLS = ["cs", "en", "de", "sk"];
    const elBody = byId("rlBody");
    const rows = readJson("rlData", []).map((r) => ({ cs: r.cs || "", en: r.en || "", de: r.de || "", sk: r.sk || "" }));

    // v jednořádkovém poli formuláře je konec řádku zapsaný jako \n (dva znaky); v datech i v souboru je to skutečný konec řádku
    const toInput = (s) => (s || "").replace(/\r\n|\r|\n/g, "\\n");
    const fromInput = (s) => (s || "").replace(/\\n/g, "\n");
    const norm = (s) => (s || "").toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g, "");
    const key = (s) => (s || "").replace(/\r\n|\r/g, "\n").trim();

    function el(tag, cls, text) {
        const e = document.createElement(tag);
        if (cls) e.className = cls;
        if (text != null) e.textContent = text;
        return e;
    }

    function buildRow(r) {
        const tr = el("tr");
        tr._row = r;
        tr.appendChild(el("td", "rl-no"));
        COLS.forEach((c) => {
            const td = el("td");
            const inp = el("input", "form-control form-control-sm");
            inp.type = "text";
            inp.value = toInput(r[c]);
            inp.dataset.col = c;
            inp.spellcheck = false;
            td.appendChild(inp);
            tr.appendChild(td);
        });
        const tdx = el("td", "rl-x");
        const x = el("button", "rl-del");
        x.type = "button";
        x.title = T.remove || "";
        x.appendChild(el("span", "material-icons-outlined-nosize", "close"));
        tdx.appendChild(x);
        tr.appendChild(tdx);
        return tr;
    }

    function render() {
        elBody.textContent = "";
        const frag = document.createDocumentFragment();
        rows.forEach((r) => frag.appendChild(buildRow(r)));
        elBody.appendChild(frag);
        refresh();
    }

    // číslování, zvýraznění nevyplněných jazyků a duplicit (stejný český výraz na dvou řádcích), filtr
    function refresh() {
        const words = norm(byId("rlSearch").value).split(/\s+/).filter((w) => w !== "");
        const onlyMissing = byId("rlOnlyMissing").checked;
        const seen = new Map();
        rows.forEach((r) => { const k = key(r.cs); if (k !== "") seen.set(k, (seen.get(k) || 0) + 1); });

        let visible = 0;
        Array.from(elBody.rows).forEach((tr, i) => {
            const r = tr._row;
            tr.cells[0].textContent = String(i + 1);
            const k = key(r.cs);
            const dup = k !== "" && seen.get(k) > 1;
            let missing = false;
            COLS.forEach((c, ci) => {
                const m = r[c].trim() === "";
                tr.cells[ci + 1].classList.toggle("rl-missing", m);
                if (m) missing = true;
            });
            tr.classList.toggle("rl-dup", dup);
            tr.cells[1].firstChild.title = dup ? (T.duplicate || "") : "";

            const hay = norm(r.cs + " " + r.en + " " + r.de + " " + r.sk);
            const show = tr._new === true || (words.every((w) => hay.indexOf(w) > -1) && (!onlyMissing || missing));
            tr.hidden = !show;
            if (show) visible++;
        });

        const empty = byId("rlEmpty");
        empty.textContent = rows.length === 0 ? (T.empty || "") : (visible === 0 ? (T.notFound || "") : "");
        empty.hidden = empty.textContent === "";
        byId("rlOwnCount").textContent = String(rows.length);
    }

    function markChanged() {
        byId("cmdSave").classList.add("rl-changed");
    }

    function clearNewFlags() {
        Array.from(elBody.rows).forEach((tr) => { tr._new = false; });
    }

    function addRow(r) {
        rows.push(r);
        const tr = buildRow(r);
        tr._new = true;     // nový řádek zůstává vidět i při zapnutém filtru
        elBody.appendChild(tr);
        return tr;
    }

    function bind() {
        elBody.addEventListener("input", (e) => {
            const inp = e.target.closest("input[data-col]");
            if (!inp) return;
            inp.closest("tr")._row[inp.dataset.col] = fromInput(inp.value);
            markChanged();
            refresh();
        });
        elBody.addEventListener("click", (e) => {
            const btn = e.target.closest(".rl-del");
            if (!btn) return;
            const tr = btn.closest("tr");
            rows.splice(rows.indexOf(tr._row), 1);
            tr.remove();
            markChanged();
            refresh();
        });
        byId("rlAdd").addEventListener("click", () => {
            const tr = addRow({ cs: "", en: "", de: "", sk: "" });
            markChanged();
            refresh();
            tr.scrollIntoView({ block: "nearest" });
            tr.cells[1].firstChild.focus();
        });
        byId("rlOnlyMissing").addEventListener("change", () => { clearNewFlags(); refresh(); });
        let timer = null;
        byId("rlSearch").addEventListener("input", () => {
            clearTimeout(timer);
            timer = setTimeout(() => { clearNewFlags(); refresh(); }, 120);
        });
        window.addEventListener("resize", fitHeight);
    }

    // ---- 2. záložka: společný slovník, jen k nahlédnutí
    const shared = readJson("rlShared", []);
    const elSharedBody = byId("rlSharedBody");

    function renderShared() {
        const frag = document.createDocumentFragment();
        shared.forEach((r, i) => {
            const tr = el("tr");
            tr._hay = norm((r.cs || "") + " " + (r.en || "") + " " + (r.de || "") + " " + (r.sk || ""));
            tr.appendChild(el("td", "rl-no", String(i + 1)));
            COLS.forEach((c) => tr.appendChild(el("td", null, r[c] || "")));
            frag.appendChild(tr);
        });
        elSharedBody.appendChild(frag);
        filterShared();
    }

    function filterShared() {
        const words = norm(byId("rlSharedSearch").value).split(/\s+/).filter((w) => w !== "");
        let visible = 0;
        Array.from(elSharedBody.rows).forEach((tr) => {
            tr.hidden = !words.every((w) => tr._hay.indexOf(w) > -1);
            if (!tr.hidden) visible++;
        });
        const empty = byId("rlSharedEmpty");
        empty.textContent = shared.length === 0 ? (T.sharedEmpty || "") : (visible === 0 ? (T.notFound || "") : "");
        empty.hidden = empty.textContent === "";
        byId("rlSharedCount").textContent = words.length > 0 ? visible + " / " + shared.length : "";
    }

    function showTab(name) {
        document.querySelectorAll(".rl-pane").forEach((p) => { p.hidden = p.dataset.pane !== name; });
        document.querySelectorAll(".rl-tab").forEach((b) => b.classList.toggle("active", b.dataset.tab === name));
        fitHeight();
    }

    function bindShared() {
        document.querySelectorAll(".rl-tab").forEach((b) => b.addEventListener("click", () => showTab(b.dataset.tab)));
        let timer = null;
        byId("rlSharedSearch").addEventListener("input", () => {
            clearTimeout(timer);
            timer = setTimeout(filterShared, 120);
        });
    }

    // tabulka vyplní zbytek okna a posouvá se uvnitř (hlavička zůstává vidět)
    function fitHeight() {
        ["rlWrap", "rlSharedWrap"].forEach((id) => {
            const wrap = byId(id);
            if (wrap.offsetParent === null) return;     //skrytá záložka
            const top = wrap.getBoundingClientRect().top + window.scrollY;
            wrap.style.height = Math.max(220, window.innerHeight - top - 34) + "px";
        });
    }

    // voláno z tlačítka Uložit změny
    window.rl_save = function () {
        const first = Array.from(elBody.rows).find((tr) => tr.classList.contains("rl-dup"));
        if (first) {
            first.hidden = false;
            first.scrollIntoView({ block: "center" });
            first.cells[1].firstChild.focus();
            _notify_message((T.duplicate || "") + ": " + first._row.cs);
            return;
        }
        byId("RowsJson").value = JSON.stringify(rows.filter((r) => COLS.some((c) => key(r[c]) !== "")));     // prázdné řádky se neukládají
        _submit();
    };

    render();
    renderShared();
    bind();
    bindShared();
    fitHeight();
})();
