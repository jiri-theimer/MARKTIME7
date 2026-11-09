// Pevné tiskové sestavy (Views/ReportsClient/ReportNoContextFramework.cshtml).
// Konfigurace z view: window._rf = { callerPrefix, callerPids, selected, txt: {...} }.
"use strict";

const _rfc = window._rf || {};
const rf_t = (key, fallback) => (_rfc.txt && _rfc.txt[key]) || fallback;
const rf_el = (id) => document.getElementById(id);

document.addEventListener("DOMContentLoaded", () => {
    const panel = rf_el("reportsPanel");
    const oc = bootstrap.Offcanvas.getOrCreateInstance(panel);

    panel.addEventListener("shown.bs.offcanvas", () => {
        document.body.classList.add("offcanvas-push");
        window.dispatchEvent(new Event("resize"));
        if (!_rfc.selected) rf_el("rf_search")?.focus();
    });
    panel.addEventListener("hidden.bs.offcanvas", () => {
        document.body.classList.remove("offcanvas-push");
        window.dispatchEvent(new Event("resize"));
    });
    oc.show();

    // výběr sestavy (tlačítka s data-* – žádné javascript: odkazy s názvem v řetězci)
    rf_el("rf_list")?.addEventListener("click", (e) => {
        const btn = e.target.closest(".rf-item");
        if (btn) showrep(btn.dataset.id, btn.dataset.name, btn.dataset.code, btn.dataset.format);
    });

    // hledání
    const search = rf_el("rf_search");
    if (search) {
        search.addEventListener("input", () => rf_filter(search.value));
        search.addEventListener("keydown", (e) => {
            if (e.key === "Enter") {
                e.preventDefault();
                const first = document.querySelector("#rf_list .rf-item:not([hidden])");
                if (first && search.value.trim() !== "") first.click();
            } else if (e.key === "Escape" && search.value !== "") {
                e.preventDefault();
                e.stopPropagation();        // Esc jen vymaže hledání, panel nezavírá
                search.value = "";
                rf_filter("");
            }
        });
    }

    rf_resize();
    let t = null;
    window.addEventListener("resize", () => { clearTimeout(t); t = setTimeout(rf_resize, 100); });

    if (_rfc.selected) {
        const btn = rf_el("report" + _rfc.selected);
        if (btn) {
            showrep(btn.dataset.id, btn.dataset.name, btn.dataset.code, btn.dataset.format);
            requestAnimationFrame(() => btn.scrollIntoView({ block: "center" }));
        }
    }
});

// výška oblasti sestavy podle okna (dříve pevně 100vh - 76px)
function rf_resize() {
    const area = rf_el("reportArea");
    if (!area) return;
    const h = window.innerHeight - area.getBoundingClientRect().top - window.scrollY;
    area.style.height = Math.max(parseInt(h), 300) + "px";
}

// zachováno kvůli zpětné kompatibilitě (globální funkce)
function showrep(x31id, x31name, x31code, x31format) {
    const name = rf_el("spanX31Name");
    name.textContent = x31name || rf_t("choose", "Vyberte tiskovou sestavu…");
    name.title = x31code ? "Report code: " + x31code : "";
    const code = rf_el("spanX31Code");
    if (code) code.textContent = x31code || "";

    let tail = "";
    if (_rfc.callerPrefix) {
        tail = "&caller_prefix=" + encodeURIComponent(_rfc.callerPrefix) + "&caller_pids=" + encodeURIComponent(_rfc.callerPids || "");
    }
    const page = x31format === "XLSX" ? "ReportNoContextXls" : "ReportNoContext";
    rf_el("fra1").src = "/ReportsClient/" + page + "?x31id=" + encodeURIComponent(x31id) + tail;

    document.querySelectorAll("#rf_list .rf-item.oznaceno").forEach(b => b.classList.remove("oznaceno"));
    const btn = rf_el("report" + x31id);
    if (btn) {
        btn.classList.add("oznaceno");
        const cat = btn.closest("details.rf-cat");
        if (cat) cat.open = true;
    }
}

// filtr nabídky: název, kód, popis i kategorie; bez diakritiky a velikosti písmen
const rf_norm = (s) => (s || "").normalize("NFD").replace(/[̀-ͯ]/g, "").toLowerCase();

function rf_filter(query) {
    const words = rf_norm(query).trim().split(/\s+/).filter(Boolean);
    const cats = Array.from(document.querySelectorAll("#rf_list details.rf-cat"));
    const info = rf_el("rf_search_info");
    let total = 0;

    cats.forEach(cat => {
        if (cat.dataset.wasOpen === undefined) cat.dataset.wasOpen = cat.open ? "1" : "0";
        const catText = rf_norm(cat.dataset.cat);
        let n = 0;
        cat.querySelectorAll(".rf-item").forEach(btn => {
            const text = catText + " " + rf_norm(btn.textContent) + " " + rf_norm(btn.dataset.code);
            const hit = words.every(w => text.includes(w));
            btn.closest("li").hidden = !hit;
            btn.hidden = !hit;
            if (hit) n++;
        });
        cat.hidden = n === 0;
        total += n;
        if (words.length) cat.open = n > 0;
    });

    if (!words.length) {
        // návrat: kategorie jako před hledáním, otevřená ta s vybranou sestavou
        cats.forEach(cat => {
            cat.open = cat.dataset.wasOpen === "1" || !!cat.querySelector(".rf-item.oznaceno");
            delete cat.dataset.wasOpen;
        });
        info.style.display = "none";
        return;
    }
    info.textContent = total ? rf_t("hits", "Nalezeno") + ": " + total : rf_t("noHits", "Nic nenalezeno.");
    info.style.display = "block";
}
