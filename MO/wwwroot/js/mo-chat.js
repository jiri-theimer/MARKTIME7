// Interní chat v MO: seznam konverzací (Chat/Index) a jedna konverzace (Chat/Thread).
// Zápisy jdou přes ChatController (moApi), živé události přes mo-live.js (událost okna 'mo:chat').
(function () {
    var cfgEl = document.getElementById('mocConfig');
    if (!cfgEl || !window.moApi) return;
    var cfg = JSON.parse(cfgEl.textContent || '{}');
    var T = cfg.t || {};

    var REACTIONS = { like: '👍', heart: '❤️', laugh: '😂', wow: '😮', sad: '😢', thanks: '🙏', done: '✅' };
    var ENTITY_ICONS = { p41: 'work_outline', p56: 'task_alt', p28: 'business', p91: 'receipt_long', o23: 'description' };
    var MAX_FILES = 5, MAX_BYTES = 20 * 1024 * 1024;

    // ── pomocné ──────────────────────────────────────────────────────────────
    function esc(s) {
        return String(s == null ? '' : s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
    }
    function p2(n) { return (n < 10 ? '0' : '') + n; }
    function parseDate(s) { return s ? new Date(s) : null; }
    function sameDay(a, b) { return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate(); }
    function fmtTime(d) { return d.getHours() + ':' + p2(d.getMinutes()); }
    function fmtDay(d) {
        var now = new Date(), y = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 1);
        if (sameDay(d, now)) return T.today;
        if (sameDay(d, y)) return T.yesterday;
        return d.getDate() + '. ' + (d.getMonth() + 1) + '. ' + d.getFullYear();
    }
    function fmtShort(d) {
        var now = new Date();
        if (sameDay(d, now)) return fmtTime(d);
        return d.getDate() + '. ' + (d.getMonth() + 1) + '.';
    }
    function fmtSize(n) {
        if (n > 1024 * 1024) return (n / 1024 / 1024).toFixed(1) + ' MB';
        return Math.max(1, Math.round(n / 1024)) + ' kB';
    }
    // text je už escapovaný; z adres udělat odkazy
    function linkify(s) {
        return s.replace(/(https?:\/\/[^\s<]+[^\s<.,;:!?)\]])/g, function (u) {
            return '<a href="' + u + '" target="_blank" rel="noopener noreferrer">' + u + '</a>';
        });
    }
    function initials(name) {
        var a = String(name || '?').trim().split(/\s+/);
        return ((a[0] || '?').charAt(0) + (a.length > 1 ? a[a.length - 1].charAt(0) : '')).toUpperCase();
    }
    function avatar(c) {
        if (c.type == 3) return '<div class="moc-ava grp"><span class="material-icons-outlined">' + (ENTITY_ICONS[c.entity] || 'link') + '</span></div>';
        if (c.type == 2) return '<div class="moc-ava grp"><span class="material-icons-outlined">' + (c.name === cfg.notifyName ? 'notifications' : 'groups') + '</span></div>';
        return '<div class="moc-ava">' + esc(initials(c.name)) + '</div>';
    }
    function toast(s) {
        var el = document.getElementById('mocStatus');
        if (!el) { return; }
        el.textContent = s || T.error;
        el.hidden = false;
        window.clearTimeout(toast._t);
        toast._t = window.setTimeout(function () { el.hidden = true; }, 5000);
    }

    // ══ seznam konverzací ════════════════════════════════════════════════════
    function initList() {
        var listEl = document.getElementById('mocList');
        var searchEl = document.getElementById('mocSearch');
        var channels = [], loading = false, again = false;

        function lastLine(c) {
            if (c.lastdeleted) return T.deleted;
            var s = c.lasttext || '';
            if (!s && c.lastfiles > 0) s = T.attachment;
            if (!s) return '';
            if (c.type != 1 && c.lastsender) return c.lastsender.split(' ')[0] + ': ' + s;
            return s;
        }

        function render() {
            var q = (searchEl.value || '').trim().toLowerCase();
            var html = '';
            channels.forEach(function (c) {
                if (q && String(c.name || '').toLowerCase().indexOf(q) < 0) return;
                var d = parseDate(c.lastdate);
                html += '<a class="moc-row' + (c.unread > 0 ? ' unread' : '') + '" href="' + moApi.url('/Chat/Thread/' + c.id) + '">'
                    + avatar(c)
                    + '<div class="moc-mid"><div class="moc-name">' + esc(c.name) + '</div><div class="moc-last">' + esc(lastLine(c)) + '</div></div>'
                    + '<div class="moc-right"><div class="moc-when">' + (d ? fmtShort(d) : '') + '</div>'
                    + (c.unread > 0 ? '<div class="moc-count">' + (c.unread > 99 ? '99+' : c.unread) + '</div>'
                        : (c.pinned ? '<span class="material-icons-outlined moc-flag">push_pin</span>' : (c.muted ? '<span class="material-icons-outlined moc-flag">notifications_off</span>' : '')))
                    + '</div></a>';
            });
            listEl.innerHTML = html || '<div class="moc-empty">' + esc(q ? T.nothing : T.empty) + '</div>';
        }

        function load() {
            if (loading) { again = true; return; }
            loading = true;
            moApi.get('/Chat/Channels').then(function (d) {
                if (d && d.ok) { channels = d.channels || []; render(); }
                else if (d && d.message) { listEl.innerHTML = '<div class="moc-empty">' + esc(d.message) + '</div>'; }
            }).catch(function () {
                if (!channels.length) listEl.innerHTML = '<div class="moc-empty">' + esc(T.error) + '</div>';
            }).then(function () {
                loading = false;
                if (again) { again = false; load(); }
            });
        }

        searchEl.addEventListener('input', render);
        window.addEventListener('mo:chat', load);
        document.addEventListener('visibilitychange', function () { if (document.visibilityState === 'visible') load(); });
        window.setInterval(function () {
            if (document.visibilityState === 'visible' && !(window.moLive && moLive.connected())) load();
        }, 20000);

        // nová přímá konverzace
        var dlg = document.getElementById('mocUsers');
        var usersEl = document.getElementById('mocUsersList');
        var usersSearch = document.getElementById('mocUsersSearch');
        var users = null;

        function renderUsers() {
            var q = (usersSearch.value || '').trim().toLowerCase();
            var html = '';
            (users || []).forEach(function (u) {
                if (u.id == cfg.me) return;
                if (q && (String(u.name || '') + ' ' + String(u.email || '')).toLowerCase().indexOf(q) < 0) return;
                html += '<a class="moc-row" href="#" data-id="' + u.id + '"><div class="moc-ava">' + esc(initials(u.name)) + '</div>'
                    + '<div class="moc-mid"><div class="moc-name">' + esc(u.name) + '</div><div class="moc-last">' + esc(u.email || '') + '</div></div></a>';
            });
            usersEl.innerHTML = html || '<div class="moc-empty">' + esc(T.nothing) + '</div>';
        }

        document.getElementById('mocNew').addEventListener('click', function () {
            usersSearch.value = '';
            if (typeof dlg.showModal === 'function') dlg.showModal(); else dlg.setAttribute('open', 'open');
            if (users) { renderUsers(); return; }
            usersEl.innerHTML = '<div class="moc-empty">…</div>';
            moApi.get('/Chat/Users').then(function (d) {
                users = (d && d.users) || [];
                renderUsers();
            }).catch(function () { usersEl.innerHTML = '<div class="moc-empty">' + esc(T.error) + '</div>'; });
        });
        document.getElementById('mocUsersClose').addEventListener('click', function () { dlg.close(); });
        usersSearch.addEventListener('input', renderUsers);
        usersEl.addEventListener('click', function (ev) {
            var a = ev.target.closest('a[data-id]');
            if (!a) return;
            ev.preventDefault();
            moApi.post('/Chat/Direct', { j02id: a.getAttribute('data-id') }).then(function (d) {
                if (d && d.ok) window.location.href = moApi.url('/Chat/Thread/' + d.u01id);
                else toast(d && d.message);
            }).catch(function () { toast(); });
        });

        load();
    }

    // ══ jedna konverzace ═════════════════════════════════════════════════════
    function initThread() {
        var msgsEl = document.getElementById('mocMsgs');
        var inputEl = document.getElementById('mocInput');
        var sendEl = document.getElementById('mocSend');
        var fileEl = document.getElementById('mocFile');
        var pendingEl = document.getElementById('mocPending');
        var subEl = document.getElementById('mocSub');

        var cur = null;             // konverzace (JSON ze serveru)
        var msgs = {};              // id -> zpráva
        var hasMore = false, loading = false, again = false, sending = false;
        var pending = [];           // soubory čekající na odeslání
        var openActs = 0;           // zpráva s rozbalenými akcemi

        function sorted() {
            return Object.keys(msgs).map(function (k) { return msgs[k]; }).sort(function (a, b) { return a.id - b.id; });
        }
        function nearBottom() {
            return (window.innerHeight + window.scrollY) >= (document.documentElement.scrollHeight - 160);
        }
        function toBottom() {
            window.scrollTo(0, document.documentElement.scrollHeight);
        }

        function canDelete(m) {
            if (m.deleted) return false;
            if (m.type == 3) return true;                       // upozornění systému si příjemce smí smazat
            return m.type == 1 && m.senderid == cfg.me;
        }

        function bubble(m) {
            var html = '';
            if (m.replyto) html += '<div class="moc-quote"><b>' + esc(m.replysender || '') + '</b> ' + esc(m.replytext || T.deleted) + '</div>';
            (m.files || []).forEach(function (f) {
                var u = moApi.url('/Chat/Attachment') + '?id=' + f.id;
                if (f.image) html += '<a href="' + u + '" target="_blank" rel="noopener"><img class="moc-img" loading="lazy" src="' + u + '" alt="' + esc(f.name) + '"></a>';
                else html += '<a class="moc-file" href="' + u + '&download=true"><span class="material-icons-outlined">attach_file</span><span>' + esc(f.name) + ' · ' + fmtSize(f.size) + '</span></a>';
            });
            html += linkify(esc(m.text || ''));
            if (m.edited) html += '<span class="moc-edited">' + esc(T.edited) + '</span>';
            return html;
        }

        function render(keepBottom) {
            var list = sorted(), html = '', prev = null, pd = null;
            if (hasMore) html += '<button type="button" class="moc-older" id="mocOlder">' + esc(T.older) + '</button>';

            list.forEach(function (m0) {
                if (m0.type == 3 && m0.deleted) return;        // smazané upozornění se nezobrazuje vůbec
                var m = m0.type == 3 ? Object.assign({}, m0, { senderid: 0, sender: 'MARKTIME' }) : m0;
                var d = parseDate(m.date);
                if (!pd || !sameDay(d, pd)) html += '<div class="moc-day">' + esc(fmtDay(d)) + '</div>';

                if (m.type == 2) {
                    html += '<div class="moc-sys">' + esc(m.text) + '</div>';
                } else {
                    var mine = m.type == 1 && m.senderid == cfg.me;
                    var first = !prev || prev.type == 2 || prev.senderid != m.senderid || !pd || !sameDay(d, pd) || (d - pd) > 5 * 60000;
                    html += '<div class="moc-m' + (mine ? ' mine' : '') + (m.type == 3 ? ' note' : '') + '" data-id="' + m.id + '">';
                    if (first) html += '<div class="moc-who">' + (mine || cur.type == 1 ? '' : esc(m.sender || '') + ' · ') + fmtTime(d) + '</div>';
                    if (m.deleted) html += '<div class="moc-bubble del">' + esc(T.deleted) + '</div>';
                    else html += '<div class="moc-bubble">' + bubble(m) + '</div>';

                    if (!m.deleted && m.reactions && m.reactions.length) {
                        var counts = {};
                        m.reactions.forEach(function (r) { counts[r.key] = (counts[r.key] || 0) + 1; });
                        html += '<div class="moc-reacts">';
                        Object.keys(counts).forEach(function (k) {
                            html += '<span class="moc-react">' + (REACTIONS[k] || esc(k)) + (counts[k] > 1 ? ' ' + counts[k] : '') + '</span>';
                        });
                        html += '</div>';
                    }
                    if (openActs == m.id && canDelete(m)) {
                        html += '<div class="moc-acts"><button type="button" class="moc-act danger" data-act="delete">' + esc(T['delete']) + '</button>'
                            + '<button type="button" class="moc-act" data-act="cancel">' + esc(T.cancel) + '</button></div>';
                    }
                    html += '</div>';
                }
                prev = m; pd = d;
            });

            // "přečteno" u přímé konverzace
            if (cur && cur.type == 1 && list.length) {
                var last = list[list.length - 1];
                if (last.type == 1 && last.senderid == cfg.me && !last.deleted && cur.partnerread >= last.id) html += '<div class="moc-seen">' + esc(T.seen) + '</div>';
            }

            if (!list.length) html += '<div class="moc-empty">' + esc(T.nomessages) + '</div>';
            msgsEl.innerHTML = html;
            if (keepBottom) toBottom();
        }

        function header() {
            if (!cur || !subEl) return;
            var s = '';
            if (cur.type == 3) s = T.record + ' · ' + cur.members + ' ' + T.members;
            else if (cur.type == 2 && cur.name !== cfg.notifyName) s = cur.members + ' ' + T.members;
            subEl.textContent = s;
            subEl.hidden = !s;
        }

        function markRead() {
            if (document.visibilityState !== 'visible' || !cur) return;
            var list = sorted();
            if (!list.length) return;
            var last = list[list.length - 1].id;
            if (last <= (cur.lastread || 0)) return;
            cur.lastread = last;
            moApi.post('/Chat/Read', { u01id: cur.id, u02id: last }).then(function (d) {
                if (d && d.ok && window.moLive) moLive.setBadge(d.count);
            }).catch(function () { });
        }

        // načte nejnovější zprávy a sloučí je s tím, co už je na obrazovce
        function load(first) {
            if (loading) { again = true; return; }
            loading = true;
            var stick = first || nearBottom();
            moApi.get('/Chat/Messages', { u01id: cfg.u01id, before: 0 }).then(function (d) {
                if (!d || !d.ok) { toast(d && d.message); return; }
                cur = d.channel;
                (d.messages || []).forEach(function (m) { msgs[m.id] = m; });
                if (first) hasMore = !!d.hasmore;
                header();
                render(stick);
                markRead();
            }).catch(function () { if (first) toast(); }).then(function () {
                loading = false;
                if (again) { again = false; load(false); }
            });
        }

        function loadOlder() {
            var list = sorted();
            if (!list.length) return;
            var before = list[0].id, h = document.documentElement.scrollHeight;
            moApi.get('/Chat/Messages', { u01id: cfg.u01id, before: before }).then(function (d) {
                if (!d || !d.ok) { toast(d && d.message); return; }
                (d.messages || []).forEach(function (m) { msgs[m.id] = m; });
                hasMore = !!d.hasmore;
                render(false);
                window.scrollTo(0, window.scrollY + (document.documentElement.scrollHeight - h));     // zůstat na stejné zprávě
            }).catch(function () { toast(); });
        }

        // ── psaní a odeslání ────────────────────────────────────────────────
        function grow() {
            inputEl.style.height = 'auto';
            inputEl.style.height = Math.min(132, inputEl.scrollHeight + 2) + 'px';
        }
        function syncSend() {
            sendEl.disabled = sending || (!(inputEl.value || '').trim() && !pending.length);
        }
        function renderPending() {
            var html = '';
            pending.forEach(function (f, i) {
                html += '<span class="moc-chip"><span class="n">' + esc(f.name) + '</span><button type="button" data-i="' + i + '" aria-label="x"><span class="material-icons-outlined">close</span></button></span>';
            });
            pendingEl.innerHTML = html;
            syncSend();
        }

        function send() {
            var text = (inputEl.value || '').trim();
            if (sending || (!text && !pending.length)) return;
            sending = true; syncSend();

            var fd = new FormData();
            fd.append('u01id', cfg.u01id);
            fd.append('text', text);
            fd.append('replyto', 0);
            pending.forEach(function (f) { fd.append('files', f, f.name); });

            moApi.post('/Chat/Send', fd).then(function (d) {
                if (!d || !d.ok) { toast(d && d.message); return; }
                inputEl.value = ''; pending = []; renderPending(); grow();
                msgs[d.message.id] = d.message;
                if (cur) cur.lastread = Math.max(cur.lastread || 0, d.message.id);     // vlastní zpráva je pro mě přečtená
                render(true);
                if (d.rejected && d.rejected.length) toast(T.rejected + ': ' + d.rejected.join(', '));
            }).catch(function () { toast(T.notsent); }).then(function () {
                sending = false; syncSend();
            });
        }

        inputEl.addEventListener('input', function () { grow(); syncSend(); });
        sendEl.addEventListener('click', send);
        document.getElementById('mocAttach').addEventListener('click', function () { fileEl.click(); });
        fileEl.addEventListener('change', function () {
            for (var i = 0; i < fileEl.files.length; i++) {
                var f = fileEl.files[i];
                if (pending.length >= MAX_FILES) { toast(T.toomany); break; }
                if (f.size > MAX_BYTES) { toast(T.toobig + ': ' + f.name); continue; }
                pending.push(f);
            }
            fileEl.value = '';
            renderPending();
        });
        pendingEl.addEventListener('click', function (ev) {
            var b = ev.target.closest('button[data-i]');
            if (!b) return;
            pending.splice(parseInt(b.getAttribute('data-i'), 10), 1);
            renderPending();
        });

        // ── akce nad zprávou ────────────────────────────────────────────────
        msgsEl.addEventListener('click', function (ev) {
            if (ev.target.closest('#mocOlder')) { loadOlder(); return; }
            if (ev.target.closest('a')) return;      // odkaz nebo příloha
            var row = ev.target.closest('.moc-m');
            if (!row) return;
            var id = parseInt(row.getAttribute('data-id'), 10), m = msgs[id];
            if (!m) return;

            var act = ev.target.closest('[data-act]');
            if (act && act.getAttribute('data-act') === 'delete') {
                moApi.post('/Chat/Delete', { u02id: id }).then(function (d) {
                    if (!d || !d.ok) { toast(d && d.message); return; }
                    if (d.message) msgs[id] = d.message; else delete msgs[id];
                    openActs = 0;
                    render(false);
                }).catch(function () { toast(); });
                return;
            }
            if (act) { openActs = 0; render(false); return; }
            if (!canDelete(m)) return;
            openActs = openActs == id ? 0 : id;
            render(false);
        });

        // ── živé události ───────────────────────────────────────────────────
        window.addEventListener('mo:chat', function (ev) {
            var e = ev.detail;
            if (!e || e.u01id != cfg.u01id) return;
            if ((e.type === 'message' || e.type === 'update') && e.message && e.message.id) {
                var stick = nearBottom();
                msgs[e.message.id] = e.message;
                render(stick && e.type === 'message');
                markRead();
            } else if (e.type === 'read') {
                if (cur && cur.type == 1 && e.actorid != cfg.me) { cur.partnerread = Math.max(cur.partnerread || 0, e.u02id || 0); render(false); }
            } else {
                load(false);
            }
        });
        window.addEventListener('mo:chat-state', function (ev) { if (ev.detail && ev.detail.connected) load(false); });
        document.addEventListener('visibilitychange', function () { if (document.visibilityState === 'visible') load(false); });
        window.setInterval(function () {
            if (document.visibilityState === 'visible' && !(window.moLive && moLive.connected())) load(false);
        }, 15000);

        syncSend();
        load(true);
    }

    if (document.getElementById('mocList')) initList();
    if (document.getElementById('mocMsgs')) initThread();
})();
