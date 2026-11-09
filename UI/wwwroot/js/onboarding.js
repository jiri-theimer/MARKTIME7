// Úvodní stránka /Home/Index – „Onboarding (začínáme)“: checklist (Views/Onboarding/_Checklist.cshtml) a záložky.
// Kontejner: #obChecklist. Po návratu z dialogu (hardrefresh) nebo jiné záložky (focus) se stav načte znovu.
(function () {
    const box = document.getElementById("obChecklist");
    if (!box) return;

    const post = (url, data) => fetch(_ep(url), {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8" },
        body: new URLSearchParams(data)
    }).then(r => { if (!r.ok) throw new Error(r.status); return r.json(); });

    let loading = false;
    async function refresh() {
        if (loading) return;
        loading = true;
        try {
            const r = await fetch(_ep("/Onboarding/Checklist"), { cache: "no-store" });
            if (!r.ok) return;
            const html = await r.text();
            box.innerHTML = html;      //partial neobsahuje <script>
            const cnt = document.querySelector(".ob-tabcount");
            if (cnt) cnt.textContent = box.querySelectorAll(".ob-step.ob-done").length + "/" + box.querySelectorAll(".ob-step").length;
        } catch (e) {
            /* stav se obnoví při příštím načtení stránky */
        } finally {
            loading = false;
        }
    }

    box.addEventListener("click", async (e) => {
        const mark = e.target.closest("[data-ob-mark]");
        if (mark) {
            mark.disabled = true;
            try {
                await post("/Onboarding/Mark", { code: mark.dataset.obMark });
                await refresh();
            } catch (err) {
                mark.disabled = false;
                _notify_message("Nepodařilo se uložit.", "error");
            }
            return;
        }
    });

    // záložky: /Home/Index#zkratky, #videa
    const tabs = document.querySelectorAll("[data-ob-hash]");
    if (tabs.length && window.bootstrap) {
        const h = location.hash.replace("#", "");
        const t = Array.from(tabs).find(b => b.dataset.obHash === h);
        if (t && h) bootstrap.Tab.getOrCreateInstance(t).show();
        tabs.forEach(b => b.addEventListener("shown.bs.tab", () => {
            history.replaceState(null, "", location.pathname + location.search + (b.dataset.obHash ? "#" + b.dataset.obHash : ""));
        }));
    }

    // uložení v dialogu (p31/Record…) volá na stránce hardrefresh(); jiné stránky s vlastní funkcí nepřepisovat
    if (typeof window.hardrefresh !== "function") {
        window.hardrefresh = function () { refresh(); };
    }
    // návrat z jiné záložky (mobile.marktime.net, kalendář) nebo zavření dialogu bez uložení
    let last = Date.now();
    const onBack = () => {
        if (document.visibilityState === "visible" && Date.now() - last > 3000) { last = Date.now(); refresh(); }
    };
    window.addEventListener("focus", onBack);
    document.addEventListener("visibilitychange", onBack);
    if (typeof window._window_close === "function") {
        const orig = window._window_close;
        window._window_close = function () {
            const r = orig.apply(this, arguments);
            setTimeout(refresh, 300);
            return r;
        };
    }
})();
