// „Zeptej se MARKTIME“ – obecný AI agent s nástroji (samostatná stránka /AiAsk). Chat, historie a hodnocení řeší společný aichat.js (AiChat).
// Konverzace včetně volání nástrojů (thread) se drží v prohlížeči a posílá se s každým dotazem.
// Tabulky vykresluje MARKTIME z dat BL (r.tables) – model je nepřepisuje.
// Stav konverzace se ukládá do sessionStorage (jen tato záložka) – přežije reload i odchod a návrat na stránku.
var _ask_thread = null;
var _ask_chat = null;
var _ask_items = [];        // vykreslené zprávy pro obnovu: {role, text, logId, tables, warnings}
var _ask_key = "aiask-state-" + _ask_user;

$(function () {
    _ask_chat = new AiChat({
        askUrl: "/AiAsk/Ask",
        page: "aiask",
        extra: function () { return { thread: _ask_thread }; },
        onAnswer: function (r, $msg) {
            _ask_thread = r.thread || null;
            ask_decorate($msg, r.tables || [], r.warnings || []);
            var h = _ask_chat.history;
            var q = h.length >= 2 ? h[h.length - 2].content : "";
            _ask_items.push({ role: "user", text: q });
            _ask_items.push({ role: "assistant", text: r.reply || "", logId: r.logId || 0, tables: r.tables || [], warnings: r.warnings || [] });
            ask_save();
        }
    });

    $("#cmdAskNew").on("click", function () {
        _ask_thread = null;
        _ask_items = [];
        try { sessionStorage.removeItem(_ask_key); } catch (e) { }
        _ask_chat.reset();
    });

    ask_restore();
});

// ------------------------------------------------------------------ sessionStorage

function ask_save() {
    var st = { v: 1, conversation: _ask_chat.conversation, thread: _ask_thread, history: _ask_chat.history, items: _ask_items };
    try {
        sessionStorage.setItem(_ask_key, JSON.stringify(st));
    } catch (e) {
        // plné úložiště (limit cca 5 MB) – konverzace pak jen nepřežije reload
        try { sessionStorage.removeItem(_ask_key); } catch (e2) { }
    }
}

function ask_restore() {
    var st = null;
    try { st = JSON.parse(sessionStorage.getItem(_ask_key) || "null"); } catch (e) { st = null; }
    if (!st || st.v !== 1 || !st.items || !st.items.length) return;

    _ask_thread = st.thread || null;
    _ask_items = st.items;
    _ask_chat.conversation = st.conversation || _ask_chat.conversation;
    _ask_chat.history = st.history || [];
    st.items.forEach(function (it) {
        if (it.role === "user") { _ask_chat.addMessage("ai-user", it.text); return; }
        var $m = _ask_chat.addMessage("ai-assistant", it.text);
        if (it.logId) { _ask_chat.lastLogId = it.logId; _ask_chat.addRating($m, it.logId); }
        ask_decorate($m, it.tables || [], it.warnings || []);
    });
}

// tabulky a upozornění do bubliny odpovědi (před hodnocení)
function ask_decorate($msg, tables, warnings) {
    var $place = $msg.find(".ai-rating");
    var put = function ($el) { if ($place.length) $place.before($el); else $msg.append($el); };
    tables.forEach(function (t) { put(ask_render_table(t)); });
    if (warnings.length) put($("<div>").addClass("ai-result-warn").text(warnings.join(" ")));
}

// ------------------------------------------------------------------ vykreslení

function ask_nf(dec) {
    return new Intl.NumberFormat("cs-CZ", { minimumFractionDigits: dec, maximumFractionDigits: dec });
}

// hodnota buňky pro zobrazení (texty, datum yyyy-MM-dd, ano/ne 1/0; čísla)
function ask_cell(col, v) {
    if (v === null || v === undefined) return "";
    if (col.type === "number") return typeof v === "number" ? ask_nf(col.decimals).format(v) : v;
    switch (col.type) {
        case "date":
            var m = /^(\d{4})-(\d{2})-(\d{2})(?:T(\d{2}:\d{2}))?/.exec(v);
            return m ? (parseInt(m[3], 10) + "." + parseInt(m[2], 10) + "." + m[1] + (m[4] ? " " + m[4] : "")) : v;
        case "bool":
            return v === "1" ? _ask_txt.yes : _ask_txt.no;
        default:
            return v;
    }
}

