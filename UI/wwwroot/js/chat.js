// Interní chat – okno /Chat/Index. Zápisy jdou přes ChatController, živé události přes SignalR hub /chathub.
var ChatApp = (function () {
    var cfg = null, T = {};
    var channels = [];          //seznam konverzací
    var cur = null;             //otevřená konverzace
    var msgs = [];              //zprávy otevřené konverzace (vzestupně)
    var hasMore = false;
    var replyTo = null;         //zpráva, na kterou odpovídám
    var editing = null;         //zpráva, kterou upravuji
    var pending = [];           //soubory čekající na odeslání
    var sending = false;
    var users = null;           //uživatelé pro dialog
    var dlgMode = null;
    var roles = [];             //obsazené role v záznamu (dialog přidání členů do konverzace k záznamu)
    var reloadTimer = null;
    var hub = null;

    var filter = "all";         //filtr seznamu konverzací: all, unread, people nebo prefix entity (p41, p56...)
    var ENTITY_ICONS = { p41: "work_outline", p56: "task_alt", p28: "business", p91: "receipt_long", o23: "description" };
    var REACTIONS = [["like", "👍"], ["heart", "❤️"], ["laugh", "😂"], ["wow", "😮"], ["sad", "😢"], ["thanks", "🙏"], ["done", "✅"]];
    var MAX_FILES = 5, MAX_BYTES = 20 * 1024 * 1024;

    function url(s) { return (typeof _ep === "function") ? _ep(s) : s; }
    function esc(s) { return $("<div>").text(s == null ? "" : s).html(); }
    function notify(s) { if (typeof _notify_message === "function") { _notify_message(esc(s || T.error)); } }
    function post(path, data) { return $.ajax({ url: url(path), type: "POST", data: data, dataType: "json" }); }
    function get(path, data) { return $.ajax({ url: url(path), type: "GET", data: data, dataType: "json", cache: false }); }

    function initials(name) {
        var a = $.trim(name || "?").split(/\s+/);
        return ((a[0] || "?").charAt(0) + (a.length > 1 ? a[a.length - 1].charAt(0) : "")).toUpperCase();
    }
    function pd(s) { return s ? new Date(s) : null; }
    function p2(n) { return (n < 10 ? "0" : "") + n; }
    function sameDay(a, b) { return a.getFullYear() == b.getFullYear() && a.getMonth() == b.getMonth() && a.getDate() == b.getDate(); }
    function fmtTime(d) { return d.getHours() + ":" + p2(d.getMinutes()); }
    function fmtDay(d) {
        var now = new Date(), y = new Date(); y.setDate(now.getDate() - 1);
        if (sameDay(d, now)) return T.today;
        if (sameDay(d, y)) return T.yesterday;
        return d.getDate() + ". " + (d.getMonth() + 1) + ". " + d.getFullYear();
    }
    function fmtShort(d) {
        if (!d) return "";
        if (sameDay(d, new Date())) return fmtTime(d);
        return d.getDate() + ". " + (d.getMonth() + 1) + ".";
    }
    function fmtSize(n) {
        if (n >= 1048576) return (n / 1048576).toFixed(1).replace(".", ",") + " MB";
        return Math.max(1, Math.round(n / 1024)) + " kB";
    }
    function linkify(text) {
        //text je už escapovaný; odkazem se stávají jen adresy http(s)
        return text.replace(/\bhttps?:\/\/[^\s<]+[^\s<.,;:!?)\]]/g, function (u) {
            return "<a href=\"" + u + "\" target=\"_blank\" rel=\"noopener noreferrer\">" + u + "</a>";
        });
    }
    // je okno chatu zobrazené? (modální okno aplikace se při zavření jen skrývá, stránka chatu v něm běží dál)
    function frameShown() {
        try { if (window.frameElement && window.frameElement.getClientRects().length === 0) return false; } catch (e) { }
        return true;
    }
    // dívá se uživatel právě na chat? Záložka na pozadí nebo zakryté okno prohlížeče se počítá jako "nedívá"
    function isVisible() {
        return !document.hidden && frameShown();
    }

    // ── zvuk nové zprávy ────────────────────────────────────────────────────────
    // krátké dvoutónové pípnutí přes Web Audio (bez zvukového souboru); nastavení je společné s horní lištou

    var audioCtx = null;
    function soundOn() { try { return window.localStorage.getItem("chat_sound") !== "0"; } catch (e) { return true; } }
    function beep() {
        if (!soundOn()) return;
        try {
            var AC = window.AudioContext || window.webkitAudioContext;
            if (!AC) return;
            if (!audioCtx) audioCtx = new AC();
            if (audioCtx.state === "suspended") audioCtx.resume();     //prohlížeč povolí zvuk až po prvním kliknutí uživatele
            var t = audioCtx.currentTime;
            $.each([[880, 0], [1175, 0.13]], function (i, n) {
                var o = audioCtx.createOscillator(), g = audioCtx.createGain();
                o.type = "sine"; o.frequency.value = n[0];
                g.gain.setValueAtTime(0.0001, t + n[1]);
                g.gain.exponentialRampToValueAtTime(0.16, t + n[1] + 0.02);
                g.gain.exponentialRampToValueAtTime(0.0001, t + n[1] + 0.22);
                o.connect(g); g.connect(audioCtx.destination);
                o.start(t + n[1]); o.stop(t + n[1] + 0.24);
            });
        } catch (e) { }
    }
    // prohlížeč dovolí zvuk až po zásahu uživatele: při prvním kliknutí nebo stisku klávesy si zvukový výstup odemknout
    function unlockAudio() {
        try {
            var AC = window.AudioContext || window.webkitAudioContext;
            if (!AC) return;
            if (!audioCtx) audioCtx = new AC();
            if (audioCtx.state === "suspended") audioCtx.resume();
        } catch (e) { }
    }
    function renderSound() {
        var on = soundOn();
        $("#chSound").toggleClass("on", on).attr("title", on ? T.soundon : T.soundoff).find(".ch-ico").text(on ? "volume_up" : "volume_off");
    }
    function isAdmin() { return !!cur && cur.type != 1 && (cur.role == 2 || cur.ownerid == cfg.me); }

    // ── seznam konverzací ───────────────────────────────────────────────────────

    function loadChannels(done) {
        get("/Chat/Channels").done(function (data) {
            if (!data.ok) return;
            channels = data.channels || [];
            if (cur) {
                var c = find(cur.id);
                if (!c) { closeThread(); } else { cur = $.extend(cur, c); renderHead(); renderSeen(); }
            }
            renderChannels();
            badge();
            if (done) done();
        });
    }
    function queueReload() {
        if (reloadTimer) window.clearTimeout(reloadTimer);
        reloadTimer = window.setTimeout(function () { loadChannels(); }, 300);
    }
    function find(id) {
        for (var i = 0; i < channels.length; i++) { if (channels[i].id == id) return channels[i]; }
        return null;
    }
    function badge() {
        var n = 0;
        $.each(channels, function (i, c) { if (!c.muted) n += c.unread; });
        try { if (window.parent && window.parent !== window && typeof window.parent._chat_badge === "function") window.parent._chat_badge(n); } catch (e) { }
    }
    function lastLine(c) {
        if (c.lastdeleted) return T.deleted;
        var s = c.lasttext || (c.lastfiles > 0 ? "📎 " + T.file : "");
        if (!s) return "";
        if (c.lastsenderid == cfg.me) return T.you + ": " + s;
        if (c.type != 1 && c.lastsender) return c.lastsender.split(" ")[0] + ": " + s;
        return s;
    }
    function inFilter(c, f) {
        if (f == "all") return true;
        if (f == "unread") return c.unread > 0;
        if (f == "people") return c.type != 3;
        return c.type == 3 && c.entity == f;
    }
    // filtr se nabízí jen pro druhy konverzací, které uživatel opravdu má; dokud má jen pár konverzací, lišta se neukazuje
    function renderFilter() {
        var keys = ["all", "unread", "people", "p41", "p56", "p28", "p91", "o23"], html = "", kinds = 0;
        $.each(keys, function (i, k) {
            var n = $.grep(channels, function (c) { return inFilter(c, k); }).length;
            if (k != "all" && n == 0 && k != filter) return;
            if (k != "all" && k != "unread") kinds++;
            html += "<button type='button' class='ch-fil" + (filter == k ? " on" : "") + "' data-fil='" + k + "'>" + esc(T["f_" + k] || k) + (k == "all" ? "" : "<small>" + n + "</small>") + "</button>";
        });
        if (kinds < 2 && channels.length < 8 && filter == "all") html = "";
        $("#chFilter").html(html);
    }
    function avatarHtml(c) {
        if (c.type == 3) return "<i class='ch-ico'>" + (ENTITY_ICONS[c.entity] || "link") + "</i>";
        return esc(initials(c.name));
    }
    function renderChannels() {
        var q = $.trim($("#chSearch").val()).toLowerCase(), html = "";
        renderFilter();
        $.each(channels, function (i, c) {
            if (!inFilter(c, filter)) return;
            if (q && c.name.toLowerCase().indexOf(q) == -1) return;
            html += "<div class='ch-item" + (cur && cur.id == c.id ? " sel" : "") + (c.unread > 0 ? " unread" : "") + "' data-id='" + c.id + "' tabindex='0' role='button'>";
            html += "<div class='ch-ava" + (c.type != 1 ? " grp" : "") + "'>" + avatarHtml(c) + "</div>";
            html += "<div class='ch-item-name'>" + esc(c.name) + "</div>";
            html += "<div class='ch-item-time'>" + fmtShort(pd(c.lastdate)) + "</div>";
            html += "<div class='ch-item-last'>" + esc(lastLine(c)) + "</div>";
            if (c.unread > 0) {
                html += "<div class='ch-badge" + (c.muted ? " muted" : "") + "'>" + (c.unread > 99 ? "99+" : c.unread) + "</div>";
            } else {
                html += "<div class='ch-item-flags'>" + (c.pinned ? "<i class='ch-ico' style='font-size:14px;'>push_pin</i>" : "") + (c.muted ? "<i class='ch-ico' style='font-size:14px;'>notifications_off</i>" : "") + "</div>";
            }
            html += "</div>";
        });
        $("#chList").html(html || "<div class='ch-empty'>" + esc(channels.length ? T.emptyfilter : T.empty) + "</div>");
    }

    // ── vlákno ──────────────────────────────────────────────────────────────────

    function openChannel(id) {
        get("/Chat/Messages", { u01id: id, before: 0 }).done(function (data) {
            if (!data.ok) { notify(data.message); loadChannels(); return; }
            cur = data.channel;
            cur.memberlist = data.members || [];
            msgs = data.messages || [];
            hasMore = data.hasmore;
            clearContext(); pending = []; renderPending();
            $("#chInput").val(""); autosize();
            $("#chWelcome").hide();
            $("#chThread").css("display", "flex");
            $("#chat").addClass("thread");
            renderHead(); renderMessages(true); renderChannels();
            markRead();
            if (window.matchMedia("(min-width: 701px)").matches) $("#chInput").trigger("focus");
        }).fail(function () { notify(); });
    }
    function closeThread() {
        cur = null; msgs = [];
        $("#chThread").hide(); $("#chWelcome").show();
        $("#chat").removeClass("thread");
        renderChannels();
    }
    function renderHead() {
        if (!cur) return;
        $("#chHeadName").text(cur.name);
        $("#chHeadAva").attr("class", "ch-ava" + (cur.type != 1 ? " grp" : "")).html(avatarHtml(cur));
        var sub = T.direct;
        if (cur.type != 1) {
            var names = $.map(cur.memberlist || [], function (m) { return m.name; });
            sub = (cur.type == 3 ? T.record + " · " : "") + cur.members + " " + T.members + (names.length ? ": " + names.join(", ") : "");
        }
        $("#chHeadSub").text(sub).attr("title", sub);
        $("#chLeave, #chMembers").toggle(cur.type != 1);
        $("#chDelete").toggle(isAdmin());
        $("#chOpenRec").toggle(cur.type == 3 && !!cur.entity && cur.recpid > 0);
        $("#chAdd").attr("title", cur.type == 1 ? T.addtodirect : T.addmembers);
        $("#chRename").toggle(cur.type == 2);
        $("#chMute").toggleClass("on", cur.muted).attr("title", cur.muted ? T.unmute : T.mute).find(".ch-ico").text(cur.muted ? "notifications_off" : "notifications");
        $("#chPin").toggleClass("on", cur.pinned).attr("title", cur.pinned ? T.unpin : T.pin);
    }

    function msgHtml(m, prev) {
        var d = pd(m.date), pdte = prev ? pd(prev.date) : null, html = "";
        if (!pdte || !sameDay(d, pdte)) html += "<div class='ch-day'><span>" + esc(fmtDay(d)) + "</span></div>";
        if (m.type == 2) return html + "<div class='ch-sys' data-id='" + m.id + "'>" + esc(m.text) + "</div>";

        var mine = m.senderid == cfg.me;
        var first = !prev || prev.type == 2 || prev.senderid != m.senderid || !pdte || !sameDay(d, pdte) || (d - pdte) > 5 * 60000;
        html += "<div class='ch-msg" + (mine ? " mine" : "") + (first ? " first" : "") + "' data-id='" + m.id + "'>";
        html += "<div class='ch-ava'>" + esc(initials(m.sender)) + "</div><div class='ch-body'>";
        if (first) html += "<div class='ch-who'>" + (mine || cur.type == 1 ? "" : esc(m.sender) + " · ") + fmtTime(d) + "</div>";

        if (m.deleted) {
            html += "<div class='ch-bubble del' title='" + fmtTime(d) + "'>" + esc(T.deleted) + "</div>";
        } else {
            if (m.text || m.replyto) {
                html += "<div class='ch-bubble' title='" + fmtTime(d) + "'>";
                if (m.replyto) html += "<div class='ch-quote' data-goto='" + m.replyto + "'><b>" + esc(m.replysender || "") + "</b> " + esc(m.replytext || T.deleted) + "</div>";
                html += linkify(esc(m.text || ""));
                if (m.edited) html += "<span class='ch-edited'>(" + esc(T.edited) + ")</span>";
                html += "</div>";
            }
            if (m.files && m.files.length) {
                html += "<div class='ch-files'>";
                $.each(m.files, function (i, f) {
                    var u = url("/Chat/Attachment") + "?id=" + f.id;
                    if (f.image) {
                        html += "<a class='ch-img' href='" + u + "' target='_blank' rel='noopener' title='" + esc(f.name) + "'><img src='" + u + "' alt='" + esc(f.name) + "' loading='lazy' /></a>";
                    } else {
                        html += "<a class='ch-file' href='" + u + "&download=true' title='" + esc(f.name) + "'><i class='ch-ico'>description</i><span class='n'>" + esc(f.name) + "</span><small>" + fmtSize(f.size) + "</small></a>";
                    }
                });
                html += "</div>";
            }
            if (m.reactions && m.reactions.length) {
                html += "<div class='ch-reacts'>";
                $.each(REACTIONS, function (i, r) {
                    var who = $.grep(m.reactions, function (x) { return x.key == r[0]; });
                    if (!who.length) return;
                    var me = $.grep(who, function (x) { return x.j02id == cfg.me; }).length > 0;
                    html += "<button type='button' class='ch-react" + (me ? " me" : "") + "' data-react='" + r[0] + "' title='" + esc($.map(who, function (x) { return x.name; }).join(", ")) + "'>" + r[1] + " " + who.length + "</button>";
                });
                html += "</div>";
            }
            html += "<div class='ch-tools'><div class='ch-pick'>";
            $.each(REACTIONS, function (i, r) { html += "<button type='button' data-react='" + r[0] + "'>" + r[1] + "</button>"; });
            html += "</div>";
            html += "<button type='button' class='ch-btn' data-act='react' title='" + esc(T.react) + "'><i class='ch-ico'>add_reaction</i></button>";
            html += "<button type='button' class='ch-btn' data-act='reply' title='" + esc(T.reply) + "'><i class='ch-ico'>reply</i></button>";
            if (cur.type == 3 && (m.text || (m.files && m.files.length))) html += "<button type='button' class='ch-btn' data-act='note' title='" + esc(T.note) + "'><i class='ch-ico'>waves</i></button>";
            if (mine) {
                html += "<button type='button' class='ch-btn' data-act='edit' title='" + esc(T.edit) + "'><i class='ch-ico'>edit</i></button>";
                html += "<button type='button' class='ch-btn' data-act='delete' title='" + esc(T["delete"]) + "'><i class='ch-ico'>delete</i></button>";
            }
            html += "</div>";
        }
        return html + "</div></div>";
    }

    function renderMessages(toBottom) {
        var box = $("#chMsgs"), el = box[0];
        var fromBottom = el.scrollHeight - el.scrollTop;
        var html = hasMore ? "<button type='button' class='ch-more' id='chMore'>" + esc(T.older) + "</button>" : "";
        if (!msgs.length) html += "<div class='ch-empty'>" + esc(T.nomsg) + "</div>";
        for (var i = 0; i < msgs.length; i++) html += msgHtml(msgs[i], i > 0 ? msgs[i - 1] : null);
        html += "<div class='ch-seen' id='chSeen'></div>";
        box.html(html);
        renderSeen();
        if (toBottom) { scrollBottom(); } else { el.scrollTop = el.scrollHeight - fromBottom; }
        box.find("img").on("load", function () { if (toBottom && nearBottom(260)) scrollBottom(); });
    }
    function renderSeen() {
        //přímá konverzace: druhá strana přečetla moji poslední zprávu
        var s = "";
        if (cur && cur.type == 1 && msgs.length) {
            var last = msgs[msgs.length - 1];
            if (last.senderid == cfg.me && !last.deleted && cur.partnerread >= last.id) s = T.seen;
        }
        $("#chSeen").text(s);
    }
    function nearBottom(px) { var el = $("#chMsgs")[0]; return el.scrollHeight - el.scrollTop - el.clientHeight < (px || 120); }
    function scrollBottom() { var el = $("#chMsgs")[0]; el.scrollTop = el.scrollHeight; }
    function msgById(id) { for (var i = 0; i < msgs.length; i++) { if (msgs[i].id == id) return msgs[i]; } return null; }

    function addMessage(m) {
        if (!cur || m.u01id != cur.id || msgById(m.id)) return;
        var stick = nearBottom() || m.senderid == cfg.me;
        msgs.push(m);
        renderMessages(stick);
    }
    function updateMessage(m) {
        if (!cur || m.u01id != cur.id) return;
        for (var i = 0; i < msgs.length; i++) {
            if (msgs[i].id == m.id) { msgs[i] = m; renderMessages(false); return; }
        }
    }
    function loadOlder() {
        if (!cur || !msgs.length) return;
        var id = cur.id;
        get("/Chat/Messages", { u01id: id, before: msgs[0].id }).done(function (data) {
            if (!data.ok || !cur || cur.id != id) return;
            msgs = (data.messages || []).concat(msgs);
            hasMore = data.hasmore;
            renderMessages(false);
        });
    }
    function markRead() {
        if (!cur || !msgs.length || !isVisible()) return;
        var last = msgs[msgs.length - 1].id, c = find(cur.id);
        if (c && c.unread == 0 && cur.lastread >= last) return;
        cur.lastread = last;
        if (c) { c.unread = 0; c.lastread = last; renderChannels(); badge(); }
        post("/Chat/Read", { u01id: cur.id, u02id: last });
    }

    // ── psaní ───────────────────────────────────────────────────────────────────

    function autosize() {
        var el = $("#chInput")[0];
        el.style.height = "auto";
        el.style.height = Math.min(el.scrollHeight + 2, 140) + "px";
    }
    function clearContext() {
        replyTo = null; editing = null;
        $("#chCtx").removeClass("on");
        $("#chAttach").prop("disabled", false);
    }
    function setReply(m) {
        editing = null; replyTo = m;
        $("#chCtxText").html("<b>" + esc(T.replyto + " " + (m.sender || "")) + ":</b> " + esc(m.text || T.file));
        $("#chCtx").addClass("on");
        $("#chInput").trigger("focus");
    }
    function setEdit(m) {
        replyTo = null; editing = m; pending = []; renderPending();
        $("#chCtxText").html("<b>" + esc(T.editing) + "</b>");
        $("#chCtx").addClass("on");
        $("#chAttach").prop("disabled", true);
        $("#chInput").val(m.text || "").trigger("focus"); autosize();
    }
    function addFiles(list) {
        if (editing) return;
        for (var i = 0; i < list.length; i++) {
            if (pending.length >= MAX_FILES) { notify(T.toomany); break; }
            if (list[i].size > MAX_BYTES) { notify(T.toobig + ": " + list[i].name); continue; }
            pending.push(list[i]);
        }
        renderPending();
    }
    function renderPending() {
        var html = "";
        $.each(pending, function (i, f) {
            html += "<div class='ch-chip'><span>" + esc(f.name || "obrázek") + "</span><button type='button' class='ch-btn' data-rm='" + i + "'><i class='ch-ico'>close</i></button></div>";
        });
        $("#chPending").html(html);
    }

    function send() {
        if (!cur || sending) return;
        var text = $.trim($("#chInput").val());

        if (editing) {
            var id = editing.id;
            sending = true;
            post("/Chat/Edit", { u02id: id, text: text }).done(function (data) {
                if (!data.ok) { notify(data.message); return; }
                updateMessage(data.message);
                clearContext(); $("#chInput").val(""); autosize();
            }).fail(function () { notify(); }).always(function () { sending = false; });
            return;
        }

        if (!text && !pending.length) return;
        var fd = new FormData();
        fd.append("u01id", cur.id);
        fd.append("text", text);
        fd.append("replyto", replyTo ? replyTo.id : 0);
        $.each(pending, function (i, f) { fd.append("files", f, f.name || ("obrazek-" + (i + 1) + ".png")); });

        sending = true;
        $("#chSend").prop("disabled", true);
        $.ajax({ url: url("/Chat/Send"), type: "POST", data: fd, processData: false, contentType: false, dataType: "json" }).done(function (data) {
            if (!data.ok) { notify(data.message); return; }
            $("#chInput").val(""); autosize();
            pending = []; renderPending(); clearContext();
            addMessage(data.message);
            if (data.rejected && data.rejected.length) notify(T.rejected + ": " + data.rejected.join(", "));
            queueReload();
        }).fail(function () { notify(); }).always(function () {
            sending = false;
            $("#chSend").prop("disabled", false);
            $("#chInput").trigger("focus");
        });
    }

    // ── dialog: nová konverzace / přidání členů ─────────────────────────────────

    // režimy: new = nová konverzace, add = přidat členy do skupiny, grow = z přímé konverzace vytvořit skupinu, members = přehled členů
    function openDialog(mode) {
        dlgMode = mode;
        $("#chDlgTitle").text(mode == "add" ? T.addmembers : (mode == "grow" ? T.addtodirect : (mode == "members" ? T.membersdlg : T.newchat)));
        $("#chDlgOk").text(mode == "add" ? T.add : (mode == "grow" ? T.create : T.start)).toggle(mode != "members");
        $("#chDlgCancel").text(mode == "members" ? T.close : $("#chDlgCancel").data("label"));
        $("#chDlgSearch, #chDlgName").val(""); $("#chDlgErr").text(""); $("#chDlgName").hide();
        $("#chDlgSearch").toggle(mode != "members");
        $("#chDlg").addClass("on");
        $("#chDlgUsers").html("");
        roles = []; renderRoles();
        if (mode == "members") { renderMembers(); return; }
        if (mode == "add" && cur && cur.type == 3) {
            get("/Chat/Roles", { u01id: cur.id }).done(function (data) {
                if (dlgMode != "add" || !data.ok) return;
                roles = data.roles || [];
                renderRoles();
            });
        }
        get("/Chat/Users").done(function (data) {
            users = data.users || [];
            $.each(users, function (i, u) { u.sel = false; });
            renderUsers();
            $("#chDlgSearch").trigger("focus");
        });
    }
    function closeDialog() { $("#chDlg").removeClass("on"); dlgMode = null; }
    function renderUsers() {
        var q = $.trim($("#chDlgSearch").val()).toLowerCase(), html = "";
        var skip = ((dlgMode == "add" || dlgMode == "grow") && cur) ? $.map(cur.memberlist || [], function (m) { return m.id; }) : [];
        $.each(users || [], function (i, u) {
            if ($.inArray(u.id, skip) > -1) return;
            if (q && (u.name + " " + (u.email || "")).toLowerCase().indexOf(q) == -1) return;
            html += "<label class='ch-user'><input type='checkbox' data-uid='" + u.id + "'" + (u.sel ? " checked" : "") + " /><span>" + esc(u.name) + "</span><small>" + esc(u.email || "") + "</small></label>";
        });
        $("#chDlgUsers").html(html || "<div class='ch-empty'>" + esc(T.nousers) + "</div>");
        toggleName();
    }
    // role v záznamu: klepnutím se označí (nebo odznačí) všichni, kdo roli zastávají
    function roleState(r) {
        var n = 0;
        $.each(users || [], function (i, u) { if (u.sel && $.inArray(u.id, r.ids) > -1) n++; });
        return n == r.ids.length && n > 0;
    }
    function renderRoles() {
        var html = "";
        $.each(roles, function (i, r) {
            html += "<button type='button' class='ch-role" + (roleState(r) ? " on" : "") + "' data-role='" + i + "'>" + esc(r.name) + "<small>" + r.ids.length + "</small></button>";
        });
        $("#chDlgRoles").html(html ? "<span>" + esc(T.roles) + ":</span>" + html : "");
    }
    function toggleName() {
        var n = $.grep(users || [], function (u) { return u.sel; }).length;
        $("#chDlgName").toggle((dlgMode == "new" && n > 1) || (dlgMode == "grow" && n > 0));
    }
    function renderMembers() {
        var html = "", admin = isAdmin();
        $.each((cur && cur.memberlist) || [], function (i, m) {
            html += "<div class='ch-user ro'><span>" + esc(m.name) + "</span>" + (m.role == 2 || m.id == cur.ownerid ? "<small>" + esc(T.admin) + "</small>" : "");
            if (admin && m.id != cfg.me) html += "<button type='button' class='ch-btn' data-kick='" + m.id + "' title='" + esc(T.remove) + "'><i class='ch-ico'>person_remove</i></button>";
            html += "</div>";
        });
        $("#chDlgUsers").html(html);
    }
    function confirmDialog() {
        var ids = $.map($.grep(users || [], function (u) { return u.sel; }), function (u) { return u.id; });
        if (!ids.length) { $("#chDlgErr").text(T.pickuser); return; }
        var req;
        if (dlgMode == "add") {
            req = post("/Chat/AddMembers", { u01id: cur.id, j02ids: ids.join(",") });
        } else if (dlgMode == "grow") {
            //přímou konverzaci nelze rozšířit: vznikne nová skupina z obou účastníků a přidaných lidí
            var gname = $.trim($("#chDlgName").val());
            if (!gname) { $("#chDlgErr").text(T.groupname); $("#chDlgName").trigger("focus"); return; }
            req = post("/Chat/Group", { name: gname, j02ids: [cur.partnerid].concat(ids).join(",") });
        } else if (ids.length == 1) {
            req = post("/Chat/Direct", { j02id: ids[0] });
        } else {
            var name = $.trim($("#chDlgName").val());
            if (!name) { $("#chDlgErr").text(T.groupname); $("#chDlgName").trigger("focus"); return; }
            req = post("/Chat/Group", { name: name, j02ids: ids.join(",") });
        }
        $("#chDlgOk").prop("disabled", true);
        req.done(function (data) {
            if (!data.ok) { $("#chDlgErr").text(data.message || T.error); return; }
            var id = data.u01id || (cur ? cur.id : 0);
            closeDialog();
            loadChannels(function () { if (id) openChannel(id); });
        }).fail(function () { $("#chDlgErr").text(T.error); }).always(function () { $("#chDlgOk").prop("disabled", false); });
    }

    // ── SignalR ─────────────────────────────────────────────────────────────────

    // ── diagnostika (Shift + klik na reproduktor) ───────────────────────────────
    // ověří celou cestu: server -> SignalR -> tento prohlížeč -> zvuk mimo kliknutí uživatele

    var lastEvent = null, pingTimer = null, pingInfo = "";
    function transportName() {
        try {
            var t = hub && hub.connection ? hub.connection.transport : null;
            if (!t) return "žádný";
            for (var k in t) {
                if (window.WebSocket && t[k] instanceof window.WebSocket) return "WebSocket";
                if (window.EventSource && t[k] instanceof window.EventSource) return "Server-Sent Events";
            }
            return "long polling";
        } catch (e) { return "?"; }
    }
    function diagLine() {
        var a = audioCtx ? audioCtx.state : "nevytvořen";
        return "spojení: " + (hub ? hub.state + " (" + transportName() + ")" : "ŽÁDNÉ – chybí knihovna SignalR") + " · zvuk: " + (soundOn() ? "zapnutý" : "VYPNUTÝ") + " · audio: " + a
            + " · poslední událost: " + (lastEvent ? lastEvent : "žádná") + " · já: " + cfg.me;
    }
    function diagShow(s, template) { if (typeof _notify_message === "function") _notify_message(esc(s), template || "info"); else window.alert(s); }
    function diagStart() {
        diagShow("Test upozornění: za 3 s má přijít zkušební událost ze serveru a zaznít zvuk. " + diagLine());
        post("/Chat/Ping").done(function (data) {
            pingInfo = data.same ? "" : " POZOR: jméno v přihlášení (" + data.claim + ") se liší od loginu v databázi (" + data.dblogin + ").";
            if (pingTimer) window.clearTimeout(pingTimer);
            pingTimer = window.setTimeout(function () {
                pingTimer = null;
                diagShow("Test upozornění: událost ze serveru NEDORAZILA – doručování přes SignalR v tomto prohlížeči nefunguje." + pingInfo + " " + diagLine(), "error");
            }, 8000);
        }).fail(function (xhr) { diagShow("Test upozornění: server neodpověděl (" + xhr.status + "). Je nasazený aktuální ChatController?", "error"); });
    }
    function diagPing() {
        if (pingTimer) { window.clearTimeout(pingTimer); pingTimer = null; }
        var before = audioCtx ? audioCtx.state : "nevytvořen";
        beep();
        window.setTimeout(function () {
            diagShow("Test upozornění: událost ze serveru DORAZILA. Zvuk " + (soundOn() ? "byl spuštěn" : "je VYPNUTÝ ikonou reproduktoru") + " (audio před: " + before + ", po: " + (audioCtx ? audioCtx.state : "nevytvořen") + ")." + pingInfo, "success");
        }, 400);
    }

    function onEvent(e) {
        lastEvent = e.type + " " + new Date().toLocaleTimeString() + (e.type == "message" ? " (od " + e.actorid + (e.muted ? ", ztlumeno" : "") + ")" : "");
        if (e.type == "ping") { diagPing(); return; }
        if (e.type == "message" && e.message) {
            //zvuk u každé cizí zprávy - i když je záložka na pozadí nebo okno prohlížeče zakryté (právě tehdy je potřeba nejvíc);
            //pokud je okno chatu v aplikaci zavřené, pípá místo něj horní lišta (layoutshare.js), aby zvuk nezazněl dvakrát
            if (e.actorid != cfg.me && !e.muted && frameShown()) beep();
            if (cur && e.u01id == cur.id) { addMessage(e.message); markRead(); }
        } else if (e.type == "update" && e.message) {
            updateMessage(e.message);
        } else if (e.type == "channel" && cur && e.u01id == cur.id) {
            get("/Chat/Messages", { u01id: cur.id, before: 0 }).done(function (data) {
                if (!data.ok || !cur || data.channel.id != cur.id) return;
                cur = $.extend(cur, data.channel); cur.memberlist = data.members || [];
                msgs = data.messages || []; hasMore = data.hasmore;
                renderHead(); renderMessages(nearBottom()); markRead();
                if (dlgMode == "members") renderMembers();
            });
        }
        queueReload();  //seznam vlevo (pořadí, náhled, nepřečtené, "přečteno") se vždy srovná ze serveru
    }
    function connect() {
        if (typeof signalR === "undefined") {
            //na serveru chybí soubor knihovny: chat by vypadal funkčně, ale nic by nechodilo živě
            $("#chStatus").text("Na serveru chybí knihovna SignalR (wwwroot/lib/signalr/signalr.min.js) – zprávy nechodí živě.").addClass("on");
            return;
        }
        //Server-Sent Events se vynechávají: IIS s dynamickou kompresí je zadržuje a události pak nedorazí včas.
        //Použije se WebSocket, a kde na IIS není zapnutý, long polling.
        var transports = signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling;
        hub = new signalR.HubConnectionBuilder().withUrl(url("/chathub"), { transport: transports }).withAutomaticReconnect([0, 2000, 5000, 10000, 30000, 60000]).build();
        hub.on("chat", onEvent);
        hub.onreconnecting(function () { $("#chStatus").addClass("on"); });
        hub.onreconnected(function () { $("#chStatus").removeClass("on"); resync(); });
        hub.onclose(function () { $("#chStatus").addClass("on"); window.setTimeout(start, 15000); });
        start();
    }
    function start() {
        hub.start().then(function () { $("#chStatus").removeClass("on"); resync(); }).catch(function () {
            $("#chStatus").addClass("on");
            window.setTimeout(start, 15000);
        });
    }
    function resync() {
        //po výpadku spojení dotáhnout, co mezitím přišlo
        loadChannels();
        if (cur) {
            var id = cur.id;
            get("/Chat/Messages", { u01id: id, before: 0 }).done(function (data) {
                if (!data.ok || !cur || cur.id != id) return;
                var known = msgs.length ? msgs[msgs.length - 1].id : 0, bottom = nearBottom();
                $.each(data.messages || [], function (i, m) { if (m.id > known) msgs.push(m); });
                renderMessages(bottom); markRead();
            });
        }
    }

    // ── události ────────────────────────────────────────────────────────────────

    function bind() {
        $("#chSearch").on("input", renderChannels);
        try { filter = window.localStorage.getItem("chat_filter") || "all"; } catch (e) { }
        $("#chFilter").on("click", "[data-fil]", function () {
            filter = $(this).data("fil");
            try { window.localStorage.setItem("chat_filter", filter); } catch (e) { }
            renderChannels();
        });
        $("#chList").on("click", ".ch-item", function () { openChannel($(this).data("id")); })
            .on("keydown", ".ch-item", function (e) { if (e.key == "Enter") openChannel($(this).data("id")); });
        $("#chNew, #chNew2").on("click", function () { openDialog("new"); });
        $("#chBack").on("click", closeThread);

        $("#chAdd").on("click", function () { openDialog(cur.type == 1 ? "grow" : "add"); });
        $("#chMembers").on("click", function () { openDialog("members"); });
        $("#chDlgCancel").data("label", $("#chDlgCancel").text());
        $("#chSound").on("click", function (e) {
            if (e.shiftKey) { diagStart(); return; }
            try { window.localStorage.setItem("chat_sound", soundOn() ? "0" : "1"); } catch (e) { }
            renderSound();
            beep();     //ukázka zvuku při zapnutí
        });
        renderSound();
        $(document).one("pointerdown keydown", unlockAudio);
        $("#chRename").on("click", function () {
            var s = window.prompt(T.rename, cur.name);
            if (s == null || !$.trim(s)) return;
            post("/Chat/Rename", { u01id: cur.id, name: s }).done(function (data) { if (!data.ok) notify(data.message); });
        });
        $("#chMute").on("click", function () {
            var v = !cur.muted;
            post("/Chat/Mute", { u01id: cur.id, value: v }).done(function (data) { if (data.ok) { cur.muted = v; renderHead(); queueReload(); } });
        });
        $("#chPin").on("click", function () {
            var v = !cur.pinned;
            post("/Chat/Pin", { u01id: cur.id, value: v }).done(function (data) { if (data.ok) { cur.pinned = v; renderHead(); queueReload(); } });
        });
        $("#chLeave").on("click", function () {
            if (!window.confirm(T.confirmleave)) return;
            post("/Chat/Leave", { u01id: cur.id }).done(function (data) { if (data.ok) { closeThread(); loadChannels(); } else { notify(data.message); } });
        });

        $("#chOpenRec").on("click", function () {
            //stránka záznamu se otevírá v hlavním okně aplikace, ne uvnitř okna chatu
            var u = url("/Record/RecPage?prefix=" + encodeURIComponent(cur.entity) + "&pid=" + cur.recpid);
            try { window.top.location.href = u; } catch (e) { window.open(u, "_blank"); }
        });
        $("#chDelete").on("click", function () {
            if (!window.confirm(T.confirmdeletechannel + "\n\n" + cur.name)) return;
            post("/Chat/DeleteChannel", { u01id: cur.id }).done(function (data) { if (data.ok) { closeThread(); loadChannels(); } else { notify(data.message); } }).fail(function () { notify(); });
        });

        $("#chMsgs").on("click", "#chMore", loadOlder)
            .on("click", ".ch-msg", function (e) {
                if ($(e.target).closest("a, button, .ch-quote").length) return;
                $("#chMsgs .ch-msg").not(this).removeClass("tools pick");
                $(this).toggleClass("tools");       //dotyková zařízení: nástroje se ukážou klepnutím na zprávu
            })
            .on("click", ".ch-quote", function () {
                var el = $("#chMsgs .ch-msg[data-id='" + $(this).data("goto") + "']");
                if (el.length) el[0].scrollIntoView({ block: "center" });
            })
            .on("click", "[data-react]", function () {
                var row = $(this).closest(".ch-msg");
                row.removeClass("pick tools");
                post("/Chat/React", { u02id: row.data("id"), key: $(this).data("react") }).done(function (data) { if (data.ok) updateMessage(data.message); });
            })
            .on("click", "[data-act]", function () {
                var row = $(this).closest(".ch-msg"), m = msgById(row.data("id")), act = $(this).data("act");
                if (!m) return;
                if (act == "react") { row.addClass("pick"); return; }
                if (act == "reply") { setReply(m); return; }
                if (act == "note") {
                    row.removeClass("tools");
                    post("/Chat/SaveAsNote", { u02id: m.id }).done(function (data) {
                        if (!data.ok) { notify(data.message); return; }
                        if (typeof _notify_message === "function") _notify_message(esc(T.notesaved), "success");
                        if (data.warning) notify(data.warning);
                    }).fail(function () { notify(); });
                    return;
                }
                if (act == "edit") { setEdit(m); return; }
                if (act == "delete" && window.confirm(T.confirmdelete)) {
                    post("/Chat/Delete", { u02id: m.id }).done(function (data) { if (data.ok) { updateMessage(data.message); queueReload(); } else { notify(data.message); } });
                }
            })
            .on("mouseleave", ".ch-msg", function () { $(this).removeClass("pick"); })
            .on("scroll", function () { if (nearBottom()) markRead(); });

        $("#chInput").on("input", autosize).on("keydown", function (e) {
            if (e.key == "Enter" && !e.shiftKey && !e.isComposing) { e.preventDefault(); send(); }
            if (e.key == "Escape" && (replyTo || editing)) { e.stopPropagation(); if (editing) { $(this).val(""); autosize(); } clearContext(); }
        }).on("paste", function (e) {
            var items = (e.originalEvent.clipboardData || {}).files;
            if (items && items.length) { e.preventDefault(); addFiles(items); }
        });
        $("#chSend").on("click", send);
        $("#chCtxClose").on("click", function () { if (editing) { $("#chInput").val(""); autosize(); } clearContext(); });
        $("#chAttach").on("click", function () { $("#chFile").trigger("click"); });
        $("#chFile").on("change", function () { addFiles(this.files); this.value = ""; });
        $("#chPending").on("click", "[data-rm]", function () { pending.splice($(this).data("rm"), 1); renderPending(); });

        var main = $("#chMain"), drag = 0;
        main.on("dragenter", function (e) { if (!cur) return; e.preventDefault(); drag++; main.addClass("drag"); })
            .on("dragover", function (e) { if (cur) e.preventDefault(); })
            .on("dragleave", function () { drag = Math.max(0, drag - 1); if (!drag) main.removeClass("drag"); })
            .on("drop", function (e) {
                drag = 0; main.removeClass("drag");
                if (!cur) return;
                e.preventDefault();
                var f = e.originalEvent.dataTransfer ? e.originalEvent.dataTransfer.files : null;
                if (f && f.length) addFiles(f);
            });

        $("#chDlgSearch").on("input", renderUsers).on("keydown", function (e) { if (e.key == "Enter") e.preventDefault(); });
        $("#chDlgName").on("keydown", function (e) { if (e.key == "Enter") { e.preventDefault(); confirmDialog(); } });
        $("#chDlgUsers").on("change", "input", function () {
            var id = $(this).data("uid"), on = this.checked;
            $.each(users, function (i, u) { if (u.id == id) u.sel = on; });
            $("#chDlgErr").text("");
            toggleName(); renderRoles();
        }).on("click", "[data-kick]", function () {
            if (!window.confirm(T.confirmremove)) return;
            var j02id = $(this).data("kick"), id = cur.id;
            post("/Chat/RemoveMember", { u01id: id, j02id: j02id }).done(function (data) {
                if (!data.ok) { $("#chDlgErr").text(data.message || T.error); return; }
                cur.memberlist = $.grep(cur.memberlist, function (m) { return m.id != j02id; });
                cur.members = cur.memberlist.length;
                renderMembers(); renderHead();
            }).fail(function () { $("#chDlgErr").text(T.error); });
        });
        $("#chDlgRoles").on("click", "[data-role]", function () {
            var r = roles[$(this).data("role")];
            if (!r) return;
            var on = !roleState(r);
            $.each(users || [], function (i, u) { if ($.inArray(u.id, r.ids) > -1) u.sel = on; });
            $("#chDlgErr").text("");
            renderUsers(); renderRoles();
        });
        $("#chDlgOk").on("click", confirmDialog);
        $("#chDlgCancel").on("click", closeDialog);
        $("#chDlg").on("mousedown", function (e) { if (e.target === this) closeDialog(); });
        $(document).on("keydown", function (e) {
            if (e.key == "Escape" && $("#chDlg").hasClass("on")) { e.stopImmediatePropagation(); closeDialog(); }
        });

        $(document).on("visibilitychange", function () { if (isVisible()) markRead(); });
        $(window).on("focus", markRead);
    }

    return {
        init: function (c) {
            cfg = c; T = c.t || {};
            if (!document.getElementById("chat")) return;
            bind();
            loadChannels(function () { if (cfg.initial > 0) openChannel(cfg.initial); });
            connect();
        },
        isVisible: isVisible
    };
})();
