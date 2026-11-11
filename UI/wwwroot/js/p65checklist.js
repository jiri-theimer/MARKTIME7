// Check-list (tabulka p65Checklist) – Views/Shared/_p65Checklist.cshtml, vzhled css/p65checklist.css (třídy chk-*).
// Záznam, kterému check-list patří, určuje cfg.prefix + cfg.pid: "p56" = úkol, "p58" = opakovaný úkol.
//
// Dva režimy ukládání:
// - uložený záznam (pid > 0): každá změna se hned posílá na /p65/* a seznam se překreslí podle odpovědi serveru,
// - nový záznam (pid = 0): položky se drží v prohlížeči a ve skrytém poli formuláře (JSON pole textů), uloží se se záznamem.
// cfg.template = true (opakovaný úkol): položky jsou vzor pro vygenerované úkoly, neodškrtávají se a nepočítá se postup.
//
// Bez jQuery a bez knihoven. Texty dodává view přes cfg.txt (kvůli překladům).
var MtChecklist = (function () {

    function init(cfg) {
        var root = document.getElementById(cfg.id);
        if (!root) return;

        var list = root.querySelector(".chk-list");
        var countEl = root.querySelector(".chk-count");
        var barEl = root.querySelector(".chk-bar > span");
        var addBox = root.querySelector(".chk-add");
        var pendingField = cfg.pendingField ? document.getElementById(cfg.pendingField) : null;
        var prefix = cfg.prefix || "p56";
        var pending = cfg.pid === 0;
        var template = !!cfg.template;
        var canEdit = !!cfg.canEdit;
        var items = cfg.items || [];
        var seq = 0;            // dočasná ID položek neuloženého úkolu
        var busy = 0;           // počet běžících požadavků
        var dragged = null;
        var txt = cfg.txt || {};

        if (pending) {
            items = (cfg.pendingNames || []).map(function (n) { return { id: --seq, name: n, done: false }; });
        }

        function notify(msg) {
            if (!msg) return;
            if (typeof window._notify_message === "function") { window._notify_message(msg); } else { alert(msg); }
        }

        // ── server ──────────────────────────────────────────────────────────────

        function post(action, data, after) {
            var body = new URLSearchParams();
            data.prefix = prefix;
            data.pid = cfg.pid;
            Object.keys(data).forEach(function (k) { body.append(k, data[k]); });
            busy++;
            root.classList.add("chk-busy");
            fetch(cfg.url + action, {
                method: "POST",
                credentials: "same-origin",
                headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8" },
                body: body.toString()
            }).then(function (r) {
                if (!r.ok) throw new Error("HTTP " + r.status);
                return r.json();
            }).then(function (d) {
                done();
                if (!d.ok) notify(d.message || txt.failed);
                canEdit = !!d.canedit;
                items = d.items || [];
                render();
                if (after) after(d);
            }).catch(function () {
                done();
                notify(txt.failed);
                reload();
            });
        }

        function done() {
            busy = Math.max(0, busy - 1);
            if (busy === 0) root.classList.remove("chk-busy");
        }

        // po chybě ukázat to, co je skutečně uložené
        function reload() {
            fetch(cfg.url + "List?prefix=" + prefix + "&pid=" + cfg.pid, { credentials: "same-origin" })
                .then(function (r) { return r.json(); })
                .then(function (d) { canEdit = !!d.canedit; items = d.items || []; render(); })
                .catch(function () { });
        }

        function syncPending() {
            if (pendingField) pendingField.value = JSON.stringify(items.map(function (i) { return i.name; }));
        }

        // ── vykreslení ──────────────────────────────────────────────────────────

        function render() {
            var editing = list.querySelector(".chk-edit");
            if (editing && document.activeElement === editing) return;      // uživatel právě přepisuje text položky

            list.textContent = "";
            items.forEach(function (it) { list.appendChild(row(it)); });

            var total = items.length, ok = items.filter(function (i) { return i.done; }).length;
            if (template) {
                countEl.textContent = total > 0 ? String(total) : "";
            } else {
                countEl.textContent = total > 0 ? ok + " / " + total : "";
                barEl.style.width = total > 0 ? Math.round(100 * ok / total) + "%" : "0";
            }
            root.classList.toggle("chk-template", template);
            root.classList.toggle("chk-empty", total === 0);
            root.classList.toggle("chk-alldone", !template && total > 0 && ok === total);
            root.classList.toggle("chk-readonly", !canEdit);
            if (addBox) addBox.hidden = !canEdit;
            if (pending) syncPending();
        }

        function row(it) {
            var li = document.createElement("li");
            li.className = "chk-item" + (it.done ? " chk-done" : "");
            li.setAttribute("data-id", it.id);

            if (canEdit) {
                var grip = document.createElement("span");
                grip.className = "chk-grip material-icons-outlined-nosize";
                grip.textContent = "drag_indicator";
                grip.title = txt.move || "";
                grip.addEventListener("mousedown", function () { li.draggable = true; });
                grip.addEventListener("mouseup", function () { li.draggable = false; });
                li.appendChild(grip);
            }

            // zaškrtávací políčko jako tlačítko: nespouští událost "change", podle které formulář hlásí neuložené změny
            var box = document.createElement("button");
            box.type = "button";
            box.className = "chk-box";
            box.setAttribute("role", "checkbox");
            box.setAttribute("aria-checked", it.done ? "true" : "false");
            box.setAttribute("aria-label", it.name);
            box.disabled = !canEdit || pending || template;
            if (it.done) {
                var tick = document.createElement("span");
                tick.className = "material-icons-outlined-nosize";
                tick.textContent = "check";
                box.appendChild(tick);
            }
            box.addEventListener("click", function () { toggle(it); });
            li.appendChild(box);

            var name = document.createElement("span");
            name.className = "chk-name";
            name.textContent = it.name;
            if (it.done && (it.by || it.at)) {
                name.title = (txt.doneBy || "") + ": " + [it.by, it.at].filter(Boolean).join(", ");
            }
            if (canEdit) {
                name.tabIndex = 0;
                name.addEventListener("click", function () { edit(li, it); });
                name.addEventListener("keydown", function (e) { if (e.key === "Enter" || e.key === "F2") { e.preventDefault(); edit(li, it); } });
            }
            li.appendChild(name);

            if (canEdit) {
                var del = document.createElement("button");
                del.type = "button";
                del.className = "chk-del";
                del.title = txt.remove || "";
                del.setAttribute("aria-label", txt.remove || "");
                var x = document.createElement("span");
                x.className = "material-icons-outlined-nosize";
                x.textContent = "close";
                del.appendChild(x);
                del.addEventListener("click", function () { remove(it); });
                li.appendChild(del);

                li.addEventListener("dragstart", function (e) {
                    dragged = li;
                    li.classList.add("chk-dragging");
                    try { e.dataTransfer.effectAllowed = "move"; e.dataTransfer.setData("text/plain", String(it.id)); } catch (ex) { }
                });
                li.addEventListener("dragend", function () {
                    li.draggable = false;
                    li.classList.remove("chk-dragging");
                    if (dragged) { dragged = null; commitOrder(); }
                });
                li.addEventListener("dragover", function (e) {
                    if (!dragged || dragged === li) return;
                    e.preventDefault();
                    var r = li.getBoundingClientRect();
                    var before = e.clientY < r.top + r.height / 2;
                    list.insertBefore(dragged, before ? li : li.nextSibling);
                });
                li.addEventListener("drop", function (e) { e.preventDefault(); });
            }
            return li;
        }

        // ── akce ────────────────────────────────────────────────────────────────

        function toggle(it) {
            if (!canEdit || pending || template) return;
            it.done = !it.done;         // hned ukázat, server potvrdí
            render();
            post("Toggle", { p65id: it.id, done: it.done });
        }

        function add(text) {
            var names = splitLines(text);
            if (names.length === 0) return;
            if (pending) {
                names.forEach(function (n) { items.push({ id: --seq, name: n, done: false }); });
                render();
                return;
            }
            post("Add", { text: names.join("\n") });
        }

        function remove(it) {
            if (pending) {
                items = items.filter(function (i) { return i !== it; });
                render();
                return;
            }
            items = items.filter(function (i) { return i !== it; });
            render();
            post("Delete", { p65id: it.id });
        }

        function rename(it, value) {
            value = (value || "").replace(/\s+/g, " ").trim();
            if (value === "" || value === it.name) return;
            it.name = value.substring(0, cfg.maxLength || 500);
            if (!pending) post("Rename", { p65id: it.id, text: it.name });
        }

        function commitOrder() {
            var ids = Array.prototype.map.call(list.children, function (li) { return parseInt(li.getAttribute("data-id"), 10); });
            var byId = {};
            items.forEach(function (i) { byId[i.id] = i; });
            var changed = ids.some(function (id, idx) { return items[idx] && items[idx].id !== id; });
            if (!changed) return;
            items = ids.map(function (id) { return byId[id]; }).filter(Boolean);
            if (pending) { render(); return; }
            post("Reorder", { ids: ids.join(",") });
        }

        // přepsání textu položky přímo v řádku: Enter nebo opuštění pole uloží, Esc vrátí původní text
        function edit(li, it) {
            if (!canEdit || li.querySelector(".chk-edit")) return;
            var name = li.querySelector(".chk-name");
            var input = document.createElement("input");
            input.type = "text";
            input.className = "chk-edit form-control form-control-sm";
            input.maxLength = cfg.maxLength || 500;
            input.value = it.name;
            var cancelled = false;
            input.addEventListener("keydown", function (e) {
                if (e.key === "Enter") { e.preventDefault(); e.stopPropagation(); input.blur(); }
                if (e.key === "Escape") { e.preventDefault(); e.stopPropagation(); cancelled = true; input.blur(); }
            });
            input.addEventListener("change", function (e) { e.stopImmediatePropagation(); });    // psaní do tohoto pole není změna formuláře úkolu
            input.addEventListener("blur", function () {
                var v = input.value;
                input.className = "";       // ať render() nepovažuje pole za rozpracované
                if (!cancelled) rename(it, v);
                render();
            });
            name.replaceWith(input);
            input.focus();
            input.select();
        }

        // co řádek, to položka; odrážky a číslování na začátku řádku pryč (stejně to dělá server)
        function splitLines(text) {
            return String(text || "").split(/\r\n|\r|\n/).map(function (s) {
                return s.replace(/^\s*(?:[-*•–]\s+|\d{1,3}[.)]\s+)?(?:\[[ xX]?\]\s*)?/, "").replace(/\s+/g, " ").trim().substring(0, cfg.maxLength || 500);
            }).filter(function (s) { return s !== ""; });
        }

        // pole pro přidání vzniká až tady – po skriptu lišty formuláře, takže se na něj neváže hlídání neuložených změn
        function buildAdd() {
            if (!addBox) return;
            var input = document.createElement("input");
            input.type = "text";
            input.className = "form-control form-control-sm chk-new";
            input.placeholder = txt.placeholder || "";
            input.maxLength = cfg.maxLength || 500;
            input.setAttribute("aria-label", txt.placeholder || "");
            input.autocomplete = "off";
            input.addEventListener("keydown", function (e) {
                if (e.key !== "Enter") return;
                e.preventDefault();
                e.stopPropagation();
                add(input.value);
                input.value = "";
            });
            input.addEventListener("paste", function (e) {
                var t = (e.clipboardData || window.clipboardData).getData("text");
                if (!/\r|\n/.test(t)) return;       // jeden řádek se vloží běžně do pole
                e.preventDefault();
                add(input.value + t);
                input.value = "";
            });
            input.addEventListener("change", function (e) { e.stopImmediatePropagation(); });    // psaní do tohoto pole není změna formuláře úkolu
            addBox.appendChild(input);
        }

        buildAdd();
        render();
    }

    return { init: init };
})();
