function _mydatepicker_init(inputID, onChangeHandler)
{
   
    // vytvoříme flatpickr instanci a uložíme ji do proměnné
    const fp = flatpickr("#" + inputID, {        
        locale: "cs",
        dateFormat: "d.m.Y",
        enableTime: false,
        time_24hr: true,               
        allowInput: true,
        onReady: function (selectedDates, dateStr, instance) {
           
            // vytvoření tlačítka
            const btn = document.createElement("button");
            btn.textContent = "Dnes";
            btn.type = "button";
            btn.classList.add("flatpickr-today-button");

            // po kliknutí nastaví dnešní datum
            btn.addEventListener("click", function () {
                instance.setDate(new Date(), true); // true = trigger onchange
            });

            // přidáme tlačítko do dolního panelu
            instance.calendarContainer.appendChild(btn);

            // CSS pro umístění tlačítka
            btn.style.margin = "5px";
            btn.style.padding = "5px 10px";
        }
    });


    const input = document.getElementById(inputID);

    input.addEventListener("input", (e) => {
        const val = e.target.value;

        // RegExp pro datum bez času: 01.12.2025
        // nebo datum s časem: 01.12.2025 13:45
        const dateRegex = /^(\d{2})\.(\d{2})\.(\d{4})( \d{2}:\d{2})?$/;

        if (dateRegex.test(val)) {
            // validní formát, nastav do kalendáře
            fp.setDate(val, true, "d.m.Y H:i");
        }
        // pokud není validní, nic neděláme (nepřepisujeme input)
    });

    // teprve po vytvoření instace přidáme listener
    input.addEventListener('keydown', (e) => {
        if (e.key === "Escape" || e.key === "Esc") {            
            fp.close();
            // odkomentuj, pokud chceš odebrat focus z inputu:
            // e.target.blur();
        }
    });

    input.addEventListener("focus", () => {
        input.select();
    });
}
