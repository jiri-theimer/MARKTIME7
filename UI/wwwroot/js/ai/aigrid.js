// Agent návrháře sloupců – formulář. Chat, historie a hodnocení řeší společný aichat.js (AiChat).
var _ai_proposal = null;    // { name, columns: [klíče] }
var _ai_byKey = {};
var _ai_initialKeys = {};   // sloupce původní tabulky – nové sloupce se zvýrazní
var _ai_chat = null;

$(function () {
    _ai_catalog.forEach(function (c) { _ai_byKey[c.key] = c; });
    fill_add_select();

    $("#ai_add").on("change", function () {
        var key = $(this).val(); if (!key) return;
        if (_ai_proposal.columns.indexOf(key) < 0) _ai_proposal.columns.push(key);
        $(this).val("");
        render_columns();
    });
    $("#ai_overwrite").on("click", function () { ai_save(true); });
    $("#ai_save").on("click", function () { ai_save(false); });
    $("#cmdAiClear").on("click", function () { ai_clear(); });

    _ai_chat = new AiChat({
        askUrl: "/AiGrid/Ask",
        page: "grid-" + (_ai_pathname || ""),
        extra: function () { return { current: read_form(), j72id: _ai_j72id }; },
        onAnswer: function (r) {
            if (r.proposal) {
                _ai_proposal.columns = r.proposal.columns || [];
                if (r.proposal.name && !$("#ai_name").val()) $("#ai_name").val(r.proposal.name);
                render_columns();
            }
            show_warnings(r.warnings || []);
        }
    });

    load_initial();
});

function load_initial() {
    _ai_proposal = { name: (_ai_initial && _ai_initial.name) || "", columns: (_ai_initial && _ai_initial.columns || []).slice() };
    _ai_initialKeys = {};
    _ai_proposal.columns.forEach(function (k) { _ai_initialKeys[k] = true; });
    $("#ai_name").val(_ai_proposal.name);
    render_columns();
    show_warnings(_ai_initial_warnings || []);
}

function fill_add_select() {
    var $s = $("#ai_add").empty().append($("<option>").val("").text(_ai_txt.add));
    var groups = {};
    _ai_catalog.forEach(function (c) { (groups[c.group] = groups[c.group] || []).push(c); });
    Object.keys(groups).forEach(function (g) {
        var $og = $("<optgroup>").attr("label", g);
        groups[g].forEach(function (c) { $og.append($("<option>").val(c.key).text(c.header).attr("title", c.hint || "")); });
        $s.append($og);
    });
}

function render_columns() {
    var cols = _ai_proposal.columns;
    var $c = $("#ai_columns").empty();
    cols.forEach(function (k, i) {
        var c = _ai_byKey[k];
        var $chip = $("<span>").addClass("ai-chip" + (_ai_initialKeys[k] ? "" : " ai-chip-new")).attr("title", c ? (c.hint || "") : k);
        if (i > 0) $chip.append($("<span>").addClass("ai-mv").attr("title", _ai_txt.left).text("‹").on("click", function () { move(i, -1); }));
        $chip.append($("<span>").addClass("ai-chip-text").text(c ? c.header : k));
        if (i < cols.length - 1) $chip.append($("<span>").addClass("ai-mv").attr("title", _ai_txt.right).text("›").on("click", function () { move(i, 1); }));
        $chip.append($("<span>").addClass("ai-x").attr("title", _ai_txt.remove).text("×").on("click", function () { cols.splice(i, 1); render_columns(); }));
        $c.append($chip);
    });
}

function move(i, dir) {
    var cols = _ai_proposal.columns, j = i + dir;
    if (j < 0 || j >= cols.length) return;
    var t = cols[i]; cols[i] = cols[j]; cols[j] = t;
    render_columns();
}

function read_form() {
    _ai_proposal.name = $("#ai_name").val();
    return _ai_proposal;
}

function show_warnings(w) {
    if (!w.length) { $("#ai_warnings").hide(); return; }
    $("#ai_warnings").html(w.map(function (x) { return $("<div>").text(x).prop("outerHTML"); }).join("")).show();
}

// Vyčistit: nová konverzace a prázdný seznam sloupců – návrh tabulky od nuly
function ai_clear() {
    _ai_chat.reset();
    _ai_proposal = { name: "", columns: [] };
    $("#ai_name").val("");
    render_columns();
    show_warnings([]);
}


function ai_save(overwrite) {
    var p = read_form();
    if (!overwrite && !$.trim(p.name)) { show_warnings([_ai_txt.nameRequired]); $("#ai_name").focus(); return; }
    $("#ai_save, #ai_overwrite").prop("disabled", true);
    $.post("/AiGrid/Save", { payload: JSON.stringify(p), j72id: _ai_j72id, logId: _ai_chat.lastLogId, overwrite: overwrite, pathname: _ai_pathname }, function (r) {
        $("#ai_save, #ai_overwrite").prop("disabled", false);
        if (!r.ok) { show_warnings(r.errors || []); return; }
        _reload_layout_and_close(r.j72id, "");      // stejně jako TheGridDesigner: obnovit grid a zavřít okno
    }).fail(function () {
        $("#ai_save, #ai_overwrite").prop("disabled", false);
        show_warnings([_ai_txt.error]);
    });
}
