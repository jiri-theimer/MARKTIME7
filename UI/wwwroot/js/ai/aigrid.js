// Agent tabulky – formulář sloupců a sloupcového filtru. Chat, historie a hodnocení řeší společný aichat.js (AiChat).
var _ai_proposal = null;    // { name, columns: [klíče], filter: [{column, op, values, from, to}], matchAny }
var _ai_byKey = {};
var _ai_initialKeys = {};   // sloupce původní tabulky – nové sloupce se zvýrazní
var _ai_chat = null;

// operátory podle typu filtru – stejně jako popup filtru v gridu (AiGridFilterCodec.OpsByType)
var _ai_ops = {
    string: ["contains", "notcontains", "starts", "equal", "notequal", "empty", "notempty"],
    number: ["interval", "gtzero", "zeroorempty", "empty", "notempty"],
    date: ["interval", "empty", "notempty"],
    bool: ["yes", "no"]
};
var _ai_textOps = ["contains", "notcontains", "starts", "equal", "notequal"];

$(function () {
    _ai_catalog.forEach(function (c) { _ai_byKey[c.key] = c; });
    fill_add_select("#ai_add", _ai_txt.add, function () { return true; });
    fill_add_select("#ai_add_filter", _ai_txt.addFilter, function (c) { return !!c.filter; });

    $("#ai_add").on("change", function () {
        var key = $(this).val(); if (!key) return;
        add_column(key);
        $(this).val("");
        render_columns();
    });
    $("#ai_add_filter").on("change", function () {
        var key = $(this).val(); if (!key) return;
        var c = _ai_byKey[key];
        _ai_proposal.filter.push({ column: key, op: _ai_ops[c.filter][0], values: [], from: null, to: null });
        add_column(key);
        $(this).val("");
        render_columns();
        render_filters();
    });
    $("#ai_match_any").on("change", function () { _ai_proposal.matchAny = $(this).is(":checked"); });
    $("#ai_overwrite").on("click", function () { ai_save("overwrite"); });
    $("#ai_save").on("click", function () { ai_save("new"); });
    $("#ai_apply_filter").on("click", function () { ai_save("filter"); });
    $("#cmdAiClear").on("click", function () { ai_clear(); });

    _ai_chat = new AiChat({
        askUrl: "/AiGrid/Ask",
        page: "grid-" + (_ai_pathname || ""),
        extra: function () { return { current: read_form(), j72id: _ai_j72id }; },
        onAnswer: function (r) {
            if (r.proposal) {
                _ai_proposal.columns = r.proposal.columns || [];
                _ai_proposal.filter = r.proposal.filter || [];
                _ai_proposal.matchAny = !!r.proposal.matchAny;
                if (r.proposal.name && !$("#ai_name").val()) $("#ai_name").val(r.proposal.name);
                render_columns();
                render_filters();
            }
            show_warnings(r.warnings || []);
        }
    });

    load_initial();
});

function load_initial() {
    var i = _ai_initial || {};
    _ai_proposal = {
        name: i.name || "",
        columns: (i.columns || []).slice(),
        filter: (i.filter || []).map(function (f) { return $.extend({}, f, { values: (f.values || []).slice() }); }),
        matchAny: !!i.matchAny
    };
    _ai_initialKeys = {};
    _ai_proposal.columns.forEach(function (k) { _ai_initialKeys[k] = true; });
    $("#ai_name").val(_ai_proposal.name);
    render_columns();
    render_filters();
    show_warnings(_ai_initial_warnings || []);
}

function fill_add_select(sel, firstText, cond) {
    var $s = $(sel).empty().append($("<option>").val("").text(firstText));
    var groups = {};
    _ai_catalog.filter(cond).forEach(function (c) { (groups[c.group] = groups[c.group] || []).push(c); });
    Object.keys(groups).forEach(function (g) {
        var $og = $("<optgroup>").attr("label", g);
        groups[g].forEach(function (c) { $og.append($("<option>").val(c.key).text(c.header).attr("title", c.hint || "")); });
        $s.append($og);
    });
}

function add_column(key) {
    if (_ai_proposal.columns.indexOf(key) < 0) _ai_proposal.columns.push(key);
}

// ------------------------------------------------------------------ sloupce

function render_columns() {
    var cols = _ai_proposal.columns;
    var filtered = {};
    _ai_proposal.filter.forEach(function (f) { filtered[f.column] = true; });
    var $c = $("#ai_columns").empty();
    cols.forEach(function (k, i) {
        var c = _ai_byKey[k];
        var $chip = $("<span>").addClass("ai-chip" + (_ai_initialKeys[k] ? "" : " ai-chip-new")).attr("title", c ? (c.hint || "") : k);
        if (i > 0) $chip.append($("<span>").addClass("ai-mv").attr("title", _ai_txt.left).text("‹").on("click", function () { move(i, -1); }));
        $chip.append($("<span>").addClass("ai-chip-text").text((filtered[k] ? "⧩ " : "") + (c ? c.header : k)));
        if (i < cols.length - 1) $chip.append($("<span>").addClass("ai-mv").attr("title", _ai_txt.right).text("›").on("click", function () { move(i, 1); }));
        $chip.append($("<span>").addClass("ai-x").attr("title", _ai_txt.remove).text("×").on("click", function () { remove_column(i); }));
        $c.append($chip);
    });
}

