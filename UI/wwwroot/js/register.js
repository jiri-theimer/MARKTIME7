// Samoobslužná registrace (Views/CreateLicenseFree/Index.cshtml a Complete.cshtml). Bez jQuery.
(function () {
    "use strict";

    function byId(id) { return document.getElementById(id); }

    function showError(text) {
        var el = byId("rgError");
        if (!el) return;
        el.textContent = text || "";
        el.hidden = !text;
        if (text) el.scrollIntoView({ block: "nearest" });
    }

    function setBusy(btn, busy) {
        if (!btn) return;
        if (busy) {
            btn.dataset.label = btn.textContent;
            if (btn.dataset.busy) btn.textContent = btn.dataset.busy;
        } else if (btn.dataset.label) {
            btn.textContent = btn.dataset.label;
        }
        btn.disabled = busy;
    }

    // Odešle pole formuláře přes fetch a vrátí odpověď serveru jako objekt; při chybě spojení objekt s textem chyby.
    // only = názvy polí, která se mají poslat (bez něj všechna).
    function post(form, url, only) {
        var body = new URLSearchParams();
        new FormData(form).forEach(function (value, name) {
            if (!only || only.indexOf(name) >= 0) body.append(name, value);
        });
        return fetch(url || form.action, {
            method: "POST",
            body: body,
            credentials: "same-origin",
            headers: { "X-Requested-With": "XMLHttpRequest" }
        }).then(function (r) {
            if (!r.ok) throw new Error("HTTP " + r.status);
            return r.json();
        }).catch(function () {
            return { ok: false, message: "Spojení se serverem se nezdařilo. Obnovte prosím stránku a zkuste to znovu." };
        });
    }

    // ── 1. zadání e-mailu ──────────────────────────────────────────────────────
    var formEmail = byId("rgForm");
    if (formEmail) {
        formEmail.addEventListener("submit", function (e) {
            e.preventDefault();
            var input = byId("rgEmail");
            var email = input.value.trim();
            if (!/^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(email)) {
                showError("Zadejte platnou e-mailovou adresu.");
                input.focus();
                return;
            }
            showError(null);
            var btn = byId("rgSend");
            setBusy(btn, true);
            post(formEmail).then(function (data) {
                setBusy(btn, false);
                if (window.grecaptcha && document.querySelector(".g-recaptcha")) {
                    try { window.grecaptcha.reset(); } catch (err) { /* captcha se ještě nenačetla */ }
                }
                if (!data.ok) {
                    showError(data.message);
                    return;
                }
                byId("rgSentTo").textContent = data.email;
                byId("rgStep1").hidden = true;
                byId("rgStep2").hidden = false;
            });
        });

        byId("rgAgain").addEventListener("click", function (e) {
            e.preventDefault();
            byId("rgStep2").hidden = true;
            byId("rgStep1").hidden = false;
            byId("rgEmail").focus();
        });
    }

    // ── 3. dokončení registrace ────────────────────────────────────────────────
    var formCreate = byId("rgCreate");
    if (formCreate) {
        var fieldErrors = function (errors) {
            formCreate.querySelectorAll(".rg-err").forEach(function (el) { el.textContent = ""; });
            formCreate.querySelectorAll(".rg-invalid").forEach(function (el) { el.classList.remove("rg-invalid"); });
            var first = null;
            (errors || []).forEach(function (err) {
                var box = formCreate.querySelector('.rg-err[data-for="' + err.field + '"]');
                var input = formCreate.querySelector('[name="' + err.field + '"]');
                if (box) box.textContent = err.message;
                if (input) {
                    input.classList.add("rg-invalid");
                    if (!first) first = input;
                }
            });
            if (first) first.focus();
        };

        var check = function () {
            var errors = [];
            var need = function (name, message) {
                var el = formCreate.querySelector('[name="' + name + '"]');
                if (el && el.value.trim() === "") errors.push({ field: name, message: message });
            };
            need("firstname", "Vyplňte jméno.");
            need("lastname", "Vyplňte příjmení.");
            need("company", "Vyplňte název firmy.");
            need("password", "Zvolte si heslo.");
            return errors;
        };

        formCreate.addEventListener("submit", function (e) {
            e.preventDefault();
            showError(null);
            var errors = check();
            fieldErrors(errors);
            if (errors.length > 0) return;

            byId("rgFormWrap").hidden = true;
            byId("rgBusy").hidden = false;
            post(formCreate).then(function (data) {
                if (data.ok) {
                    location.replace(data.redirect || "/");
                    return;
                }
                byId("rgBusy").hidden = true;
                byId("rgFormWrap").hidden = false;
                fieldErrors(data.errors);
                if (data.message) showError(data.message);
            });
        });

        var btnLoad = byId("rgLoad");
        btnLoad.addEventListener("click", function () {
            var ico = byId("rgRegID").value.replace(/\s/g, "");
            fieldErrors([]);
            if (!/^[0-9]{6,10}$/.test(ico)) {
                fieldErrors([{ field: "regid", message: "Zadejte IČO (jen číslice)." }]);
                return;
            }
            setBusy(btnLoad, true);
            post(formCreate, "/CreateLicenseFree/LoadRejstrik", ["__RequestVerificationToken", "t", "regid", "country"]).then(function (data) {
                setBusy(btnLoad, false);
                if (!data.ok) {
                    fieldErrors([{ field: "regid", message: data.message }]);
                    return;
                }
                byId("rgCompany").value = data.name;
                if (data.ico) byId("rgRegID").value = data.ico;
            });
        });

        var eye = byId("rgEye");
        if (eye) {
            eye.addEventListener("click", function () {
                var input = byId("rgPassword");
                var show = input.type === "password";
                input.type = show ? "text" : "password";
                eye.classList.toggle("lg-eye-on", show);
                input.focus();
            });
        }
    }
})();
