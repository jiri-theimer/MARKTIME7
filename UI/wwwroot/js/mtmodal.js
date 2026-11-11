// Modální okno aplikace – jedna implementace pro _Layout, _LayoutModal a _LayoutRecord.
//
// Okno je nativní <dialog> (fokus, inertní pozadí a vrstvu nad stránkou řeší prohlížeč), obsah se načítá do iframe.
// Kvůli zbytku aplikace zůstávají původní ID prvků (myModalContainer, myModalContent, myModalFrame, fraModalContent,
// myModalContentHeader, spanToggle) a u otevřeného okna platí jako dřív myModalContainer.style.display == "block".
//
// Layouty si nechávají své funkce _window_open / _window_close (liší se v tom, jak skládají adresu a komu zavření předají)
// a volají z nich MtModal.open / MtModal.close / MtModal.toggle.
var MtModal = (function () {
    var cfg = {
        nested: false,              // okno otevřené ze stránky, která sama běží v okně (_LayoutModal, _LayoutRecord)
        handleBackdrop: true,       // klik do ztmaveného pozadí okno zavře
        confirmText: "Chcete zavřít okno bez uložení změn?",
        closeTitle: "Zavřít",
        toggleTitle: "Maximalizovat / obnovit",
        onLoad: null,               // voláno po načtení stránky do iframe
        address: false,             // zapisovat adresu okna do adresního řádku (#w=…) – jen hlavní stránka (_Layout), viz „adresa okna“ níže
        addressPattern: /\/[A-Za-z0-9_]+\/(record|recpage|info)\/?$/i   // která okna adresu dostanou: záznamy a info stránky
    };
    var dlg = null, win = null, head = null, title = null, fra = null;
    var drag = null, downOnBackdrop = false, closing = false, unloadTimer = null;
    var ADDR = "#w=", addrOpened = null;        // addrOpened: adresa, se kterou bylo okno otevřeno (null = okno adresu nemá)

    function $id(id) { return document.getElementById(id); }

    function build() {
        dlg = document.createElement("dialog");
        dlg.id = "myModalContainer";
        dlg.className = "mtm" + (cfg.nested ? " mtm-nested" : "");
        dlg.style.display = "none";
        // statická kostra bez uživatelského textu; titulek se nastavuje přes textContent
        dlg.innerHTML =
            "<div id='myModalContent' tabindex='-1'>" +
            "<div id='myModalContentContainerHeader'>" +
            "<span id='myModalContentHeader'></span>" +
            "<button type='button' class='mtm-btn nonmobile' id='cmdModalToggle'><span id='spanToggle' class='material-icons-outlined-btn'>crop_landscape</span></button>" +
            "<button type='button' class='mtm-btn' id='cmdModalClose'><span class='material-icons-outlined-btn'>close</span></button>" +
            "</div>" +
            "<div id='myModalFrame'>" +
            "<iframe id='fraModalContent' name='fraModalContent'></iframe>" +
            "<div class='mtm-loader'><span class='mtm-spinner'></span></div>" +
            "</div>" +
            "</div>";
        document.body.appendChild(dlg);

        win = $id("myModalContent"); head = $id("myModalContentContainerHeader"); title = $id("myModalContentHeader"); fra = $id("fraModalContent");
        $id("cmdModalToggle").title = cfg.toggleTitle;
        $id("cmdModalClose").title = cfg.closeTitle;
        dlg.setAttribute("aria-labelledby", "myModalContentHeader");
        dlg.setAttribute("closedby", "none");       // prohlížeč okno sám nezavírá, o zavření rozhoduje close()

        fra.addEventListener("load", function () {
            dlg.classList.remove("mtm-loading");
            addressOnFrameLoad();
            if (typeof cfg.onLoad === "function") { try { cfg.onLoad(); } catch (e) { } }
        });

        // zavření: přes globální _window_close layoutu, aby platila jeho pravidla (neuložené změny, předání rodiči)
        $id("cmdModalClose").addEventListener("click", requestClose);
        $id("cmdModalToggle").addEventListener("click", toggle);
        head.addEventListener("dblclick", function (e) { if (!e.target.closest("button")) toggle(); });

        // Escape obsluhují layouty v keydown (jako dřív). Prohlížeč okno sám zavřít nesmí – obešel by dotaz na neuložené změny.
        // Starší prohlížeče bez closedby pošlou "cancel"; kdyby okno přesto zavřely, hned se vrátí zpět.
        dlg.addEventListener("cancel", function (e) { e.preventDefault(); });
        dlg.addEventListener("close", function () {
            if (closing || dlg.style.display !== "block") return;
            try { dlg.showModal(); } catch (e) { }
        });

        // klik do ztmaveného pozadí (stisk i uvolnění mimo okno, aby okno nezavřelo dotažení myši z okna ven)
        dlg.addEventListener("mousedown", function (e) { downOnBackdrop = (e.target === dlg); });
        dlg.addEventListener("click", function (e) {
            var hit = (e.target === dlg) && downOnBackdrop;
            downOnBackdrop = false;
            if (hit && cfg.handleBackdrop) requestClose();
        });

        // přesouvání za záhlaví (myš i dotyk)
        head.addEventListener("pointerdown", dragStart);
        head.addEventListener("pointermove", dragMove);
        head.addEventListener("pointerup", dragEnd);
        head.addEventListener("pointercancel", dragEnd);

        window.addEventListener("resize", keepInView);
        hookNotify();
    }

    function ensure() {
        if (dlg) return dlg;
        var old = $id("myModalContainer");
        if (old && old.parentNode) old.parentNode.removeChild(old);     // pozůstatek staré kostry
        build();
        return dlg;
    }

    function isOpen() { return !!dlg && dlg.open; }
    function isPhone() { return document.documentElement.classList.contains("mt-phone"); }
    function isMax() { return dlg.classList.contains("mtm-max") || isPhone(); }

    function requestClose() {
        // layout může mít vlastní _window_close (např. předání zavření rodiči); pro samotné okno je to close()
        if (cfg.nested && typeof window._window_in_modal_close === "function") { window._window_in_modal_close(); return; }
        if (!cfg.nested && typeof window._window_close === "function") { window._window_close(); return; }
        close();
    }

    // ── otevření / zavření ──────────────────────────────────────────────────────

    // url: hotová adresa pro iframe ("" = adresu neměnit); maximized: true = přes celou plochu
    function open(url, header, maximized) {
        ensure();
        if (typeof header !== "undefined" && header !== null) title.textContent = header;
        setMax(!!maximized);
        resetPosition();

        if (url) {
            if (unloadTimer) { clearTimeout(unloadTimer); unloadTimer = null; }
            dlg.classList.add("mtm-loading");
            fra.src = url;
            addrOpened = addressable(url);
            if (addrOpened) { addressSet(addrOpened); } else { addressClear(); }
        }
        if (!dlg.open) {
            try { dlg.showModal(); } catch (e) { dlg.setAttribute("open", ""); }
        }
        dlg.style.display = "block";
        document.documentElement.classList.add("mtm-open");
        moveNotify();
        try { win.focus({ preventScroll: true }); } catch (e) { }     // fokus na okno, ne na tlačítko Zavřít (mezerník by okno zavřel)
    }

    function hasUnsavedChanges() {
        try {
            var doc = fra.contentWindow.document;
            var el = doc ? doc.getElementById("toolbar_changeinfo") : null;
            return !!(el && (el.textContent || "") !== "");
        } catch (e) {
            return false;
        }
    }

    // vrací false, pokud uživatel zavření odmítl kvůli neuloženým změnám
    function close(force) {
        if (!dlg) return true;
        if (!force && isOpen() && hasUnsavedChanges() && !window.confirm(cfg.confirmText)) return false;
        hide();
        return true;
    }

    function hide() {
        if (!dlg) return;
        addrOpened = null;
        addressClear();
        closing = true;
        dlg.style.display = "none";
        if (dlg.open) { try { dlg.close(); } catch (e) { dlg.removeAttribute("open"); } }
        closing = false;
        document.documentElement.classList.remove("mtm-open");
        moveNotify();
        try { window.focus(); } catch (e) { }
        unloadLater();
    }

    // Po zavření se stránka z iframe uvolní: přestanou běžet její časovače, spojení a přehrávání a uvolní se paměť.
    // Až po doběhnutí právě běžícího skriptu – o zavření často žádá sama stránka v okně (window.parent._window_close()).
    function unloadLater() {
        if (unloadTimer) clearTimeout(unloadTimer);
        unloadTimer = setTimeout(function () {
            unloadTimer = null;
            if (!dlg || isOpen()) return;
            try {
                fra.contentWindow.location.replace("about:blank");      // replace: bez nového kroku v historii prohlížeče
            } catch (e) {
                fra.src = "about:blank";
            }
        }, 0);
    }

    function setMax(on) {
        dlg.classList.toggle("mtm-max", on);
        var s = $id("spanToggle");
        if (s) s.textContent = on ? "close_fullscreen" : "crop_landscape";
    }
    function toggle() {
        if (!dlg || cfg.nested || isPhone()) return;
        setMax(!dlg.classList.contains("mtm-max"));
        resetPosition();
    }

    // ── adresa okna ─────────────────────────────────────────────────────────────
    // Otevřené okno se záznamem je vidět v adresním řádku jako #w=<zakódovaná adresa stránky v okně>. Odkaz tak jde zkopírovat,
    // poslat nebo otevřít v nové záložce a po obnovení stránky (F5) se okno otevře znovu.
    // - Jen hlavní stránka (cfg.address) a jen okna podle cfg.addressPattern; okna otevřená z okna adresu nemají.
    // - Adresa se mění přes history.replaceState: stránka se nenačítá a v historii prohlížeče nepřibývají kroky.
    // - Hodnota je zakódovaná (base64url), aby ji kód, který hledá text v location.href, nezaměnil za adresu stránky.
    // - Adresa platí, jen dokud je v okně stránka, se kterou bylo otevřeno. Jakmile z ní okno odchází (uložení, postback),
    //   adresa zmizí – volající stránka se pak může obnovit přes location.href bez #.
    // - Stránka, která # používá sama, zůstává bez adresy okna.

    // vrací adresu vhodnou do adresního řádku (cesta + parametry v rámci aplikace), jinak null
    function addressable(url) {
        if (!cfg.address || cfg.nested || typeof url !== "string" || !window.history || !history.replaceState) return null;
        if (url.length > 1500 || url.charAt(0) !== "/" || url.charAt(1) === "/" || url.charAt(1) === "\\") return null;
        var u;
        try { u = new URL(url, window.location.origin); } catch (e) { return null; }
        if (u.origin !== window.location.origin || u.hash !== "") return null;      // jen stránky této aplikace
        if (!cfg.addressPattern.test(u.pathname)) return null;
        return u.pathname + u.search;
    }
    function addressEncode(s) {
        return btoa(encodeURI(s)).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
    }
    function addressDecode(s) {
        try {
            s = s.replace(/-/g, "+").replace(/_/g, "/");
            while (s.length % 4) s += "=";
            return decodeURI(atob(s));
        } catch (e) { return null; }
    }
    function addressOwnsHash() {
        var h = window.location.hash;
        return h === "" || h === "#" || h.indexOf(ADDR) === 0;
    }
    function addressSet(url) {
        if (!addressOwnsHash()) return;
        try { history.replaceState(history.state, "", window.location.pathname + window.location.search + ADDR + addressEncode(url)); } catch (e) { }
    }
    function addressClear() {
        if (!cfg.address || window.location.hash.indexOf(ADDR) !== 0) return;
        try { history.replaceState(history.state, "", window.location.pathname + window.location.search); } catch (e) { }
    }
    function addressOnFrameLoad() {
        if (!cfg.address || !addrOpened) return;
        try {
            var w = fra.contentWindow, loc = w.location;
            if (isOpen() && loc.pathname + loc.search === addrOpened) { addressSet(addrOpened); } else { addressClear(); }
            w.addEventListener("pagehide", addressClear);       // okno odchází ze stránky (uložení, postback, jiný odkaz)
        } catch (e) { addressClear(); }
    }
    // po načtení hlavní stránky: otevřít okno z adresy a pohlídat obnovení stránky přes hardrefresh
    function addressFromHash() {
        if (window.location.hash.indexOf(ADDR) !== 0) return;
        var url = addressable(addressDecode(window.location.hash.substring(ADDR.length)) || "");
        if (url) { open(url, "", false); } else { addressClear(); }
    }
    function addressStart() {
        addressFromHash();
        window.addEventListener("hashchange", addressFromHash);     // odkaz vložený do adresního řádku na téže stránce (replaceState tuto událost nevyvolá)
        // hardrefresh volá stránka v okně ještě před zavřením okna a typicky dělá location.href = location.href;
        // s # v adrese by to prohlížeč vzal jen jako skok na kotvu a stránku by neobnovil
        if (typeof window.hardrefresh === "function" && !window.hardrefresh.mtm) {
            var orig = window.hardrefresh;
            window.hardrefresh = function () {
                addrOpened = null;
                addressClear();
                return orig.apply(this, arguments);
            };
            window.hardrefresh.mtm = true;
        }
    }

    // ── přesouvání ──────────────────────────────────────────────────────────────

    function resetPosition() {
        dlg.classList.remove("mtm-moved");
        win.style.left = ""; win.style.top = "";
    }
    function clamp(x, y) {
        var w = win.offsetWidth, vw = window.innerWidth, vh = window.innerHeight;
        x = Math.min(Math.max(x, 80 - w), vw - 80);      // z okna musí zůstat vidět aspoň kus záhlaví
        y = Math.min(Math.max(y, 0), vh - 34);
        return { x: x, y: y };
    }
    function dragStart(e) {
        if (isMax() || e.button > 0 || e.target.closest("button")) return;
        var r = win.getBoundingClientRect();
        drag = { dx: e.clientX - r.left, dy: e.clientY - r.top, id: e.pointerId, moved: false, left: r.left, top: r.top };
        try { head.setPointerCapture(e.pointerId); } catch (ex) { }
    }
    function dragMove(e) {
        if (!drag || e.pointerId !== drag.id) return;
        if (!drag.moved) {
            if (Math.abs(e.clientX - drag.dx - drag.left) < 3 && Math.abs(e.clientY - drag.dy - drag.top) < 3) return;   // drobné cuknutí při kliknutí není přesun
            drag.moved = true;
            win.style.left = drag.left + "px"; win.style.top = drag.top + "px";
            dlg.classList.add("mtm-moved", "mtm-dragging");
        }
        var p = clamp(e.clientX - drag.dx, e.clientY - drag.dy);
        win.style.left = p.x + "px"; win.style.top = p.y + "px";
    }
    function dragEnd(e) {
        if (!drag || e.pointerId !== drag.id) return;
        try { head.releasePointerCapture(e.pointerId); } catch (ex) { }
        drag = null;
        dlg.classList.remove("mtm-dragging");
    }
    function keepInView() {
        if (!dlg || !dlg.classList.contains("mtm-moved")) return;
        var r = win.getBoundingClientRect(), p = clamp(r.left, r.top);
        win.style.left = p.x + "px"; win.style.top = p.y + "px";
    }

    // ── oznámení (_notify_message) ──────────────────────────────────────────────
    // Otevřený dialog je ve vrstvě nad celou stránkou, takže oznámení vložená do <body> by zůstala pod ním.
    // Po dobu otevření se proto kontejner oznámení přesouvá do dialogu.

    function moveNotify() {
        var box = $id("notify_container");
        if (!box) return;
        var target = isOpen() ? dlg : document.body;
        if (box.parentNode !== target) target.appendChild(box);
    }
    function hookNotify() {
        if (typeof window._notify_message !== "function" || window._notify_message.mtm) return;
        var orig = window._notify_message;
        window._notify_message = function () {
            var ret = orig.apply(this, arguments);
            moveNotify();
            return ret;
        };
        window._notify_message.mtm = true;
    }

    return {
        init: function (options) {
            for (var k in (options || {})) { if (Object.prototype.hasOwnProperty.call(options, k)) cfg[k] = options[k]; }
            if (!cfg.nested) ensure();      // hlavní stránka má okno připravené hned, vnořené vzniká až při prvním otevření
            if (cfg.address && !cfg.nested) {
                // až po načtení celé stránky: skripty view (vlastní hardrefresh) už jsou definované
                if (document.readyState === "loading") { document.addEventListener("DOMContentLoaded", addressStart); } else { addressStart(); }
            }
            return this;
        },
        open: open,
        close: close,
        toggle: toggle,
        isOpen: isOpen,
        isMaximized: function () { return !!dlg && isMax(); },
        element: function () { return dlg; },
        ensure: ensure
    };
})();
