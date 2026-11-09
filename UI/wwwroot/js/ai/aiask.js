// „Zeptej se MARKTIME“ – obecný AI agent s nástroji (samostatná stránka /AiAsk). Chat, historie a hodnocení řeší společný aichat.js (AiChat).
// Konverzace včetně volání nástrojů (thread) se drží v prohlížeči a posílá se s každým dotazem.
// Tabulky vykresluje MARKTIME z dat BL (r.tables) – model je nepřepisuje.
// Stav konverzace se ukládá do sessionStorage (jen tato záložka) – přežije reload i odchod a návrat na stránku.
var _ask_thread = null;
var _ask_chat = null;
var _ask_items = [];        // vykreslené zprávy pro obnovu: {role, text, logId, tables, warnings}
var _ask_key = "aiask-state-" + _ask_user;
if (typeof _ask_edit !== "undefined" && _ask_edit) _ask_key += "-w" + _ask_edit.x55id;      //úprava widgetu = vlastní konverzace, běžnou nepřepíše

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
        if (_ask_edit) ask_edit_seed();      //při úpravě widgetu začít znovu od jeho zadání
    });

    if (!ask_restore() && _ask_edit) ask_edit_seed();

    //?q=… (např. z okna upozornění) → položit dotaz a parametr z adresy odstranit, aby se po reloadu neopakoval
    try {
        var q = new URLSearchParams(location.search).get("q");
        if (q) {
            history.replaceState(null, "", location.pathname);
            _ask_chat.send(q);
        }
    } catch (e) { }
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
    if (!st || st.v !== 1 || !st.items || !st.items.length) return false;

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
    return true;
}

// úprava AI widgetu: úvodní zpráva s jeho současnou tabulkou (spočítanou bez AI) a thread se zadáním pro model
function ask_edit_seed() {
    _ask_thread = _ask_edit.thread || null;
    var tables = [];
    if (_ask_edit.table) { _ask_edit.table.seed = true; tables.push(_ask_edit.table); }
    var $m = _ask_chat.addMessage("ai-assistant", _ask_edit.intro);
    ask_decorate($m, tables, []);
    _ask_items = [{ role: "assistant", text: _ask_edit.intro, logId: 0, tables: tables, warnings: [] }];
    ask_save();
}

