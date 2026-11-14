// Web push v MO: zapnutí/vypnutí upozornění na nové zprávy chatu v tomto prohlížeči.
// Veřejný klíč serveru je v <body data-mo-push-key="...">; bez něj (server nemá upozornění nastavená) se nic neděje.
//   moPush.state()   -> Promise: 'unsupported' | 'denied' | 'off' | 'on'
//   moPush.enable()  -> Promise<bool>   (musí se volat z kliknutí uživatele - prohlížeč se ptá na povolení)
//   moPush.disable() -> Promise
//   moPush.test()    -> Promise s odpovědí serveru
(function () {
    var key = document.body ? (document.body.getAttribute('data-mo-push-key') || '') : '';
    var me = document.body ? (document.body.getAttribute('data-mo-me') || '') : '';
    var SYNC_KEY = 'mo-push-sync';

    function supported() {
        return !!key && !!window.moApi && 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window;
    }

    function keyBytes(s) {
        var pad = '='.repeat((4 - s.length % 4) % 4);
        var raw = window.atob((s + pad).replace(/-/g, '+').replace(/_/g, '/'));
        var out = new Uint8Array(raw.length);
        for (var i = 0; i < raw.length; i++) out[i] = raw.charCodeAt(i);
        return out;
    }

    function sameKey(sub) {
        try {
            var cur = sub.options && sub.options.applicationServerKey;
            if (!cur) return true;
            var a = new Uint8Array(cur), b = keyBytes(key);
            if (a.length !== b.length) return false;
            for (var i = 0; i < a.length; i++) if (a[i] !== b[i]) return false;
            return true;
        } catch (e) { return true; }
    }

    function registration() {
        return navigator.serviceWorker.ready;
    }

    function save(sub) {
        var j = sub.toJSON();
        return moApi.post('/Push/Subscribe', { endpoint: j.endpoint, p256dh: j.keys && j.keys.p256dh, auth: j.keys && j.keys.auth }).then(function (d) {
            if (d && d.ok) {
                try { localStorage.setItem(SYNC_KEY, JSON.stringify({ me: me, t: Date.now() })); } catch (e) { }
            }
            return !!(d && d.ok);
        });
    }

    function state() {
        if (!supported()) return Promise.resolve('unsupported');
        if (Notification.permission === 'denied') return Promise.resolve('denied');
        return registration().then(function (reg) { return reg.pushManager.getSubscription(); }).then(function (sub) {
            return sub && Notification.permission === 'granted' ? 'on' : 'off';
        }).catch(function () { return 'off'; });
    }

    function enable() {
        if (!supported()) return Promise.resolve(false);
        return Notification.requestPermission().then(function (perm) {
            if (perm !== 'granted') return false;
            return registration().then(function (reg) {
                return reg.pushManager.getSubscription().then(function (sub) {
                    // odběr s jiným klíčem serveru (klíče byly vyměněny) je potřeba zrušit a založit znovu
                    if (sub && !sameKey(sub)) return sub.unsubscribe().then(function () { return null; });
                    return sub;
                }).then(function (sub) {
                    return sub || reg.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: keyBytes(key) });
                });
            }).then(save);
        });
    }

    function disable() {
        if (!supported()) return Promise.resolve();
        try { localStorage.removeItem(SYNC_KEY); } catch (e) { }
        return registration().then(function (reg) { return reg.pushManager.getSubscription(); }).then(function (sub) {
            if (!sub) return;
            var endpoint = sub.endpoint;
            return sub.unsubscribe().catch(function () { }).then(function () {
                return moApi.post('/Push/Unsubscribe', { endpoint: endpoint }).catch(function () { });
            });
        }).catch(function () { });
    }

    // Jednou denně (a hned po přihlášení jiného uživatele) potvrdit odběr na serveru: prohlížeč ho může potichu obnovit.
    function sync() {
        if (!supported() || Notification.permission !== 'granted') return;
        var last = null;
        try { last = JSON.parse(localStorage.getItem(SYNC_KEY)); } catch (e) { last = null; }
        if (last && last.me === me && (Date.now() - last.t) < 24 * 3600 * 1000) return;
        registration().then(function (reg) { return reg.pushManager.getSubscription(); }).then(function (sub) {
            if (!sub) return;
            if (!sameKey(sub)) { return enable(); }
            return save(sub);
        }).catch(function () { });
    }

    window.moPush = {
        supported: supported,
        state: state,
        enable: enable,
        disable: disable,
        test: function () { return moApi.post('/Push/Test', {}); }
    };

    // Odhlášení: nejdřív zrušit odběr, aby na tento telefon nechodily zprávy odhlášeného uživatele.
    document.addEventListener('click', function (ev) {
        var a = ev.target.closest ? ev.target.closest('a[data-mo-logout]') : null;
        if (!a || !supported() || a.getAttribute('data-mo-done') === '1') return;
        ev.preventDefault();
        var go = function () { a.setAttribute('data-mo-done', '1'); window.location.href = a.href; };
        var t = window.setTimeout(go, 2000);       // odhlášení nesmí viset na síti
        disable().then(function () { window.clearTimeout(t); go(); });
    });

    if (document.readyState === 'complete') sync(); else window.addEventListener('load', sync);
})();
