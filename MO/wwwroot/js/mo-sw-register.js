// Registrace service workeru (viz /service-worker.js) - cachuje statické assety a poskytuje
// offline fallback stránku. Bezpečně se přeskočí v prohlížečích/kontextech bez podpory.
if ('serviceWorker' in navigator) {
    window.addEventListener('load', function () {
        navigator.serviceWorker.register('/service-worker.js').catch(function () {
            // Tichý fallback - appka musí fungovat i bez service workeru (např. http:// v develu)
        });
    });
}
