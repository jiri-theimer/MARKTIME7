// Kapacitní plány (Views/r11/Index.cshtml). Čistý JS bez knihoven.
// Pohled "people": řádek osoby = vytížení (naplánované hodiny proti kapacitě = pracovní fond minus absence), pod ním projekty osoby.
// Pohled "projects": řádek projektu = naplánované hodiny projektu, pod ním lidé projektu; proužek dole ukazuje celkové vytížení člověka.
// Rozpad hodin na dny počítá server (r11/Data); po každé změně se data načtou znovu.
// Dny se počítají jako celé číslo (počet dní od 1.1.1970 v UTC), aby do výpočtů nezasahovalo časové pásmo ani letní čas.
var MtCapacity = (function () {
    var DAY = 86400000;
    var PX = { day: 30, week: 12, month: 4 };

    function pad2(n) { return n < 10 ? "0" + n : "" + n; }
    function parseDay(s) { if (!s) return null; var a = s.split("-"); return Math.round(Date.UTC(+a[0], +a[1] - 1, +a[2]) / DAY); }
    function isoDay(n) { var d = new Date(n * DAY); return d.getUTCFullYear() + "-" + pad2(d.getUTCMonth() + 1) + "-" + pad2(d.getUTCDate()); }
    function human(n) { var d = new Date(n * DAY); return d.getUTCDate() + "." + (d.getUTCMonth() + 1) + "." + d.getUTCFullYear(); }
    function humanShort(n) { var d = new Date(n * DAY); return d.getUTCDate() + "." + (d.getUTCMonth() + 1) + "."; }
    function dow(n) { return (new Date(n * DAY).getUTCDay() + 6) % 7; }     // 0 = pondělí
    function monthStart(n) { var d = new Date(n * DAY); return Math.round(Date.UTC(d.getUTCFullYear(), d.getUTCMonth(), 1) / DAY); }
    function nextMonth(n) { var d = new Date(n * DAY); return Math.round(Date.UTC(d.getUTCFullYear(), d.getUTCMonth() + 1, 1) / DAY); }
    function esc(s) {
        if (s === null || s === undefined) return "";
        return String(s).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#39;");
    }
    function num(n) {       // hodiny: nejvýše 1 desetinné místo, bez zbytečné nuly
        var v = Math.round((n || 0) * 10) / 10;
        return ("" + v).replace(".", ",");
    }
    function num0(n) { return "" + Math.round(n || 0); }
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
        for (var k in data) { if (data[k] !== undefined && data[k] !== null) body.append(k, data[k]); }
        return fetch(url, { method: "POST", headers: { "Content-Type": "application/x-www-form-urlencoded", "X-Requested-With": "XMLHttpRequest" }, body: body.toString() })
            .then(function (r) { return r.json(); });
    }

    function init(opt) {
        var el = opt.el;
        var tra = opt.tra || {};
        var st = {
            people: [], rowById: {}, collapsed: {}, zoom: PX[opt.zoom] ? opt.zoom : "week", months: opt.months || 4,
            filter: "", today: null, start: null, days: 0, px: 12, loaded: false, drag: null, d1: null,
            view: opt.view === "projects" ? "projects" : "people", j11id: opt.j11id || 0, j18id: opt.j18id || 0,
            heat: opt.heat === "actual" ? "actual" : "plan"     // co ukazují buňky v minulosti: plán proti kapacitě, nebo vykázáno proti plánu
        };
        var tip = document.getElementById("rc_tip");
        var dlg = document.getElementById("rc_dlg");
        var storeKey = "r11-collapsed-" + (opt.p41id || 0);

        try {
            var raw = window.sessionStorage.getItem(storeKey);
            if (raw) { raw.split(",").forEach(function (k) { if (k) st.collapsed[k] = true; }); }
        } catch (e) { }
        function saveCollapsed() {
            try { window.sessionStorage.setItem(storeKey, Object.keys(st.collapsed).join(",")); } catch (e) { }
        }

        // ---------- data ----------
        function load(keepScroll) {
            var pos = keepScroll ? { left: el.scrollLeft, top: el.scrollTop } : null;
            var url = "/r11/Data?p41id=" + (opt.p41id || 0) + "&months=" + st.months + "&j11id=" + st.j11id + "&j18id=" + st.j18id + (st.d1 !== null ? "&d1=" + isoDay(st.d1) : "");
            return fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (!data || !data.ok) {
                        el.innerHTML = "<div class='rc-empty'>" + esc(data && data.message ? data.message : tra.chyba) + "</div>";
                        return;
                    }
                    st.people = data.people || [];
                    st.rowById = {};
                    st.people.forEach(function (p) {
                        (p.rows || []).forEach(function (r) {
                            r.person = p;
                            st.rowById[r.r11id] = r;
                            (r.blocks || []).forEach(function (b) { b.n1 = parseDay(b.d1); b.n2 = parseDay(b.d2); });
                        });
                    });
                    st.start = parseDay(data.start);
                    st.d1 = st.start;
                    st.days = data.days;
                    st.today = parseDay(data.today);
                    st.holidays = {};   // den -> název svátku
                    (data.holidays || []).forEach(function (x) { var hd = parseDay(x.d); if (hd !== null) st.holidays[hd] = x.name || ""; });
                    var first = !st.loaded;
                    st.loaded = true;
                    render();
                    if (pos) { el.scrollTop = pos.top; el.scrollLeft = pos.left; }
                    else if (first) { scrollToDay(st.today); }
                })
                .catch(function () {
                    el.innerHTML = "<div class='rc-empty'>" + esc(tra.chyba) + "</div>";
                });
        }

        function leftWidth() {
            var v = parseInt(window.getComputedStyle(el).getPropertyValue("--rc-left"), 10);
            return isNaN(v) ? 590 : v;
        }

        // ---------- sloupce vytížení: den / týden / měsíc ----------
        function buckets() {
            var out = [], s = st.start, e = st.start + st.days, d = s, to;
            while (d < e) {
                if (st.zoom === "day") to = d + 1;
                else if (st.zoom === "week") to = d + (7 - dow(d));
                else to = nextMonth(d);
                if (to > e) to = e;
                out.push({ s: d, e: to - 1 });
                d = to;
            }
            return out;
        }

        // Režim Skutečnost: období, které už začalo, ukazuje vykázáno / plán. Vrací null pro budoucí období (to zůstává v režimu plánu).
        function actCell(b, act, plan, name) {
            if (st.heat !== "actual" || st.today === null || b.s > st.today) return null;
            act = Math.round(act * 10) / 10; plan = Math.round(plan * 10) / 10;
            var w = (b.e - b.s + 1) * st.px;
            var cls = "rc-s-none";
            if (act > 0.049 || plan > 0.049) {
                if (act > plan * 1.1 + 0.049) cls = "rc-a-over";            // vykázáno víc než plán
                else if (act < plan * 0.8 - 0.049) cls = "rc-a-under";      // vykázáno výrazně méně než plán
                else cls = "rc-a-ok";
            }
            var txt = "";
            if (cls !== "rc-s-none") {
                if (w >= 64) txt = num0(act) + "/" + num0(plan);
                else if (w >= 24) txt = num0(act);
            }
            var title = (name ? name + "\n" : "") + (b.s === b.e ? human(b.s) : human(b.s) + " – " + human(b.e)) + "\n" + tra.vykazano + ": " + num(act) + " h\n" + tra.plan + ": " + num(plan) + " h\n" + tra.rozdil + ": " + (act - plan > 0 ? "+" : "") + num(act - plan) + " h";
            return "<div class='rc-sum " + cls + (b.e >= st.today ? " rc-now" : "") + "' style='left:" + ((b.s - st.start) * st.px) + "px;width:" + w + "px;' title='" + esc(title) + "'>" + txt + "</div>";
        }

        // mini = jen barevný proužek vytížení člověka (v pohledu po projektech), bez čísel
        function sumHtml(p, bks, mini) {
            var h = [];
            bks.forEach(function (b) {
                var plan = 0, cap = 0, abs = 0, i;
                for (i = b.s - st.start; i <= b.e - st.start; i++) { plan += p.plan[i] || 0; cap += p.cap[i] || 0; abs += (p.abs ? p.abs[i] : 0) || 0; }
                if (!mini) {
                    var a = 0;
                    if (p.act) for (i = b.s - st.start; i <= b.e - st.start; i++) a += p.act[i] || 0;
                    var cell = actCell(b, a, plan, null);
                    if (cell !== null) { h.push(cell); return; }
                }
                plan = Math.round(plan * 10) / 10; cap = Math.round(cap * 10) / 10; abs = Math.round(abs * 10) / 10;
                var cls = "rc-s-none";
                if (plan > 0.049) {
                    if (plan > cap + 0.049) cls = "rc-s-over";
                    else if (cap > 0 && plan >= cap * 0.8) cls = "rc-s-full";
                    else cls = "rc-s-free";
                }
                var w = (b.e - b.s + 1) * st.px;
                var txt = "";
                if (plan > 0.049 || st.zoom !== "day") {
                    if (w >= 64) txt = num0(plan) + "/" + num0(cap);
                    else if (w >= 24) txt = plan > 0.049 ? num0(plan) : "";
                }
                // období, kde je jen absence a nic naplánováno: vyšrafovaná buňka s písmenem A
                var absOnly = (abs > 0.049 && cls === "rc-s-none");
                var absCls = abs > 0.049 ? (absOnly ? " rc-abs rc-abs-full" : " rc-abs") : "";
                if (absOnly && !mini && w >= 20) txt = esc(tra.absencezkratka || "A");
                var title = (mini ? p.name + "\n" : "") + (b.s === b.e ? human(b.s) : human(b.s) + " – " + human(b.e)) + "\n" + tra.plan + ": " + num(plan) + " h\n" + tra.kapacita + ": " + num(cap) + " h\n" +
                    (plan > cap ? tra.pretizeni + ": " + num(plan - cap) + " h" : tra.volno + ": " + num(cap - plan) + " h") + (abs > 0.049 ? "\n" + tra.absence + ": " + num(abs) + " h" : "");
                if (mini) {
                    if (cls !== "rc-s-none" || abs > 0.049) h.push("<div class='rc-mini " + cls + absCls + "' style='left:" + ((b.s - st.start) * st.px) + "px;width:" + w + "px;' title='" + esc(title) + "'></div>");
                    return;
                }
                h.push("<div class='rc-sum " + cls + absCls + "' style='left:" + ((b.s - st.start) * st.px) + "px;width:" + w + "px;' title='" + esc(title) + "'>" + txt + "</div>");
            });
            return h.join("");
        }

        function clip(n1, n2) {     // úsek oříznutý na zobrazené období; null = celý mimo
            var s = Math.max(n1, st.start), e = Math.min(n2, st.start + st.days - 1);
            if (e < s) return null;
            return { left: (s - st.start) * st.px, width: (e - s + 1) * st.px };
        }

        function trackHtml(r, W, mini) {
            var h = [mini || ""], c;
            var pf = parseDay(r.pf), pu = parseDay(r.pu);
            if (pf !== null && pu !== null && pu >= pf) {
                c = clip(pf, pu);
                if (c) h.push("<div class='rc-pp' style='left:" + c.left + "px;width:" + c.width + "px;'></div>");
                if (r.spread && c) {
                    h.push("<div class='rc-bar rc-spread' style='left:" + c.left + "px;width:" + c.width + "px;'><span>" + num(r.total) + " h " + esc(tra.obdobi) + "</span></div>");
                }
            }
            (r.blocks || []).forEach(function (b) {
                c = clip(b.n1, b.n2);
                if (!c) return;
                var title = human(b.n1) + " – " + human(b.n2) + "\n" + num(b.hours) + " h" + (b.text ? "\n" + b.text : "");
                h.push("<div class='rc-bar" + (r.edit > 0 ? " rc-ed" : "") + "' data-b='" + b.id + "' style='left:" + c.left + "px;width:" + c.width + "px;' title='" + esc(title) + "'>" +
                    (r.edit > 0 ? "<b class='rc-hl'></b>" : "") + "<span>" + num(b.hours) + " h" + (b.text && c.width >= 120 ? " · " + esc(b.text) : "") + "</span>" + (r.edit > 0 ? "<b class='rc-hr'></b>" : "") + "</div>");
            });
            return "<div class='rc-track" + (r.edit > 0 ? " rc-draw" : "") + "' style='width:" + W + "px;'>" + h.join("") + "</div>";
        }

        function actHtml(r) {
            var act = (r.actfa || 0) + (r.actnefa || 0);
            var cls = "rc-act";
            if (r.total > 0 && act > r.total + 0.049) cls += " rc-bad";
            else if (r.total > 0 && act >= r.total * 0.8) cls += " rc-warn";
            var title = tra.vykazano + ": " + num(act) + " / " + num(r.total) + " h\nFa: " + num(r.actfa) + " / " + num(r.fa) + " h\nNefa: " + num(r.actnefa) + " / " + num(r.nefa) + " h";
            // rozdělení Fa × Nefa se nehlídá, překročení jedné složky se jen zvýrazní
            if (tra.otevritukony) title += "\n" + tra.otevritukony;
            return "<div class='" + cls + " rc-actlink' data-act='1' title='" + esc(title) + "'><span class='rc-fa" + (r.actfa > r.fa + 0.049 ? " rc-ovr" : "") + "'>" + num(r.actfa) + "</span> + <span class='rc-nefa" + (r.actnefa > r.nefa + 0.049 ? " rc-ovr" : "") + "'>" + num(r.actnefa) + "</span></div>";
        }

        // Zbývá = strop minus vykázáno: kolik hodin se na projekt ještě smí vykázat. Záporné číslo = strop je překročený.
        function restHtml(r) {
            var act = (r.actfa || 0) + (r.actnefa || 0);
            var rest = Math.round((r.total - act) * 10) / 10;
            var title = tra.zbyva2 + ": " + num(rest) + " h\n" + tra.strop + ": " + num(r.total) + " h\n" + tra.vykazano + ": " + num(act) + " h";
            return "<div class='rc-fc" + (rest < -0.049 ? " rc-bad" : "") + "' title='" + esc(title) + "'>" + num(rest) + "</div>";
        }

        // Skupiny k vykreslení podle pohledu: {key, name, sub, person|null, rows}. Hledání (od 2 znaků) filtruje skupiny i řádky.
        function groups() {
            var f = st.filter, out = [];
            function hit(txt) { return f.length < 2 || (txt || "").toLowerCase().indexOf(f) >= 0; }
            if (st.view === "projects") {
                var map = {}, list = [];
                st.people.forEach(function (p) {
                    (p.rows || []).forEach(function (r) {
                        var g = map[r.p41id];
                        if (!g) { g = map[r.p41id] = { key: "p" + r.p41id, name: r.project, sub: r.client, code: r.code, p41id: r.p41id, person: null, rows: [], ctx: r.ctx }; list.push(g); }
                        g.rows.push(r);
                    });
                });
                list.sort(function (x, y) { return (y.ctx ? 1 : 0) - (x.ctx ? 1 : 0) || (x.name || "").localeCompare(y.name || ""); });
                list.forEach(function (g) {
                    var own = hit((g.name || "") + " " + (g.code || "") + " " + (g.sub || ""));
                    var rows = own ? g.rows : g.rows.filter(function (r) { return hit(r.person.name); });
                    if (rows.length > 0) out.push({ key: g.key, name: g.name, sub: g.sub, p41id: g.p41id, person: null, rows: rows });
                });
                return out;
            }
            st.people.forEach(function (p) {
                var own = hit(p.name);
                var rows = own ? (p.rows || []) : (p.rows || []).filter(function (r) { return hit((r.project || "") + " " + (r.code || "") + " " + (r.client || "")); });
                if (own || rows.length > 0) out.push({ key: "" + p.j02id, name: p.name, sub: null, person: p, rows: rows });
            });
            return out;
        }

        // součet plánu projektu po sloupcích (pohled po projektech): bez kapacity, proto neutrální barva
        function projSumHtml(rows, bks) {
            var h = [];
            bks.forEach(function (b) {
                var plan = 0, act = 0, i;
                rows.forEach(function (r) {
                    for (i = b.s - st.start; i <= b.e - st.start; i++) { plan += (r.dayplan ? r.dayplan[i] : 0) || 0; act += (r.dayact ? r.dayact[i] : 0) || 0; }
                });
                var cell = actCell(b, act, plan, null);
                if (cell !== null) { h.push(cell); return; }
                plan = Math.round(plan * 10) / 10;
                if (plan <= 0.049) return;
                var w = (b.e - b.s + 1) * st.px;
                h.push("<div class='rc-sum rc-s-plan' style='left:" + ((b.s - st.start) * st.px) + "px;width:" + w + "px;' title='" + esc((b.s === b.e ? human(b.s) : human(b.s) + " – " + human(b.e)) + "\n" + tra.plan + ": " + num(plan) + " h") + "'>" + (w >= 24 ? num0(plan) : "") + "</div>");
            });
            return h.join("");
        }

        function render() {
            st.px = PX[st.zoom];
            markZoom();
            if (st.people.length === 0) {
                el.innerHTML = "<div class='rc-empty'>" + esc(tra.zadnadata) + "</div>";
                return;
            }
            var px = st.px, W = st.days * px, leftW = leftWidth(), end = st.start + st.days;
            var h = [], lines = [], d;

            h.push("<div class='rc-content' style='width:" + (leftW + W) + "px;'>");
            h.push("<div class='rc-head'><div class='rc-corner'><div class='rc-c-name'>" + esc(st.view === "projects" ? tra.projektosoba : tra.osoba) + "</div><div class='rc-c-cap'>" + esc(tra.strop) + "</div><div class='rc-c-pln'>" + esc(tra.rozpl) + "</div><div class='rc-c-act'>" + esc(tra.vykazano) + "</div><div class='rc-c-fc'>" + esc(tra.zbyva2) + "</div></div>");
            h.push("<div class='rc-axis' style='width:" + W + "px;'><div class='rc-ax1'>");
            var m = st.start;
            while (m < end) {
                var nm = Math.min(nextMonth(m), end), mw = (nm - m) * px;
                h.push("<span style='left:" + ((m - st.start) * px) + "px;width:" + mw + "px;'>" + (mw >= 34 ? esc(monthLabel(m, mw >= 110)) : "") + "</span>");
                if (m > st.start) lines.push("<i style='left:" + ((m - st.start) * px) + "px;'></i>");
                m = nm;
            }
            h.push("</div><div class='rc-ax2'>");
            if (st.zoom === "day") {
                for (d = st.start; d < end; d++) {
                    var hol = st.holidays && st.holidays.hasOwnProperty(d);
                    h.push("<span class='rc-day" + (dow(d) >= 5 ? " rc-we" : "") + (hol ? " rc-hol" : "") + "'" + (hol ? " title='" + esc(st.holidays[d]) + "'" : "") + " style='left:" + ((d - st.start) * px) + "px;width:" + px + "px;'>" + new Date(d * DAY).getUTCDate() + "</span>");
                }
            } else if (st.zoom === "week") {
                for (d = st.start + ((7 - dow(st.start)) % 7); d < end; d += 7) {
                    h.push("<span style='left:" + ((d - st.start) * px) + "px;width:" + (7 * px) + "px;'>" + humanShort(d) + "</span>");
                }
            }
            h.push("</div></div></div>");

            // pozadí: víkendy a linky dnů/týdnů (období začíná 1. dnem měsíce, proto posun podle dne v týdnu)
            var bg = "";
            if (st.zoom !== "month") {
                var unit = (st.zoom === "day") ? px : 7 * px, off = -dow(st.start) * px;
                bg = "background-image:repeating-linear-gradient(to right,#eef0f3 0,#eef0f3 1px,transparent 1px,transparent " + unit + "px)," +
                    "linear-gradient(to right,transparent 0,transparent " + (5 * px) + "px,#f6f7f9 " + (5 * px) + "px,#f6f7f9 " + (7 * px) + "px);" +
                    "background-size:" + unit + "px 100%," + (7 * px) + "px 100%;background-position:" + (st.zoom === "day" ? 0 : off) + "px 0," + off + "px 0;";
            }
            // svátky: podbarvený sloupec přes celou výšku (v měsíčním měřítku by byl jen čárka, tam se nekreslí)
            if (st.zoom !== "month" && st.holidays) {
                Object.keys(st.holidays).forEach(function (k) {
                    var hd = Number(k);
                    if (hd >= st.start && hd < end) lines.push("<i class='rc-holcol' style='left:" + ((hd - st.start) * px) + "px;width:" + px + "px;'></i>");
                });
            }
            if (st.today >= st.start && st.today < end) {
                lines.push("<i class='rc-today' style='left:" + ((st.today - st.start) * px + Math.floor(px / 2)) + "px;'></i>");
            }
            h.push("<div class='rc-bg' style='left:" + leftW + "px;top:var(--rc-head);width:" + W + "px;bottom:0;" + bg + "'>" + lines.join("") + "</div>");

            var bks = buckets();
            var vis = groups();
            var byProject = st.view === "projects";
            if (vis.length === 0) h.push("<div class='rc-empty'>" + esc(tra.zadnyvysledek) + "</div>");
            vis.forEach(function (it) {
                var p = it.person;
                var open = !st.collapsed[it.key] || st.filter.length >= 2;
                var total = 0, acts = 0;
                it.rows.forEach(function (r) { total += r.total || 0; acts += (r.actfa || 0) + (r.actnefa || 0); });
                h.push("<div class='rc-row rc-person' data-g='" + esc(it.key) + "'" + (byProject ? " data-p41='" + it.p41id + "'" : "") + "><div class='rc-cell'><div class='rc-name'>");
                h.push("<button type='button' class='rc-tgl' data-tgl='1'>" + (open ? "&#9662;" : "&#9656;") + "</button>");
                if (byProject) h.push("<a data-openg='1'>" + esc(it.name) + "</a>" + (it.sub ? "<span class='rc-sub'>" + esc(it.sub) + "</span>" : ""));
                else h.push("<span>" + esc(it.name) + "</span>");
                if (p && p.hidden > 0) h.push("<span class='rc-sub' title='" + esc(tra.skryte) + "'>+" + p.hidden + "</span>");
                h.push("</div><div class='rc-cap'>" + num(total) + "</div><div class='rc-pln'></div><div class='rc-act'>" + num(acts) + "</div><div class='rc-fc'>" + num(total - acts) + "</div></div>");
                h.push("<div class='rc-track' style='width:" + W + "px;'>" + (byProject ? projSumHtml(it.rows, bks) : sumHtml(p, bks, false)) + "</div></div>");
                if (!open) return;
                it.rows.forEach(function (r) {
                    var pln;
                    if (r.spread) pln = "<span class='rc-muted' title='" + esc(num(r.total) + " h " + tra.obdobi) + "'>" + esc(tra.obdobi.split(" ")[1] || "") + "</span>";
                    else if ((r.blocks || []).length === 0) pln = "<span class='rc-muted' title='" + esc(tra.nerozplanovano) + "'>–</span>";
                    else pln = num(r.planned) + " / " + num(r.total);
                    h.push("<div class='rc-row rc-proj" + (r.ctx && !byProject ? " rc-ctx" : "") + "' data-r='" + r.r11id + "'><div class='rc-cell'><div class='rc-name'>");
                    if (byProject) {
                        h.push("<span title='" + esc(r.person.name + (r.text ? "\n" + r.text : "")) + "'>" + esc(r.person.name) + "</span>");
                    } else {
                        h.push("<a data-open='1' title='" + esc(r.project + (r.code ? " (" + r.code + ")" : "") + (r.text ? "\n" + r.text : "")) + "'>" + esc(r.project) + "</a>");
                        if (r.client) h.push("<span class='rc-sub'>" + esc(r.client) + "</span>");
                    }
                    h.push("</div><div class='rc-cap'>");
                    if (r.edit > 0) h.push("<button type='button' data-cap='1' title='" + esc(tra.upravitstrop + ": " + num(r.fa) + " Fa + " + num(r.nefa) + " Nefa") + "'>" + num(r.total) + "</button>");
                    else h.push("<span title='" + esc(num(r.fa) + " Fa + " + num(r.nefa) + " Nefa") + "'>" + num(r.total) + "</span>");
                    h.push("</div><div class='rc-pln'>" + pln + "</div>" + actHtml(r) + restHtml(r) + "</div>");
                    h.push(trackHtml(r, W, byProject ? sumHtml(r.person, bks, true) : "") + "</div>");
                });
            });
            h.push("</div>");
            el.innerHTML = h.join("");
        }

        function markZoom() {
            var btns = document.querySelectorAll(".rc-zoom [data-zoom]");
            for (var i = 0; i < btns.length; i++) { btns[i].classList.toggle("active", btns[i].getAttribute("data-zoom") === st.zoom); }
        }
        function scrollToDay(n) {
            if (n === null || st.start === null) return;
            var view = Math.max(0, el.clientWidth - leftWidth());
            el.scrollLeft = Math.max(0, (n - st.start) * st.px - Math.floor(view / 3));
        }
        function keepScrollRender() {
            var left = el.scrollLeft, top = el.scrollTop;
            render();
            el.scrollTop = top; el.scrollLeft = left;
        }

        // ---------- dialogy ----------
        function hideDlg() { if (dlg) { dlg.style.display = "none"; dlg.innerHTML = ""; } st.dlg = null; }
        function placeDlg(x, y) {
            dlg.style.display = "block";
            var w = dlg.offsetWidth || 320, hh = dlg.offsetHeight || 200;
            var vw = window.innerWidth || 1200, vh = window.innerHeight || 800;
            dlg.style.left = Math.max(8, Math.min(x, vw - w - 8)) + "px";
            dlg.style.top = Math.max(8, Math.min(y, vh - hh - 8)) + "px";
        }
        function dlgRow(label, id, value, isnum) {
            return "<div class='rc-dlg-row'><label for='" + id + "'>" + esc(label) + "</label><div><input type='text' id='" + id + "' class='form-control" + (isnum ? " rc-num' inputmode='decimal" : "' maxlength='300") + "' autocomplete='off' value='" + esc(value) + "'/></div></div>";
        }
        function dlgFoot(candelete) {
            return "<div class='rc-dlg-foot'><button type='button' class='btn btn-primary' data-dlg='save'>" + esc(tra.ulozit) + "</button><button type='button' class='btn btn-outline-secondary' data-dlg='cancel'>" + esc(tra.zrusit) + "</button>" +
                (candelete ? "<button type='button' class='btn btn-outline-danger rc-del' data-dlg='delete'>" + esc(tra.smazat) + "</button>" : "") + "</div>";
        }
        function val(id) { var x = document.getElementById(id); return x ? x.value : ""; }

        // blok plánu: b = existující blok, nebo {n1,n2} nového
        function openBlockDlg(r, b, x, y) {
            var others = 0;
            (r.blocks || []).forEach(function (o) { if (!b.id || o.id !== b.id) others += o.hours || 0; });
            var rest = Math.max(0, Math.round((r.total - others) * 100) / 100);
            var hours = b.id ? b.hours : rest;
            dlg.innerHTML = "<div class='rc-dlg-head'>" + esc(tra.blok) + "</div><div class='rc-dlg-sub'>" + esc(r.person.name + " · " + r.project) + "<br>" + human(b.n1) + " – " + human(b.n2) + " · " + esc(tra.zbyva) + " " + num(rest) + " h</div>" +
                dlgRow(tra.hodiny, "rc_d_hours", ("" + hours).replace(".", ","), true) + dlgRow(tra.poznamka, "rc_d_text", b.text || "", false) + dlgFoot(!!b.id);
            st.dlg = { kind: "block", row: r, block: b };
            placeDlg(x, y);
            var inp = document.getElementById("rc_d_hours");
            if (inp) { inp.focus(); inp.select(); }
        }

        // strop se zakládá a upravuje ve standardním okně záznamu (r11/Record); po uložení okno zavolá hardrefresh stránky
        function openRecord(r11id, j02id) {
            hideDlg();
            if (typeof _window_open !== "function") return;
            _window_open("/r11/Record?pid=" + (r11id || 0) + (r11id ? "" : "&p41id=" + (opt.p41id || 0) + "&j02id=" + (j02id || 0)));
        }

        function done(data) {
            if (data && data.ok) { hideDlg(); return load(true); }
            notify(data && data.message ? data.message : tra.chyba, "error");
            return null;
        }
        function fail() { notify(tra.chyba, "error"); }

        function dlgAction(action) {
            var g = st.dlg;
            if (!g) return;
            if (action === "cancel") { hideDlg(); keepScrollRender(); return; }
            if (action === "delete") { post("/r11/DeleteBlock", { r12id: g.block.id }).then(done).catch(fail); return; }
            post("/r11/SaveBlock", { r12id: g.block.id || 0, r11id: g.row.r11id, d1: isoDay(g.block.n1), d2: isoDay(g.block.n2), hours: val("rc_d_hours"), text: val("rc_d_text") }).then(done).catch(fail);
        }

        if (dlg) {
            dlg.addEventListener("click", function (e) {
                var b = e.target.closest("[data-dlg]");
                if (b) dlgAction(b.getAttribute("data-dlg"));
            });
            dlg.addEventListener("keydown", function (e) {
                if (e.key === "Enter") { e.preventDefault(); dlgAction("save"); }
                if (e.key === "Escape") { e.preventDefault(); dlgAction("cancel"); }
            });
        }

        var cmdAdd = document.getElementById("rc_add");
        if (cmdAdd) cmdAdd.addEventListener("click", function () { openRecord(0, 0); });

        // ---------- tažení myší ----------
        function rowOf(node) {
            var div = node.closest(".rc-proj");
            return div ? st.rowById[div.getAttribute("data-r")] : null;
        }
        function blockOf(r, bar) {
            var id = parseInt(bar.getAttribute("data-b"), 10), out = null;
            (r.blocks || []).forEach(function (b) { if (b.id === id) out = b; });
            return out;
        }
        function showTip(e, n1, n2) {
            if (!tip) return;
            tip.textContent = (n1 === n2 ? human(n1) : human(n1) + " – " + human(n2) + "  (" + (n2 - n1 + 1) + " " + tra.dni + ")");
            tip.style.display = "block";
            tip.style.left = (e.clientX + 14) + "px";
            tip.style.top = (e.clientY + 16) + "px";
        }
        function hideTip() { if (tip) tip.style.display = "none"; }
        function setBar(g) {
            var s = Math.max(g.n1, st.start), e = Math.min(g.n2, st.start + st.days - 1);
            g.bar.style.left = ((s - st.start) * st.px) + "px";
            g.bar.style.width = (Math.max(1, e - s + 1) * st.px) + "px";
        }

        function onPointerDown(e) {
            if (e.button !== undefined && e.button !== 0) return;
            if (st.drag) return;
            var t = e.target, bar = t.closest(".rc-bar"), r, b, track;
            if (bar && bar.classList.contains("rc-ed")) {
                r = rowOf(bar);
                b = r ? blockOf(r, bar) : null;
                if (!b) return;
                st.drag = { row: r, block: b, bar: bar, mode: t.classList.contains("rc-hl") ? "l" : (t.classList.contains("rc-hr") ? "r" : "move"), x0: e.clientX + el.scrollLeft, n1o: b.n1, n2o: b.n2, n1: b.n1, n2: b.n2, moved: false };
            } else {
                if (bar) return;
                track = t.closest(".rc-track.rc-draw");
                if (!track) return;
                r = rowOf(track);
                if (!r) return;
                var day = st.start + Math.floor((e.clientX - track.getBoundingClientRect().left) / st.px);
                bar = document.createElement("div");
                bar.className = "rc-bar rc-ed rc-drag";
                track.appendChild(bar);
                st.drag = { row: r, bar: bar, track: track, mode: "new", x0: e.clientX, anchor: day, n1: day, n2: day, moved: false };
                setBar(st.drag);
            }
            hideDlg();
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
                day = Math.max(st.start, Math.min(st.start + st.days - 1, day));
                g.n1 = Math.min(g.anchor, day); g.n2 = Math.max(g.anchor, day);
            } else {
                var dd = Math.round((e.clientX + el.scrollLeft - g.x0) / st.px);
                if (dd !== 0) g.moved = true;
                if (g.mode === "move") { g.n1 = g.n1o + dd; g.n2 = g.n2o + dd; }
                if (g.mode === "l") { g.n1 = Math.min(g.n1o + dd, g.n2o); g.n2 = g.n2o; }
                if (g.mode === "r") { g.n2 = Math.max(g.n2o + dd, g.n1o); g.n1 = g.n1o; }
            }
            g.bar.classList.add("rc-drag");
            setBar(g);
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
        function onPointerCancel() { endDrag(); keepScrollRender(); }

        function onPointerUp(e) {
            var g = endDrag();
            if (!g) return;
            g.bar.classList.remove("rc-drag");
            if (g.mode === "new") {
                if (!g.moved) { g.bar.remove(); return; }
                var others = 0;
                (g.row.blocks || []).forEach(function (o) { others += o.hours || 0; });
                if (g.row.total - others <= 0.004) {
                    g.bar.remove();
                    notify(tra.vycerpano, "error");
                    return;
                }
                openBlockDlg(g.row, { n1: g.n1, n2: g.n2 }, e.clientX + 10, e.clientY + 10);
                return;
            }
            if (g.n1 === g.n1o && g.n2 === g.n2o) {
                setBar(g);
                if (!g.moved) openBlockDlg(g.row, g.block, e.clientX + 10, e.clientY + 10);     // prostý klik na blok = úprava hodin a poznámky
                return;
            }
            g.bar.classList.add("rc-saving");
            post("/r11/SaveBlock", { r12id: g.block.id, r11id: g.row.r11id, d1: isoDay(g.n1), d2: isoDay(g.n2) })
                .then(function (data) {
                    if (data && data.ok) return load(true);
                    notify(data && data.message ? data.message : tra.chyba, "error");
                    keepScrollRender();
                })
                .catch(function () { fail(); keepScrollRender(); });
        }

        // ---------- události ----------
        el.addEventListener("pointerdown", onPointerDown);
        el.addEventListener("click", function (e) {
            var t = e.target, r;
            if (t.closest("[data-tgl]")) {
                var div = t.closest(".rc-person");
                var id = div ? div.getAttribute("data-g") : null;
                if (!id) return;
                if (st.collapsed[id]) { delete st.collapsed[id]; } else { st.collapsed[id] = true; }
                saveCollapsed();
                keepScrollRender();
                return;
            }
            if (t.closest("[data-cap]")) {
                r = rowOf(t);
                if (r) openRecord(r.r11id, 0);
                return;
            }
            if (t.closest("[data-act]")) {
                // vykázané hodiny: tabulka časových úkonů osoby na projektu, bez omezení období
                r = rowOf(t);
                if (r && r.person && typeof zoom_open === "function") {
                    // stejné okno se seznamem úkonů jako v kalendáři
                    zoom_open(t.closest("[data-act]"), "/p31dayline/Zoom?j02id=" + r.person.j02id + "&p41id=" + r.p41id + "&d1=01.01.2000&d2=31.12.2100&tabquery=time");
                }
                return;
            }
            if (t.closest("[data-openg]")) {
                var grp = t.closest(".rc-person");
                var p41id = grp ? parseInt(grp.getAttribute("data-p41"), 10) : 0;
                if (p41id > 0 && typeof _edit === "function") _edit("p41", p41id);
                return;
            }
            if (t.closest("[data-open]")) {
                r = rowOf(t);
                if (r && typeof _edit === "function") _edit("p41", r.p41id);
            }
        });
        var zoomBtns = document.querySelectorAll(".rc-zoom [data-zoom]");
        for (var i = 0; i < zoomBtns.length; i++) {
            zoomBtns[i].addEventListener("click", function () {
                var z = this.getAttribute("data-zoom");
                if (!PX[z] || z === st.zoom) return;
                var view = Math.max(0, el.clientWidth - leftWidth());
                var c = st.start !== null ? st.start + (el.scrollLeft + view / 2) / st.px : null;
                st.zoom = z;
                post("/Common/SetUserParam", { key: "r11-zoom", value: z }).catch(function () { });
                hideDlg();
                render();
                if (c !== null) el.scrollLeft = Math.max(0, (c - st.start) * st.px - view / 2);
            });
        }
        function go(d1) { st.d1 = d1; hideDlg(); load(false).then(function () { el.scrollLeft = 0; }); }
        var x = document.getElementById("rc_prev");
        if (x) x.addEventListener("click", function () { if (st.d1 !== null) go(monthStart(st.d1 - 1)); });
        x = document.getElementById("rc_next");
        if (x) x.addEventListener("click", function () { if (st.d1 !== null) go(nextMonth(st.d1)); });
        x = document.getElementById("rc_today");
        if (x) x.addEventListener("click", function () {
            if (st.today === null) return;
            hideDlg();
            st.d1 = monthStart(st.today);
            load(false).then(function () { scrollToDay(st.today); });
        });
        x = document.getElementById("rc_months");
        if (x) x.addEventListener("change", function () {
            var n = parseInt(this.value, 10);
            if (!(n >= 1 && n <= 12)) return;
            st.months = n;
            post("/Common/SetUserParam", { key: "r11-months", value: "" + n }).catch(function () { });
            hideDlg();
            load(true);
        });
        x = document.getElementById("rc_xls");
        if (x) x.addEventListener("click", function () {     // export bere zobrazené období a měřítko; hledání se na něj nevztahuje
            if (st.d1 === null) return;
            window.location.href = "/r11/Export?p41id=" + (opt.p41id || 0) + "&d1=" + isoDay(st.d1) + "&months=" + st.months + "&zoom=" + st.zoom + "&j11id=" + st.j11id + "&j18id=" + st.j18id;
        });
        function markView() {
            var b = document.querySelectorAll(".rc-view [data-view]");
            for (var k = 0; k < b.length; k++) { b[k].classList.toggle("active", b[k].getAttribute("data-view") === st.view); }
        }
        var viewBtns = document.querySelectorAll(".rc-view [data-view]");
        for (i = 0; i < viewBtns.length; i++) {
            viewBtns[i].addEventListener("click", function () {
                var v = this.getAttribute("data-view") === "projects" ? "projects" : "people";
                if (v === st.view) return;
                st.view = v;
                post("/Common/SetUserParam", { key: "r11-view", value: v }).catch(function () { });
                hideDlg();
                markView();
                keepScrollRender();
            });
        }
        markView();
        function markHeat() {
            var b = document.querySelectorAll(".rc-heat [data-heat]");
            for (var k = 0; k < b.length; k++) { b[k].classList.toggle("active", b[k].getAttribute("data-heat") === st.heat); }
            var lg = document.querySelectorAll("[data-legend]");
            for (k = 0; k < lg.length; k++) { lg[k].style.display = lg[k].getAttribute("data-legend") === st.heat ? "" : "none"; }
        }
        var heatBtns = document.querySelectorAll(".rc-heat [data-heat]");
        for (i = 0; i < heatBtns.length; i++) {
            heatBtns[i].addEventListener("click", function () {
                var v = this.getAttribute("data-heat") === "actual" ? "actual" : "plan";
                if (v === st.heat) return;
                st.heat = v;
                post("/Common/SetUserParam", { key: "r11-heat", value: v }).catch(function () { });
                hideDlg();
                markHeat();
                keepScrollRender();
            });
        }
        markHeat();
        ["j11id", "j18id"].forEach(function (key) {      // filtr lidí podle týmu / střediska
            var cbx = document.getElementById("rc_" + key);
            if (!cbx) return;
            cbx.addEventListener("change", function () {
                st[key] = parseInt(this.value, 10) || 0;
                post("/Common/SetUserParam", { key: "r11-" + key, value: "" + st[key] }).catch(function () { });
                hideDlg();
                load(true);
            });
        });
        var txt = document.getElementById("rc_search"), timer = null;
        if (txt) txt.addEventListener("input", function () {
            var v = this.value;
            window.clearTimeout(timer);
            timer = window.setTimeout(function () { st.filter = (v || "").trim().toLowerCase(); keepScrollRender(); }, 200);
        });

        markZoom();
        load(false);

        return { reload: function () { return load(true); }, state: st };
    }

    return { init: init };
})();
