// Společný chat AI agentů MARKTIME.
// Historie konverzace se drží v prohlížeči (server je bezstavový), každá odpověď má hodnocení 👍/👎 (/Ai/Rate).
//
// var chat = new AiChat({
//     askUrl: "/AiTotals/Ask",               // akce agenta – přijme { payload: JSON }
//     page: "p31totals",                      // → j98ContextPage
//     extra: function () { return {...}; },   // doplňující data agenta do payloadu (např. stav formuláře)
//     onAnswer: function (r) { ... }          // úspěšná odpověď (r.reply, r.logId, r.warnings + data agenta)
// });
// chat.lastLogId – j98ID poslední odpovědi (pro vazbu na uložený výsledek)
// Předchozí dotazy uživatele: tlačítko 🕘 a šipka nahoru/dolů v poli dotazu (feature = data-feature v #ai_chat, viz _AiChat.cshtml).

function AiChat(opt) {
    var self = this;
    self.opt = opt;
    self.history = [];          // [{role, content}] – assistant content = raw JSON odpovědi modelu
    self.lastLogId = 0;
    self.conversation = (window.crypto && crypto.randomUUID) ? crypto.randomUUID() : ("c" + Date.now() + Math.random().toString(16).slice(2));
    self.$chat = $(opt.chat || "#ai_chat");
    self.$prompt = $(opt.prompt || "#ai_prompt");
    self.$send = $(opt.send || "#ai_send");
    self.txt = window._aichat_txt || {};
    self.feature = opt.feature || self.$chat.data("feature");
    self.past = [];             // předchozí dotazy (nejnovější první)
    self.pastIndex = -1;        // pozice při procházení šipkami
    self.$history = $(opt.historyBox || "#ai_history");
    self.$historyBtn = $(opt.historyBtn || "#ai_history_btn");

    self.$send.on("click", function () { self.send(); });
    self.$prompt.on("keydown", function (e) {
        if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); self.send(); return; }
        if (e.key === "Escape") { self.$history.hide(); return; }
        if ((e.key === "ArrowUp" || e.key === "ArrowDown") && self.past.length && self.canBrowse(e.key)) {
            e.preventDefault();
            self.browse(e.key === "ArrowUp" ? 1 : -1);
        }
    });
    self.$historyBtn.on("click", function (e) { e.stopPropagation(); self.toggleHistory(); });
    $(document).on("click", function () { self.$history.hide(); });
    self.loadHistory();
    self.$prompt.focus();
}

// nová konverzace: smaže zprávy (kromě úvodní), historii pro model a vygeneruje nové ID konverzace
AiChat.prototype.reset = function () {
    this.$chat.children().not(":first").remove();
    this.history = [];
    this.lastLogId = 0;
    this.pastIndex = -1;
    this.conversation = (window.crypto && crypto.randomUUID) ? crypto.randomUUID() : ("c" + Date.now() + Math.random().toString(16).slice(2));
    this.$prompt.val("").focus();
};

AiChat.prototype.loadHistory = function () {
    var self = this;
    if (!self.feature) return;
    $.get("/Ai/History", { feature: self.feature }, function (list) {
        self.past = list || [];
        self.$historyBtn.toggle(self.past.length > 0);
    });
};

// šipky listují historií jen v prázdném poli nebo když uživatel už historií prochází
AiChat.prototype.canBrowse = function (key) {
    var v = this.$prompt.val();
    return v === "" || this.pastIndex >= 0;
};

AiChat.prototype.browse = function (dir) {
    var i = this.pastIndex + dir;
    if (i < 0) { this.pastIndex = -1; this.$prompt.val(""); return; }
    if (i >= this.past.length) i = this.past.length - 1;
    this.pastIndex = i;
    this.$prompt.val(this.past[i]);
};

AiChat.prototype.toggleHistory = function () {
    var self = this;
    if (self.$history.is(":visible")) { self.$history.hide(); return; }
    self.$history.empty().append($("<div>").addClass("ai-history-title").text(self.txt.history || ""));
    self.past.forEach(function (q) {
        self.$history.append($("<div>").addClass("ai-history-item").attr("title", q).text(q).on("click", function (e) {
            e.stopPropagation();
            self.$prompt.val(q).focus();
            self.$history.hide();
        }));
    });
    self.$history.show();
};

AiChat.prototype.remember = function (text) {
    this.past = [text].concat(this.past.filter(function (q) { return q !== text; })).slice(0, 30);
    this.pastIndex = -1;
    this.$historyBtn.show();
};

AiChat.prototype.addMessage = function (cls, text) {
    var $m = $("<div>").addClass("ai-msg " + cls).text(text);
    this.$chat.append($m);
    this.$chat.scrollTop(this.$chat[0].scrollHeight);
    return $m;
};

AiChat.prototype.send = function (textOverride) {
    var self = this;
    var text = $.trim(textOverride || self.$prompt.val());
    if (!text) return;
    self.$prompt.val("");
    self.addMessage("ai-user", text);
    self.history.push({ role: "user", content: text });

    var $wait = self.addMessage("ai-assistant ai-wait", self.txt.thinking);
    self.$send.prop("disabled", true);

    var payload = $.extend({ messages: self.history, conversation: self.conversation, page: self.opt.page },
        self.opt.extra ? self.opt.extra() : {});

    $.post(self.opt.askUrl, { payload: JSON.stringify(payload) }, function (r) {
        $wait.remove();
        self.$send.prop("disabled", false);
        if (!r.ok) {
            self.addMessage("ai-error", self.txt.error + ": " + (r.error || ""));
            self.history.pop();      // neúspěšný dotaz do historie nepatří
            return;
        }
        self.remember(text);
        var $msg = self.addMessage("ai-assistant", r.reply || "");
        if (r.logId) { self.lastLogId = r.logId; self.addRating($msg, r.logId); }
        self.history.push({ role: "assistant", content: r.raw || r.reply || "" });
        if (self.opt.onAnswer) self.opt.onAnswer(r);
    }).fail(function () {
        $wait.remove();
        self.$send.prop("disabled", false);
        self.addMessage("ai-error", self.txt.error);
        self.history.pop();
    });
};

// hodnocení odpovědi 👍 / 👎 (u 👎 volitelný komentář)
AiChat.prototype.addRating = function ($msg, logId) {
    var self = this;
    var $r = $("<div>").addClass("ai-rating");
    var $up = $("<span>").addClass("ai-thumb").attr("title", self.txt.rateUp).text("👍");
    var $down = $("<span>").addClass("ai-thumb").attr("title", self.txt.rateDown).text("👎");
    var $note = $("<div>").addClass("ai-rating-note").hide()
        .append($("<input>").addClass("form-control form-control-sm").attr("placeholder", self.txt.rateComment))
        .append($("<button>").attr("type", "button").addClass("btn btn-sm btn-outline-secondary").text(self.txt.send).on("click", function () {
            self.sendRating(logId, -1, $note.find("input").val(), $r);
        }));
    $up.on("click", function () { self.sendRating(logId, 1, null, $r); });
    $down.on("click", function () { $down.addClass("ai-thumb-on"); $up.removeClass("ai-thumb-on"); $note.show(); $note.find("input").focus(); });
    $r.append($up).append($down).append($note);
    $msg.append($r);
};

AiChat.prototype.sendRating = function (logId, rating, comment, $r) {
    var self = this;
    $.post("/Ai/Rate", { logId: logId, rating: rating, comment: comment || "" }, function () {
        $r.empty().append($("<span>").addClass("ai-rated").text((rating === 1 ? "👍 " : "👎 ") + self.txt.thanks));
    });
};