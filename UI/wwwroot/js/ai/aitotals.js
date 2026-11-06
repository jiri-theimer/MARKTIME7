// Agent Součtů – formulář návrhu šablony. Chat, historie a hodnocení řeší společný aichat.js (AiChat).
var _ai_proposal = null;    // aktuální návrh (klíče sloupců)
var _ai_byKey = {};
var _ai_chat = null;

$(function () {
    _ai_catalog.forEach(function (c) { _ai_byKey[c.key] = c; });
    fill_select("#ai_period", _ai_periods.map(function (p) { return { v: p.pid, t: p.name }; }));
    fill_select("#ai_state", _ai_states.map(function (p) { return { v: p.id, t: p.name }; }));
    fill_select("#ai_tab", _ai_tabs.map(function (p) { return { v: p.id, t: p.name }; }));
    fill_add_select("#ai_add_rows", "dimension");
    fill_add_select("#ai_add_columns", "dimension");
    fill_add_select("#ai_add_measures", "measure");

    $(".ai-add").on("change", function () {
        var key = $(this).val(); if (!key) return;
        var t = $(this).data("target");
        ensure_proposal();
        if (t === "measures") _ai_proposal.measures.push({ key: key, aggregation: "sum" });
        else _ai_proposal[t].push(key);
        $(this).val("");
        render_chips();
    });
    $("#ai_period").on("change", function () { toggle_custom(); });
    $("#ai_form select, #ai_form input").on("change", function () { $(this).closest(".ai-confirm").removeClass("ai-confirm"); });
    $("#ai_save").on("click", function () { ai_save(false); });
    // přepsání výchozí šablony – potvrzení druhým kliknutím (bez dialogu)
    $("#ai_overwrite").on("click", function () {
        var $b = $(this);
        if (!$b.data("armed")) {
            $b.data("armed", true).removeClass("btn-outline-warning").addClass("btn-warning").text(_ai_txt.overwriteConfirm);
            setTimeout(function () { ai_overwrite_reset(); }, 5000);
            return;
        }
        ai_overwrite_reset();
        ai_save(true);
    });

    _ai_chat = new AiChat({
        askUrl: "/AiTotals/Ask",
        page: "p31totals",
        extra: function () { return { current: _ai_proposal ? read_form() : null }; },
        onAnswer: function (r) {
            if (r.proposal) {
                _ai_proposal = r.proposal;
                write_form(r.proposal, r.confirm || []);
            }
            show_warnings(r.warnings || []);
        }
    });
});

function fill_select(sel, items) {
    var $s = $(sel).empty();
    items.forEach(function (i) { $s.append($("<option>").val(i.v).text(i.t)); });
}

function fill_add_select(sel, role) {
    var $s = $(sel).empty().append($("<option>").val("").text(_ai_txt.add));
    var groups = {};
    _ai_catalog.filter(function (c) { return c.role === role; }).forEach(function (c) {
        (groups[c.group] = groups[c.group] || []).push(c);
    });
    Object.keys(groups).forEach(function (g) {
        var $og = $("<optgroup>").attr("label", g);
        groups[g].forEach(function (c) { $og.append($("<option>").val(c.key).text(c.header).attr("title", c.hint || "")); });
        $s.append($og);
    });
}

function ensure_proposal() {
    if (!_ai_proposal) _ai_proposal = { name: "", rows: [], columns: [], measures: [], state: 0, tab: "", period: { value: 0, field: "p31Date", d1: null, d2: null }, chart: 0 };
    $("#ai_form").show(); $("#ai_form_empty").hide();
}

function write_form(p, confirm) {
    ensure_proposal();
    $("#ai_name").val(p.name);
    $("#ai_state").val(p.state);
    $("#ai_tab").val(p.tab || "");
    $("#ai_chart").val(p.chart || 0);
    var per = p.period || { value: 0, field: "p31Date" };
    $("#ai_period").val(per.value);
    $("#ai_period_field").val(per.field || "p31Date");
    $("#ai_d1").val(per.d1 || ""); $("#ai_d2").val(per.d2 || "");
    toggle_custom();
    render_chips();

    $("#ai_box_period, #ai_box_state, #ai_box_tab").removeClass("ai-confirm");
    confirm.forEach(function (c) { $("#ai_box_" + c).addClass("ai-confirm"); });
}

