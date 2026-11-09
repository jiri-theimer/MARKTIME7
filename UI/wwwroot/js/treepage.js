// Strom projektů (Views/TreePage/Index.cshtml). Konfigurace z view: window._tp = { groupby, pid, binquery, j72id, txt: {...} }.
// Globální názvy funkcí zůstávají – volají je markup, partialy filtrů (change_statequery) a site.js.
"use strict";

const _tpc = window._tp || {};
var _groupby = _tpc.groupby;
var _pid = String(_tpc.pid || "0");

const tp_t = (key, fallback) => (_tpc.txt && _tpc.txt[key]) || fallback;
const CLS_HIT_SUMMARY = "tree-hit-summary";
const CLS_HIT_LEAF = "tree-hit-leaf";

let _tpSearchActive = false;        // při hledání se nezavírají sourozenecké větve (accordion)
let _tpSuppressAccordionUntil = 0;  // programové otevření větví (výběr záznamu při načtení) – accordion nespouštět

$(document).ready(function () {
    register_menu("cmdgridquerymenu", "cmdgridquerymenu_menu", "/Menu/grid_query?prefix=p41&j72id=" + (_tpc.j72id || 0));

    const key = "p41-treepage_panel1_size";
    let defWidth = null;
    try { defWidth = localStorage.getItem(key); } catch (e) { }
    if (defWidth == null) {
        defWidth = 400;
        try { localStorage.setItem(key, defWidth); } catch (e) { }
    }
    _splitter_init("2", "p41-treepage");
    _splitter_resize_after_init("2", defWidth);

    //záložky: adresy, počty, skryté načtené záložky, adresa v prohlížeči, výška (wwwroot/js/recpage.js)
    RecTabs.init({
        tabsPrefix: "p41", pid: _pid, tabParamKey: "treepage-tab-p41",
        extraParams: { caller: "grid" },
        history: { pid: "pid", tab: "tab" }, heightBox: "divTabContent"
    });

    _mainmenu_highlight_current("cmdTreePage");

    if (String(_tpc.binquery) !== "1") {
        $(".showmore").toggle();
    }

    tree1_init();

    if (_pid !== "0") {
        selectTreeLeafById(_pid);
        leftgrid_handle_rowselect(_pid);
    }
});

// ------------------------------------------------------------------ záložky a nastavení

function tabclick(tab) {    //uložit aktuální záložku do profilu uživatele + zobrazit (RecTabs)
    return RecTabs.tabClick(tab);
}

function reload() {
    _redirect("/TreePage/Index?groupby=" + encodeURIComponent(_groupby) + "&pid=" + _pid);
}

function rcm_from_tabs(e) {
    e.target.setAttribute("menu_je_inicializovano", null);  //vyčistit paměť o inicializaci menu v rámci tohoto tlačítka
    _cm(e, "p41", _pid, "grid");
}

function change_statequery(val) {
    reload();
}

function reload_setting(el) {
    $.post(_ep("/Common/SetUserParam"), { key: "treepage-groupby", value: el.value }, function () {
        _groupby = el.value;
        reload();
    });
}

function reload_setting_mask(cbx) {
    $.post(_ep("/Common/SetUserParam"), { key: "treepage-projectmask", value: cbx.value }, function () {
        reload();
    });
}

// schválit/vyúčtovat úkony projektu vybraného ve stromu (tg_approve z layoutshare.js čte výběr gridu, který tu není)
function tree_approve() {
    if (!_pid || _pid === "0") {
        _notify_message(tp_t("noProject", "Nejdříve vyberte projekt ve stromu."));
        return;
    }
    let url = "/p31approveinput/Index?prefix=p41&millis=" + new Date().getTime();
    const sq = document.getElementById("_p31StateQueryHiddenValue");
    if (sq && sq.value !== "0") {
        url += "&p31statequery=" + sq.value;
    }
    tg_approve_open_finalurl(url, "p41", _pid);
}

function handle_render_tabs(pid) { RecTabs.renderBadges(pid); }     //zpětná kompatibilita
function callback_render_tabs(data) { RecTabs.applyBadges(data); }

// ------------------------------------------------------------------ strom

function tp_parent_details(el) {
    return el.parentElement ? el.parentElement.closest("details.branch") : null;
}

