// Neurčitý loading overlay - zobrazí se přes celou obrazovku, dokud se stránka nepřekreslí
// (typicky při uploadu přílohy, kde přesné procento nejde bez přechodu na AJAX spolehlivě zjistit).
function moShowUploadOverlay(text) {
    var overlay = document.getElementById('mo-upload-overlay');
    if (!overlay) return;

    var textEl = document.getElementById('mo-upload-overlay-text');
    if (textEl) textEl.textContent = text || '';

    overlay.classList.add('open');
}