function read_form() {
    var p = _ai_proposal;
    p.name = $("#ai_name").val();
    p.state = parseInt($("#ai_state").val() || "0");
    p.tab = $("#ai_tab").val() || "";
    p.chart = parseInt($("#ai_chart").val() || "0");
    var v = parseInt($("#ai_period").val() || "0");
    p.period = {
        value: v, field: $("#ai_period_field").val(),
        d1: v === 1 ? ($("#ai_d1").val() || null) : null,
        d2: v === 1 ? ($("#ai_d2").val() || null) : null
    };
    $("#ai_measures .ai-chip").each(function (i) {
        if (p.measures[i]) p.measures[i].aggregation = $(this).find("select").val();
    });
    return p;
}

function render_chips() {
    var p = _ai_proposal;
    ["rows", "columns"].forEach(function (t) {
        var $c = $("#ai_" + t).empty();
        p[t].forEach(function (k, i) { $c.append(chip(k, function () { p[t].splice(i, 1); render_chips(); })); });
    });
    var $m = $("#ai_measures").empty();
    p.measures.forEach(function (m, i) {
        var $ch = chip(m.key, function () { read_form(); p.measures.splice(i, 1); render_chips(); });
        var $agg = $("<select>");
        [["sum", "Σ součet"], ["average", "průměr"], ["count", "počet"], ["min", "min"], ["max", "max"]].forEach(function (a) {
            $agg.append($("<option>").val(a[0]).text(a[1]));
        });
        $agg.val(m.aggregation || "sum");
        $ch.find(".ai-x").before($agg);
        $m.append($ch);
    });
}

function chip(key, onRemove) {
    var c = _ai_byKey[key];
    var $x = $("<span>").addClass("ai-x").text("×").on("click", onRemove);
    return $("<span>").addClass("ai-chip").attr("title", c ? (c.hint || "") : key)
        .append($("<span>").text(c ? c.header : key)).append($x);
}

function toggle_custom() {
    $("#ai_period_custom").toggle($("#ai_period").val() === "1");
}

function show_warnings(w) {
    if (!w.length) { $("#ai_warnings").hide(); return; }
    $("#ai_warnings").html(w.map(function (x) { return $("<div>").text(x).prop("outerHTML"); }).join("")).show();
}

function ai_overwrite_reset() {
    $("#ai_overwrite").data("armed", false).removeClass("btn-warning").addClass("btn-outline-warning").text(_ai_txt.overwrite);
}

function ai_save(overwrite) {
    if (!_ai_proposal) return;
    var p = read_form();
    $("#ai_save, #ai_overwrite").prop("disabled", true);
    $.post("/AiTotals/Save", { payload: JSON.stringify(p), logId: _ai_chat.lastLogId, overwrite: overwrite }, function (r) {
        $("#ai_save, #ai_overwrite").prop("disabled", false);
        if (!r.ok) { show_warnings(r.errors || []); return; }
        // Součty načítají layout přednostně z localStorage – u přepsané šablony by jinak zůstal starý
        try { localStorage.removeItem("webdatarocks_layout-" + r.j79id); } catch (e) { }
        if (window.parent && window.parent !== window && typeof window.parent.reload_explicit_j79id === "function") {
            window.parent.reload_explicit_j79id(r.j79id);     // stránka Součtů
            if (typeof window.parent._window_close === "function") window.parent._window_close();
        } else {
            location.href = "/p31totals/Index?j79id=" + r.j79id;
        }
    }).fail(function () {
        $("#ai_save, #ai_overwrite").prop("disabled", false);
        show_warnings([_ai_txt.error]);
    });
}
