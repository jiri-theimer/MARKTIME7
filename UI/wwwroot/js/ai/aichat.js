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

    self.$send.on("click", function () { self.send(); });
    self.$prompt.on("keydown", function (e) { if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); self.send(); } });
    self.$prompt.focus();
}

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
