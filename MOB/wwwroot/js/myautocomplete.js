function _myautocomplete_init(hiddenInputID, suggestionsString, attachToBody = false) {
    // převede vstupní string na pole stringů
    const data = suggestionsString
        .split(",")
        .map(s => s.trim())
        .filter(s => s.length > 0);

    const root = document.getElementById('autocomplete-root-' + hiddenInputID);
    const input = document.getElementById('autocomplete-input-' + hiddenInputID);
    const listbox = document.getElementById('autocomplete-listbox-' + hiddenInputID);
    const hiddenInput = document.getElementById(hiddenInputID);
    const status = document.getElementById('autocomplete-status-' + hiddenInputID);

    let filtered = [];
    let activeIndex = -1;

    // pokud attachToBody => přesuneme listbox a nastavíme absolute
    if (attachToBody) {
        listbox.style.position = 'absolute';
        listbox.style.zIndex = '9999';
        document.body.appendChild(listbox);
    }

    function updateListboxPosition() {
        if (!attachToBody) return;
        const rect = input.getBoundingClientRect();
        listbox.style.left = rect.left + window.scrollX + 'px';
        listbox.style.top = rect.bottom + window.scrollY + 'px';
        listbox.style.width = rect.width + 'px';
    }

    function openList() {
        if (listbox.classList.contains('hidden')) {
            if (attachToBody) updateListboxPosition();
            listbox.classList.remove('hidden');
            input.setAttribute('aria-expanded', 'true');
        }
    }

    function closeList() {
        if (!listbox.classList.contains('hidden')) {
            listbox.classList.add('hidden');
            input.setAttribute('aria-expanded', 'false');
            activeIndex = -1;
            input.removeAttribute('aria-activedescendant');
        }
    }

    function updateStatus(text) {
        status.textContent = text;
    }

    function renderList(items) {
        listbox.innerHTML = '';
        if (!items || items.length === 0) {
            closeList();
            updateStatus('Žádné položky');
            return;
        }

        items.forEach((item, idx) => {
            const li = document.createElement('li');
            li.id = `autocomplete-item-${idx}`;
            li.role = 'option';
            li.className = 'px-3 py-2 cursor-pointer truncate transition-colors duration-150 ease-in-out rounded-md select-none';

            if (idx === activeIndex) {
                li.classList.add('bg-primary', 'text-primary-content');
                li.setAttribute('aria-selected', 'true');
            } else {
                li.classList.add('text-base-content');
                li.setAttribute('aria-selected', 'false');
                li.classList.add('hover:bg-primary', 'hover:text-primary-content');
            }

            li.textContent = item;

            li.addEventListener('mousedown', (e) => {
                e.preventDefault();
                selectItem(idx);
            });

            listbox.appendChild(li);
        });

        updateStatus(`${items.length} položek nalezeno`);
        openList();
    }

    function selectItem(index) {
        const item = filtered[index];
        if (!item) return;
        input.value = item;
        hiddenInput.value = item;
        updateStatus(`Vybrána položka ${item}`);
        closeList();
        input.focus();
    }

    function updateFilter() {
        const val = input.value.trim().toLowerCase();
        filtered = val ? data.filter(d => d.toLowerCase().includes(val)) : [...data];
        if (filtered.length === 0) activeIndex = -1;
        else if (activeIndex >= filtered.length) activeIndex = filtered.length - 1;
        renderList(filtered);
        hiddenInput.value = input.value;
    }

    function updateAriaActiveDescendant() {
        if (activeIndex >= 0) {
            input.setAttribute('aria-activedescendant', `autocomplete-item-${activeIndex}`);
        } else {
            input.removeAttribute('aria-activedescendant');
        }
    }

    function scrollActiveItemIntoView() {
        const el = document.getElementById(`autocomplete-item-${activeIndex}`);
        if (el) el.scrollIntoView({ block: 'nearest' });
    }

    // Events
    input.addEventListener('focus', () => {
        input.select();
        filtered = [...data];
        activeIndex = -1;
        renderList(filtered);
    });

    input.addEventListener('input', () => {
        updateFilter();
    });

    input.addEventListener('keydown', (e) => {
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            if (filtered.length === 0) return;
            if (listbox.classList.contains('hidden')) {
                filtered = [...data];
                activeIndex = 0;
            } else {
                activeIndex = (activeIndex + 1) % filtered.length;
            }
            renderList(filtered);
            updateAriaActiveDescendant();
            scrollActiveItemIntoView();
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            if (filtered.length === 0) return;
            if (listbox.classList.contains('hidden')) {
                filtered = [...data];
                activeIndex = filtered.length - 1;
            } else {
                activeIndex = (activeIndex - 1 + filtered.length) % filtered.length;
            }
            renderList(filtered);
            updateAriaActiveDescendant();
            scrollActiveItemIntoView();
        } else if (e.key === 'Enter') {
            e.preventDefault();
            if (activeIndex >= 0) selectItem(activeIndex);
        } else if (e.key === 'Escape') {
            if (!listbox.classList.contains('hidden')) {
                e.preventDefault();
                closeList();
                input.focus();
            }
        }
    });

    document.addEventListener('click', (e) => {
        if (!root.contains(e.target)) {
            closeList();
        }
    });

    input.addEventListener('blur', () => {
        setTimeout(() => closeList(), 150);
    });


    if (attachToBody) {
        window.addEventListener('resize', updateListboxPosition);
        window.addEventListener('scroll', updateListboxPosition, true);
    }

    // přiřazení výchozí hodnoty
    input.value = hiddenInput.value;
}
