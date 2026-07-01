// MO - klientské preference (localStorage)
// Klíče: mo-theme, mo-calendar-type, mo-show-weekend

(function () {
    var KEY_THEME = 'mo-theme';
    var KEY_CAL_TYPE = 'mo-calendar-type';
    var KEY_SHOW_WEEKEND = 'mo-show-weekend';

    function getPref(key, def) {
        try {
            var v = localStorage.getItem(key);
            return v === null ? def : v;
        } catch (e) { return def; }
    }

    function setPref(key, value) {
        try { localStorage.setItem(key, value); } catch (e) { /* soukromý režim apod. */ }
    }

    // ==== Skin (light/dark) ====
    window.moApplyTheme = function () {
        var theme = getPref(KEY_THEME, 'light');
        document.documentElement.setAttribute('data-theme', theme);
        return theme;
    };

    window.moSetTheme = function (theme) {
        setPref(KEY_THEME, theme);
        document.documentElement.setAttribute('data-theme', theme);
    };

    // ==== Typ kalendáře (month / week) ====
    window.moRememberCalendarType = function (type) {
        setPref(KEY_CAL_TYPE, type);
    };

    window.moGetCalendarType = function () {
        return getPref(KEY_CAL_TYPE, 'month');
    };

    // ==== Zobrazit víkend ====
    window.moRememberShowWeekend = function (value) {
        setPref(KEY_SHOW_WEEKEND, value ? '1' : '0');
    };

    // Po načtení DOMu: dosynchronizovat theme radio + všechny odkazy "Kalendář" na poslední typ
    document.addEventListener('DOMContentLoaded', function () {
        var theme = getPref(KEY_THEME, 'light');
        var radio = document.querySelector('input[name="theme-dropdown"][value="' + theme + '"]');
        if (radio) radio.checked = true;

        if (moGetCalendarType() === 'week') {
            var d = new Date();
            var iso = d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0');
            document.querySelectorAll('[data-mo-cal-link]').forEach(function (a) {
                a.setAttribute('href', '/Calendar/Week?d=' + iso);
            });
        }

        // Dynamický "zpět" odkaz (Day view) - Měsíc nebo Týden agenda podle poslední preference
        var backLink = document.getElementById('backLink');
        if (backLink) {
            var useWeek = moGetCalendarType() === 'week';
            backLink.setAttribute('href', useWeek ? backLink.dataset.weekHref : backLink.dataset.monthHref);
            var label = document.getElementById('backLinkLabel');
            if (label) label.textContent = useWeek ? backLink.dataset.weekLabel : backLink.dataset.monthLabel;
        }
    });
})();