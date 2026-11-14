// Společné volání serveru z JavaScriptu v MO (fetch + antiforgery token + cesta aplikace).
//   moApi.get("/Chat/Channels", { a: 1 }).then(function (data) { ... })
//   moApi.post("/Chat/Read", { u01id: 5, u02id: 9 })       - objekt nebo FormData
// Vrací Promise s JSON odpovědí. Při vypršeném přihlášení přesměruje na login.
(function () {
    var root = (document.body && document.body.getAttribute('data-mo-root')) || '/';
    if (root.charAt(root.length - 1) !== '/') root += '/';

    function url(path) {
        return root + String(path || '').replace(/^\//, '');
    }

    function token() {
        var m = document.querySelector('meta[name="mo-xsrf"]');
        return m ? m.getAttribute('content') : '';
    }

    function query(params) {
        if (!params) return '';
        var a = [];
        Object.keys(params).forEach(function (k) {
            if (params[k] !== undefined && params[k] !== null) a.push(encodeURIComponent(k) + '=' + encodeURIComponent(params[k]));
        });
        return a.length ? '?' + a.join('&') : '';
    }

    function handle(res) {
        // vypršené přihlášení: server přesměruje na login a fetch vrátí HTML přihlašovací stránky
        if (res.redirected && /\/Login\//i.test(res.url)) {
            window.location.href = url('/Login/UserLogin') + '?returnurl=' + encodeURIComponent(window.location.pathname + window.location.search);
            return new Promise(function () { });
        }
        if (!res.ok) throw new Error('HTTP ' + res.status);
        return res.json();
    }

    window.moApi = {
        url: url,
        get: function (path, params) {
            return fetch(url(path) + query(params), { credentials: 'same-origin', headers: { 'X-Requested-With': 'XMLHttpRequest' }, cache: 'no-store' }).then(handle);
        },
        post: function (path, data) {
            var body;
            if (data instanceof FormData) {
                body = data;
            } else {
                body = new URLSearchParams();
                Object.keys(data || {}).forEach(function (k) {
                    if (data[k] !== undefined && data[k] !== null) body.append(k, data[k]);
                });
            }
            return fetch(url(path), {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'RequestVerificationToken': token(), 'X-Requested-With': 'XMLHttpRequest' },
                body: body
            }).then(handle);
        }
    };
})();
