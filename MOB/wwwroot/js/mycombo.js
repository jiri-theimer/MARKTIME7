function _mycombo_init(hiddenID, ajaxprefix, hiddenSelectedTextID, jsonSuggestions, attachToBody = false, onChangeHandler) {
    const hiddenInput = document.getElementById(hiddenID);
    const input = document.getElementById("combo-input-" + hiddenID);
    const list = document.getElementById("combo-list-" + hiddenID);
    const prefix = ajaxprefix || "";
    const selectedtextid = hiddenSelectedTextID || "";
    const clearbtn = document.getElementById("combo-clearbtn-"+hiddenID);

    let options = jsonSuggestions || [];
    let filtered = options;
    let focusedIndex = -1;

    if (selectedtextid != "")
    {
        input.value = document.getElementById(selectedtextid).value;
        clearbtn.style.display = "block";
    }

    if (attachToBody) {
        list.style.position = 'absolute';
        list.style.zIndex = '9999';
        document.body.appendChild(list);
    }

    function updateListPosition() {
        if (!attachToBody) return;
        const rect = input.getBoundingClientRect();
        list.style.left = rect.left + window.scrollX + 'px';
        list.style.top = rect.bottom + window.scrollY + 'px';
        list.style.width = rect.width + 'px';
    }

    // --- Parametr pro počet sloupců ---
    let columns = 1;
    function setGridColumns(num) {
        num = Math.min(Math.max(num, 1), 6);
        list.classList.remove('grid-cols-1', 'grid-cols-2', 'grid-cols-3', 'grid-cols-4', 'grid-cols-5', 'grid-cols-6');
        list.classList.add(`grid-cols-${num}`);
    }
    setGridColumns(columns);

    // --- Přístupnost ---
    input.setAttribute("role", "combobox");
    input.setAttribute("aria-autocomplete", "list");
    input.setAttribute("aria-expanded", "false");
    input.setAttribute("aria-controls", list.id);
    list.setAttribute("role", "listbox");

    // --- Scrollable seznam ---
    list.style.maxHeight = "200px";
    list.style.overflowY = "auto";

    // --- Highlight ---
    function highlightMatch(label, query) {
        if (!query) return label;
        query = query.trim();
        if (!query) return label;
        const regex = new RegExp(`(${query})`, "ig");
        
        return "<span>" + label.replace(regex, `<strong>$1</strong>`) + "</span>";
    }

    function renderList(items) {
        list.innerHTML = '';
        if (items.length === 0) {
            const li = document.createElement('li');
            li.className = 'col-span-full px-4 py-2 text-sm text-gray-500';
            li.textContent = 'Žádné položky';
            list.appendChild(li);
            focusedIndex = -1;
            return;
        }

        // --- seskupení položek ---
        const grouped = {};
        const ungrouped = [];
        items.forEach(item => {
            if (item.group) {
                if (!grouped[item.group]) grouped[item.group] = [];
                grouped[item.group].push(item);
            } else {
                ungrouped.push(item);
            }
        });

        const allGroups = Object.entries(grouped);

        // --- vykreslení skupin ---
        allGroups.forEach(([groupName, groupItems]) => {
            const liGroup = document.createElement('li');
            liGroup.className = 'col-span-full px-2 py-1 italic text-primary bg-base-300'; // <-- base barva a full width
            liGroup.textContent = groupName;
            list.appendChild(liGroup);

            groupItems.forEach(item => {
                const li = document.createElement('li');
                const btn = document.createElement('button');
                btn.type = "button";
                btn.className = 'w-full text-left';
                btn.tabIndex = "-1";
                btn.setAttribute("role", "option");
                btn.setAttribute("aria-selected", "false");
                btn.dataset.pid = item.pid;
                btn.innerHTML = highlightMatch(item.label, input.value);
                btn.addEventListener('click', () => selectItemByPid(btn.dataset.pid));
                li.appendChild(btn);
                list.appendChild(li);
            });
        });

        // --- vykreslení položek bez skupiny ---
        ungrouped.forEach(item => {
            const li = document.createElement('li');
            const btn = document.createElement('button');
            btn.type = "button";
            btn.className = 'w-full text-left';
            btn.tabIndex = "-1";
            btn.setAttribute("role", "option");
            btn.setAttribute("aria-selected", "false");
            btn.dataset.pid = item.pid;
            btn.innerHTML = highlightMatch(item.label, input.value);
            if (item.css_style != null) {
                btn.setAttribute("style", item.css_style);
            }
            
            btn.addEventListener('click', () => selectItemByPid(btn.dataset.pid));
            li.appendChild(btn);
            list.appendChild(li);
        });

        // --- zobraz seznam před scrollem ---
        list.classList.remove('hidden');

        // Scroll na aktuálně vybranou položku
        if (focusedIndex >= 0) {
            const buttons = list.querySelectorAll('li button');
            if (buttons[focusedIndex]) {
                buttons[focusedIndex].scrollIntoView({ block: "nearest", behavior: "smooth" });
            }
        }
    }


    function showList() {
        if (attachToBody) updateListPosition();

        list.classList.remove('hidden');
        input.setAttribute("aria-expanded", "true");

    }
    function hideList() {
        list.classList.add('hidden');
        input.setAttribute("aria-expanded", "false");
    }

    function selectItemByPid(pid) {
        const item = options.find(o => o.pid == pid);
        if (!item) return;

        input.value = item.label;
        hiddenInput.value = item.pid;
        clearbtn.style.display = "block";

        if (selectedtextid != "") {
            document.getElementById(selectedtextid).value = item.label;
        }

        hideList();
        
        if (typeof onChangeHandler === "function")
        {
            
            onChangeHandler();
        }
        
        // --- označí celý text pro rychlé přepsání ---
        input.select();
    }

    function updateFocus(newIndex) {
        const items = list.querySelectorAll('li button');
        if (items.length === 0) return;

        if (focusedIndex >= 0 && focusedIndex < items.length) {
            items[focusedIndex].classList.remove('bg-primary', 'text-white');
            items[focusedIndex].setAttribute("aria-selected", "false");
        }

        if (newIndex < 0) newIndex = items.length - 1;
        if (newIndex >= items.length) newIndex = 0;

        items[newIndex].classList.add('bg-primary', 'text-white');
        items[newIndex].setAttribute("aria-selected", "true");

        items[newIndex].scrollIntoView({ block: "nearest", behavior: "smooth" });

        focusedIndex = newIndex;
    }

    input.addEventListener('input', () => {
        hiddenInput.value = "0";
        if (selectedtextid != "") {
            document.getElementById(selectedtextid).value = null;
        }
        
        const val = input.value.toLowerCase();
        filtered = options.filter(opt => opt.label.toLowerCase().includes(val));
        renderList(filtered);
        showList();
    });

    input.addEventListener('focus', () => {
        if ((!options || options.length === 0) && prefix.trim() !== "") {
            load_ajax_suggestions();
        } else {
            openListAndSetFocus();
            input.select();
        }
    });

    input.addEventListener('mousedown', e => {
        if (list.classList.contains('hidden')) {
            // pouze otevřeme seznam, text v inputu zůstane označený / editable
            openListAndSetFocus();
        }
    });


    input.addEventListener('keydown', e => {
        const items = list.querySelectorAll('li button');
        switch (e.key) {
            case "ArrowDown":
            case "ArrowUp":
                e.preventDefault();
                if (list.classList.contains('hidden')) {
                    openListAndSetFocus();
                } else if (items.length > 0) {
                    updateFocus(e.key === "ArrowDown" ? focusedIndex + 1 : focusedIndex - 1);
                }
                break;
            case "Enter":
                e.preventDefault();
                if (focusedIndex >= 0 && focusedIndex < items.length) {
                    selectItemByPid(items[focusedIndex].dataset.pid);
                }
                break;
            case "Escape":
                e.preventDefault();
                hideList();
                break;
        }
    });

    function setFocusedIndexFromValue() {
        if (input.value) {
            const idx = filtered.findIndex(item => item.label === input.value);
            if (idx >= 0) {
                focusedIndex = idx;
                updateFocus(focusedIndex); // zvýrazní a scrollne
            }
        } else if (filtered.length > 0) {
            focusedIndex = 0;
            updateFocus(focusedIndex); // zvýrazní první položku a scrollne
        } else {
            focusedIndex = -1;
        }
    }

    document.addEventListener('click', e => {
        if (!input.contains(e.target) && !list.contains(e.target)) {
            hideList();
        }
    });

    clearbtn.addEventListener('click', e => {        
        hiddenInput.value = "0";
        input.value = "";
        if (selectedtextid != "") {
            document.getElementById(selectedtextid).value = "";
        }
        hideList();
        clearbtn.style.display = "none";
        input.focus();
        if (typeof onChangeHandler === "function") onChangeHandler();
    });

    input.addEventListener('blur', e => {
        const related = e.relatedTarget;
        if (!list.contains(related)) {
            setTimeout(() => hideList(), 50);
        }
    });

    function setInitialSelection(pid) {
        
        const found = options.find(opt => opt.pid === pid);
        if (found) {
            input.value = found.label;
            hiddenInput.value = found.pid;
            clearbtn.style.display = "block";
        } else {
            clearbtn.style.display = "none";
        }
        
    }


    window.addEventListener('resize', updateListPosition);
    window.addEventListener('scroll', updateListPosition, true);

    if (hiddenInput.value != null && hiddenInput.value != "" && prefix=="") {
        setInitialSelection(hiddenInput.value);
    }

    function openListAndSetFocus() {
        filtered = options;
        renderList(filtered);
        showList();
        requestAnimationFrame(() => setFocusedIndexFromValue());
    }

    function load_ajax_suggestions() {
        _ajaxPost("/AjaxListener/GetMyComboSuggestions", { prefix: prefix }, ajax_success_callback, ajax_error_callback);
    }
    function ajax_success_callback(data) {
        if (!Array.isArray(data)) data = [];
        options = data;
        filtered = options;
        renderList(filtered);
        showList();
        requestAnimationFrame(() => setFocusedIndexFromValue());
        input.select();
    }
    function ajax_error_callback(status, statusText) {
        alert("Chyba při načítání suggestions:" + statusText);
    }

    
    
}
