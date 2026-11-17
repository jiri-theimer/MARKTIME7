// Rozpočet projektu: mřížka aktivita × měsíc nad položkami finančního plánu (p49). Bez projektu přehled rozpočtů.
// Čisté výpočty (compute*, parseAmount, cellClass) jsou oddělené od vykreslení, aby šly testovat.
var MtBudget = (function () {
    var WARN = 0.8;

    function num(n) {
        if (n === null || n === undefined || isNaN(n)) return "";
        var s = (Math.round(n * 100) / 100).toFixed(2);
        if (s.slice(-3) === ".00") s = s.slice(0, -3);
        var p = s.split(".");
        p[0] = p[0].replace(/\B(?=(\d{3})+(?!\d))/g, " ");
        return p.join(",");
    }
    function parseAmount(s) {
        s = String(s === null || s === undefined ? "" : s).replace(/[\s ]/g, "").replace(",", ".");
        if (s === "") return 0;
        if (!/^\d+(\.\d+)?$/.test(s)) return NaN;
        return Number(s);
    }
    function esc(s) { return String(s === null || s === undefined ? "" : s).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;"); }
    function sum(a) { var t = 0; for (var i = 0; i < a.length; i++) t += a[i] || 0; return t; }

    // barva buňky: výdaje hlídají překročení, příjmy ukazují splnění
    function cellClass(side, plan, act) {
        plan = plan || 0; act = act || 0;
        if (side === 2) return (plan > 0 && act >= plan - 0.005) ? "bg-done" : "";
        if (plan <= 0) return act > 0.005 ? "bg-noplan" : "";
        if (act > plan + 0.005) return "bg-over";
        if (act >= plan * WARN) return "bg-warn";
        return "";
    }

    // součty po stranách a měnách: { "CZK": { exp:{plan[12],act[12],planTotal,actTotal}, rev:{...} } }
    function computeTotals(rows) {
        var out = {};
        rows.forEach(function (r) {
            var cur = r.currency || "";
            if (!out[cur]) {
                out[cur] = {};
                ["exp", "rev"].forEach(function (k) { out[cur][k] = { plan: new Array(12).fill(0), act: new Array(12).fill(0), planTotal: 0, actTotal: 0 }; });
            }
            var t = out[cur][r.side === 2 ? "rev" : "exp"];
            for (var m = 0; m < 12; m++) { t.plan[m] += r.plan[m] || 0; t.act[m] += r.act[m] || 0; }
            t.planTotal += r.planTotal || 0; t.actTotal += r.actTotal || 0;
        });
        return out;
    }

    function init(opt) {
        var T = opt.texts || {};
        var root = document.getElementById(opt.root);
        var state = { year: opt.year, data: null, extra: [] };   // extra: řádky přidané uživatelem, zatím bez částky
        var api = {};

        function post(url, data, done) {
            $.post(url, data).done(function (d) {
                if (d && d.ok) { done(d); } else { fail(d && d.message); }
            }).fail(function () { fail(); });
        }
        function fail(msg) {
            msg = msg || T.chyba;
            if (typeof _notify_message === "function") { _notify_message(msg, "error"); } else { alert(msg); }
        }

        function load() {
            $.getJSON("/p49/BudgetData", { p41id: opt.p41id, year: state.year }).done(function (d) {
                if (!d || !d.ok) { fail(d && d.message); return; }
                state.data = d;
                if (d.mode === "overview") { renderOverview(); } else { renderProject(); }
            }).fail(function () { fail(); });
        }
        api.reload = load;

        // ---------- přehled rozpočtů ----------
        function renderOverview() {
            var rows = state.data.rows || [];
            var q = (document.getElementById("bgSearch") || {}).value || "";
            q = q.toLowerCase();
            if (q) rows = rows.filter(function (r) { return ((r.project || "") + " " + (r.code || "") + " " + (r.client || "")).toLowerCase().indexOf(q) >= 0; });
            if (rows.length === 0) { root.innerHTML = "<div class='bg-empty'>" + esc(T.prazdnoPrehled) + "</div>"; return; }
            var h = ["<table class='bg-grid'><thead><tr><th class='bg-name'>" + esc(T.projekt) + "</th><th class='bg-name' style='position:static;'>" + esc(T.klient) + "</th><th></th>",
                "<th>" + esc(T.vydajeRozpocet) + "</th><th>" + esc(T.vydajeSkut) + "</th><th>" + esc(T.vydajeZbyva) + "</th><th>%</th><th>" + esc(T.prijmyRozpocet) + "</th><th>" + esc(T.prijmySkut) + "</th></tr></thead><tbody>"];
            rows.forEach(function (r) {
                var rest = r.planExp - r.actExp;
                var pct = r.planExp > 0 ? Math.round(100 * r.actExp / r.planExp) + " %" : "";
                h.push("<tr class='bg-prow' data-p41id='" + r.p41id + "'><td class='bg-name'>" + esc(r.project) + " <span class='bg-muted'>" + esc(r.code) + "</span></td><td>" + esc(r.client) + "</td><td>" + esc(r.currency) + "</td>"
                    + "<td class='bg-num'>" + num(r.planExp) + "</td><td class='bg-num " + cellClass(1, r.planExp, r.actExp) + "'>" + num(r.actExp) + "</td>"
                    + "<td class='bg-num" + (rest < 0 ? " bg-neg" : "") + "'>" + (r.planExp > 0 ? num(rest) : "") + "</td><td class='bg-num'>" + pct + "</td>"
                    + "<td class='bg-num'>" + num(r.planRev) + "</td><td class='bg-num " + cellClass(2, r.planRev, r.actRev) + "'>" + num(r.actRev) + "</td></tr>");
            });
            h.push("</tbody></table>");
            root.innerHTML = h.join("");
        }

        // ---------- rozpočet projektu ----------
        function allRows() {
            var d = state.data, rows = (d.rows || []).slice();
            state.extra = state.extra.filter(function (e) { return !rows.some(function (r) { return r.key === e.key; }); });
            return rows.concat(state.extra);
        }

        // shrnutí celého projektu: výdaje, příjmy a rozdíl (po měnách)
        function summaryHtml(totals) {
            var h = [];
            Object.keys(totals).sort().forEach(function (cur) {
                var t = totals[cur];
                var planDiff = t.rev.planTotal - t.exp.planTotal, actDiff = t.rev.actTotal - t.exp.actTotal;
                h.push("<div class='bg-sumrow'>"
                    + box(T.vydaje, T.rozpocet, t.exp.planTotal, t.exp.actTotal, cur, cellClass(1, t.exp.planTotal, t.exp.actTotal))
                    + box(T.prijmy, T.odhad, t.rev.planTotal, t.rev.actTotal, cur, "")
                    + box(T.rozdil, T.planVelke, planDiff, actDiff, cur, "bg-box-diff")
                    + "</div>");
            });
            return h.join("");
        }
        function box(title, planLabel, plan, act, cur, css) {
            return "<div class='bg-box " + css + "'><div class='bg-box-title'>" + esc(title) + "</div>"
                + "<div class='bg-box-main'>" + signed(plan) + " <span class='bg-muted'>" + esc(cur) + "</span></div>"
                + "<div class='bg-box-sub'>" + esc(planLabel) + " · " + esc(T.skutecnost) + " " + signed(act) + "</div></div>";
        }

        function renderProject() {
            var d = state.data, rows = allRows();
            var now = new Date(), curM = (now.getFullYear() === d.year) ? now.getMonth() : -1;
            fillToolbar();
            var totals = computeTotals(rows);
            if (Object.keys(totals).length === 0) {
                totals[d.currency || ""] = computeTotals([{ currency: d.currency || "", side: 1, plan: [], act: [] }])[d.currency || ""];
            }
            var cols = 16;

            var h = ["<div class='bg-summary'>" + summaryHtml(totals) + "</div>"];
            h.push("<table class='bg-grid" + (d.canEdit ? " bg-edit" : "") + "'><thead><tr><th class='bg-name' rowspan='2'>" + esc(T.aktivita) + "</th>");
            h.push("<th colspan='12' class='bg-grp'>" + esc(T.rozpocet) + " " + esc(T.vRoce) + " " + d.year + "</th><th colspan='3' class='bg-grp bg-sum'>" + esc(T.celyProjekt) + "</th></tr><tr>");
            for (var m = 0; m < 12; m++) h.push("<th class='" + (m === curM ? "bg-now" : "") + "'>" + esc(T.mesice[m]) + "</th>");
            h.push("<th class='bg-sum'>" + esc(T.rozpocet) + "</th><th class='bg-sum'>" + esc(T.skutecnostVelke) + "</th><th class='bg-sum'>" + esc(T.zbyvaVelke) + "</th></tr></thead><tbody>");

            [1, 2].forEach(function (side) {
                h.push("<tr class='bg-side'><td class='bg-name' colspan='" + cols + "'>" + esc(side === 2 ? T.prijmy : T.vydaje) + "</td></tr>");
                var lastSection = null;
                rows.filter(function (r) { return (r.side === 2 ? 2 : 1) === side; }).forEach(function (r) {
                    if (r.p34name !== lastSection) {
                        h.push("<tr class='bg-section'><td class='bg-name' colspan='" + cols + "'>" + esc(r.p34name) + "</td></tr>");
                        lastSection = r.p34name;
                    }
                    var multi = (d.currency && r.currency && r.currency !== d.currency);
                    h.push("<tr data-key='" + esc(r.key) + "'><td class='bg-name' title='" + esc(r.name) + "'>" + esc(r.name) + (multi ? " <span class='bg-muted'>" + esc(r.currency) + "</span>" : "") + "</td>");
                    for (var m2 = 0; m2 < 12; m2++) {
                        var items = (r.items && r.items[m2]) || [], pl = r.plan[m2] || 0, ac = r.act[m2] || 0;
                        var tip = esc(T.rozpocet) + ": " + num(pl) + ", " + esc(T.skutecnost) + ": " + num(ac);
                        h.push("<td class='bg-cell " + cellClass(r.side, pl, ac) + (m2 === curM ? " bg-now" : "") + "' data-m='" + m2 + "' title='" + tip + "'>"
                            + "<span class='bg-plan" + (pl ? "" : " bg-zero") + "'>"
                            + (items.length > 0 ? "<span class='bg-items" + (items.length > 1 ? " bg-multi" : "") + "' title='" + esc(T.polozky) + "'>" + (items.length > 1 ? items.length + "×" : "⋯") + "</span>" : "")
                            + "<span class='bg-val'>" + (pl ? num(pl) : "") + "</span></span>"
                            + (!pl && ac ? "<span class='bg-act'>" + num(ac) + " " + esc(T.bezRozpoctu) + "</span>" : "")
                            + "</td>");
                    }
                    var rest = (r.planTotal || 0) - (r.actTotal || 0);
                    h.push("<td class='bg-num bg-sum'>" + num(r.planTotal || 0) + "</td>");
                    h.push("<td class='bg-num bg-sum " + cellClass(r.side, r.planTotal, r.actTotal) + "'>" + num(r.actTotal || 0) + "</td>");
                    h.push("<td class='bg-num bg-sum" + (rest < 0 ? " bg-neg" : "") + "'>" + ((r.planTotal || 0) > 0 ? num(rest) : "") + "</td></tr>");
                });
                if (d.canEdit) {
                    var acts = (d.activities || []).filter(function (a) { return (a.side === 2 ? 2 : 1) === side; });
                    var optHtml = ["<option value=''>" + esc(side === 2 ? T.pridatPrijem : T.pridatVydaj) + "</option>"], grp = null;
                    acts.forEach(function (a) {
                        if (a.p34name !== grp) { if (grp !== null) optHtml.push("</optgroup>"); optHtml.push("<optgroup label='" + esc(a.p34name) + "'>"); grp = a.p34name; }
                        optHtml.push("<option value='" + a.id + "'>" + esc(a.name) + "</option>");
                    });
                    if (grp !== null) optHtml.push("</optgroup>");
                    h.push("<tr class='bg-addrow'><td class='bg-name' colspan='" + cols + "'>"
                        + (acts.length > 0 ? "<select class='form-select bg-addsel' data-side='" + side + "'>" + optHtml.join("") + "</select>" : "<span class='bg-muted'>" + esc(T.zadneAktivity) + "</span>")
                        + "</td></tr>");
                }
                Object.keys(totals).sort().forEach(function (cur) {
                    var s = totals[cur][side === 2 ? "rev" : "exp"];
                    if (Object.keys(totals).length > 1 && s.planTotal === 0 && s.actTotal === 0) return;
                    h.push(totalRow("bg-total", (side === 2 ? T.prijmyCelkem : T.vydajeCelkem) + (Object.keys(totals).length > 1 ? " " + cur : ""), s, side));
                });
            });

            Object.keys(totals).sort().forEach(function (cur) {
                var t = totals[cur];
                var diff = { plan: [], act: [], planTotal: t.rev.planTotal - t.exp.planTotal, actTotal: t.rev.actTotal - t.exp.actTotal };
                for (var m3 = 0; m3 < 12; m3++) { diff.plan.push(t.rev.plan[m3] - t.exp.plan[m3]); diff.act.push(t.rev.act[m3] - t.exp.act[m3]); }
                h.push(totalRow("bg-profit", T.rozdil + (Object.keys(totals).length > 1 ? " " + cur : ""), diff, 0));
            });
            h.push("</tbody></table>");
            if (d.canEdit) {
                h.push("<div class='bg-foot'>" + esc(T.pokrocile) + ": <a href='/TheGrid/FlatView?prefix=p49'>" + esc(T.tabulka) + "</a> · <a href='javascript:void(0)' data-newdetail='1'>" + esc(T.sDetailem) + "</a></div>");
            }
            root.innerHTML = h.join("");
        }

        function signed(n) { return "<span" + (n < -0.005 ? " class='bg-neg'" : "") + ">" + num(n) + "</span>"; }
        function totalRow(css, label, s, side) {
            var h = "<tr class='" + css + "'><td class='bg-name'>" + esc(label) + "</td>";
            for (var m = 0; m < 12; m++) {
                h += "<td class='bg-num'>" + (s.plan[m] ? signed(s.plan[m]) : "") + "</td>";
            }
            h += "<td class='bg-num bg-sum'>" + signed(s.planTotal) + "</td>";
            h += "<td class='bg-num bg-sum " + (side ? cellClass(side, s.planTotal, s.actTotal) : "") + "'>" + signed(s.actTotal) + "</td>";
            h += "<td class='bg-num bg-sum'>" + (side === 1 && s.planTotal > 0 ? signed(s.planTotal - s.actTotal) : "") + "</td></tr>";
            return h;
        }

        function fillToolbar() {
            var d = state.data;
            var sel = document.getElementById("bgYear");
            if (sel) {
                sel.innerHTML = (d.years || [d.year]).map(function (y) { return "<option value='" + y + "'" + (y === d.year ? " selected" : "") + ">" + y + "</option>"; }).join("");
            }
        }

        function addActivity(id) {
            var d = state.data, a = (d.activities || []).filter(function (x) { return x.id === id; })[0];
            if (!a) return;
            var key = id + "-" + d.j27id;
            if (!findRow(key)) {
                state.extra.push({ key: key, p32id: id, name: a.name, p34name: a.p34name, side: a.side, j27id: d.j27id, currency: d.currency, plan: new Array(12).fill(0), act: new Array(12).fill(0), items: [], planTotal: 0, actTotal: 0 });
                renderProject();
            }
            var tr = root.querySelector("tr[data-key='" + key + "']");
            if (tr) {
                if (tr.scrollIntoView) tr.scrollIntoView({ block: "center" });
                var now = new Date(); var m = (now.getFullYear() === d.year) ? now.getMonth() : 0;
                var td = tr.querySelector("td.bg-cell[data-m='" + m + "']"); if (td) editCell(td);
            }
        }

        function findRow(key) { return allRows().filter(function (r) { return r.key === key; })[0]; }

        // ---------- úprava buňky ----------
        function editCell(td) {
            var d = state.data;
            if (!d.canEdit || td.querySelector("input")) return;
            var r = findRow(td.parentNode.getAttribute("data-key")); if (!r) return;
            var m = Number(td.getAttribute("data-m"));
            var items = (r.items && r.items[m]) || [];
            if (items.length > 1) { showItems(td, r, m); return; }
            var span = td.querySelector(".bg-val");
            var inp = document.createElement("input");
            inp.className = "bg-input"; inp.value = r.plan[m] ? String(r.plan[m]).replace(".", ",") : "";
            span.style.display = "none"; span.parentNode.appendChild(inp); inp.focus(); inp.select();
            var finished = false;
            function finish(save) {
                if (finished) return; finished = true;
                var v = parseAmount(inp.value);
                if (inp.parentNode) inp.parentNode.removeChild(inp);
                span.style.display = "";
                if (!save) return;
                if (isNaN(v)) { fail(); return; }
                if (Math.abs(v - (r.plan[m] || 0)) < 0.005) return;
                post("/p49/BudgetSaveCell", { p41id: opt.p41id, p32id: r.p32id, j27id: r.j27id, year: d.year, month: m + 1, amount: String(v) }, function () { load(); });
            }
            inp.addEventListener("keydown", function (e) {
                if (e.key === "Enter") { e.preventDefault(); finish(true); }
                if (e.key === "Escape") { e.preventDefault(); finish(false); }
            });
            inp.addEventListener("blur", function () { finish(true); });
        }

        // ---------- položky buňky ----------
        function closePop() { var p = document.getElementById("bgPop"); if (p) p.parentNode.removeChild(p); }
        function showItems(td, r, m) {
            closePop();
            var d = state.data, items = (r.items && r.items[m]) || [];
            var h = ["<h6>" + esc(r.name) + " – " + esc(T.mesice[m]) + " " + d.year + "</h6><table>"];
            items.forEach(function (it) {
                h.push("<tr><td>" + esc(it.date) + "</td><td style='text-align:right;'>" + num(it.amount) + "</td><td>" + esc(it.text) + (it.done ? " <span class='bg-muted'>(" + esc(T.realizovano) + ")</span>" : "") + "</td>"
                    + "<td style='white-space:nowrap;'>" + (d.canEdit ? "<a href='javascript:void(0)' data-edit='" + it.id + "'>" + esc(T.detail) + "</a>" : "")
                    + (d.canEdit && !it.done ? " · <a href='javascript:void(0)' data-real='" + it.id + "'>" + esc(T.realizovat) + "</a>" : "") + "</td></tr>");
            });
            h.push("</table><div class='bg-pop-actions'>"
                + (d.canEdit ? "<button type='button' class='btn btn-light' data-new='1'>" + esc(T.nova) + "</button>" : "")
                + "<button type='button' class='btn btn-light' data-close='1'>" + esc(T.zavrit) + "</button></div>");
            var pop = document.createElement("div"); pop.id = "bgPop"; pop.className = "bg-pop"; pop.innerHTML = h.join("");
            document.body.appendChild(pop);
            var rc = td.getBoundingClientRect();
            pop.style.left = Math.max(8, Math.min(rc.left, window.innerWidth - pop.offsetWidth - 12)) + "px";
            pop.style.top = Math.max(8, Math.min(rc.bottom + 4, window.innerHeight - pop.offsetHeight - 12)) + "px";
            pop.addEventListener("click", function (e) {
                var a = e.target;
                if (a.getAttribute("data-close")) { closePop(); }
                else if (a.getAttribute("data-edit")) { closePop(); _window_open("/p49/Record?pid=" + a.getAttribute("data-edit"), 3); }
                else if (a.getAttribute("data-real")) { closePop(); _window_open("/p31/Record?pid=0&p49id=" + a.getAttribute("data-real"), 3); }
                else if (a.getAttribute("data-new")) { closePop(); _window_open("/p49/Record?pid=0&p41id=" + opt.p41id, 3); }
            });
        }

        // ---------- události ----------
        root.addEventListener("click", function (e) {
            var t = e.target;
            var prow = t.closest ? t.closest("tr.bg-prow") : null;
            if (prow) { window.location.href = "/p49/Budget?p41id=" + prow.getAttribute("data-p41id"); return; }
            if (t.getAttribute && t.getAttribute("data-newdetail")) { _window_open("/p49/Record?pid=0&p41id=" + opt.p41id, 3); return; }
            var td = t.closest ? t.closest("td.bg-cell") : null;
            if (!td || !td.parentNode.getAttribute("data-key")) return;
            if (t.classList && t.classList.contains("bg-items")) {
                showItems(td, findRow(td.parentNode.getAttribute("data-key")), Number(td.getAttribute("data-m"))); return;
            }
            if (t.tagName !== "INPUT") editCell(td);
        });
        root.addEventListener("change", function (e) {
            var s = e.target;
            if (!s.classList || !s.classList.contains("bg-addsel")) return;
            var id = Number(s.value); s.value = "";
            if (id && state.data) addActivity(id);
        });
        document.addEventListener("keydown", function (e) { if (e.key === "Escape") closePop(); });

        var el = document.getElementById("bgYear");
        if (el) el.addEventListener("change", function () { state.year = Number(el.value); load(); });
        var prev = document.getElementById("bgPrev"); if (prev) prev.addEventListener("click", function () { state.year--; load(); });
        var next = document.getElementById("bgNext"); if (next) next.addEventListener("click", function () { state.year++; load(); });
        var search = document.getElementById("bgSearch"); if (search) search.addEventListener("input", function () { if (state.data) renderOverview(); });
        load();
        return api;
    }

    return { init: init, num: num, parseAmount: parseAmount, cellClass: cellClass, computeTotals: computeTotals };
})();

if (typeof module !== "undefined") { module.exports = MtBudget; }