function ask_render_table(t) {
    var isList = t.kind === "list";
    var $box = $("<div>").addClass("ai-result");
    var dims = t.columns.filter(function (c) { return !c.measure; }).length;

    var $head = $("<div>").addClass("ai-result-head");
    if (t.title) $head.append($("<div>").addClass("ai-result-title").text(t.title));
    if (t.description) $head.append($("<div>").addClass("ai-result-desc").text(t.description));
    if (isList) $head.append($("<div>").addClass("ai-result-desc").text(_ask_txt.countList.replace("{0}", ask_nf(0).format(t.groups || 0))));
    $box.append($head);

    if (!t.rows || t.rows.length === 0) {
        $box.append($("<div>").addClass("ai-result-desc").text(_ask_txt.empty));
    } else {
        var $wrap = $("<div>").addClass("ai-result-scroll");
        var $tab = $("<table>").addClass("table table-sm table-hover ai-table");
        var $tr = $("<tr>");
        t.columns.forEach(function (c) { $tr.append($("<th>").addClass(c.type === "number" ? "ai-num" : "").text(c.name)); });
        $tab.append($("<thead>").append($tr));

        var $body = $("<tbody>");
        var canDrillRow = !isList && t.canDrill && dims > 0;
        var canCard = isList && t.ids && t.ids.length;
        t.rows.forEach(function (row, ri) {
            var $r = $("<tr>");
            t.columns.forEach(function (c, i) {
                $r.append($("<td>").addClass(c.type === "number" ? "ai-num" : (c.type === "string" ? "ai-text" : "")).text(ask_cell(c, row[i])));
            });
            if (canDrillRow) {
                $r.addClass("ai-click-row").attr("title", _ask_txt.drillRow).on("click", function () { ask_drill(t, row); });
            } else if (canCard && t.ids[ri]) {
                $r.addClass("ai-click-row").attr("title", _ask_txt.cardRow).on("click", function () { ask_card(t.entity, t.ids[ri]); });
            }
            $body.append($r);
        });
        $tab.append($body);

        // celkem: agregace s víc řádky, výpis se sčítatelnými sloupci
        var showTotal = t.total && (isList ? t.groups > 1 : (dims > 0 && (t.rows.length > 1 || t.truncated)));
        if (showTotal) {
            var $f = $("<tr>");
            var labelDone = false;
            t.columns.forEach(function (c, i) {
                var txt = "";
                if (c.measure) txt = ask_cell(c, t.total[i]);
                else if (!labelDone) { txt = _ask_txt.total; labelDone = true; }
                $f.append($("<td>").addClass(c.type === "number" ? "ai-num" : "").text(txt));
            });
            $tab.append($("<tfoot>").append($f));
        }
        $wrap.append($tab);
        $box.append($wrap);
    }

    if (t.truncated) {
        var msg = isList ? _ask_txt.shownList : _ask_txt.shown;
        $box.append($("<div>").addClass("ai-result-desc").text(msg.replace("{0}", t.rows.length).replace("{1}", ask_nf(0).format(t.groups))));
    }

    var $btns = $("<div>").addClass("ai-result-buttons");
    if (t.guid) {
        if (!isList) {
            $btns.append($("<button>").attr("type", "button").addClass("btn btn-light btn-sm").attr("title", _ask_txt.pivotTitle)
                .append($("<span>").addClass("material-icons-outlined-btn").text("functions")).append(document.createTextNode(_ask_txt.pivot))
                .on("click", function () { _window_open("/AiAsk/Pivot?guid=" + encodeURIComponent(t.guid), 2); }));
        }
        if (t.canDrill) {
            $btns.append($("<button>").attr("type", "button").addClass("btn btn-light btn-sm").attr("title", _ask_txt.drillTitle)
                .append($("<span>").addClass("material-icons-outlined-btn").text("list")).append(document.createTextNode(_ask_txt.drill))
                .on("click", function () { ask_drill(t, null); }));
        }
    }
    if ($btns.children().length) $box.append($btns);
    return $box;
}

// karta záznamu: úkon jako formulář v okně, ostatní entity jako stránka záznamu v nové záložce (chat zůstane otevřený)
function ask_card(prefix, pid) {
    if (prefix === "p31") { _edit("p31", pid); return; }
    window.open("/Record/RecPage?prefix=" + encodeURIComponent(prefix) + "&pid=" + pid, "_blank");
}

// zdrojové úkony: row = null → celý výsledek, jinak podmínky na hodnoty dimenzí řádku (formát myPivotDrill)
function ask_drill(t, row) {
    var conds = [], label = [];
    if (row) {
        t.columns.forEach(function (c, i) {
            if (c.measure) return;
            var v = row[i];
            if (v === null || v === undefined) conds.push({ f: c.name, mode: "in", empty: true, values: [] });
            else conds.push({ f: c.name, mode: "in", empty: false, values: [v] });
            label.push(ask_cell(c, v) || "–");
        });
    }
    var lbl = (t.title || "") + (label.length ? ": " + label.join(" / ") : "");
    $.ajax({ url: "/AiAsk/DrillDown?guid=" + encodeURIComponent(t.guid), type: "POST", contentType: "application/json", data: JSON.stringify({ conds: conds, label: lbl }) })
        .done(function (r) {
            if (r.error) { _notify_message(r.error, "warning"); return; }
            _window_open("/p31totals/Drill?guid=" + r.guid + "&count=" + r.count + "&label=" + encodeURIComponent(lbl), 2);
        })
        .fail(function () { _notify_message(_ask_txt.drillError, "error"); });
}
