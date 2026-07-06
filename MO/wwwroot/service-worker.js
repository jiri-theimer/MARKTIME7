// Minimální service worker pro PWA:
// - statické assety (CSS/JS/ikony) se cachují cache-first, ať appka rychleji naskočí i na
//   slabém signálu
// - HTML stránky (navigace) jdou vždy network-first - v MO se zobrazují osobní/aktuální data
//   (úkony, úkoly...), takže by bylo nebezpečné je agresivně cachovat a ukazovat neaktuální
//   obsah; cache tady slouží jen jako fallback při úplném výpadku připojení (offline.html)
const CACHE_NAME = 'mo-static-v2';

const STATIC_ASSETS = [
    '/css/app.css',
    '/js/mo-prefs.js',
    '/js/mo-double-submit-guard.js',
    '/js/mo-upload-overlay.js',
    '/images/icons/icon-192.png',
    '/images/icons/icon-512.png',
    '/offline.html'
];

self.addEventListener('install', function (event) {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(function (cache) { return cache.addAll(STATIC_ASSETS); })
            .catch(function () { /* nevadí, když se něco z listu nepodaří predownloadovat */ })
    );
    self.skipWaiting();
});

self.addEventListener('activate', function (event) {
    event.waitUntil(
        caches.keys().then(function (keys) {
            return Promise.all(
                keys.filter(function (k) { return k !== CACHE_NAME; })
                    .map(function (k) { return caches.delete(k); })
            );
        })
    );
    self.clients.claim();
});

self.addEventListener('fetch', function (event) {
    var req = event.request;
    if (req.method !== 'GET') return;

    var url = new URL(req.url);
    if (url.origin !== location.origin) return;

    // Navigace (HTML stránky) - vždy zkusit síť jako první, offline stránka jen jako fallback
    if (req.mode === 'navigate') {
        event.respondWith(
            fetch(req).catch(function () {
                return caches.match('/offline.html');
            })
        );
        return;
    }

    // Statické assety - cache first, doplnění cache na pozadí
    if (url.pathname.startsWith('/css/') || url.pathname.startsWith('/js/') || url.pathname.startsWith('/images/')) {
        event.respondWith(
            caches.match(req).then(function (cached) {
                if (cached) return cached;
                return fetch(req).then(function (res) {
                    var resClone = res.clone();
                    caches.open(CACHE_NAME).then(function (cache) { cache.put(req, resClone); });
                    return res;
                });
            })
        );
    }
});