// grid filtruje jen podle zobrazených sloupců – s odebraným sloupcem zmizí i jeho podmínky
function remove_column(i) {
    var key = _ai_proposal.columns[i];
    _ai_proposal.columns.splice(i, 1);
    _ai_proposal.filter = _ai_proposal.filter.filter(function (f) { return f.column !== key; });
    render_columns();
    render_filters();
}

function move(i, dir) {
    var cols = _ai_proposal.columns, j = i + dir;
    if (j < 0 || j >= cols.length) return;
    var t = cols[i]; cols[i] = cols[j]; cols[j] = t;
    render_columns();
}

// ------------------------------------------------------------------ filtr

function render_filters() {
    var $f = $("#ai_filters").empty();
    _ai_proposal.filter.forEach(function (f, i) {
        var c = _ai_byKey[f.column] || { header: f.column, filter: "string" };
        var $row = $("<div>").addClass("ai-filter-row");
        $row.append($("<span>").addClass("ai-filter-col").attr("title", c.hint || "").text(c.header));

        var $op = $("<select>").addClass("form-select form-select-sm ai-filter-op");
        (_ai_ops[c.filter] || []).forEach(function (o) { $op.append($("<option>").val(o).text(_ai_txt.ops[o] || o)); });
        $op.val(f.op).on("change", function () { f.op = $(this).val(); render_filters(); });
        $row.append($op);

        if (_ai_textOps.indexOf(f.op) >= 0) {
            $row.append($("<input>").addClass("form-control form-control-sm ai-filter-val").attr("placeholder", _ai_txt.valuesHint)
                .val((f.values || []).join("; "))
                .on("change", function () {
                    f.values = $(this).val().split(";").map(function (x) { return $.trim(x); }).filter(function (x) { return x !== ""; });
                }));
        }
        if (f.op === "interval") {
            var type = c.filter === "date" ? "date" : "number";
            $row.append($("<input>").attr("type", type).addClass("form-control form-control-sm ai-filter-int").val(f.from || "")
                .on("change", function () { f.from = $(this).val() || null; }));
            $row.append($("<span>").text("–"));
            $row.append($("<input>").attr("type", type).addClass("form-control form-control-sm ai-filter-int").val(f.to || "")
                .on("change", function () { f.to = $(this).val() || null; }));
        }
        $row.append($("<span>").addClass("ai-x").attr("title", _ai_txt.remove).text("×").on("click", function () {
            _ai_proposal.filter.splice(i, 1);
            render_filters();
            render_columns();
        }));
        $f.append($row);
    });
    $("#ai_match_any").prop("checked", !!_ai_proposal.matchAny);
    $("#ai_box_match").toggle(_ai_proposal.filter.length > 1);
}

// ------------------------------------------------------------------ formulář

function read_form() {
    _ai_proposal.name = $("#ai_name").val();
    _ai_proposal.matchAny = $("#ai_match_any").is(":checked") && _ai_proposal.filter.length > 1;
    return _ai_proposal;
}

function show_warnings(w) {
    if (!w.length) { $("#ai_warnings").hide(); return; }
    $("#ai_warnings").html(w.map(function (x) { return $("<div>").text(x).prop("outerHTML"); }).join("")).show();
}

// Vyčistit: nová konverzace, prázdný seznam sloupců i filtr – návrh tabulky od nuly
function ai_clear() {
    _ai_chat.reset();
    _ai_proposal = { name: "", columns: [], filter: [], matchAny: false };
    $("#ai_name").val("");
    render_columns();
    render_filters();
    show_warnings([]);
}

// mode: overwrite = přepsat tabulku, new = uložit jako novou, filter = jen použít filtr
function ai_save(mode) {
    var p = read_form();
    if (mode === "new" && !$.trim(p.name)) { show_warnings([_ai_txt.nameRequired]); $("#ai_name").focus(); return; }
    var $btns = $("#ai_save, #ai_overwrite, #ai_apply_filter").prop("disabled", true);
    $.post("/AiGrid/Save", { payload: JSON.stringify(p), j72id: _ai_j72id, logId: _ai_chat.lastLogId, mode: mode, pathname: _ai_pathname }, function (r) {
        $btns.prop("disabled", false);
        if (!r.ok) { show_warnings(r.errors || []); return; }
        _reload_layout_and_close(r.j72id, "");      // stejně jako TheGridDesigner: obnovit grid a zavřít okno
    }).fail(function () {
        $btns.prop("disabled", false);
        show_warnings([_ai_txt.error]);
    });
}
