// Drawer menu ("Více") je normálně čistě CSS řešení (label[for] + checkbox:checked ~ sibling).
// Na některých (hlavně mobilních) prohlížečích se ale ukázalo, že hned po prvním načtení stránky
// první klik na label checkbox nepřepne (funguje až po další navigaci/překreslení stránky) -
// pravděpodobně kvůli tomu, že prohlížeč ještě nestihl checkbox (position:fixed, 0x0, opacity:0)
// plně "rozvrhnout", než na něj uživatel klikl. Řešení: obejít nativní label mechanismus úplně
// a přepínat checked stav ručně přes JS, které běží spolehlivě od DOMContentLoaded.
document.addEventListener('DOMContentLoaded', function () {
    var checkbox = document.getElementById('mo-drawer');
    if (!checkbox) return;

    function toggle(e) {
        e.preventDefault();
        checkbox.checked = !checkbox.checked;
    }

    document.querySelectorAll('label[for="mo-drawer"]').forEach(function (label) {
        // touchend reaguje spolehlivě i na úplně první dotek po načtení stránky (na rozdíl
        // od syntetického "click", který na některých mobilních prohlížečích první dotek jen
        // "zaostří" stránku a teprve druhý dotek vyhodnotí jako klik). Zavolání preventDefault()
        // uvnitř touchend navíc podle specifikace potlačí následný syntetický click, takže se
        // nepřepne dvakrát.
        label.addEventListener('touchend', toggle, { passive: false });
        label.addEventListener('click', toggle);
    });
});