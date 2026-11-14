// AI asistent v MO (Ai/Index): dotazy nad daty a vykázání hodin s potvrzovací kartou.
// Konverzace včetně volání nástrojů (thread) se drží v prohlížeči a posílá se s každým dotazem;
// stav je v sessionStorage, takže přežije odskok jinam v aplikaci a návrat.
// Tabulky vykresluje aplikace z dat serveru (r.tables), model je nepřepisuje.
// Úkon (r.proposals) agent jen připraví - zapíše ho až tlačítko „Zapsat“ na kartě (/Ai/UkonConfirm).
(function () {
    var cfgEl = document.getElementById('moaConfig');
    if (!cfgEl || !window.moApi) return;
    var cfg = JSON.parse(cfgEl.textContent || '{}');
    var T = cfg.t || {};
    var KEY = 'mo-ai-state-' + cfg.me;
    var MAX_ROWS = 30;

    var msgsEl = document.getElementById('moaMsgs');
    var introEl = document.getElementById('moaIntro');
    var inputEl = document.getElementById('moaInput');
    var sendEl = document.getElementById('moaSend');

    var st = fresh();           // { conversation, thread, history: [{role, content}], items: [...] }
    var busy = false;

    function uuid() {
        return (window.crypto && crypto.randomUUID) ? crypto.randomUUID() : ('c' + Date.now() + Math.random().toString(16).slice(2));
    }
    function fresh() {
        return { v: 1, conversation: uuid(), thread: null, history: [], items: [] };
    }
    function save() {
        try { sessionStorage.setItem(KEY, JSON.stringify(st)); }
        catch (e) { try { sessionStorage.removeItem(KEY); } catch (e2) { } }     // plné úložiště: konverzace jen nepřežije reload
    }
    function restore() {
        var s = null;
        try { s = JSON.parse(sessionStorage.getItem(KEY) || 'null'); } catch (e) { s = null; }
        if (s && s.v === 1 && s.items && s.items.length) st = s;
    }

    function el(tag, cls, text) {
        var e = document.createElement(tag);
        if (cls) e.className = cls;
        if (text !== undefined && text !== null) e.textContent = text;
        return e;
    }
    function nearBottom() {
        return (window.innerHeight + window.scrollY) >= (document.documentElement.scrollHeight - 200);
    }
    function toBottom() {
        window.scrollTo(0, document.documentElement.scrollHeight);
    }

    // ── formátování buněk tabulky ───────────────────────────────────────────
    function nf(decimals) {
        return new Intl.NumberFormat('cs-CZ', { minimumFractionDigits: decimals || 0, maximumFractionDigits: decimals || 0 });
    }
    function cell(col, v) {
        if (v === null || v === undefined) return '';
        if (col.type === 'number') return typeof v === 'number' ? nf(col.decimals).format(v) : v;
        if (col.type === 'date') {
            var m = /^(\d{4})-(\d{2})-(\d{2})(?:T(\d{2}:\d{2}))?/.exec(v);
            return m ? (parseInt(m[3], 10) + '.' + parseInt(m[2], 10) + '.' + m[1] + (m[4] ? ' ' + m[4] : '')) : v;
        }
        if (col.type === 'bool') return v === '1' ? T.yes : T.no;
        return v;
    }

    function renderTable(t) {
        var box = el('div', 'moa-result');
        if (t.title) box.appendChild(el('div', 'moa-result-title', t.title));
        if (t.description) box.appendChild(el('div', 'moa-result-desc', t.description));
        var isList = t.kind === 'list';
        if (isList) box.appendChild(el('div', 'moa-result-desc', T.count.replace('{0}', nf(0).format(t.groups || 0))));

        var cols = t.columns || [], rows = t.rows || [];
        if (cols.length && rows.length) {
            var wrap = el('div', 'moa-table-wrap'), table = el('table', 'moa-table');
            var thead = el('thead'), trh = el('tr');
            cols.forEach(function (c) { trh.appendChild(el('th', c.type === 'number' ? 'num' : '', c.name)); });
            thead.appendChild(trh); table.appendChild(thead);

            var tbody = el('tbody');
            rows.slice(0, MAX_ROWS).forEach(function (row) {
                var tr = el('tr');
                cols.forEach(function (c, i) { tr.appendChild(el('td', c.type === 'number' ? 'num' : '', cell(c, row[i]))); });
                tbody.appendChild(tr);
            });
            table.appendChild(tbody);

            var dims = cols.filter(function (c) { return !c.measure; }).length;
            var showTotal = t.total && (isList ? t.groups > 1 : (dims > 0 && (rows.length > 1 || t.truncated)));
            if (showTotal) {
                var tfoot = el('tfoot'), trf = el('tr'), labelDone = false;
                cols.forEach(function (c, i) {
                    var txt = '';
                    if (c.measure) txt = cell(c, t.total[i]);
                    else if (!labelDone) { txt = T.total; labelDone = true; }
                    trf.appendChild(el('td', c.type === 'number' ? 'num' : '', txt));
                });
                tfoot.appendChild(trf); table.appendChild(tfoot);
            }
            wrap.appendChild(table); box.appendChild(wrap);

            var rest = Math.max(rows.length, t.groups || 0) - Math.min(rows.length, MAX_ROWS);
            if (rest > 0) box.appendChild(el('div', 'moa-result-desc', T.more.replace('{0}', nf(0).format(rest))));
        }
        return box;
    }

    // ── karta úkonu: open = návrh s tlačítky, saved = zapsaný úkon, cancelled = zrušený návrh ─────────────
    function ukonSummary(c) {
        return [c.datum, c.projekt, c.aktivita, (c.hodiny || '') + ' h', c.text].filter(function (x) { return x; }).join(', ');
    }

    // poznámka pro model do konverzace: co uživatel udělal mimo chat (potvrdil / zrušil návrh)
    function threadNote(text) {
        var th;
        try { th = JSON.parse(st.thread || '[]'); } catch (e) { return; }
        if (!Array.isArray(th) || !th.length) return;
        var block = { type: 'text', text: text };
        var last = th[th.length - 1];
        if (last && last.role === 'user' && Array.isArray(last.content)) last.content.push(block);
        else th.push({ role: 'user', content: [block] });
        st.thread = JSON.stringify(th);
    }

    function fillUkon(box, p, item) {
        var c = p.card || {};
        box.innerHTML = '';
        box.className = 'moa-result moa-ukon' + (p.state === 'saved' ? ' saved' : (p.state === 'cancelled' ? ' cancelled' : ''));
        box.appendChild(el('div', 'moa-result-title', p.state === 'saved' ? T.ukonSavedTitle : (p.state === 'cancelled' ? T.ukonCancelledTitle : T.ukonTitle)));

        var dl = el('dl', 'moa-ukon-grid');
        var row = function (label, value, strong) {
            if (value === null || value === undefined || value === '') return;
            dl.appendChild(el('dt', '', label));
            dl.appendChild(el('dd', strong ? 'strong' : '', value));
        };
        row(T.ukonDate, c.datum, true);
        row(T.ukonTime, c.cas);
        row(T.ukonProject, c.projekt, true);
        row(T.ukonClient, c.klient);
        row(T.ukonActivity, c.aktivita ? (c.aktivita + (c.sesit ? ' (' + c.sesit + ')' : '')) : c.sesit);
        row(T.ukonTask, c.ukol);
        row(T.ukonHours, c.hodiny, true);
        row(T.ukonText, c.text);
        row(T.ukonTextInternal, c.textInterni);
        box.appendChild(dl);
        if (c.poznamka) box.appendChild(el('div', 'moa-result-desc', c.poznamka));

        var btns = el('div', 'moa-btns');
        if (p.state === 'open') {
            var bSave = el('button', 'moa-btn primary', T.ukonSave), bCancel = el('button', 'moa-btn', T.ukonCancel);
            bSave.type = 'button'; bCancel.type = 'button';
            var warn = el('div', 'moa-warn'); warn.hidden = true;

            bSave.addEventListener('click', function () {
                if (p.state !== 'open') return;
                bSave.disabled = true; bCancel.disabled = true; warn.hidden = true;
                var fail = function (msg) { bSave.disabled = false; bCancel.disabled = false; warn.textContent = msg || T.ukonError; warn.hidden = false; };
                moApi.post('/Ai/UkonConfirm', { guid: c.guid, logId: (item && item.logId) || 0 }).then(function (r) {
                    if (!r || !r.ok) { fail(r && r.error); return; }
                    p.card = r.ukon || p.card;
                    p.state = 'saved';
                    threadNote('[MARKTIME] Uživatel potvrdil návrh úkonu tlačítkem Zapsat. Úkon je v MARKTIME zapsán: ' + ukonSummary(p.card) + '.');
                    fillUkon(box, p, item);
                    save();
                }).catch(function () { fail(null); });
            });
            bCancel.addEventListener('click', function () {
                if (p.state !== 'open') return;
                p.state = 'cancelled';
                moApi.post('/Ai/UkonCancel', { guid: c.guid }).catch(function () { });
                threadNote('[MARKTIME] Uživatel návrh úkonu zrušil, nic nebylo zapsáno: ' + ukonSummary(c) + '.');
                fillUkon(box, p, item);
                save();
            });
            btns.appendChild(bSave); btns.appendChild(bCancel);
            box.appendChild(btns);
            box.appendChild(warn);
        } else if (p.state === 'saved' && c.pid) {
            var a = el('a', 'moa-btn', T.ukonOpen);
            a.href = moApi.url('/Ukon/Edit/' + c.pid);
            btns.appendChild(a);
            box.appendChild(btns);
        }
    }

    // ── zprávy ──────────────────────────────────────────────────────────────
    function addUser(text) {
        var m = el('div', 'moc-m mine');
        m.appendChild(el('div', 'moc-bubble', text));
        msgsEl.appendChild(m);
        return m;
    }

    function addAssistant(item) {
        var m = el('div', 'moc-m moa-answer');
        if (item.text) m.appendChild(el('div', 'moc-bubble', item.text));
        (item.tables || []).forEach(function (t) { m.appendChild(renderTable(t)); });
        var firstOpen = null;
        (item.proposals || []).forEach(function (p) {
            var box = el('div');
            fillUkon(box, p, item);
            m.appendChild(box);
            if (!firstOpen && p.state === 'open') firstOpen = box;
        });
        if (item.warnings && item.warnings.length) m.appendChild(el('div', 'moa-warn', item.warnings.join(' ')));
        if (item.error) m.appendChild(el('div', 'moa-warn', item.error));
        msgsEl.appendChild(m);
        return firstOpen || m;
    }

    function renderAll() {
        Array.prototype.slice.call(msgsEl.querySelectorAll('.moc-m, .moa-wait')).forEach(function (n) { n.remove(); });
        introEl.hidden = st.items.length > 0;
        st.items.forEach(function (it) {
            if (it.role === 'user') addUser(it.text); else addAssistant(it);
        });
    }

    function syncSend() {
        sendEl.disabled = busy || !(inputEl.value || '').trim();
    }
    function grow() {
        inputEl.style.height = 'auto';
        inputEl.style.height = Math.min(132, inputEl.scrollHeight + 2) + 'px';
    }

    function send(textOverride) {
        var text = (textOverride || inputEl.value || '').trim();
        if (busy || !text) return;
        busy = true;
        inputEl.value = ''; grow(); syncSend();
        introEl.hidden = true;

        st.history.push({ role: 'user', content: text });
        st.items.push({ role: 'user', text: text });
        addUser(text);
        var wait = el('div', 'moa-wait', T.thinking);
        msgsEl.appendChild(wait);
        toBottom();

        var payload = { messages: st.history, conversation: st.conversation, page: 'mo-ai', thread: st.thread };
        var done = function (item, focusEl) {
            wait.remove();
            st.items.push(item);
            var target = addAssistant(item);
            save();
            busy = false; syncSend();
            if (target && target.scrollIntoView) target.scrollIntoView({ block: 'start', behavior: 'smooth' });
        };

        moApi.post('/Ai/Ask', { payload: JSON.stringify(payload) }).then(function (r) {
            if (!r || !r.ok) {
                st.history.pop();       // neúspěšný dotaz do konverzace pro model nepatří
                done({ role: 'assistant', text: '', error: (r && r.error) || T.error });
                return;
            }
            st.thread = r.thread || null;
            st.history.push({ role: 'assistant', content: r.reply || '' });
            done({
                role: 'assistant', text: r.reply || '', logId: r.logId || 0, tables: r.tables || [], warnings: r.warnings || [],
                proposals: (r.proposals || []).map(function (c) { return { card: c, state: 'open' }; })
            });
        }).catch(function () {
            st.history.pop();
            done({ role: 'assistant', text: '', error: T.network });
        });
    }

    inputEl.addEventListener('input', function () { grow(); syncSend(); });
    sendEl.addEventListener('click', function () { send(); });
    msgsEl.addEventListener('click', function (ev) {
        var b = ev.target.closest('.moa-example');
        if (b) send(b.getAttribute('data-q'));
    });
    document.getElementById('moaNew').addEventListener('click', function () {
        if (busy) return;
        st = fresh();
        try { sessionStorage.removeItem(KEY); } catch (e) { }
        renderAll();
        window.scrollTo(0, 0);
    });

    restore();
    renderAll();
    if (st.items.length) toBottom();
    syncSend();
})();
