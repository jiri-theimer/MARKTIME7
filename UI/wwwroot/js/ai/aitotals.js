// AI návrh šablony Součtů – klient (historie konverzace se drží v prohlížeči, server je bezstavový)
var _ai_history = [];       // [{role, content}] – assistant content = raw JSON odpovědi modelu
var _ai_proposal = null;    // aktuální návrh (klíče sloupců)
var _ai_byKey = {};
var _ai_conversation = (window.crypto && crypto.randomUUID) ? crypto.randomUUID() : ("c" + Date.now() + Math.random().toString(16).slice(2));
var _ai_lastLogId = 0;      // j98ID posledního úspěšného dotazu – předává se při uložení šablony

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
    $("#ai_prompt").on("keydown", function (e) { if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); ai_send(); } });
    $("#ai_prompt").focus();
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

function add_msg(cls, text) {
    var $m = $("<div>").addClass("ai-msg " + cls).text(text);
    $("#ai_chat").append($m);
    $("#ai_chat").scrollTop($("#ai_chat")[0].scrollHeight);
    return $m;
}

function ai_send() {
    var text = $.trim($("#ai_prompt").val());
    if (!text) return;
    $("#ai_prompt").val("");
    add_msg("ai-user", text);
    _ai_history.push({ role: "user", content: text });

    var $wait = add_msg("ai-assistant", _ai_txt.thinking);
    $("#ai_send").prop("disabled", true);

    var payload = { messages: _ai_history, current: _ai_proposal ? read_form() : null, conversation: _ai_conversation, page: "p31totals" };
    $.post("/AiTotals/Ask", { payload: JSON.stringify(payload) }, function (r) {
        $wait.remove();
        $("#ai_send").prop("disabled", false);
        if (!r.ok) {
            add_msg("ai-error", _ai_txt.error + ": " + (r.error || ""));
            _ai_history.pop();      // neúspěšný dotaz do historie nepatří
            return;
        }
        var $msg = add_msg("ai-assistant", r.reply || "");
        if (r.logId) { _ai_lastLogId = r.logId; add_rating($msg, r.logId); }
        _ai_history.push({ role: "assistant", content: r.raw || r.reply || "" });
        if (r.proposal) {
            _ai_proposal = r.proposal;
            write_form(r.proposal, r.confirm || []);
        }
        show_warnings(r.warnings || []);
    }).fail(function () {
        $wait.remove();
        $("#ai_send").prop("disabled", false);
        add_msg("ai-error", _ai_txt.error);
        _ai_history.pop();
    });
}

// hodnocení odpovědi 👍 / 👎 (u 👎 volitelný komentář)
function add_rating($msg, logId) {
    var $r = $("<div>").addClass("ai-rating");
    var $up = $("<span>").addClass("ai-thumb").attr("title", _ai_txt.rateUp).text("👍");
    var $down = $("<span>").addClass("ai-thumb").attr("title", _ai_txt.rateDown).text("👎");
    var $note = $("<div>").addClass("ai-rating-note").hide()
        .append($("<input>").addClass("form-control form-control-sm").attr("placeholder", _ai_txt.rateComment))
        .append($("<button>").attr("type", "button").addClass("btn btn-sm btn-outline-secondary").text(_ai_txt.send).on("click", function () {
            send_rating(logId, -1, $note.find("input").val(), $r);
        }));
    $up.on("click", function () { send_rating(logId, 1, null, $r); });
    $down.on("click", function () { $down.addClass("ai-thumb-on"); $up.removeClass("ai-thumb-on"); $note.show(); $note.find("input").focus(); });
    $r.append($up).append($down).append($note);
    $msg.append($r);
}

function send_rating(logId, rating, comment, $r) {
    $.post("/AiTotals/Rate", { logId: logId, rating: rating, comment: comment || "" }, function () {
        $r.empty().append($("<span>").addClass("ai-rated").text(rating === 1 ? "👍 " + _ai_txt.thanks : "👎 " + _ai_txt.thanks));
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

function ai_save() {
    if (!_ai_proposal) return;
    var p = read_form();
    $("#ai_save").prop("disabled", true);
    $.post("/AiTotals/Save", { payload: JSON.stringify(p), logId: _ai_lastLogId }, function (r) {
        $("#ai_save").prop("disabled", false);
        if (!r.ok) { show_warnings(r.errors || []); return; }
        if (window.parent && window.parent !== window && typeof window.parent.reload_explicit_j79id === "function") {
            window.parent.reload_explicit_j79id(r.j79id);     // stránka Součtů
            if (typeof window.parent._window_close === "function") window.parent._window_close();
        } else {
            location.href = "/p31totals/Index?j79id=" + r.j79id;
        }
    }).fail(function () {
        $("#ai_save").prop("disabled", false);
        show_warnings([_ai_txt.error]);
    });
}
