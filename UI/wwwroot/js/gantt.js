// Gantt projektů a úkolů (Views/gantt/Index.cshtml). Čistý JS bez knihoven.
// Dny se počítají jako celé číslo (počet dní od 1.1.1970 v UTC), aby do výpočtů nezasahovalo časové pásmo ani letní čas.
var MtGantt = (function () {
    var DAY = 86400000;
    var PX = { day: 30, week: 12, month: 4 };       // šířka jednoho dne v px podle měřítka
    var PAD = { day: 7, week: 21, month: 62 };      // rezerva dní před a za plánem

    function pad2(n) { return n < 10 ? "0" + n : "" + n; }
    function parseDay(s) { if (!s) return null; var a = s.split("-"); return Math.round(Date.UTC(+a[0], +a[1] - 1, +a[2]) / DAY); }
    function isoDay(n) { var d = new Date(n * DAY); return d.getUTCFullYear() + "-" + pad2(d.getUTCMonth() + 1) + "-" + pad2(d.getUTCDate()); }
    function human(n) { var d = new Date(n * DAY); return d.getUTCDate() + "." + (d.getUTCMonth() + 1) + "." + d.getUTCFullYear(); }
    function humanShort(n) { var d = new Date(n * DAY); return d.getUTCDate() + "." + (d.getUTCMonth() + 1) + "." + ("" + d.getUTCFullYear()).substring(2); }
    function dow(n) { return (new Date(n * DAY).getUTCDay() + 6) % 7; }     // 0 = pondělí
    function monthStart(n) { var d = new Date(n * DAY); return Math.round(Date.UTC(d.getUTCFullYear(), d.getUTCMonth(), 1) / DAY); }
    function nextMonth(n) { var d = new Date(n * DAY); return Math.round(Date.UTC(d.getUTCFullYear(), d.getUTCMonth() + 1, 1) / DAY); }
    function esc(s) {
        if (s === null || s === undefined) return "";
        return String(s).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#39;");
    }
    function monthLabel(n, wide) {
        var d = new Date(n * DAY);
        if (!wide) return (d.getUTCMonth() + 1) + "/" + ("" + d.getUTCFullYear()).substring(2);
        try {
            return new Intl.DateTimeFormat(document.documentElement.lang || "cs", { month: "long", year: "numeric", timeZone: "UTC" }).format(d);
        } catch (e) {
            return (d.getUTCMonth() + 1) + "/" + d.getUTCFullYear();
        }
    }
    function notify(msg, template) {
        if (typeof _notify_message === "function") { _notify_message(msg, template); }
    }
    function post(url, data) {
        var body = new URLSearchParams();
        for (var k in data) { body.append(k, data[k]); }
        return fetch(url, { method: "POST", headers: { "Content-Type": "application/x-www-form-urlencoded", "X-Requested-With": "XMLHttpRequest" }, body: body.toString() });
    }

    function init(opt) {
        var el = opt.el;
        var tra = opt.tra || {};
        var st = {
            rows: [], byKey: {}, kids: {}, collapsed: {}, zoom: PX[opt.zoom] ? opt.zoom : "week", tasks: !!opt.tasks,
            filter: "", today: null, start: 0, end: 0, px: 12, truncated: false, loaded: false, drag: null
        };
        var tip = document.getElementById("gt_tip");
        var storeKey = "gantt-collapsed-" + (opt.p41id || 0);

        try {
            var raw = window.sessionStorage.getItem(storeKey);
            if (raw) { raw.split(",").forEach(function (k) { if (k) st.collapsed[k] = true; }); }
        } catch (e) { }

        function saveCollapsed() {
            try { window.sessionStorage.setItem(storeKey, Object.keys(st.collapsed).join(",")); } catch (e) { }
        }

        // ---------- data ----------
        function load(keepScroll) {
            var pos = keepScroll ? { left: el.scrollLeft, top: el.scrollTop, start: st.start } : null;
            return fetch("/gantt/Data?p41id=" + (opt.p41id || 0) + "&tasks=" + (st.tasks ? "true" : "false"), { headers: { "X-Requested-With": "XMLHttpRequest" } })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (!data || !data.ok) {
                        el.innerHTML = "<div class='gt-empty'>" + esc(data && data.message ? data.message : tra.chyba) + "</div>";
                        return;
                    }
                    st.rows = data.rows || [];
                    st.byKey = {}; st.kids = {};
                    st.rows.forEach(function (r) {
                        r.n1 = parseDay(r.d1); r.n2 = parseDay(r.d2);
                        st.byKey[r.key] = r;
                        if (r.parent) { st.kids[r.parent] = (st.kids[r.parent] || 0) + 1; }
                    });
                    st.today = parseDay(data.today);
                    st.truncated = !!data.truncated;
                    var first = !st.loaded;
                    st.loaded = true;
                    render();
                    if (pos) {
                        el.scrollTop = pos.top;
                        el.scrollLeft = Math.max(0, pos.left + (pos.start - st.start) * st.px);
                    } else if (first) {
                        scrollToDay(firstInterestingDay());
                    }
                })
                .catch(function () {
                    el.innerHTML = "<div class='gt-empty'>" + esc(tra.chyba) + "</div>";
                });
        }

        function firstInterestingDay() {
            var min = null;
            st.rows.forEach(function (r) { if (r.n1 !== null && (min === null || r.n1 < min)) min = r.n1; });
            if (min === null) return st.today;
            // dnešek, pokud leží uvnitř plánů; jinak začátek prvního plánu
            var max = null;
            st.rows.forEach(function (r) { if (r.n2 !== null && (max === null || r.n2 > max)) max = r.n2; });
            return (st.today >= min && st.today <= max) ? st.today : min;
        }

        // ---------- viditelné řádky ----------
        function visibleRows() {
            var out = [];
            var f = st.filter;
            if (f.length >= 2) {
                var hit = {}, anc = {};
                st.rows.forEach(function (r) {
                    var own = ((r.name || "") + " " + (r.code || "")).toLowerCase().indexOf(f) >= 0;
                    hit[r.key] = own || (r.parent && hit[r.parent] === true);      // řádky pod nalezeným projektem se ukazují také
                    if (own) {
                        var p = r.parent, guard = 0;
                        while (p && guard++ < 20) { anc[p] = true; p = st.byKey[p] ? st.byKey[p].parent : null; }
                    }
                });
                st.rows.forEach(function (r) { if (hit[r.key] || anc[r.key]) out.push(r); });
                return out;
            }
            var hidden = {};
            st.rows.forEach(function (r) {
                if (r.parent && (hidden[r.parent] || st.collapsed[r.parent])) { hidden[r.key] = true; return; }
                out.push(r);
            });
            return out;
        }

        // ---------- rozsah osy ----------
        function computeRange() {
            st.px = PX[st.zoom];
            var min = st.today, max = st.today;
            st.rows.forEach(function (r) {
                if (r.n1 !== null) { if (r.n1 < min) min = r.n1; if (r.n2 > max) max = r.n2; }
            });
            var start = min - PAD[st.zoom], end = max + PAD[st.zoom] + 1;
            start = (st.zoom === "month") ? monthStart(start) : start - dow(start);
            var leftW = leftWidth();
            var need = Math.ceil(Math.max(0, el.clientWidth - leftW) / st.px) + 1;      // osa má vyplnit celou šířku okna
            if (end - start < need) end = start + need;
            end = (st.zoom === "month") ? nextMonth(end - 1) : end + ((7 - dow(end)) % 7);
            st.start = start; st.end = end;
        }

        function leftWidth() {
            var v = parseInt(window.getComputedStyle(el).getPropertyValue("--gt-left"), 10);
            return isNaN(v) ? 430 : v;
        }

        // ---------- vykreslení ----------
        function barHtml(r) {
            var left = (r.n1 - st.start) * st.px, width = (r.n2 - r.n1 + 1) * st.px;
            var cls = "gt-bar" + (r.edit > 0 ? " gt-ed" : " gt-ro") + (r.onlyend ? " gt-end" : "");
            var style = "left:" + left + "px;width:" + width + "px;";
            if (r.color && /^#[0-9a-fA-F]{3,8}$/.test(r.color)) style += "background:" + r.color + ";";
            var title = r.name + (r.code ? " (" + r.code + ")" : "") + "\n" + (r.onlyend ? tra.termin + ": " + human(r.n2) : human(r.n1) + " – " + human(r.n2));
            if (r.hours) title += "\n" + tra.hodiny + ": " + r.hours;
            var s = "<div class='" + cls + "' style='" + style + "' title='" + esc(title) + "'>";
            if (r.edit > 0) s += "<b class='gt-hl'></b>";
            s += "<span>" + (width >= 70 ? esc(r.name) : "") + "</span>";
            if (r.edit > 0) s += "<b class='gt-hr'></b>";
            s += "</div>";
            if (width < 70) s += "<div class='gt-out' style='left:" + (left + width) + "px;'>" + esc(r.name) + "</div>";
            return s;
        }

        function render() {
            if (st.rows.length === 0) {
                el.innerHTML = "<div class='gt-empty'>" + esc(tra.zadnadata) + "</div>";
                setInfo(0);
                return;
            }
            computeRange();
            var px = st.px, W = (st.end - st.start) * px, leftW = leftWidth();
            var rows = visibleRows();
            var h = [];

            h.push("<div class='gt-content' style='width:" + (leftW + W) + "px;'>");

            // hlavička
            h.push("<div class='gt-head'><div class='gt-corner'><div class='gt-c-name'>" + esc(tra.nazev) + "</div><div class='gt-c-per'>" + esc(tra.obdobi) + "</div><div class='gt-c-hrs'>" + esc(tra.hodiny) + "</div></div>");
            h.push("<div class='gt-axis' style='width:" + W + "px;'><div class='gt-ax1'>");
            var lines = [];
            var m = st.start;
            while (m < st.end) {
                var nm = Math.min(nextMonth(m), st.end);
                var mw = (nm - m) * px;
                h.push("<span style='left:" + ((m - st.start) * px) + "px;width:" + mw + "px;'>" + (mw >= 34 ? esc(monthLabel(m, mw >= 110)) : "") + "</span>");
                if (m > st.start) lines.push("<i style='left:" + ((m - st.start) * px) + "px;'></i>");
                m = nm;
            }
            h.push("</div><div class='gt-ax2'>");
            var d;
            if (st.zoom === "day") {
                for (d = st.start; d < st.end; d++) {
                    h.push("<span class='gt-day" + (dow(d) >= 5 ? " gt-we" : "") + "' style='left:" + ((d - st.start) * px) + "px;width:" + px + "px;'>" + new Date(d * DAY).getUTCDate() + "</span>");
                }
            } else if (st.zoom === "week") {
                for (d = st.start; d < st.end; d += 7) {
                    var dd = new Date(d * DAY);
                    h.push("<span style='left:" + ((d - st.start) * px) + "px;width:" + (7 * px) + "px;'>" + dd.getUTCDate() + "." + (dd.getUTCMonth() + 1) + ".</span>");
                }
            }
            h.push("</div></div></div>");

            // pozadí: víkendy, linky dnů/týdnů, hranice měsíců, dnešek
            var bg = "";
            if (st.zoom !== "month") {
                var unit = (st.zoom === "day") ? px : 7 * px;
                bg = "background-image:repeating-linear-gradient(to right,#eef0f3 0,#eef0f3 1px,transparent 1px,transparent " + unit + "px)," +
                    "linear-gradient(to right,transparent 0,transparent " + (5 * px) + "px,#f6f7f9 " + (5 * px) + "px,#f6f7f9 " + (7 * px) + "px);" +
                    "background-size:" + unit + "px 100%," + (7 * px) + "px 100%;";
            }
            if (st.today >= st.start && st.today < st.end) {
                lines.push("<i class='gt-today' style='left:" + ((st.today - st.start) * px + Math.floor(px / 2)) + "px;'></i>");
            }
            h.push("<div class='gt-bg' style='left:" + leftW + "px;top:var(--gt-head);width:" + W + "px;bottom:0;" + bg + "'>" + lines.join("") + "</div>");

            // řádky
            if (rows.length === 0) {
                h.push("<div class='gt-empty'>" + esc(tra.zadnyvysledek) + "</div>");
            }
            rows.forEach(function (r) {
                var haskids = st.kids[r.key] > 0;
                var open = !st.collapsed[r.key] || st.filter.length >= 2;
                h.push("<div class='gt-row gt-" + r.prefix + (r.closed ? " gt-closed" : "") + "' data-key='" + esc(r.key) + "'>");
                h.push("<div class='gt-cell'><div class='gt-name' style='padding-left:" + (8 + r.depth * 16) + "px;'>");
                h.push("<button type='button' class='gt-tgl'" + (haskids ? " data-tgl='1'" : " tabindex='-1'") + ">" + (haskids ? (open ? "&#9662;" : "&#9656;") : "") + "</button>");
                h.push("<span class='material-icons-outlined gt-ico'>" + (r.prefix === "p41" ? "work_outline" : "task_alt") + "</span>");
                h.push("<a data-open='1' title='" + esc(r.name + (r.code ? " (" + r.code + ")" : "")) + "'>" + esc(r.name) + "</a>");
                if (r.sub) h.push("<span class='gt-sub'>" + esc(r.sub) + "</span>");
                h.push("</div><div class='gt-per'>" + (r.n1 !== null ? (r.onlyend ? humanShort(r.n2) : humanShort(r.n1) + " – " + humanShort(r.n2)) : "") + "</div>");
                h.push("<div class='gt-hrs'>" + (r.hours ? esc(r.hours) : "") + "</div></div>");
                if (r.n1 !== null) {
                    h.push("<div class='gt-track' style='width:" + W + "px;'>" + barHtml(r) + "</div>");
                } else if (r.edit > 0) {
                    h.push("<div class='gt-track gt-draw' style='width:" + W + "px;'><span class='gt-hint'>" + esc(tra.bezplanu) + "</span></div>");
                } else {
                    h.push("<div class='gt-track' style='width:" + W + "px;'></div>");
                }
                h.push("</div>");
            });

            h.push("</div>");
            el.innerHTML = h.join("");
            setInfo(rows.length);
            markZoom();
        }

        function setInfo(n) {
            var info = document.getElementById("gt_info");
            if (info) info.textContent = (n > 0 ? "(" + n + ")" : "") + (st.truncated ? " " + tra.oriznuto : "");
        }
        function markZoom() {
            var btns = document.querySelectorAll(".gt-zoom [data-zoom]");
            for (var i = 0; i < btns.length; i++) { btns[i].classList.toggle("active", btns[i].getAttribute("data-zoom") === st.zoom); }
        }
        function scrollToDay(n) {
            if (n === null) return;
            var view = Math.max(0, el.clientWidth - leftWidth());
            el.scrollLeft = Math.max(0, (n - st.start) * st.px - Math.floor(view / 3));
        }
        function centerDay() {
            var view = Math.max(0, el.clientWidth - leftWidth());
            return st.start + Math.round((el.scrollLeft + view / 2) / st.px);
        }

        // ---------- tažení myší ----------
        function rowOf(node) {
            var div = node.closest(".gt-row");
            return div ? st.byKey[div.getAttribute("data-key")] : null;
        }
        function showTip(e, n1, n2) {
            if (!tip) return;
            tip.textContent = (n1 === n2 ? human(n1) : human(n1) + " – " + human(n2) + "  (" + (n2 - n1 + 1) + " " + tra.dni + ")");
            tip.style.display = "block";
            tip.style.left = (e.clientX + 14) + "px";
            tip.style.top = (e.clientY + 16) + "px";
        }
        function hideTip() { if (tip) tip.style.display = "none"; }

        function onPointerDown(e) {
            if (e.button !== undefined && e.button !== 0) return;
            if (st.drag) return;
            var t = e.target;
            var bar = t.closest(".gt-bar");
            var r, track;
            if (bar && bar.classList.contains("gt-ed")) {
                r = rowOf(bar);
                if (!r) return;
                st.drag = {
                    row: r, bar: bar, mode: t.classList.contains("gt-hl") ? "l" : (t.classList.contains("gt-hr") ? "r" : "move"),
                    x0: e.clientX + el.scrollLeft, n1o: r.n1, n2o: r.n2, n1: r.n1, n2: r.n2, moved: false
                };
            } else {
                track = t.closest(".gt-track.gt-draw");
                if (!track) return;
                r = rowOf(track);
                if (!r) return;
                var day = st.start + Math.floor((e.clientX - track.getBoundingClientRect().left) / st.px);
                bar = document.createElement("div");
                bar.className = "gt-bar gt-ed gt-drag";
                bar.style.left = ((day - st.start) * st.px) + "px";
                bar.style.width = st.px + "px";
                track.appendChild(bar);
                st.drag = { row: r, bar: bar, track: track, mode: "new", x0: e.clientX, anchor: day, n1o: null, n2o: null, n1: day, n2: day, moved: false };
            }
            e.preventDefault();
            document.addEventListener("pointermove", onPointerMove);
            document.addEventListener("pointerup", onPointerUp);
            document.addEventListener("pointercancel", onPointerCancel);
        }

        function onPointerMove(e) {
            var g = st.drag;
            if (!g) return;
            if (g.mode === "new") {
                if (Math.abs(e.clientX - g.x0) > 4) g.moved = true;
                var day = st.start + Math.floor((e.clientX - g.track.getBoundingClientRect().left) / st.px);
                day = Math.max(st.start, Math.min(st.end - 1, day));
                g.n1 = Math.min(g.anchor, day); g.n2 = Math.max(g.anchor, day);
            } else {
                var dd = Math.round((e.clientX + el.scrollLeft - g.x0) / st.px);
                if (dd !== 0) g.moved = true;
                if (g.mode === "move") { g.n1 = g.n1o + dd; g.n2 = g.n2o + dd; }
                if (g.mode === "l") { g.n1 = Math.min(g.n1o + dd, g.n2o); g.n2 = g.n2o; }
                if (g.mode === "r") { g.n2 = Math.max(g.n2o + dd, g.n1o); g.n1 = g.n1o; }
            }
            g.bar.classList.add("gt-drag");
            g.bar.style.left = ((g.n1 - st.start) * st.px) + "px";
            g.bar.style.width = ((g.n2 - g.n1 + 1) * st.px) + "px";
            showTip(e, g.n1, g.n2);
        }

        function endDrag() {
            document.removeEventListener("pointermove", onPointerMove);
            document.removeEventListener("pointerup", onPointerUp);
            document.removeEventListener("pointercancel", onPointerCancel);
            hideTip();
            var g = st.drag;
            st.drag = null;
            return g;
        }

        function onPointerCancel() {
            endDrag();
            keepScrollRender();
        }

        function onPointerUp() {
            var g = endDrag();
            if (!g) return;
            g.bar.classList.remove("gt-drag");
            var changed = (g.mode === "new") ? g.moved : (g.n1 !== g.n1o || g.n2 !== g.n2o);
            if (!changed) {
                if (g.mode === "new") { g.bar.remove(); }     // bez překreslení: prvek pod myší musí zůstat stejný, jinak by nedošel dvojklik
                return;
            }
            st.lastDragAt = Date.now();
            save(g);
        }

        function keepScrollRender() {
            var left = el.scrollLeft, top = el.scrollTop, start = st.start;
            render();
            el.scrollTop = top;
            el.scrollLeft = Math.max(0, left + (start - st.start) * st.px);
        }

        function save(g) {
            g.bar.classList.add("gt-saving");
            post("/gantt/SaveDates", { prefix: g.row.prefix, pid: g.row.pid, d1: isoDay(g.n1), d2: isoDay(g.n2) })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (data && data.ok) {
                        g.row.n1 = g.n1; g.row.n2 = g.n2; g.row.d1 = isoDay(g.n1); g.row.d2 = isoDay(g.n2);
                        if (g.n1 !== g.n2) g.row.onlyend = false;
                        if (g.row.edit === 2) g.row.edit = 1;
                    } else {
                        if (g.row.edit === 2 && g.mode !== "new") g.row.edit = 0;   // server zápis odmítl: příště už pruh nenabízet k tažení
                        notify(data && data.message ? data.message : tra.chyba, "error");
                    }
                    keepScrollRender();
                })
                .catch(function () {
                    notify(tra.chyba, "error");
                    keepScrollRender();
                });
        }

        // ---------- události ----------
        el.addEventListener("pointerdown", onPointerDown);
        el.addEventListener("click", function (e) {
            var t = e.target;
            var tgl = t.closest("[data-tgl]");
            var r;
            if (tgl) {
                r = rowOf(tgl);
                if (!r) return;
                if (st.collapsed[r.key]) { delete st.collapsed[r.key]; } else { st.collapsed[r.key] = true; }
                saveCollapsed();
                keepScrollRender();
                return;
            }
            if (t.closest("[data-open]")) {
                r = rowOf(t);
                if (r && typeof _edit === "function") _edit(r.prefix, r.pid);
            }
        });
        el.addEventListener("dblclick", function (e) {
            var bar = e.target.closest(".gt-bar");
            if (!bar) return;
            if (st.lastDragAt && Date.now() - st.lastDragAt < 600) return;   // dvojklik těsně po tažení neotevírá kartu
            var r = rowOf(bar);
            if (r && typeof _edit === "function") _edit(r.prefix, r.pid);
        });

        var zoomBtns = document.querySelectorAll(".gt-zoom [data-zoom]");
        for (var i = 0; i < zoomBtns.length; i++) {
            zoomBtns[i].addEventListener("click", function () {
                var z = this.getAttribute("data-zoom");
                if (!PX[z] || z === st.zoom) return;
                var c = st.loaded && st.rows.length > 0 ? centerDay() : null;
                st.zoom = z;
                post("/Common/SetUserParam", { key: "gantt-zoom", value: z });
                render();
                if (c !== null) {
                    var view = Math.max(0, el.clientWidth - leftWidth());
                    el.scrollLeft = Math.max(0, (c - st.start) * st.px - view / 2);
                }
            });
        }
        var cmdToday = document.getElementById("gt_today");
        if (cmdToday) cmdToday.addEventListener("click", function () { scrollToDay(st.today); });

        var chkTasks = document.getElementById("gt_tasks");
        if (chkTasks) chkTasks.addEventListener("change", function () {
            st.tasks = this.checked;
            post("/Common/SetUserParam", { key: "gantt-tasks", value: st.tasks ? "1" : "0" });
            load(true);
        });

        var txt = document.getElementById("gt_search");
        var timer = null;
        if (txt) txt.addEventListener("input", function () {
            var val = this.value;
            window.clearTimeout(timer);
            timer = window.setTimeout(function () {
                st.filter = (val || "").trim().toLowerCase();
                keepScrollRender();
            }, 200);
        });

        markZoom();
        load(false);

        return {
            reload: function () { return load(true); },
            state: st
        };
    }

    return { init: init };
})();
