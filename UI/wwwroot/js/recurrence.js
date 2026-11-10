// Živý náhled opakování (p58 opakovaný úkol, p40 opakovaný úkon). Orientační výpočet – přesný plán vytváří databáze po uložení.
// Posun data jako BO.Code.Bas.get_posun_datum (měsíce se zachováním konce měsíce, pak dny), počet cyklů jako BO.Code.Recurrence.GetPocetCyklu.
//
// mtRecurrence.preview({
//   host: "p58Preview",                        id prvku pro výpis
//   type: "Rec.p58RecurrenceType",             name přepínače typu opakování
//   start: "Rec_p58BaseDateStart", end: "Rec_p58BaseDateEnd",   id skrytých polí mydate
//   base: "RD",                                popisek data cyklu
//   lines: [{ label, icon, d: "id pole dní", m: "id pole měsíců", on: "id checkboxu (volitelné)", gen: true }],
//   warn: () => "text varování" | null,        volitelné dodatečné varování
//   txt: { count, toomany, missing, more, past }
// })
(function () {
    "use strict";

    function date(id) {
        const el = document.getElementById(id) || document.getElementById(id + "helper");
        if (!el || !el.value) return null;
        const m = el.value.trim().match(/^(\d{1,2})\.(\d{1,2})\.(\d{4})/);
        return m ? new Date(+m[3], +m[2] - 1, +m[1]) : null;
    }
    function num(id) {
        if (!id) return 0;
        const el = document.getElementById("num" + id) || document.getElementById(id);
        if (!el) return 0;
        const n = parseInt(String(el.value).replace(/\s/g, "").replace(",", "."), 10);
        return isNaN(n) ? 0 : n;
    }
    function addMonths(d, months) {
        const r = new Date(d.getFullYear(), d.getMonth() + months, 1);
        r.setDate(Math.min(d.getDate(), new Date(r.getFullYear(), r.getMonth() + 1, 0).getDate()));
        return r;
    }
    function shift(d, days, months) {
        let r = new Date(d);
        if (months !== 0) {
            r = addMonthsEom(r, months);
        }
        r.setDate(r.getDate() + days);
        return r;
    }
    function isEom(d) { return d.getDate() === new Date(d.getFullYear(), d.getMonth() + 1, 0).getDate(); }
    function addMonthsEom(d, months) {     // poslední den měsíce zůstává posledním dnem (30.11. → 31.12. → 31.1. → 28.2.)
        const r = addMonths(d, months);
        return isEom(d) ? new Date(r.getFullYear(), r.getMonth() + 1, 0) : r;
    }
    function next(d, type, step) {
        switch (type) {
            case "Day": return new Date(d.getFullYear(), d.getMonth(), d.getDate() + step);
            case "Week": return new Date(d.getFullYear(), d.getMonth(), d.getDate() + 7 * step);
            case "Weeks2": return new Date(d.getFullYear(), d.getMonth(), d.getDate() + 14 * step);
            case "Quarter": return addMonthsEom(d, 3 * step);
            case "Year": return addMonthsEom(d, 12 * step);
            default: return addMonthsEom(d, step);
        }
    }
    function fmt(d) {
        return d.toLocaleDateString("cs-CZ", { weekday: "short", day: "numeric", month: "numeric", year: "numeric" });
    }
    function esc(s) {
        return String(s).replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
    }

    function render(cfg, state) {
        const host = document.getElementById(cfg.host);
        if (!host) return;
        const typeEl = document.querySelector("input[name='" + cfg.type + "']:checked");
        const type = typeEl ? typeEl.value : "Month";
        const d1 = date(cfg.start), d2 = date(cfg.end);
        const lines = cfg.lines.map(l => ({ l, on: !l.on || (document.getElementById(l.on) || {}).checked, d: num(l.d), m: num(l.m) }));
        const extra = cfg.warn ? cfg.warn() : null;
        const sig = JSON.stringify([type, d1 && d1.getTime(), d2 && d2.getTime(), lines.map(x => [x.on, x.d, x.m]), extra]);
        if (sig === state.last) return;
        state.last = sig;

        if (!d1) {
            host.innerHTML = "<div class='p58-warn'>" + esc(cfg.txt.missing) + "</div>";
            return;
        }
        let count = 0;
        if (d2) {
            let t = new Date(d1);
            while (t < d2 && count <= 201) { count++; t = next(d1, type, count); }
        }
        const today = new Date(); today.setHours(0, 0, 0, 0);
        const max = 4;
        let html = "<ol class='p58-list'>";
        for (let i = 0; i < max; i++) {
            const base = next(d1, type, i);
            if (d2 && base > d2) break;
            html += "<li><div class='p58-li-rd'><span>" + esc(cfg.base) + "</span><strong>" + esc(fmt(base)) + "</strong></div><div class='p58-li-meta'>";
            lines.forEach(x => {
                if (!x.on) return;
                const d = shift(base, x.d, x.m);
                const past = x.l.gen && d < today;
                html += "<span" + (past ? " class='p58-past' title='" + esc(cfg.txt.past) + "'" : "") + "><span class='material-icons-outlined-nosize'>" + esc(x.l.icon) + "</span>" + esc(x.l.label) + " " + esc(fmt(d)) + "</span>";
            });
            html += "</div></li>";
        }
        html += "</ol>";
        if (count > max) html += "<div class='p58-more'>" + esc(cfg.txt.more) + "</div>";
        if (d2) html += "<div class='p58-count'>" + esc(cfg.txt.count) + ": <strong>" + (count > 200 ? "200+" : count) + "</strong></div>";
        if (count > 200) html += "<div class='p58-warn'>" + esc(cfg.txt.toomany) + "</div>";
        if (extra) html += "<div class='p58-warn'>" + esc(extra) + "</div>";
        host.innerHTML = html;
    }

    window.mtRecurrence = {
        shift: shift,
        preview: function (cfg) {
            const state = { last: "" };
            const run = () => render(cfg, state);
            run();
            document.addEventListener("input", run);
            document.addEventListener("change", run);
            // datepicker a mynumber mění skrytá pole bez události → krátká kontrola změn (porovnává se jen podpis)
            setInterval(run, 600);
            return run;
        }
    };
})();
