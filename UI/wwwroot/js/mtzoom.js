// MtZoom – společné "zoom" okno: seznam úkonů apod. otevřený přímo pod místem kliknutí,
// takže zůstává vidět hodnota, na kterou uživatel kliknul (kalendář, dayline, docházka, kapacitní plán...).
// Volání:  zoom_open(prvek, url)  /  zoom_close()  /  zoom_toggle()   – stejné názvy jako dřív na jednotlivých stránkách.
// Stránka nic připravovat nemusí; prvek #zoomwin si okno vytvoří samo (existující na stránce použije).
var MtZoom = (function () {
    var MIN_H = 240, DEF_H = 450, GAP = 10, EDGE = 8, KEY_H = "mtzoom-height";
    var st = { ctl: null, url: null, max: false, above: false, h: null, drag: null, moved: null, mv: null };

    function win() {
        var c = document.getElementById("zoomwin");
        if (!c) { c = document.createElement("div"); c.id = "zoomwin"; }
        if (c.parentNode !== document.body) document.body.appendChild(c);   // souřadnice okna jsou vůči dokumentu
        if (!c.classList.contains("mtzoom")) c.classList.add("mtzoom");
        c.style.zIndex = "2000";
        if (!c.mtzoomBound) {       // kliknutí v okně (např. tažení za okraj) nesmí stránka brát jako "klik mimo"
            c.mtzoomBound = true;
            c.addEventListener("click", function (e) { e.stopPropagation(); });
        }
        return c;
    }
    function isOpen() {
        var c = document.getElementById("zoomwin");
        return !!c && c.style.display === "block";
    }
    function viewport() {
        var de = document.documentElement;
        return {
            w: window.innerWidth || de.clientWidth, h: window.innerHeight || de.clientHeight,
            sx: window.pageXOffset || de.scrollLeft || 0, sy: window.pageYOffset || de.scrollTop || 0
        };
    }
    function storedHeight() {
        if (st.h) return st.h;
        try { var n = parseInt(window.localStorage.getItem(KEY_H), 10); if (n >= MIN_H) return n; } catch (e) { }
        return DEF_H;
    }
    function fullUrl(url) {
        if (typeof _ep === "function" && typeof _relpath === "string" && url.substring(0, 1) === "/" && _relpath !== "/" && url.indexOf(_relpath) !== 0) {
            return _ep(url);    // stránky posílají adresu s virtuální složkou i bez ní
        }
        return url;
    }

    // Umístění: pod kliknutý prvek, nevejde-li se, tak nad něj; výška se případně zmenší, aby prvek zůstal vidět.
    // Vrací { left, top, width, height, above, arrow } v souřadnicích dokumentu.
    function compute(rect, vp, wantH) {
        var w = Math.min(vp.w - 2 * EDGE, 1700);
        if (w < 320) w = vp.w;
        var left = vp.sx + Math.max(0, (vp.w - w) / 2);
        var top0 = vp.sy + EDGE, bottom0 = vp.sy + vp.h - EDGE;
        var rTop = rect.top + vp.sy, rBottom = rect.bottom + vp.sy;
        var below = bottom0 - (rBottom + GAP), aboveSpace = (rTop - GAP) - top0;
        var h = Math.max(MIN_H, Math.min(wantH, vp.h - 2 * EDGE));
        var above = false, top;
        if (below >= h) { top = rBottom + GAP; }
        else if (aboveSpace >= h) { above = true; top = rTop - GAP - h; }
        else if (below >= MIN_H || aboveSpace >= MIN_H) {
            if (below >= aboveSpace) { h = below; top = rBottom + GAP; }
            else { above = true; h = aboveSpace; top = rTop - GAP - h; }
        } else {
            top = Math.max(top0, bottom0 - h);     // není místo nad ani pod: okno prvek překryje
            above = null;
        }
        var cx = rect.left + vp.sx + rect.width / 2 - left;
        return { left: Math.round(left), top: Math.round(top), width: Math.round(w), height: Math.round(h), above: above, arrow: Math.round(Math.max(22, Math.min(w - 22, cx))) };
    }

    function place() {
        if (!isOpen() || st.max || !st.ctl) return;
        var c = win();
        if (st.moved) {     // okno si uživatel přetáhl jinam: zůstává, kde ho nechal
            c.style.left = st.moved.left + "px"; c.style.top = st.moved.top + "px"; c.style.width = st.moved.width + "px"; c.style.height = st.moved.height + "px";
            return;
        }
        var p = compute(st.ctl.getBoundingClientRect(), viewport(), storedHeight());
        st.above = p.above === true;
        c.style.left = p.left + "px"; c.style.top = p.top + "px"; c.style.width = p.width + "px"; c.style.height = p.height + "px";
        c.classList.toggle("mtzoom-above", p.above === true);
        c.classList.toggle("mtzoom-noarrow", p.above === null);
        var a = c.querySelector(".mtzoom-arrow"); if (a) a.style.left = p.arrow + "px";
    }

    function mark(ctl) {
        var old = document.querySelectorAll(".zoom_ctl_current");
        for (var i = 0; i < old.length; i++) old[i].classList.remove("zoom_ctl_current");
        if (ctl && ctl.classList) ctl.classList.add("zoom_ctl_current");
    }

    function open(ctl, url) {
        if (ctl && ctl.jquery) ctl = ctl[0];
        url = fullUrl(String(url || ""));
        if (isOpen() && st.ctl === ctl && st.url === url) { close(); return; }   // druhý klik na stejné místo okno zavře
        st.ctl = ctl; st.url = url; st.max = false; st.moved = null;     // pozice se nepamatuje, okno se vždy otevře u hodnoty
        mark(ctl);

        var c = win();
        c.className = "mtzoom";
        c.innerHTML = "<div class='mtzoom-arrow'></div><div class='mtzoom-body'><div class='mtzoom-loading'><span></span></div>"
            + "<iframe id='frazoom' title='Zoom'></iframe></div><div class='mtzoom-move'></div><div class='mtzoom-grip'></div>";
        c.style.display = "block";
        place();

        var fra = c.querySelector("iframe");
        fra.addEventListener("load", function () {
            c.classList.add("mtzoom-ready");
            try { fra.contentWindow.document.addEventListener("keydown", onKey); } catch (e) { }    // Esc i z obsahu okna
        });
        fra.src = url;
        c.querySelector(".mtzoom-grip").addEventListener("mousedown", gripDown);
        c.querySelector(".mtzoom-move").addEventListener("mousedown", moveDown);
    }

    function close() {
        var c = document.getElementById("zoomwin");
        if (c) { c.style.display = "none"; c.innerHTML = ""; c.classList.remove("mtzoom-max"); }
        mark(null);
        st.ctl = null; st.url = null; st.max = false; st.moved = null;
    }

    function toggle() {
        if (!isOpen()) return;
        var c = win();
        st.max = !st.max;
        c.classList.toggle("mtzoom-max", st.max);
        if (st.max) { c.style.left = ""; c.style.top = ""; c.style.width = ""; c.style.height = ""; }
        else place();
    }

    // změna výšky tažením za spodní (u okna nad prvkem horní) okraj; výška se pamatuje
    function gripDown(e) {
        if (st.max) return;
        var c = win();
        st.drag = { y: e.clientY, h: c.offsetHeight };
        c.classList.add("mtzoom-drag");     // překryje iframe, aby tažení neutíkalo do obsahu
        document.addEventListener("mousemove", gripMove); document.addEventListener("mouseup", gripUp);
        e.preventDefault();
    }
    function gripMove(e) {
        if (!st.drag) return;
        var d = e.clientY - st.drag.y;
        st.h = Math.max(MIN_H, st.drag.h + (st.above ? -d : d));
        if (st.moved) st.moved.height = Math.round(st.h);
        place();
    }
    function gripUp() {
        document.removeEventListener("mousemove", gripMove); document.removeEventListener("mouseup", gripUp);
        var c = document.getElementById("zoomwin"); if (c) c.classList.remove("mtzoom-drag");
        if (st.drag && st.h) { try { window.localStorage.setItem(KEY_H, String(Math.round(st.h))); } catch (e) { } }
        st.drag = null;
    }

    // přetažení okna myší za horní lištu; pozice platí jen do zavření okna
    function moveDown(e) {
        if (st.max || e.button > 0) return;
        var c = win();
        st.mv = { x: e.clientX, y: e.clientY, left: c.offsetLeft, top: c.offsetTop, on: false };
        document.addEventListener("mousemove", moveMove); document.addEventListener("mouseup", moveUp);
        e.preventDefault();
    }
    function moveMove(e) {
        if (!st.mv) return;
        var dx = e.clientX - st.mv.x, dy = e.clientY - st.mv.y;
        if (!st.mv.on && Math.abs(dx) < 4 && Math.abs(dy) < 4) return;      // drobný pohyb při kliknutí není tažení
        var c = win(), vp = viewport();
        if (!st.mv.on) {
            st.mv.on = true;
            st.moved = { left: c.offsetLeft, top: c.offsetTop, width: c.offsetWidth, height: c.offsetHeight };
            st.above = false;
            c.classList.remove("mtzoom-above"); c.classList.add("mtzoom-noarrow", "mtzoom-drag", "mtzoom-moving");
        }
        var w = st.moved.width;
        st.moved.left = Math.round(Math.max(vp.sx - w + 120, Math.min(vp.sx + vp.w - 120, st.mv.left + dx)));   // kus lišty musí zůstat na obrazovce
        st.moved.top = Math.round(Math.max(vp.sy, Math.min(vp.sy + vp.h - 40, st.mv.top + dy)));
        place();
    }
    function moveUp() {
        document.removeEventListener("mousemove", moveMove); document.removeEventListener("mouseup", moveUp);
        var c = document.getElementById("zoomwin"); if (c) c.classList.remove("mtzoom-drag", "mtzoom-moving");
        st.mv = null;
    }

    function onKey(e) { if ((e.key === "Escape" || e.keyCode === 27) && isOpen()) close(); }

    if (typeof document !== "undefined" && document.addEventListener) {
        document.addEventListener("keydown", onKey);
        window.addEventListener("resize", place);
        document.addEventListener("scroll", place, true);      // i posun vnitřního panelu stránky: okno drží u kliknuté hodnoty
    }

    return { open: open, close: close, toggle: toggle, isOpen: isOpen, compute: compute, place: place };
})();

// názvy, které volají stránky i samotné okno (window.parent.zoom_close() apod.)
function zoom_open(ctl, url) { MtZoom.open(ctl, url); }
function zoom_close() { MtZoom.close(); }
function zoom_toggle() { MtZoom.toggle(); }

if (typeof module !== "undefined") { module.exports = MtZoom; }
