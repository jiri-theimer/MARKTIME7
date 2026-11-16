// "Najít text" pro textová pole záznamu: panel s hledáním v textech dříve uložených záznamů (obdoba p31ft v p31record.js,
// stejný vzhled – třídy .p31ft* v site.css). Použito na kartě vyúčtování (p91/Record: Text faktury, Technický text).
//
// Tlačítko:  <button onclick="mtFindText.toggle(this)" data-url="/p91/FindTexts" data-field="text1" data-target="Rec_p91Text1"
//                    data-client="Rec_p28ID" data-pid="123" data-texts="idPrvkuSTexty">
//   data-url     adresa (POST: q, field, p28id, scope, exclude_pid) → { items: [{ text, date, code, client, count }], more }
//   data-target  id textarea, kam se text vloží;  data-client  id pole s ID klienta (zúžení „Tento klient“)
//   data-texts   id prvku s přeloženými texty v atributech data-* (ph, all, client, recent, found, empty, norecent, loading, error, counttitle, hint)
// Klik / Enter = nahradit text, Ctrl+klik / Ctrl+Enter = připojit na konec. Text s HTML značkami se v seznamu ukazuje bez značek, vkládá se celý.
var mtFindText = (function () {
    var _panel = null, _btn = null, _items = [], _active = -1, _seq = 0, _timer = null, _scope = "all", _words = [];

    function T(key, def) {
        var holder = _btn ? document.getElementById(_btn.getAttribute("data-texts") || "") : null;
        return (holder && holder.getAttribute("data-" + key)) || def;
    }
    function txt() { return _btn ? document.getElementById(_btn.getAttribute("data-target")) : null; }
    function clientid() {
        var c = _btn ? document.getElementById(_btn.getAttribute("data-client") || "") : null;
        return c ? (parseInt(c.value, 10) || 0) : 0;
    }
    function esc(s) { return String(s == null ? "" : s).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;"); }
    function fold(s) { return s.normalize("NFD").replace(/[̀-ͯ]/g, "").toLowerCase(); }   //bez diakritiky, malá písmena
    function is_html(s) { return /<\s*\/?\s*[a-zA-Z][^>]*>/.test(s); }
    //text bez HTML značek pro zobrazení v seznamu (DOMParser skripty nespouští)
    function plain(s) {
        if (!is_html(s)) return s;
        var html = s.replace(/<\s*br\s*\/?>/gi, "\n").replace(/<\/\s*(p|div|li|h[1-6]|tr)\s*>/gi, "\n");
        var doc = new DOMParser().parseFromString(html, "text/html");
        return (doc.body.textContent || "").replace(/ /g, " ").replace(/\n{3,}/g, "\n\n").trim();
    }

    function is_open() { return _panel !== null && _panel.style.display !== "none"; }

    function build() {
        _panel = document.createElement("div");
        _panel.className = "p31ft";
        _panel.setAttribute("role", "dialog");
        _panel.style.display = "none";
        _panel.innerHTML =
            "<div class='p31ft-head'><span class='material-icons-outlined p31ft-ico' aria-hidden='true'>search</span>"
            + "<input type='search' class='p31ft-q' autocomplete='off' spellcheck='false'>"
            + "<button type='button' class='btn-close p31ft-close' tabindex='-1' title='Esc'></button></div>"
            + "<div class='p31ft-scope'>"
            + "<button type='button' class='p31ft-chip' data-scope='all'></button>"
            + "<button type='button' class='p31ft-chip' data-scope='client'></button>"
            + "<span class='p31ft-info'></span></div>"
            + "<div class='p31ft-list'></div>"
            + "<div class='p31ft-foot'></div>";
        document.body.appendChild(_panel);

        var q = _panel.querySelector(".p31ft-q");
        q.addEventListener("input", function () {
            clearTimeout(_timer);
            _timer = setTimeout(load, 250);
        });
        q.addEventListener("keydown", on_keydown);
        _panel.querySelector(".p31ft-close").addEventListener("click", function () { close(true); });
        _panel.querySelector(".p31ft-scope").addEventListener("click", function (e) {
            var chip = e.target.closest(".p31ft-chip");
            if (!chip || chip.disabled) return;
            set_scope(chip.getAttribute("data-scope"));
            load();
            q.focus();
        });
        _panel.querySelector(".p31ft-list").addEventListener("click", function (e) {
            var row = e.target.closest(".p31ft-item");
            if (row) choose(parseInt(row.getAttribute("data-i"), 10), e.ctrlKey || e.metaKey);
        });
        document.addEventListener("mousedown", function (e) {
            if (!is_open()) return;
            if (_panel.contains(e.target) || (_btn && _btn.contains(e.target))) return;
            close(false);   //klik mimo panel
        });
        window.addEventListener("resize", function () { if (is_open()) place(); });
    }

    function set_scope(scope) {
        if (scope !== "client" || clientid() === 0) scope = "all";      //bez vybraného klienta není podle čeho zúžit
        _scope = scope;
        try { localStorage.setItem("mtfindtext_scope", scope); } catch (e) { }
        var chips = _panel.querySelectorAll(".p31ft-chip");
        for (var i = 0; i < chips.length; i++) chips[i].classList.toggle("active", chips[i].getAttribute("data-scope") === _scope);
    }

    function place() {     //pod pole textu, ve stejné šířce; když dole není místo, přisadit k dolnímu okraji okna
        var t = txt();
        if (!t) return;
        var r = t.getBoundingClientRect();
        var w = Math.max(Math.min(r.width, 820), Math.min(420, window.innerWidth - 16));
        var left = Math.max(8, Math.min(r.left, window.innerWidth - w - 8));
        var maxh = 460;
        var top = r.bottom + 4;
        if (top + 280 > window.innerHeight) top = Math.max(8, window.innerHeight - Math.min(maxh, window.innerHeight - 16) - 8);
        maxh = Math.max(200, Math.min(maxh, window.innerHeight - top - 8));
        _panel.style.width = w + "px";
        _panel.style.left = (left + window.scrollX) + "px";
        _panel.style.top = (top + window.scrollY) + "px";
        _panel.style.maxHeight = maxh + "px";
    }

    function open(btn) {
        _btn = btn;
        if (!txt()) return;
        if (_panel === null) build();

        _panel.querySelector(".p31ft-q").placeholder = T("ph", "Hledat v textech…");
        _panel.querySelector(".p31ft-foot").textContent = T("hint", "Klik nebo Enter = nahradit text · Ctrl+klik nebo Ctrl+Enter = připojit na konec textu");
        var chips = _panel.querySelectorAll(".p31ft-chip");
        chips[0].textContent = T("all", "Vše");
        chips[1].textContent = T("client", "Tento klient");
        chips[1].disabled = clientid() === 0;

        var scope = "all";
        try { scope = localStorage.getItem("mtfindtext_scope") || "all"; } catch (e) { }
        _items = [];
        _panel.style.display = "flex";
        set_scope(scope);
        place();

        var q = _panel.querySelector(".p31ft-q");
        q.value = "";
        q.focus();
        load();
    }

    function close(focus_text) {
        if (!is_open()) return;
        clearTimeout(_timer);
        _seq++;     //rozpracovaná odpověď serveru se už nevykreslí
        _panel.style.display = "none";
        if (focus_text && txt()) txt().focus();
    }

    function toggle(btn) {
        if (is_open() && _btn === btn) close(true);
        else { if (is_open()) close(false); open(btn); }
    }

    function status(text, spinner) {
        _items = [];
        _active = -1;
        _panel.querySelector(".p31ft-info").textContent = "";
        _panel.querySelector(".p31ft-list").innerHTML = "<div class='p31ft-status'>" + (spinner ? "<span class='spinner-border spinner-border-sm me-2' role='status'></span>" : "") + esc(text) + "</div>";
    }

    function load() {
        var q = _panel.querySelector(".p31ft-q").value.trim();
        var seq = ++_seq;
        if (_items.length === 0) status(T("loading", "Načítám…"), true);

        var pars = {
            q: q,
            field: _btn.getAttribute("data-field"),
            p28id: clientid(),
            scope: _scope,
            exclude_pid: parseInt(_btn.getAttribute("data-pid"), 10) || 0
        };
        $.post(_ep(_btn.getAttribute("data-url")), pars, function (data) {
            if (seq !== _seq || !is_open()) return;     //mezitím uživatel psal dál nebo panel zavřel
            _words = fold(q).split(/\s+/).filter(function (w) { return w.length >= 2; });
            render(data, q);
        }).fail(function () {
            if (seq !== _seq || !is_open()) return;
            status(T("error", "Texty se nepodařilo načíst."), false);
        });
    }

    function render(data, q) {
        _items = (data && data.items) ? data.items : [];
        _active = -1;
        var list = _panel.querySelector(".p31ft-list");
        if (_items.length === 0) {
            status(q === "" ? T("norecent", "Zatím žádné texty.") : T("empty", "Žádný text neodpovídá hledání."), false);
            return;
        }
        _panel.querySelector(".p31ft-info").textContent = (q === "" ? T("recent", "Naposledy použité texty") : T("found", "Nalezeno") + ": " + _items.length + (data.more ? "+" : ""));

        var s = [];
        for (var i = 0; i < _items.length; i++) {
            var c = _items[i];
            var meta = [esc(c.date)];
            if (c.code) meta.push(esc(c.code));
            if (c.client) meta.push(esc(c.client));
            s.push("<div class='p31ft-item' data-i='" + i + "'><div class='p31ft-text'>" + mark(plain(c.text)) + "</div>"
                + "<div class='p31ft-meta'>" + meta.join(" · ")
                + (is_html(c.text) ? "<span class='p31ft-count'>HTML</span>" : "")
                + (c.count > 1 ? "<span class='p31ft-count' title='" + esc(T("counttitle", "Kolikrát byl text použit")) + "'>" + c.count + "×</span>" : "")
                + "</div></div>");
        }
        list.innerHTML = s.join("");
        list.scrollTop = 0;
    }

    //zvýraznění hledaných slov bez ohledu na diakritiku a velikost písmen (stejně hledá server)
    function mark(text) {
        if (_words.length === 0) return esc(text);
        var folded = "", map = [];
        for (var i = 0; i < text.length; i++) {
            var f = fold(text.charAt(i));
            for (var k = 0; k < f.length; k++) { folded += f.charAt(k); map.push(i); }
        }
        var on = new Array(text.length);
        _words.forEach(function (w) {
            var pos = folded.indexOf(w);
            while (pos > -1) {
                for (var m = pos; m < pos + w.length; m++) on[map[m]] = true;
                pos = folded.indexOf(w, pos + w.length);
            }
        });
        var out = "", open = false;
        for (var j = 0; j < text.length; j++) {
            if (on[j] && !open) { out += "<mark>"; open = true; }
            if (!on[j] && open) { out += "</mark>"; open = false; }
            out += esc(text.charAt(j));
        }
        if (open) out += "</mark>";
        return out;
    }

    function set_active(i) {
        var rows = _panel.querySelectorAll(".p31ft-item");
        if (rows.length === 0) return;
        if (i < 0) i = 0;
        if (i > rows.length - 1) i = rows.length - 1;
        if (_active > -1 && rows[_active]) rows[_active].classList.remove("active");
        _active = i;
        rows[i].classList.add("active");
        rows[i].scrollIntoView({ block: "nearest" });
    }

    function on_keydown(e) {
        if (e.key === "ArrowDown") { e.preventDefault(); set_active(_active + 1); }
        else if (e.key === "ArrowUp") { e.preventDefault(); set_active(_active - 1); }
        else if (e.key === "Enter") {
            e.preventDefault();     //Enter nesmí odeslat formulář
            if (_items.length > 0) choose(_active > -1 ? _active : 0, e.ctrlKey || e.metaKey);
        }
        else if (e.key === "Escape") { e.preventDefault(); e.stopPropagation(); close(true); }
    }

    //vložení textu: append=false nahradí celý text, append=true připojí na nový řádek. Přes insertText, aby šlo vrátit Ctrl+Z.
    function choose(i, append) {
        var c = _items[i], t = txt();
        if (!c || !t) return;
        var text = c.text;
        close(false);
        mtFindText.insert(t, text, append);
    }

    // společné i pro jiné úpravy textového pole (např. zrušení formátování): změna jde vrátit Ctrl+Z a pole ohlásí změnu
    function insert(t, text, append) {
        t.focus();
        if (append && t.value.trim() !== "") {
            text = (/\n$/.test(t.value) ? "" : "\n") + text;
            t.setSelectionRange(t.value.length, t.value.length);
        } else {
            append = false;
            t.setSelectionRange(0, t.value.length);
        }
        var before = t.value, ok = false;
        try { ok = document.execCommand("insertText", false, text); } catch (e) { ok = false; }
        if (!ok || t.value === before) {
            t.value = append ? before + text : text;
        }
        t.dispatchEvent(new Event("input", { bubbles: true }));
        t.dispatchEvent(new Event("change", { bubbles: true }));
        t.setSelectionRange(t.value.length, t.value.length);
        if (typeof _toolbar_warn2save_changes === "function") _toolbar_warn2save_changes();
    }

    return { toggle: toggle, close: close, insert: insert, plain: plain, isHtml: is_html };
})();
