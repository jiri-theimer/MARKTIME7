// Uživatelská pole ve formuláři záznamu (Views/Shared/_FreeFields.cshtml): pole s vícenásobným výběrem.
//
// <div class="ff-ms" data-ff-ms data-fixed="true|false" data-options='["A","B"]'>
//     <input type="hidden" class="ff-ms-value" ...>     hodnoty oddělené znakem |
//     <div class="ff-ms-box"><input type="text" class="ff-ms-input"></div>
//     <div class="ff-ms-panel" hidden></div>
// </div>
//
// data-fixed="true"  – vybírá se jen z nabídky (x28IsFixedDataSource = 1)
// data-fixed="false" – nabídka je nápověda, uživatel může zapsat i vlastní hodnotu (Enter, čárka nebo středník)
//
// Skript snese opakované načtení i opakované volání MtFreeFields.init() (postback bez znovunačtení stránky). Bez jQuery.

var MtFreeFields = window.MtFreeFields || (function () {
    var DELIM = "|";
    var _open = null;       // právě otevřené pole

    function parse(s) {
        var out = [];
        (s || "").split(DELIM).forEach(function (p) {
            p = p.trim();
            if (p !== "" && out.indexOf(p) === -1) out.push(p);
        });
        return out;
    }

    function norm(s) {
        s = (s || "").toLowerCase();
        return s.normalize ? s.normalize("NFD").replace(/[̀-ͯ]/g, "") : s;
    }

    function el(tag, cls, text) {
        var e = document.createElement(tag);
        if (cls) e.className = cls;
        if (text != null) e.textContent = text;
        return e;
    }

    function setup(root) {
        var hidden = root.querySelector(".ff-ms-value");
        var box = root.querySelector(".ff-ms-box");
        var input = root.querySelector(".ff-ms-input");
        var panel = root.querySelector(".ff-ms-panel");
        if (!hidden || !box || !input || !panel) return;

        var fixed = root.getAttribute("data-fixed") === "true";
        var options = [];
        try { options = JSON.parse(root.getAttribute("data-options") || "[]"); } catch (e) { options = []; }
        var T = {
            add: root.getAttribute("data-text-add") || "Přidat",
            none: root.getAttribute("data-text-none") || "Žádná hodnota neodpovídá",
            remove: root.getAttribute("data-text-remove") || "Odebrat",
            unknown: root.getAttribute("data-text-unknown") || "Hodnota není v oboru hodnot pole",
            placeholder: root.getAttribute("data-placeholder") || ""
        };

        var values = parse(hidden.value);
        var rows = [];          // řádky nabídky: { value, add }
        var active = -1;

        function commit() {
            var v = values.join(DELIM);
            if (hidden.value === v) return;
            hidden.value = v;
            hidden.dispatchEvent(new Event("change", { bubbles: true }));   // formulář si tím všimne neuložené změny
        }

        function renderChips() {
            Array.prototype.slice.call(box.querySelectorAll(".ff-chip")).forEach(function (c) { box.removeChild(c); });
            values.forEach(function (v) {
                var chip = el("span", "ff-chip");
                if (fixed && options.indexOf(v) === -1) {
                    chip.className += " ff-chip-unknown";
                    chip.title = T.unknown;
                }
                chip.appendChild(el("span", "ff-chip-text", v));
                var x = el("button", "ff-chip-x", "×");
                x.type = "button";
                x.tabIndex = -1;
                x.setAttribute("aria-label", T.remove + ": " + v);
                x.addEventListener("click", function (e) {
                    e.stopPropagation();
                    toggle(v, false);
                    input.focus();
                });
                chip.appendChild(x);
                box.insertBefore(chip, input);
            });
            input.placeholder = values.length === 0 ? T.placeholder : "";
            root.classList.toggle("ff-ms-empty", values.length === 0);
        }

        function buildRows() {
            var q = norm(input.value.trim());
            rows = [];
            options.forEach(function (o) {
                if (q === "" || norm(o).indexOf(q) > -1) rows.push({ value: o, add: false });
            });
            var typed = input.value.trim();
            if (!fixed && typed !== "" && options.indexOf(typed) === -1 && values.indexOf(typed) === -1) {
                rows.push({ value: typed, add: true });
            }
        }

        function renderPanel() {
            buildRows();
            panel.innerHTML = "";
            if (rows.length === 0) {
                panel.appendChild(el("div", "ff-ms-none", T.none));
                active = -1;
                return;
            }
            if (active >= rows.length) active = rows.length - 1;
            rows.forEach(function (r, idx) {
                var selected = values.indexOf(r.value) > -1;
                var row = el("div", "ff-ms-opt" + (selected ? " is-selected" : "") + (idx === active ? " is-active" : "") + (r.add ? " ff-ms-add" : ""));
                row.setAttribute("role", "option");
                row.setAttribute("aria-selected", selected ? "true" : "false");
                row.appendChild(el("span", "ff-ms-check", r.add ? "+" : (selected ? "✓" : "")));
                row.appendChild(el("span", "ff-ms-opt-text", r.add ? T.add + " „" + r.value + "“" : r.value));
                row.addEventListener("mousedown", function (e) { e.preventDefault(); });     // kurzor zůstane v poli
                row.addEventListener("click", function () {
                    pick(r);
                    input.focus();
                });
                panel.appendChild(row);
            });
            var act = panel.querySelector(".is-active");
            if (act && act.scrollIntoView) act.scrollIntoView({ block: "nearest" });
        }

        function toggle(v, selected) {
            var i = values.indexOf(v);
            if (selected === undefined) selected = i === -1;
            if (selected && i === -1) values.push(v);
            if (!selected && i > -1) values.splice(i, 1);
            commit();
            renderChips();
            if (isOpen()) renderPanel();
        }

        function pick(r) {
            if (r.add) {
                addTyped();
                return;
            }
            var hadFilter = input.value !== "";
            input.value = "";
            if (hadFilter) active = -1;
            toggle(r.value);
        }

        // vlastní hodnota zapsaná uživatelem (jen volný výběr)
        function addTyped() {
            var typed = input.value.split(DELIM).join(" ").trim();
            if (typed === "") return false;
            if (fixed) {
                var match = options.filter(function (o) { return norm(o) === norm(typed); })[0];
                if (!match) return false;
                typed = match;
            } else {
                // stejná hodnota jinými velikostmi písmen se bere z nabídky
                var same = options.filter(function (o) { return norm(o) === norm(typed); })[0];
                if (same) typed = same;
            }
            input.value = "";
            active = -1;
            toggle(typed, true);
            return true;
        }

        function isOpen() {
            return !panel.hidden;
        }

        function open() {
            if (isOpen()) return;
            if (_open && _open !== api) _open.close();
            _open = api;
            panel.hidden = false;
            root.classList.add("ff-ms-open");
            renderPanel();
        }

        function close() {
            if (!isOpen()) return;
            panel.hidden = true;
            root.classList.remove("ff-ms-open");
            active = -1;
            if (_open === api) _open = null;
        }

        var api = { close: close, root: root };

        box.addEventListener("mousedown", function (e) {
            if (e.target === input) return;
            e.preventDefault();
            input.focus();
            open();
        });
        input.addEventListener("focus", open);
        input.addEventListener("click", open);
        input.addEventListener("input", function () {
            if (!fixed && /[,;]/.test(input.value)) {
                // čárka nebo středník potvrzuje zapsanou hodnotu
                var parts = input.value.split(/[,;]/);
                input.value = parts.pop();
                parts.forEach(function (p) {
                    p = p.split(DELIM).join(" ").trim();
                    if (p !== "") toggle(p, true);
                });
            }
            active = input.value.trim() === "" ? -1 : 0;
            open();
            renderPanel();
        });
        input.addEventListener("blur", function () {
            // rozepsaná vlastní hodnota se při odchodu z pole neztratí
            if (!fixed) addTyped(); else input.value = "";
            setTimeout(function () {
                if (document.activeElement !== input) close();
            }, 0);
        });
        input.addEventListener("keydown", function (e) {
            switch (e.key) {
                case "ArrowDown":
                    e.preventDefault();
                    open();
                    if (rows.length > 0) { active = (active + 1) % rows.length; renderPanel(); }
                    break;
                case "ArrowUp":
                    e.preventDefault();
                    open();
                    if (rows.length > 0) { active = active <= 0 ? rows.length - 1 : active - 1; renderPanel(); }
                    break;
                case "Enter":
                    e.preventDefault();     // Enter nesmí odeslat formulář
                    e.stopPropagation();
                    if (isOpen() && active > -1 && rows[active]) {
                        if (input.value.trim() !== "" && !rows[active].add && values.indexOf(rows[active].value) > -1) {
                            // napsaná hodnota už vybraná je: Enter ji neodebere
                            input.value = "";
                            active = -1;
                            renderPanel();
                        } else {
                            pick(rows[active]);
                        }
                    } else {
                        addTyped();
                    }
                    break;
                case "Escape":
                    if (isOpen()) {
                        e.preventDefault();
                        e.stopPropagation();    // Escape zavře jen nabídku, ne okno záznamu
                        input.value = "";
                        close();
                    }
                    break;
                case "Backspace":
                    if (input.value === "" && values.length > 0) {
                        toggle(values[values.length - 1], false);
                    }
                    break;
            }
        });

        renderChips();
    }

    function init(scope) {
        var list = (scope || document).querySelectorAll("[data-ff-ms]");
        Array.prototype.forEach.call(list, function (root) {
            if (root.getAttribute("data-ff-ready") === "1") return;
            root.setAttribute("data-ff-ready", "1");
            setup(root);
        });
    }

    // klepnutí mimo pole zavře nabídku
    document.addEventListener("mousedown", function (e) {
        if (_open && !_open.root.contains(e.target)) _open.close();
    });

    return { init: init, parse: parse };
})();
