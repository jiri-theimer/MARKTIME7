// Sdílená logika záložek stránky záznamu: Record/RecPage, TheGrid/MasterView, TreePage/Index.
// Stránky volají RecTabs.init({...}) a své globální funkce (leftgrid_handle_rowselect, thegrid_handle_rowselect,
// tabclick, handle_render_tabs, callback_render_tabs) jen přesměrují sem.
//
// Co řeší:
//  - adresy záložek: při změně záznamu se nastaví parametry pid / master_pid / go2pid (dříve replaceAll "pid=…" v textu adresy),
//  - počty na záložkách (TheGrid/getTabs): rozběhnutý požadavek se při výběru dalšího záznamu zruší → vždy počty k poslednímu záznamu,
//  - už načtené záložky se při přepínání jen skryjí a znovu ukážou (bez nového načtení stránky v iframe),
//  - adresa v prohlížeči (history.replaceState) nese vybraný záznam a záložku → F5, Zpět i odkaz kolegovi vrátí stejný stav,
//  - výška obsahu záložek i po změně velikosti okna.
"use strict";

const RecTabs = (() => {
    const PID_PARAMS = ["pid", "master_pid", "go2pid"];
    const MAX_CACHED = 6;                   // kolik skrytých záložek držet
    const CACHE_TTL = 5 * 60 * 1000;        // starší skrytá záložka se při návratu načte znovu

    let cfg = {};
    let pid = "0";
    let badgeAbort = null;
    const cache = new Map();                // klíč záložky → skrytý iframe

    const links = () => Array.from(document.querySelectorAll("#navtabs a[target='fra_subgrid']"));
    const activeLink = () => document.querySelector("#navtabs a[target='fra_subgrid'].active");
    const frame = () => document.getElementById("fra_subgrid");
    const keyOf = (a) => (a.id || "").replace(/^tab/, "");
    const isJs = (href) => !href || href.trim().toLowerCase().startsWith("javascript:");

    function normUrl(href) {
        try { const u = new URL(href, location.href); return u.pathname + u.search; } catch (e) { return href || ""; }
    }

    // nastaví pid do známých parametrů adresy (jen pokud v adrese jsou); extra = parametry, které se nastaví vždy
    function withPid(href, newPid, extra) {
        if (isJs(href)) return href;
        let u;
        try { u = new URL(href, location.href); } catch (e) { return href; }
        PID_PARAMS.forEach(k => { if (u.searchParams.has(k)) u.searchParams.set(k, newPid); });
        if (extra) Object.keys(extra).forEach(k => { if (extra[k] !== null && extra[k] !== undefined && extra[k] !== "") u.searchParams.set(k, extra[k]); });
        return u.pathname + u.search + u.hash;
    }

    // ------------------------------------------------------------------ iframe záložek

    function wire(f) {
        f.addEventListener("load", () => {
            let loc = null;
            try { loc = f.contentWindow.location; } catch (e) { }
            if (loc && loc.href === "about:blank") return;          //prázdný iframe před nastavením src se nepočítá
            f.dataset.cur = loc ? loc.pathname + loc.search : "";
            // opakované načtení viditelné záložky (uložení záznamu, obnovení, přechod uvnitř) = data se mohla změnit → skryté záložky zahodit
            if (f.dataset.loadedOnce === "1" && f.id === "fra_subgrid") dropCache();
            f.dataset.loadedOnce = "1";
        });
    }

    function dropCache() {
        cache.forEach(f => f.remove());
        cache.clear();
    }

    function park(f) {
        if (!f.dataset.tab || !cfg.keepAlive) { return false; }
        f.removeAttribute("id");
        f.name = "fra_subgrid_" + f.dataset.tab;
        f.style.display = "none";
        f.dataset.ts = String(Date.now());
        if (cache.has(f.dataset.tab)) cache.get(f.dataset.tab).remove();
        cache.set(f.dataset.tab, f);
        while (cache.size > MAX_CACHED) {
            const oldest = cache.keys().next().value;
            cache.get(oldest).remove();
            cache.delete(oldest);
        }
        return true;
    }

    function activate(f, prev) {
        f.id = "fra_subgrid";
        f.name = "fra_subgrid";
        f.style.display = "";
        if (prev) {     // výšku nastavují splitter / stránka jen aktuálnímu iframe
            if (prev.getAttribute("height")) f.setAttribute("height", prev.getAttribute("height"));
            if (prev.style.height) f.style.height = prev.style.height;
        }
    }

    //telefon: záložky jsou v jednom vodorovně posuvném řádku (site.css) – aktivní posunout do pohledu
    function revealTab(a) {
        if (!a || !window._device || !_device.isMobile) return;
        const ul = a.closest("#navtabs");
        if (!ul) return;
        const li = a.closest("li") || a;
        const l = li.offsetLeft, r = l + li.offsetWidth;
        if (l < ul.scrollLeft) ul.scrollLeft = Math.max(0, l - 40);
        else if (r > ul.scrollLeft + ul.clientWidth) ul.scrollLeft = r - ul.clientWidth + 40;
    }

    function showTab(a) {
        revealTab(a);
        const href = a.getAttribute("href");
        const key = keyOf(a);
        const cur = frame();
        if (!cur) return;

        if (!cfg.keepAlive) {
            cur.dataset.tab = key;
            cur.src = href;
            return;
        }
        if (cur.dataset.tab === key && cur.dataset.url === normUrl(href)) return;     // už je zobrazená

        let next = cache.get(key);
        const fresh = next && next.dataset.url === normUrl(href) && next.dataset.cur === normUrl(href)
            && (Date.now() - Number(next.dataset.ts || 0)) < CACHE_TTL;
        if (next) cache.delete(key);

        if (!fresh) {
            if (next) next.remove();
            next = cur.cloneNode(false);
            next.removeAttribute("id");
            next.removeAttribute("src");
            next.dataset.loadedOnce = "";
            next.dataset.cur = "";
            next.dataset.tab = key;
            next.dataset.url = normUrl(href);
            next.src = href;                        //src před vložením do stránky (jinak se nejdřív načte about:blank)
            wire(next);
            cur.parentNode.insertBefore(next, cur.nextSibling);
        }

        if (!park(cur)) cur.remove();
        activate(next, cur);
    }

    // ------------------------------------------------------------------ počty na záložkách

    function renderBadges(forPid) {
        forPid = String(forPid);
        if (!forPid || forPid === "0") return;
        if (badgeAbort) badgeAbort.abort();
        badgeAbort = new AbortController();
        const t1 = document.getElementById("tabtab1");
        if (t1) t1.classList.add("rt-loading");

        fetch(_ep("/TheGrid/getTabs"), {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8", "X-Requested-With": "XMLHttpRequest" },
            body: new URLSearchParams({ prefix: cfg.tabsPrefix, pid: forPid }),
            signal: badgeAbort.signal
        })
            .then(r => { if (!r.ok) throw new Error("HTTP " + r.status); return r.json(); })
            .then(data => { if (forPid === pid) applyBadges(data); })
            .catch(e => { if (e.name !== "AbortError" && t1) t1.classList.remove("rt-loading"); });
    }

    function applyBadges(data) {
        if (!data || !data.length) return;
        const t1 = document.getElementById("tabtab1");
        if (t1) { t1.innerHTML = data[0].name; t1.classList.remove("rt-loading"); }     // HTML ze serveru (NavTabsSupport), jako dosud
        for (let i = 1; i < data.length; i++) {
            const el = document.getElementById("tab" + data[i].entity);
            if (el) el.innerHTML = data[i].badge != null ? data[i].name + data[i].badge : data[i].name;
        }
    }

    // ------------------------------------------------------------------ adresa v prohlížeči

    function updateHistory() {
        if (!cfg.history || !window.history || !history.replaceState) return;
        try {
            const u = new URL(location.href);
            if (pid && pid !== "0") u.searchParams.set(cfg.history.pid, pid);
            const a = activeLink();
            if (a && cfg.history.tab) u.searchParams.set(cfg.history.tab, keyOf(a));
            history.replaceState(history.state, "", u.pathname + u.search + u.hash);
        } catch (e) { }
    }

    // ------------------------------------------------------------------ výška

    function fitHeight() {
        if (!cfg.heightBox) return;
        const box = document.getElementById(cfg.heightBox);
        if (!box) return;
        let h = window.innerHeight - box.getBoundingClientRect().top - window.scrollY;     //i na telefonu do spodního okraje (dříve napevno 400 px)
        box.style.height = Math.max(parseInt(h), 150) + "px";
    }

    // ------------------------------------------------------------------ veřejné API

    return {
        /**
         * @param {object} o
         *  tabsPrefix  – prefix pro TheGrid/getTabs
         *  pid         – aktuální záznam
         *  tabParamKey – uživatelský parametr naposledy vybrané záložky
         *  extraParams – parametry nastavené do adres záložek při změně záznamu (např. { caller: "grid" }, { rez: "…" })
         *  history     – { pid: "pid" | "go2pid", tab: "tab" } nebo null
         *  heightBox   – id kontejneru, jehož výška se dopočítává podle okna (nebo null – výšku řídí splitter)
         *  keepAlive   – false = každé přepnutí záložky načte stránku znovu (původní chování)
         */
        init(o) {
            cfg = Object.assign({ keepAlive: true, extraParams: null, history: null, heightBox: null }, o || {});
            pid = String(cfg.pid || "0");
            const f = frame();
            if (f) {
                wire(f);
                const a = activeLink();
                if (a) f.dataset.tab = keyOf(a);
                f.dataset.url = normUrl(f.getAttribute("src") || "");
                if (f.getAttribute("src")) f.dataset.loadedOnce = "";
            }
            fitHeight();
            revealTab(activeLink());
            let t = null;
            window.addEventListener("resize", () => { clearTimeout(t); t = setTimeout(fitHeight, 120); });
            updateHistory();
        },

        get pid() { return pid; },

        /** Změna záznamu (výběr v gridu / stromu). */
        select(newPid) {
            newPid = String(newPid);
            if (!newPid || newPid === "0") return;
            links().forEach(a => a.setAttribute("href", withPid(a.getAttribute("href"), newPid, cfg.extraParams)));
            ["linkGo2Grid", "linkMainTab"].forEach(id => {
                const a = document.getElementById(id);
                if (a && a.getAttribute("href")) a.setAttribute("href", withPid(a.getAttribute("href"), newPid, null));
            });

            const same = newPid === pid;
            if (!same) dropCache();
            const a = activeLink() || links()[0];
            const f = frame();
            if (a && f && !isJs(a.getAttribute("href")) && !(same && f.getAttribute("src"))) {    //stejný záznam (první výběr řádku po načtení) = nenačítat znovu
                f.dataset.tab = keyOf(a);
                f.dataset.url = normUrl(a.getAttribute("href"));
                f.dataset.loadedOnce = "";      // nové načtení po změně záznamu není „změna dat“
                f.src = a.getAttribute("href");
            }
            pid = newPid;
            renderBadges(pid);
            updateHistory();
        },

        /** Klik na záložku – volat z onclick="return tabclick(this)"; vrací false (navigaci obstará RecTabs). */
        tabClick(a) {
            const href = a.getAttribute("href");
            if (isJs(href)) return true;
            links().forEach(x => x.classList.remove("active"));
            a.classList.add("active");
            if (cfg.tabParamKey) {
                fetch(_ep("/Common/SetUserParam"), {
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8", "X-Requested-With": "XMLHttpRequest" },
                    body: new URLSearchParams({ key: cfg.tabParamKey, value: keyOf(a) })
                }).catch(() => { });
            }
            showTab(a);
            updateHistory();
            return false;
        },

        renderBadges,
        applyBadges,
        fitHeight,
        dropCache
    };
})();
