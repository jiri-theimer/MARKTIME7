// Opakovaná vyúčtování (p75): rychlé nastavení a srozumitelný náhled plánu v kartě předpisu.
// Výpočet dat kopíruje proceduru p76_generate_recurrence a BL (období úkonů), aby náhled odpovídal uloženému plánu.
var MtP75 = (function () {
    var STEP = { Month: 1, Quarter: 3, Year: 12 };
    var MAXROWS = 200;

    function pad(n) { return (n < 10 ? "0" : "") + n; }

    function parseDate(s) {
        if (!s) return null;
        var m = /^\s*(\d{1,2})\.\s*(\d{1,2})\.\s*(\d{4})/.exec(String(s));
        if (!m) return null;
        var d = new Date(Number(m[3]), Number(m[2]) - 1, Number(m[1]));
        if (d.getMonth() !== Number(m[2]) - 1) return null;
        return d;
    }
    function fmt(d) { return pad(d.getDate()) + "." + pad(d.getMonth() + 1) + "." + d.getFullYear(); }
    function addDays(d, n) { return new Date(d.getFullYear(), d.getMonth(), d.getDate() + n); }
    function endOfMonth(y, m) { return new Date(y, m + 1, 0); }   // m od nuly
    function isEndOfMonth(d) { return addDays(d, 1).getDate() === 1; }
    function addMonths(d, n) {
        // stejně jako DATEADD(month): den se zkrátí na poslední den cílového měsíce
        var last = endOfMonth(d.getFullYear(), d.getMonth() + n);
        var day = Math.min(d.getDate(), last.getDate());
        return new Date(last.getFullYear(), last.getMonth(), day);
    }
    function quarterOf(d) { return Math.floor(d.getMonth() / 3) + 1; }

    function parseMask(mask, d) {
        var s = String(mask || "").replace(/</g, "[").replace(/>/g, "]");
        s = s.replace(/\[MM\]/g, pad(d.getMonth() + 1)).replace(/\[DD\]/g, pad(d.getDate()))
            .replace(/\[YYYY\]/g, String(d.getFullYear())).replace(/\[QQ\]/g, pad(quarterOf(d)))
            .replace(/\[Q\]/g, String(quarterOf(d))).replace(/\[M\]/g, String(d.getMonth() + 1));
        return s.replace(/[\[\]]/g, "");
    }

    // období úkonů, které se do vyúčtování vezmou
    function periodOf(base, periodFlag) {
        var y = base.getFullYear();
        switch (Number(periodFlag)) {
            case 1: return { d1: new Date(y, base.getMonth(), 1), d2: endOfMonth(y, base.getMonth()) };
            case 2: var q = quarterOf(base); return { d1: new Date(y, 3 * q - 3, 1), d2: endOfMonth(y, 3 * q - 1) };
            case 3: return { d1: new Date(y, 0, 1), d2: new Date(y, 11, 31) };
            default: return null;
        }
    }

    // cfg: { type:'Month'|'Quarter'|'Year', start:Date, end:Date|null, genDays:int, supplyFlag:int, maturityDays:int, periodFlag:int, mask:string }
    function buildPlan(cfg) {
        var rows = [];
        var step = STEP[cfg.type];
        if (!step || !cfg.start) return rows;
        var end = cfg.end || new Date(cfg.start.getFullYear() + 1, cfg.start.getMonth(), cfg.start.getDate());
        var eom = isEndOfMonth(cfg.start);
        for (var i = 0; i < MAXROWS; i++) {
            var base = addMonths(cfg.start, i * step);
            if (eom) base = endOfMonth(base.getFullYear(), base.getMonth());
            if (base > end) break;
            var gen = addDays(base, Number(cfg.genDays) || 0);
            var supply = gen;
            switch (Number(cfg.supplyFlag)) {
                case 2: supply = base; break;
                case 3: supply = addDays(new Date(base.getFullYear(), base.getMonth(), 1), -1); break;
                case 4: supply = endOfMonth(base.getFullYear(), base.getMonth()); break;
            }
            rows.push({
                base: base, gen: gen, supply: supply,
                maturity: addDays(base, Number(cfg.maturityDays) || 0),
                period: periodOf(base, cfg.periodFlag),
                name: parseMask(cfg.mask, base)
            });
        }
        return rows;
    }

    // upozornění na nastavení, které obvykle není záměr
    function warnings(cfg, rows) {
        var w = [];
        if (rows.length === 0) return w;
        var r = rows[0];
        if (!r.period) {
            w.push("Období úkonů není omezené: do každého vyúčtování půjdou všechny dosud nevyúčtované úkony bez ohledu na datum.");
        } else if (r.gen <= r.period.d2) {
            w.push("Vyúčtování se vygeneruje " + fmt(r.gen) + ", tedy dřív, než období " + fmt(r.period.d1) + " – " + fmt(r.period.d2) + " skončí. Úkony vykázané později se do něj nedostanou.");
        }
        if (r.maturity < r.gen) {
            w.push("Splatnost " + fmt(r.maturity) + " je dřív než den vystavení " + fmt(r.gen) + ".");
        }
        if (rows.length >= MAXROWS) {
            w.push("Plán je omezen na " + MAXROWS + " vyúčtování.");
        }
        return w;
    }

    // rychlé nastavení: fakturace za právě probíhající období, vystavení první den po jeho konci
    function preset(kind, today) {
        var y = today.getFullYear(), m = today.getMonth();
        var start, periodFlag, mask;
        if (kind === "Quarter") {
            var q = quarterOf(today);
            start = endOfMonth(y, 3 * q - 1); periodFlag = 2; mask = "Čtvrtletní vyúčtování [YYYY]/Q[Q]";
        } else if (kind === "Year") {
            start = new Date(y, 11, 31); periodFlag = 3; mask = "Roční vyúčtování [YYYY]";
        } else {
            kind = "Month"; start = endOfMonth(y, m); periodFlag = 1; mask = "Měsíční vyúčtování [YYYY]/[MM]";
        }
        return { type: kind, start: start, end: null, genDays: 1, supplyFlag: 2, maturityDays: 15, periodFlag: periodFlag, mask: mask };
    }

    // ---------- napojení na formulář ----------
    var DEFAULT_MASKS = ["Měsíční vyúčtování [YYYY]/[MM]", "Čtvrtletní vyúčtování [YYYY]/Q[Q]", "Roční vyúčtování [YYYY]"];

    function val(id) { var e = document.getElementById(id); return e ? e.value : ""; }

    function readForm() {
        var type = "";
        ["Month", "Quarter", "Year"].forEach(function (t) {
            var e = document.getElementById("chk" + t);
            if (e && e.checked) type = t;
        });
        return {
            type: type,
            start: parseDate(val("Rec_p75BaseDateStarthelper") || val("Rec_p75BaseDateStart")),
            end: parseDate(val("Rec_p75BaseDateEndhelper") || val("Rec_p75BaseDateEnd")),
            genDays: Number(String(val("Rec_p75Generate_DaysToBase_D")).replace(",", ".")) || 0,
            supplyFlag: Number(val("Rec_p75DateSupplyFlag")) || 1,
            maturityDays: Number(String(val("Rec_p75DateMaturityDaysAfter")).replace(",", ".")) || 0,
            periodFlag: Number(val("Rec_p75PeriodFlag")) || 0,
            mask: val("Rec_p75InvoiceText")
        };
    }

    function setDate(id, d) {
        var s = d ? fmt(d) : "";
        var h = document.getElementById(id + "helper"); if (h) h.value = s;
        var e = document.getElementById(id); if (e) e.value = s;
    }
    function setNumber(id, n) {
        if (typeof mynumber_changevalue === "function" && document.getElementById("num" + id)) {
            mynumber_changevalue(id, n, 0);
        } else {
            var e = document.getElementById(id); if (e) e.value = n;
        }
    }

    function applyPreset(kind) {
        var p = preset(kind, new Date());
        var chk = document.getElementById("chk" + p.type); if (chk) chk.checked = true;
        setDate("Rec_p75BaseDateStart", p.start);
        setDate("Rec_p75BaseDateEnd", null);
        setNumber("Rec_p75Generate_DaysToBase_D", p.genDays);
        setNumber("Rec_p75DateMaturityDaysAfter", p.maturityDays);
        var e = document.getElementById("Rec_p75DateSupplyFlag"); if (e) e.value = String(p.supplyFlag);
        e = document.getElementById("Rec_p75PeriodFlag"); if (e) e.value = String(p.periodFlag);
        e = document.getElementById("Rec_p75InvoiceText");
        if (e && (e.value.trim() === "" || DEFAULT_MASKS.indexOf(e.value.trim()) >= 0)) e.value = p.mask;
        render();
    }

    function esc(s) { return String(s).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;"); }

    var _lastKey = null;
    function render() {
        var box = document.getElementById("p75PlanPreview");
        if (!box) return;
        var cfg = readForm();
        var key = JSON.stringify([cfg.type, cfg.start && cfg.start.getTime(), cfg.end && cfg.end.getTime(), cfg.genDays, cfg.supplyFlag, cfg.maturityDays, cfg.periodFlag, cfg.mask]);
        if (key === _lastKey) return;
        _lastKey = key;

        var rows = buildPlan(cfg);
        if (rows.length === 0) {
            box.innerHTML = "<span class='text-muted'>Vyplňte opakování a první rozhodné datum, zobrazí se náhled plánu.</span>";
            return;
        }
        var h = [];
        h.push("<table class='table table-sm' style='margin-bottom:6px;'><tr><th>Vystaví se</th><th>Za úkony z období</th><th>Datum plnění</th><th>Splatnost</th><th>Text vyúčtování</th></tr>");
        rows.slice(0, 3).forEach(function (r) {
            h.push("<tr><td>" + fmt(r.gen) + "</td><td>" + (r.period ? fmt(r.period.d1) + " – " + fmt(r.period.d2) : "bez omezení") + "</td><td>" + fmt(r.supply) + "</td><td>" + fmt(r.maturity) + "</td><td>" + esc(r.name) + "</td></tr>");
        });
        h.push("</table>");
        var last = rows[rows.length - 1];
        h.push("<div>Celkem <strong>" + rows.length + "</strong> vyúčtování, poslední se vystaví " + fmt(last.gen) + ".</div>");
        warnings(cfg, rows).forEach(function (w) {
            h.push("<div style='color:#b02a37;margin-top:4px;'>" + esc(w) + "</div>");
        });
        box.innerHTML = h.join("");
    }

    function init() {
        if (!document.getElementById("p75PlanPreview")) return;
        render();
        // kalendář mění hodnotu bez události, proto se náhled obnovuje i pravidelně
        ["change", "input", "click", "focusout"].forEach(function (ev) { document.addEventListener(ev, function () { setTimeout(render, 50); }); });
        setInterval(render, 800);
    }

    return { parseDate: parseDate, fmt: fmt, addMonths: addMonths, buildPlan: buildPlan, periodOf: periodOf, warnings: warnings, preset: preset, parseMask: parseMask, applyPreset: applyPreset, render: render, init: init };
})();

if (typeof module !== "undefined") { module.exports = MtP75; }