// „Uložit do widgetu“ – nové zadání nahradí zadání upravovaného widgetu
function ask_replace(t, $box) {
    $.post("/Widgets/AiReplace", { x55id: _ask_edit.x55id, guid: t.guid }, function (r) {
        if (!r || !r.ok) { _notify_message((r && r.error) || _ask_txt.pinError, "warning"); return; }
        $box.find(".ai-pin").remove();
        $box.append($("<div>").addClass("ai-pin").append($("<span>").text(_ask_txt.savedTo + " "))
            .append($("<a>").attr({ href: r.url, target: "_top" }).text(_ask_txt.pinOpen)));
    }).fail(function () { _notify_message(_ask_txt.pinError, "error"); });
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
        var canCard = isList && t.ids && t.ids.length;
        var $tr = $("<tr>");
        if (canCard) $tr.append($("<th>").addClass("ai-rec-col"));
        t.columns.forEach(function (c) { $tr.append($("<th>").addClass(c.type === "number" ? "ai-num" : "").text(c.name)); });
        $tab.append($("<thead>").append($tr));

        var $body = $("<tbody>");
        var canDrillRow = !isList && t.canDrill && dims > 0;
        t.rows.forEach(function (row, ri) {
            var $r = $("<tr>");
            //výpis: ikona karty záznamu v prvním sloupci
            if (canCard) {
                var $c0 = $("<td>").addClass("ai-rec-col");
                if (t.ids[ri]) $c0.append(ask_rec_icon(t.entity, t.ids[ri], false));
                $r.append($c0);
            }
            t.columns.forEach(function (c, i) {
                var $td = $("<td>").addClass(c.type === "number" ? "ai-num" : (c.type === "string" ? "ai-text" : "")).text(ask_cell(c, row[i]));
                //agregace: ikona karty u dimenze, která odpovídá jednomu záznamu (projekt, klient…)
                var lid = c.link && t.linkIds && t.linkIds[ri] ? t.linkIds[ri][i] : 0;
                if (lid) $td.prepend(ask_rec_icon(c.link, lid));
                $r.append($td);
            });
            if (canDrillRow) {
                $r.addClass("ai-click-row").attr("title", _ask_txt.drillRow).on("click", function (e) { if (!ask_on_icon(e)) ask_drill(t, row); });
            } else if (canCard && t.ids[ri]) {
                $r.addClass("ai-click-row").attr("title", _ask_txt.cardRow).on("click", function (e) { if (!ask_on_icon(e)) ask_card(t.entity, t.ids[ri]); });
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
        if (typeof _ask_edit !== "undefined" && _ask_edit && !t.seed) {
            $btns.append($("<button>").attr("type", "button").addClass("btn btn-primary btn-sm").attr("title", _ask_txt.saveToTitle)
                .append($("<span>").addClass("material-icons-outlined-btn").text("save")).append(document.createTextNode(_ask_txt.saveTo))
                .on("click", function () { ask_replace(t, $box); }));
        }
        if (typeof _ask_pin !== "undefined" && _ask_pin && !t.seed) {
            $btns.append($("<button>").attr("type", "button").addClass("btn btn-light btn-sm").attr("title", _ask_txt.pinTitle)
                .append($("<span>").addClass("material-icons-outlined-btn").text("push_pin")).append(document.createTextNode(_ask_txt.pin))
                .on("click", function () { ask_pin(t, $box); }));
        }
    }
    if ($btns.children().length) $box.append($btns);
    return $box;
}

// ☰ kontextové menu záznamu (stejné jako v gridu – akce a oprávnění řeší server) + ↗ otevřít kartu
// klik na ikony nespustí akci řádku (drill / karta) – hlídá ask_on_icon
// withOpen = false: řádek sám otevírá stránku záznamu (výpis) → ↗ by byla zbytečná
function ask_rec_icon(prefix, pid, withOpen) {
    var $g = $("<span>").addClass("ai-rec-g");
    $("<a>").attr("href", "javascript:void(0)").addClass("ai-rec ai-rec-cm").attr("title", _ask_txt.cardMenu).text("☰")
        .on("click", function (e) { _cm(e, prefix, pid); })       //bez stopPropagation – menu zobrazí plugin contextmenu.js v obsluze kliknutí na document
        .appendTo($g);
    if (withOpen === false) return $g;
    $("<a>").attr("href", "#").addClass("ai-rec").attr("title", prefix === "p31" ? _ask_txt.cardP31 : _ask_txt.cardRec)
        .append($("<span>").addClass("material-icons-outlined-nosize").text(prefix === "p31" ? "edit_note" : "open_in_new"))
        .on("click", function (e) { e.preventDefault(); ask_card(prefix, pid); })
        .appendTo($g);
    return $g;
}

function ask_on_icon(e) { return $(e.target).closest(".ai-rec-g").length > 0; }

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

// ------------------------------------------------------------------ „Na plochu“: výsledek jako osobní AI widget na dashboardu
// Ukládá se zadání (ne data); dashboard ho při každém zobrazení přepočítá z databáze – bez volání AI.
// Widget se zakládá do virtuální skupiny dashboardu „Moje AI widgety“ (x54id = -1, není v x54).
function ask_pin(t, $box) {
    if ($box.find(".ai-pin").length) return;
    $.post("/Widgets/AiPinInfo", { guid: t.guid }, function (info) {
        if (!info || !info.ok) { _notify_message((info && info.error) || _ask_txt.pinError, "warning"); return; }

        var $p = $("<div>").addClass("ai-pin");
        var $name = $("<input>").attr({ type: "text", maxlength: 100 }).addClass("form-control form-control-sm").val(info.title);
        $p.append($("<label>").text(_ask_txt.pinName)).append($name);

        var $live = null;
        if (info.period) {
            $live = $("<input>").attr({ type: "checkbox", id: "ai_pin_live_" + t.guid }).prop("checked", true);
            $p.append($("<div>").addClass("ai-pin-check").append($live)
                .append($("<label>").attr("for", "ai_pin_live_" + t.guid).text(_ask_txt.pinLive + ": " + info.period.name)));
        } else {
            $p.append($("<div>").addClass("ai-result-desc").text(_ask_txt.pinFixed));
        }

        $p.append($("<div>").addClass("ai-result-desc").text(_ask_txt.pinCount.replace("{0}", info.count).replace("{1}", info.limit)));

        var $save = $("<button>").attr("type", "button").addClass("btn btn-primary btn-sm").text(_ask_txt.pinSave);
        var $cancel = $("<button>").attr("type", "button").addClass("btn btn-light btn-sm").text(_ask_txt.pinCancel)
            .on("click", function () { $p.remove(); });
        $p.append($("<div>").addClass("ai-pin-buttons").append($save).append($cancel));
        if (info.count >= info.limit) $save.prop("disabled", true);

        $save.on("click", function () {
            $save.prop("disabled", true);
            $.post("/Widgets/AiCreate", {
                guid: t.guid, name: $name.val(),
                live: $live ? $live.prop("checked") : false
            }, function (r) {
                if (!r || !r.ok) { _notify_message((r && r.error) || _ask_txt.pinError, "warning"); $save.prop("disabled", false); return; }
                $p.empty().append($("<span>").text(_ask_txt.pinDone + " "))
                    .append($("<a>").attr({ href: r.url, target: "_top" }).text(_ask_txt.pinOpen));
            }).fail(function () { _notify_message(_ask_txt.pinError, "error"); $save.prop("disabled", false); });
        });

        $box.append($p);
        $name.trigger("focus").trigger("select");
    }).fail(function () { _notify_message(_ask_txt.pinError, "error"); });
}
