// Postback bez znovunačtení stránky ("měkký postback") pro formuláře v _LayoutRecord.
//
// Server dělá totéž co dřív: stejný POST, stejná akce kontroleru, stejné view. Rozdíl je jen v prohlížeči:
// odpověď se nestáhne jako nová stránka, ale vymění se z ní obsah formuláře #form1.
//
// Zapnutí ve view:  <script>window._softpostback = true;</script>
// Podmínky pro view: vše, co se postbackem mění, je uvnitř těla view (tedy uvnitř #form1), a skripty view snesou
// opakované spuštění (deklarace přes var/function, ne let/const na nejvyšší úrovni).
// Prvek s atributem data-soft-keep-display si přes postback pamatuje, zda byl rozbalený (style.display).

var MtSoftPostback = (function () {
    var _busy = false;
    var JS_TYPES = ["", "text/javascript", "application/javascript"];

    function enabled() {
        return window._softpostback === true
            && typeof window.fetch === "function"
            && typeof window.DOMParser === "function"
            && !!document.getElementById("form1")
            && !!document.getElementById("mtPageState");
    }

    function isBusy() {
        return _busy;
    }

    // data formuláře ve stejném tvaru, v jakém je posílá klasické odeslání (urlencoded, bez souborů, konce řádků CRLF)
    function formBody(form) {
        var p = new URLSearchParams();
        new FormData(form).forEach(function (v, k) {
            if (typeof v === "string") p.append(k, v.replace(/\r\n|\r|\n/g, "\r\n"));
        });
        return p;
    }

    function isJs(script) {
        return JS_TYPES.indexOf((script.getAttribute("type") || "").trim().toLowerCase()) > -1;
    }

    // uvolnit pomocné prvky, které si ovládací prvky starého obsahu vytvořily mimo formulář (nabídky, kalendář, bubliny)
    function cleanup(form) {
        var $f = window.jQuery ? window.jQuery(form) : null;
        if (!$f) return;
        try {
            $f.find("[data-bs-toggle='dropdown']").each(function () {
                var inst = window.bootstrap && bootstrap.Dropdown ? bootstrap.Dropdown.getInstance(this) : null;
                if (inst) inst.dispose();
            });
        } catch (e) { }
        try {
            $f.find("[data-bs-toggle='tooltip'],[data-toggle='tooltip']").each(function () {
                var inst = window.bootstrap && bootstrap.Tooltip ? bootstrap.Tooltip.getInstance(this) : null;
                if (inst) inst.dispose();
            });
        } catch (e) { }
        try {
            $f.find("input").each(function () {
                if (window.jQuery(this).data("datepicker")) window.jQuery(this).datepicker("destroy");
            });
        } catch (e) { }
        try {
            if (window.jQuery.fn.qtip) $f.find("[data-hasqtip]").qtip("destroy", true);
        } catch (e) { }
    }

    function removeNodes(nodes) {
        nodes.forEach(function (n) {
            if (window.jQuery && n.nodeType === 1) {
                window.jQuery(n).remove();      // uvolní i události a data jQuery
            } else if (n.parentNode) {
                n.parentNode.removeChild(n);
            }
        });
    }

    // skripty z odpovědi spustit ve stejném pořadí jako při běžném načtení stránky
    function runScripts(container, list, done) {
        var i = 0;
        function next() {
            if (i >= list.length) { done(); return; }
            var def = list[i];
            var marker = container.querySelector("script[data-soft-idx='" + i + "']");
            i++;
            if (!marker) { next(); return; }

            var s = document.createElement("script");
            def.attrs.forEach(function (a) { s.setAttribute(a.name, a.value); });
            if (def.src) {
                s.async = false;
                s.onload = next;
                s.onerror = next;
                marker.parentNode.replaceChild(s, marker);
            } else {
                s.text = def.text;
                marker.parentNode.replaceChild(s, marker);  // vložením se skript hned provede
                next();
            }
        }
        next();
    }

    function showWholeResponse(html) {
        // odpověď není tento formulář (přihlášení, chybová stránka, přesměrování) – zobrazit ji tak, jak přišla
        document.open();
        document.write(html);
        document.close();
    }

    function classicSubmit(form, ocas) {
        if (ocas !== "") form.action = form.action + "?" + ocas;
        form.submit();
    }

    function apply(form, html) {
        var doc = new DOMParser().parseFromString(html, "text/html");
        var newForm = doc.getElementById("form1");
        var stateEl = doc.getElementById("mtPageState");
        if (!newForm || !stateEl) {
            showWholeResponse(html);
            return false;
        }

        var state = {};
        try { state = JSON.parse(stateEl.textContent || "{}"); } catch (e) { state = {}; }

        // skripty zatím znehybnit, spustí se až po vložení (runScripts)
        var scripts = [];
        Array.prototype.forEach.call(newForm.querySelectorAll("script"), function (sc) {
            if (!isJs(sc)) return;
            var attrs = [];
            Array.prototype.forEach.call(sc.attributes, function (a) { attrs.push({ name: a.name, value: a.value }); });
            scripts.push({ attrs: attrs, src: sc.getAttribute("src"), text: sc.textContent });
            sc.setAttribute("data-soft-idx", String(scripts.length - 1));
            sc.setAttribute("type", "application/x-softpostback");
            sc.removeAttribute("src");
            sc.textContent = "";
        });

        var scrollX = window.pageXOffset, scrollY = window.pageYOffset;

        // co si má pamatovat rozbalení
        var keep = {};
        Array.prototype.forEach.call(form.querySelectorAll("[data-soft-keep-display][id]"), function (el) {
            keep[el.id] = el.style.display;
        });

        // vnořené rámce se stejnou adresou (nahrávání příloh) ponechat i s jejich stavem, pokud to prohlížeč umí
        var holder = null, kept = [];
        if (typeof Element.prototype.moveBefore === "function") {
            Array.prototype.forEach.call(form.querySelectorAll("iframe[id]"), function (oldFra) {
                var newFra = null;
                Array.prototype.forEach.call(newForm.querySelectorAll("iframe[id]"), function (f) {
                    if (f.id === oldFra.id && f.getAttribute("src") === oldFra.getAttribute("src")) newFra = f;
                });
                if (!newFra) return;
                if (!holder) {
                    holder = document.createElement("div");
                    holder.style.display = "none";
                    document.body.appendChild(holder);
                }
                try {
                    holder.moveBefore(oldFra, null);
                    newFra.setAttribute("data-soft-kept", String(kept.length));
                    newFra.removeAttribute("id");
                    newFra.removeAttribute("src");
                    kept.push(oldFra);
                } catch (e) { }
            });
        }

        cleanup(form);
        removeNodes(Array.prototype.slice.call(form.childNodes));

        var frag = document.createDocumentFragment();
        Array.prototype.forEach.call(newForm.childNodes, function (n) {
            frag.appendChild(document.importNode(n, true));
        });
        form.appendChild(frag);

        kept.forEach(function (oldFra, idx) {
            var ph = form.querySelector("iframe[data-soft-kept='" + idx + "']");
            try {
                if (ph) { ph.parentNode.moveBefore(oldFra, ph); ph.parentNode.removeChild(ph); }
            } catch (e) { }
        });
        if (holder && holder.parentNode) holder.parentNode.removeChild(holder);

        // záhlaví a titulek okna (mění se např. se sešitem)
        var newHeader = doc.getElementById("mtPageHeader"), header = document.getElementById("mtPageHeader");
        if (newHeader && header) header.innerHTML = newHeader.innerHTML;
        if (doc.title) document.title = doc.title;

        runScripts(form, scripts, function () {
            var finish = function () {
                Object.keys(keep).forEach(function (id) {
                    var el = document.getElementById(id);
                    if (el && el.hasAttribute("data-soft-keep-display")) el.style.display = keep[id];
                });
                window.scrollTo(scrollX, scrollY);
                if (typeof window._recordpage_after_render === "function") {
                    window._recordpage_after_render(state);
                }
                document.dispatchEvent(new CustomEvent("softpostback_done", { detail: state }));
            };
            // $(document).ready obsluhy ze skriptů view běží po již načtené stránce se zpožděním – dokončení zařadit až za ně
            if (window.jQuery) { window.jQuery(finish); } else { finish(); }
        });
        return true;
    }

    function run(oper, ocas) {
        if (_busy) return;
        var form = document.getElementById("form1");
        if (typeof oper === "undefined" || oper === null) oper = "";
        if (typeof ocas === "undefined" || ocas === null) ocas = "";

        document.getElementById("IsPostback").value = "true";
        document.getElementById("PostbackOper").value = oper;
        var counter = document.getElementById("PostbackBylUzKolikrat");
        if (counter) counter.value = String((parseInt(counter.value, 10) || 0) + 1);

        var url = form.action + (ocas !== "" ? "?" + ocas : "");
        var body = formBody(form);

        _busy = true;
        form.style.pointerEvents = "none";
        // ukazatel načítání až když odpověď trvá déle – u rychlé odpovědi by jen problikl
        var loaderTimer = setTimeout(function () {
            if (typeof window._loading_show === "function") window._loading_show("postback");
        }, 300);

        var release = function () {
            clearTimeout(loaderTimer);
            _busy = false;
            form.style.pointerEvents = "";
        };

        fetch(url, {
            method: "POST",
            credentials: "same-origin",
            headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8", "X-MT-SoftPostback": "1" },
            body: body.toString()
        }).then(function (resp) {
            return resp.text();
        }).then(function (html) {
            release();
            if (typeof window._loading_hide === "function") window._loading_hide();
            try {
                apply(form, html);
            } catch (e) {
                // výměna obsahu se nepovedla – odpověď serveru už máme, zobrazit ji celou (bez druhého odeslání)
                showWholeResponse(html);
            }
        }, function () {
            // síť selhala dřív, než přišla odpověď – odeslat postaru
            release();
            classicSubmit(form, ocas);
        });
    }

    return { enabled: enabled, run: run, isBusy: isBusy };
})();
