// Zabraňuje dvojitému odeslání formuláře (typicky rychlé opakované ťuknutí na "Uložit" na
// pomalém mobilním připojení) - po prvním submitu zablokuje další odeslání téhož formuláře
// a vizuálně ztlumí tlačítka, dokud se stránka nepřenačte.
//
// Netýká se GET formulářů (filtry) - tam žádné riziko duplicitního zápisu není a blokace
// by jen zbytečně bránila rychlému přefiltrování.
//
// Opt-out: <form data-no-double-submit-guard="true"> pro formuláře, kde je opakované
// odeslání žádoucí (v MO se to zatím nikde nevyužívá, ale nechávám únikovou cestu).
(function () {
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (!form || form.tagName !== 'FORM') return;
        if (form.method && form.method.toLowerCase() === 'get') return;
        if (form.dataset.noDoubleSubmitGuard === 'true') return;

        if (form.dataset.mosubmitted === 'true') {
            e.preventDefault();
            return;
        }
        form.dataset.mosubmitted = 'true';

        var buttons = form.querySelectorAll('button[type="submit"], input[type="submit"]');
        buttons.forEach(function (btn) {
            btn.disabled = true;
            btn.style.opacity = '.6';
            btn.style.pointerEvents = 'none';
        });
    }, true);

    // Bezpečnostní pojistka pro návrat přes tlačítko Zpět v prohlížeči (bfcache) -
    // formulář i tlačítka by jinak zůstaly natrvalo zablokované.
    window.addEventListener('pageshow', function (e) {
        if (!e.persisted) return;

        document.querySelectorAll('form[data-mosubmitted="true"]').forEach(function (f) {
            f.dataset.mosubmitted = 'false';
        });
        document.querySelectorAll('button[type="submit"]:disabled, input[type="submit"]:disabled').forEach(function (b) {
            b.disabled = false;
            b.style.opacity = '';
            b.style.pointerEvents = '';
        });
    });
})();
