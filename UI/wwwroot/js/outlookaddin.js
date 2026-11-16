// Panel doplňku pro Outlook (Views/OutlookAddin/Pane.cshtml).
// - Přihlášení: token z okna „Připojit“ (OutlookAddinConnect/Index → Office.context.ui.messageParent), uložený v nastavení doplňku
//   ve schránce (roamingSettings – platí ve všech Outloocích uživatele); každé volání serveru nese hlavičku Authorization: Bearer.
// - E-mail: celý jako .eml (getAsFileAsync, Mailbox 1.14); starší Outlook pošle jen předmět, odesílatele, datum a text.
// - Úkol / úkon: server e-mail uloží do fronty a panel otevře prohlížeč s MARKTIME a předvyplněným formulářem (OutlookAddin/Go).
// - FILEBOX: uloží se rovnou, bez prohlížeče.
(function () {
    "use strict";

    var TEXTS = {
        cs: {
            loading: "Načítám…",
            connectLead: "Doplněk zatím není připojený k vašemu účtu v MARKTIME.",
            connect: "Připojit k MARKTIME",
            task: "Založit úkol",
            ukon: "Vykázat úkon",
            filebox: "Uložit do FILEBOX",
            hint: "Úkol a úkon se otevřou v prohlížeči jako předvyplněný formulář MARKTIME; e-mail bude jejich přílohou.",
            disconnect: "Odpojit doplněk",
            openLink: "Otevřít formulář v MARKTIME",
            sending: "Předávám e-mail do MARKTIME…",
            opened: "Formulář se otevřel v prohlížeči. Zkontrolujte ho a uložte.",
            saved: "E-mail je uložený ve FILEBOXu.",
            popup: "Prohlížeč okno zablokoval. Otevřete formulář odkazem níže.",
            noDialog: "Tento Outlook neumí otevřít přihlašovací okno.",
            connectFailed: "Připojení se nepodařilo.",
            expired: "Připojení vypršelo nebo bylo zrušeno. Připojte doplněk znovu.",
            failed: "Akce se nepodařila.",
            noItem: "Vyberte e-mail.",
            textOnly: "Tento Outlook neumí předat celý e-mail: přenese se předmět, odesílatel a text, bez příloh."
        },
        en: {
            loading: "Loading…",
            connectLead: "The add-in is not connected to your MARKTIME account yet.",
            connect: "Connect to MARKTIME",
            task: "Create task",
            ukon: "Report time entry",
            filebox: "Save to FILEBOX",
            hint: "Task and time entry open in the browser as a pre-filled MARKTIME form; the e-mail becomes their attachment.",
            disconnect: "Disconnect add-in",
            openLink: "Open the form in MARKTIME",
            sending: "Sending the e-mail to MARKTIME…",
            opened: "The form has opened in the browser. Check it and save.",
            saved: "The e-mail is saved in FILEBOX.",
            popup: "The browser blocked the window. Open the form with the link below.",
            noDialog: "This Outlook cannot open the sign-in window.",
            connectFailed: "Connecting failed.",
            expired: "The connection expired or was revoked. Connect the add-in again.",
            failed: "The action failed.",
            noItem: "Select an e-mail.",
            textOnly: "This Outlook cannot pass the whole e-mail: subject, sender and text are transferred, without attachments."
        }
    };
    var TOKEN_KEY = "marktime_token";
    var T = TEXTS.cs, token = null, busy = false;
    var root = document.body.getAttribute("data-root") || "/";
    var origin = window.location.origin;

    function $(id) { return document.getElementById(id); }
    function show(id, on) { $(id).hidden = !on; }
    function url(path) { return origin + root + path; }
    function status(text, kind) {
        var el = $("oaStatus");
        el.textContent = text || "";
        el.hidden = !text;
        el.className = "oa-status" + (kind ? " oa-" + kind : "");
    }
    function setBusy(on) {
        busy = on;
        ["oaCmdTask", "oaCmdUkon", "oaCmdFilebox", "oaCmdConnect"].forEach(function (id) { $(id).disabled = on; });
    }
    function applyTexts() {
        var lang = "cs";
        try { lang = (Office.context.displayLanguage || "cs").toLowerCase(); } catch (e) { }
        T = (lang.indexOf("cs") === 0 || lang.indexOf("sk") === 0) ? TEXTS.cs : TEXTS.en;
        document.documentElement.lang = T === TEXTS.cs ? "cs" : "en";
        Array.prototype.forEach.call(document.querySelectorAll("[data-t]"), function (el) { el.textContent = T[el.getAttribute("data-t")] || ""; });
    }

    // ── token ────────────────────────────────────────────────────────────────────
    function tokenLoad() {
        try { var t = Office.context.roamingSettings.get(TOKEN_KEY); if (t) return t; } catch (e) { }
        try { return window.localStorage.getItem(TOKEN_KEY); } catch (e) { }
        return null;
    }
    function tokenSave(t) {
        token = t;
        try {
            if (t) Office.context.roamingSettings.set(TOKEN_KEY, t); else Office.context.roamingSettings.remove(TOKEN_KEY);
            Office.context.roamingSettings.saveAsync(function () { });
        } catch (e) { }
        try { if (t) window.localStorage.setItem(TOKEN_KEY, t); else window.localStorage.removeItem(TOKEN_KEY); } catch (e) { }
    }

    // ── server ───────────────────────────────────────────────────────────────────
    function api(path, body) {
        var opt = { method: body === undefined ? "GET" : "POST", headers: { "Authorization": "Bearer " + token }, credentials: "omit", cache: "no-store" };
        if (body !== undefined && body !== null) {
            opt.headers["Content-Type"] = "application/json";
            opt.body = JSON.stringify(body);
        }
        return fetch(url(path), opt).then(function (r) {
            if (r.status === 401) { var e = new Error(T.expired); e.unauthorized = true; throw e; }
            return r.json().catch(function () { return {}; }).then(function (data) {
                if (!r.ok) throw new Error(data.message || T.failed);
                return data;
            });
        });
    }
    function onError(e) {
        setBusy(false);
        if (e && e.unauthorized) { tokenSave(null); showConnect(); status(T.expired, "error"); return; }
        status((e && e.message) || T.failed, "error");
    }

    // ── zobrazení ────────────────────────────────────────────────────────────────
    function showConnect() {
        show("oaLoading", false); show("oaMain", false); show("oaConnect", true);
        $("oaUser").textContent = "";
    }
    function showMain(me) {
        show("oaLoading", false); show("oaConnect", false); show("oaMain", true);
        $("oaUser").textContent = me.name || me.login || "";
        show("oaCmdTask", !!me.canTask);
        show("oaCmdUkon", !!me.canUkon);
        var item = Office.context.mailbox && Office.context.mailbox.item;
        if (!item) { status(T.noItem); return; }
        $("oaSubject").textContent = item.subject || "";
        var from = item.from || item.sender;
        $("oaFrom").textContent = from ? ((from.displayName || "") + (from.emailAddress ? " <" + from.emailAddress + ">" : "")) : "";
        if (typeof item.getAsFileAsync !== "function") status(T.textOnly);
    }
    function start() {
        token = tokenLoad();
        if (!token) { showConnect(); return; }
        api("OutlookAddin/Me").then(showMain).catch(function (e) {
            if (e && e.unauthorized) { tokenSave(null); showConnect(); } else { showConnect(); status((e && e.message) || T.failed, "error"); }
        });
    }

    // ── připojení ────────────────────────────────────────────────────────────────
    function connect() {
        status("");
        if (!Office.context.ui || !Office.context.ui.displayDialogAsync) { status(T.noDialog, "error"); return; }
        Office.context.ui.displayDialogAsync(url("OutlookAddin/ConnectStart"), { height: 70, width: 35, promptBeforeOpen: false }, function (res) {
            if (res.status !== Office.AsyncResultStatus.Succeeded) { status(T.connectFailed + " " + ((res.error && res.error.message) || ""), "error"); return; }
            var dialog = res.value;
            dialog.addEventHandler(Office.EventType.DialogMessageReceived, function (arg) {
                var data = null;
                try { data = JSON.parse(arg.message); } catch (e) { }
                try { dialog.close(); } catch (e) { }
                if (data && data.token) { tokenSave(data.token); show("oaConnect", false); show("oaLoading", true); start(); }
                else { status(T.connectFailed, "error"); }
            });
        });
    }
    function disconnect(ev) {
        ev.preventDefault();
        tokenSave(null);
        status("");
        showConnect();
    }

    // ── e-mail ───────────────────────────────────────────────────────────────────
    function readMail() {
        var item = Office.context.mailbox.item;
        return new Promise(function (resolve, reject) {
            if (!item) { reject(new Error(T.noItem)); return; }
            var base = { subject: item.subject || "" };
            if (typeof item.getAsFileAsync === "function") {
                item.getAsFileAsync(function (res) {
                    if (res.status === Office.AsyncResultStatus.Succeeded && res.value) { base.eml = res.value; resolve(base); }
                    else { readText(item, base, resolve); }      // např. chráněná zpráva: aspoň text
                });
            } else {
                readText(item, base, resolve);
            }
        });
    }
    function readText(item, base, resolve) {
        var from = item.from || item.sender;
        if (from) { base.fromName = from.displayName || ""; base.fromAddress = from.emailAddress || ""; }
        try { if (item.dateTimeCreated) base.date = item.dateTimeCreated.toISOString(); } catch (e) { }
        item.body.getAsync(Office.CoercionType.Text, function (res) {
            base.body = res.status === Office.AsyncResultStatus.Succeeded ? (res.value || "") : "";
            resolve(base);
        });
    }

    // ── akce ─────────────────────────────────────────────────────────────────────
    function openBrowser(goUrl, win) {
        show("oaOpenLink", false);
        if (Office.context.ui && typeof Office.context.ui.openBrowserWindow === "function") {      // klasický Outlook, Mac: výchozí prohlížeč
            Office.context.ui.openBrowserWindow(goUrl);
            return true;
        }
        if (win && !win.closed) { win.location.href = goUrl; return true; }
        var w = window.open(goUrl, "_blank");
        if (w) return true;
        var a = $("oaOpenLink").querySelector("a");     // okno zablokováno: odkaz, na který uživatel klikne sám
        a.href = goUrl;
        show("oaOpenLink", true);
        return false;
    }
    function action(target) {
        if (busy) return;
        setBusy(true);
        status(T.sending);
        show("oaOpenLink", false);
        // na webu se okno musí otevřít hned při kliknutí (později by ho prohlížeč zablokoval); adresu dostane po předání e-mailu
        var win = null;
        if (target !== "o27" && !(Office.context.ui && typeof Office.context.ui.openBrowserWindow === "function")) {
            try { win = window.open("about:blank", "_blank"); } catch (e) { win = null; }
        }
        readMail()
            .then(function (mail) { return api("OutlookAddin/Push", mail); })
            .then(function (res) {
                if (target === "o27") {
                    return api("OutlookAddin/SaveToFilebox?guid=" + encodeURIComponent(res.guid), null).then(function () { status(T.saved, "ok"); });
                }
                var ok = openBrowser(url("OutlookAddin/Go?target=" + target + "&guid=" + encodeURIComponent(res.guid)), win);
                status(ok ? T.opened : T.popup, ok ? "ok" : "error");
            })
            .then(function () { setBusy(false); })
            .catch(function (e) { try { if (win) win.close(); } catch (x) { } onError(e); });
    }

    Office.onReady(function () {
        applyTexts();
        $("oaCmdConnect").addEventListener("click", connect);
        $("oaCmdDisconnect").addEventListener("click", disconnect);
        Array.prototype.forEach.call(document.querySelectorAll("[data-action]"), function (b) {
            b.addEventListener("click", function () { action(b.getAttribute("data-action")); });
        });
        start();
    });
})();