function tree1_init() {
    const tree = document.getElementById("tree1");
    if (!tree) return;

    // accordion: otevření větve zavře její sourozence (ne při hledání a ne při programovém otevření)
    tree.querySelectorAll("details.branch").forEach(d => {
        d.addEventListener("toggle", () => {
            if (!d.open || _tpSearchActive || performance.now() < _tpSuppressAccordionUntil) return;
            const parent = tp_parent_details(d);
            const siblings = Array.from((parent || tree).querySelectorAll(":scope > details.branch"));
            siblings.forEach(other => { if (other !== d && other.open) other.open = false; });
        });
    });

    tree.addEventListener("click", e => {
        const summary = e.target.closest("summary");
        if (summary) {
            const pid = summary.dataset.id;
            if (summary.dataset.prefix === "p41" && pid && pid !== _pid) {
                leftgrid_handle_rowselect(pid);
            }
            return;   // prohlížeč normálně otevře/zavře <details>
        }
        const leafLink = e.target.closest("a[data-id]");
        if (leafLink && leafLink.dataset.prefix === "p41") {
            leftgrid_handle_rowselect(leafLink.dataset.id);
        }
    });
}

function leftgrid_handle_rowselect(pid) {
    pid = String(pid);
    RecTabs.select(pid);        //adresy záložek, obsah aktivní záložky, počty, adresa v prohlížeči

    const tree = document.getElementById("tree1");
    tree.querySelectorAll("a.nav-link.active, summary.active").forEach(el => el.classList.remove("active"));
    const leaf = tree.querySelector(`a[data-id="${CSS.escape(pid)}"]`) || tree.querySelector(`summary[data-id="${CSS.escape(pid)}"]`);
    if (leaf) leaf.classList.add("active");

    _pid = pid;
    $("#pid").val(pid);
}

function selectTreeLeafById(leafId) {
    const tree = document.getElementById("tree1");
    if (!tree) return;
    const id = CSS.escape(String(leafId));
    const leaf = tree.querySelector(`a[data-id="${id}"][data-prefix="p41"]`) || tree.querySelector(`summary[data-id="${id}"][data-prefix="p41"]`);
    if (!leaf) return;

    _tpSuppressAccordionUntil = performance.now() + 500;
    let d = leaf.closest("details.branch");
    while (d) {
        d.open = true;
        d = tp_parent_details(d);
    }

    tree.querySelectorAll("a.nav-link.active, summary.active").forEach(el => el.classList.remove("active"));
    leaf.classList.add("active");
    leaf.scrollIntoView({ behavior: "smooth", block: "center" });
}

// ------------------------------------------------------------------ hledání

function tp_search_info(text) {
    const el = document.getElementById("tp_search_info");
    if (!el) return;
    el.textContent = text || "";
    el.style.display = text ? "block" : "none";
}

function tp_clear_hits(tree) {
    tree.querySelectorAll("." + CLS_HIT_SUMMARY + ", ." + CLS_HIT_LEAF).forEach(el => el.classList.remove(CLS_HIT_SUMMARY, CLS_HIT_LEAF));
}

function searchTree(input) {
    const tree = document.getElementById("tree1");
    if (!tree) return;
    const query = (input?.value || "").trim();
    const allDetails = Array.from(tree.querySelectorAll("details.branch"));

    tp_clear_hits(tree);

    if (query.length < 3) {
        _tpSearchActive = false;
        allDetails.forEach(d => (d.open = false));
        tp_search_info(query.length > 0 ? tp_t("minChars", "Zadejte aspoň 3 znaky.") : "");
        if (_pid !== "0") selectTreeLeafById(_pid);       // vrátit se k vybranému projektu
        return;
    }

    _tpSearchActive = true;
    const q = query.toLocaleLowerCase();
    const keepOpen = new Set();
    let first = null, count = 0;

    // větve (summary) i listy na všech úrovních – včetně listů přímo v kořeni stromu
    tree.querySelectorAll("summary, .leaf > a[data-id]").forEach(el => {
        if (!(el.textContent || "").toLocaleLowerCase().includes(q)) return;
        const isSummary = el.tagName === "SUMMARY";
        el.classList.add(isSummary ? CLS_HIT_SUMMARY : CLS_HIT_LEAF);
        if (el.dataset.prefix === "p41") count++;
        if (!first) first = el;
        let d = isSummary ? el.parentElement : el.closest("details.branch");
        while (d) {
            keepOpen.add(d);
            d = tp_parent_details(d);
        }
    });

    allDetails.forEach(d => (d.open = keepOpen.has(d)));
    tp_search_info(first ? tp_t("hits", "Nalezeno") + ": " + count : tp_t("noHits", "Nic nenalezeno."));

    if (first) {
        requestAnimationFrame(() => first.scrollIntoView({ behavior: "smooth", block: "center" }));
    }
}

function clearTreeSearch() {
    const input = document.getElementById("treeSearch");
    input.value = "";
    searchTree(input);
    input.focus();
}
