// MO mycombo - client-side multi-column picker
// API: mc_open / mc_close / mc_filter / mc_pick / mc_clear

(function () {
    // Normalizace: lowercase + odstranění diakritiky
    function mc_normalize(s) {
        return (s || '').toString().toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
    }
    window.mc_normalize = mc_normalize;

    // Čas posledního zavření per combo - pojistka proti "ghost clicku"
    // (opožděný syntetický click po touchend, který by mohl dopadnout
    // na nově odkryté trigger tlačítko a combo znovu otevřít).
    var mcLastClose = {};

    window.mc_open = function (ctlId) {
        if (Date.now() - (mcLastClose[ctlId] || 0) < 400) return;

        var modal = document.getElementById(ctlId + '_modal');
        if (!modal || typeof modal.showModal !== 'function') return;

        // Delegovaný listener pro klik na položku a pro search - připojí se jen jednou
        if (!modal.dataset.wired) {
            modal.dataset.wired = '1';

            var list = modal.querySelector('.mc-list');
            if (list) {
                list.addEventListener('click', function (e) {
                    var item = e.target.closest('.mc-item');
                    if (item) window.mc_pick(ctlId, item);
                });
            }

            var search = modal.querySelector('input[type="search"]');
            if (search) {
                search.addEventListener('input', function () {
                    window.mc_filter(ctlId, this.value);
                });
            }
        }

        modal.showModal();

        var search2 = modal.querySelector('input[type="search"]');
        if (search2) { search2.value = ''; window.mc_filter(ctlId, ''); }

        // Naskrolovat na aktuálně vybranou položku (zvýraznění řeší server-side render
        // přes třídu mc-item-selected, případně mc_pick níže pro stejnou session bez reloadu)
        var selected = modal.querySelector('.mc-item-selected');
        if (selected) {
            requestAnimationFrame(function () {
                selected.scrollIntoView({ block: 'center' });
            });
        }
    };

    window.mc_close = function (ctlId) {
        var modal = document.getElementById(ctlId + '_modal');
        if (modal && typeof modal.close === 'function') { modal.close(); }
        mcLastClose[ctlId] = Date.now();
    };

    window.mc_filter = function (ctlId, q) {
        var modal = document.getElementById(ctlId + '_modal');
        if (!modal) return;

        var qn = mc_normalize(q);
        var items = modal.querySelectorAll('.mc-item');
        var visible = 0;

        items.forEach(function (it) {
            var hs = mc_normalize(it.dataset.haystack || '');
            var show = !qn || hs.indexOf(qn) >= 0;
            it.style.display = show ? '' : 'none';
            if (show) visible++;
        });

        // Skupinové hlavičky - schovat pokud všechny jejich položky jsou skryté
        modal.querySelectorAll('.mc-group-header').forEach(function (hdr) {
            var grp = hdr.dataset.group;
            var hasVisible = false;
            if (grp) {
                modal.querySelectorAll('.mc-item[data-group="' + grp + '"]').forEach(function (it) {
                    if (it.style.display !== 'none') hasVisible = true;
                });
            } else {
                // Hlavičky bez skupiny (prázdný GroupBy) - vždy viditelné pokud existují
                hasVisible = true;
            }
            hdr.style.display = hasVisible ? '' : 'none';
        });

        var empty = modal.querySelector('.mc-empty');
        if (empty) empty.classList.toggle('hidden', visible > 0);

        var count = modal.querySelector('.mc-count');
        if (count) count.textContent = visible;
    };

    window.mc_pick = function (ctlId, itemEl) {
        var hidden = document.getElementById(ctlId);
        if (!hidden) return;

        var id = itemEl.dataset.id || '0';
        var text = itemEl.dataset.text || '';

        hidden.value = id;

        var triggerText = document.getElementById(ctlId + '_text');
        if (triggerText) {
            triggerText.textContent = text;
            triggerText.classList.remove('text-base-content/40');
        }

        // Přesunout zvýraznění na nově vybranou položku (server render zvýrazňuje jen
        // při prvním načtení stránky; tady to musíme udržet konzistentní i bez reloadu)
        var modal = document.getElementById(ctlId + '_modal');
        if (modal) {
            var prevSel = modal.querySelector('.mc-item-selected');
            if (prevSel) {
                prevSel.classList.remove('mc-item-selected', 'bg-primary/10', 'border-l-primary');
                prevSel.classList.add('border-l-transparent');
            }
            itemEl.classList.remove('border-l-transparent');
            itemEl.classList.add('mc-item-selected', 'bg-primary/10', 'border-l-primary');
        }

        window.mc_close(ctlId);

        // Cascade (např. načtení úkolů) až po zavření - nezávisle na výsledku
        hidden.dispatchEvent(new Event('change', { bubbles: true }));
    };

    window.mc_clear = function (ctlId) {
        var hidden = document.getElementById(ctlId);
        if (!hidden) return;

        hidden.value = '0';

        var triggerText = document.getElementById(ctlId + '_text');
        if (triggerText) {
            triggerText.textContent = triggerText.dataset.placeholder || 'vyberte...';
            triggerText.classList.add('text-base-content/40');
        }

        hidden.dispatchEvent(new Event('change', { bubbles: true }));
    };
})();