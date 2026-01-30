function _number_init(strInputID, dblDefaultValue, intMin, intMax,intDecimals) {
    const hiddenInput = document.getElementById(strInputID);
    const input = document.getElementById("number-"+strInputID);
    const errorMsg = document.getElementById('error-msg-' + strInputID);
    
    
    // Parametry
    let format = "#,###,###,###." + "00000000000".substring(0, intDecimals);  // formát - tisícové oddělovače mohou být čárky i tečky
    if (intDecimals == 0)
    {
        format = "#,###,###,###";   //celé číslo
    }
    const min = intMin;
    const max = intMax;
    const decimalSeparator = ",";      // desetinný oddělovač
    const thousandSeparator = ".";     // tisícový oddělovač (měl by být jiný než decimalSeparator)

    // Parsování formátu
    const formatParts = format.split('.');
    const intFormat = formatParts[0] || "";
    const decFormat = formatParts[1] || "";

    const decimals = decFormat.length;

    // Odstraníme z intFormat všechny čárky i tečky (tisícové oddělovače)
    const maxIntDigits = intFormat.replace(/[.,]/g, '').length;

    const step = 1 / Math.pow(10, decimals);

    // Parsování vstupu na číslo
    function parseInput(value) {
        value = value.trim();
        if (thousandSeparator) {
            const escThousandSep = thousandSeparator.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
            const re = new RegExp(escThousandSep, 'g');
            value = value.replace(re, '');
        }
        if (decimalSeparator !== '.') {
            value = value.replace(decimalSeparator, '.');
        }
        const num = parseFloat(value);
        return isNaN(num) ? null : num;
    }

    // Formátování čísla na string podle formátu a oddělovačů
    function formatValue(val) {
        let str = val.toFixed(decimals);
        let parts = str.split('.');

        if (thousandSeparator) {
            parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, thousandSeparator);
        }

        let result = parts.join(decimalSeparator);

        return result;
    }

    // Validace
    function validateValue(val) {
        
        if (isNaN(val)) return "Není číslo";
        
        if (val < min) return `Hodnota musí být ≥ ${min}`;
        if (val > max) return `Hodnota musí být ≤ ${max}`;

        const intPartStr = Math.floor(val).toString();
        if (intPartStr.length > maxIntDigits) return `Maximálně ${maxIntDigits} číslic před desetinnou čárkou`;

        return "";
    }

    // Nastavení hodnoty do inputu s validací a formátováním
    function setValue(val) {
        const err = validateValue(val);
        if (err) {
            errorMsg.textContent = err;
            return false;
        }
        errorMsg.textContent = "";
        
        input.value = formatValue(val);
        
        hiddenInput.value = formatForServer(input.value);
        
        return true;
    }

    // Změna hodnoty o delta
    function changeValue(delta) {
        let val = parseInput(input.value);
        
        if (val === null) val = 0;
        val = +(val + delta).toFixed(decimals);
        if (val < min) val = min;
        if (val > max) val = max;
        setValue(val);
    }

    // Výchozí hodnota
    const defaultValue =dblDefaultValue;

    // Inicializace
    if (!setValue(defaultValue)) {
        setValue(min);
    }

    // Události
    input.addEventListener('keydown', (e) => {
        if (e.key === 'ArrowUp') {
            e.preventDefault();
            changeValue(step);
        } else if (e.key === 'ArrowDown') {
            e.preventDefault();
            changeValue(-step);
        } else if (e.key === 'Enter') {
            input.blur();
        } else if (e.key === 'Escape') {
            input.blur();
        }
    });

    

    input.addEventListener('input', () => {
        const val = parseInput(input.value);
        if (val === null) {
            hiddenInput.value = "0";
            errorMsg.textContent = "";
            return;
        }
        
        const err = validateValue(val);
        errorMsg.textContent = err;
        
    });

   

    input.addEventListener('blur', () => {
        const val = parseInput(input.value);
        if (val === null || validateValue(val)) {
            setValue(0);
        } else {
            setValue(val);
        }
    });

    input.addEventListener('focus', () => {
        input.select();
    });


    function formatForServer(val) {
        // převede číslo na string s českou desetinnou čárkou, bez tisícových oddělovačů
        if (val === null || val=="") return '0';
        let str = val.replaceAll(".", "").replaceAll(" ", "");
        //let str = val.toFixed(decimals); // zafixuje počet desetinných míst
        return str;
    }
}