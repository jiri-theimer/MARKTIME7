// Dashboard (Views/Widgets/Index.cshtml) – logika plochy.
// Konfigurace z view: window._wd = { x54id, p28id, j02idme, j02idsys, skin, cols, autorefresh, count, txt: {...} }.
// Přetahování: SortableJS (umí i dotyk); rozložení se ukládá ve stejném formátu jako dříve jQuery UI ("sort=1&sort=2|sort=3…").
// Obsah widgetů se vkládá přes jQuery .html() – SQL widgety s grafem obsahují <script>, který innerHTML nespustí.
// Globální názvy funkcí zůstávají (volají je taghelpery a onclick v markupu).
"use strict";

const _wdc = window._wd || {};
// zpětná kompatibilita: dříve globální proměnné stránky
var _x54id = String(_wdc.x54id);
var _p28id = String(_wdc.p28id);
var _j02idme = String(_wdc.j02idme);
var _j02idsys = String(_wdc.j02idsys);
var _skin = _wdc.skin;
var _ColumnsPerPage = String(_wdc.cols);

// ------------------------------------------------------------------ pomocné

async function wd_post(url, data) {
    const r = await fetch(_ep(url), {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8", "X-Requested-With": "XMLHttpRequest" },
        body: new URLSearchParams(data || {})
    });
    if (!r.ok) throw new Error("HTTP " + r.status);
    return r.json();
}
async function wd_get(url, data) {
    const r = await fetch(_ep(url) + (data ? (url.includes("?") ? "&" : "?") + new URLSearchParams(data) : ""), { headers: { "X-Requested-With": "XMLHttpRequest" } });
    if (!r.ok) throw new Error("HTTP " + r.status);
    return r.json();
}
const wd_t = (key, fallback) => (_wdc.txt && _wdc.txt[key]) || fallback;
const wd_err = (e) => _notify_message(wd_t("error", "Chyba") + (e && e.message ? ": " + e.message : ""));
const wd_el = (id) => document.getElementById(id);

// ------------------------------------------------------------------ start

document.addEventListener("DOMContentLoaded", () => {
    wd_el("cmddashboard")?.classList.add("active");

    // přetahování widgetů mezi sloupci
    if (window.Sortable) {
        document.querySelectorAll(".wd-board > .column").forEach(col => {
            Sortable.create(col, {
                group: "wd-board",
                handle: ".portlet-header",
                filter: "a, button, input, select, .dropdown-menu",
                preventOnFilter: false,      // klik na tlačítka v hlavičce musí projít
                draggable: ".portlet",
                animation: 150,
                ghostClass: "portlet-placeholder",
                chosenClass: "wd-chosen",
                dragClass: "wd-drag",
                delayOnTouchOnly: true,
                delay: 200,                  // na dotyku dlouhé podržení = přetažení, krátký tah = posun stránky
                onEnd: (e) => { if (e.from !== e.to || e.oldIndex !== e.newIndex) save_dock_state(); }
            });
        });
    }

    if (window._device && _device.type === "Phone") {
        const cols = wd_el("ColumnsPerPage");
        if (cols) cols.style.display = "none";
    }

    if (_j02idsys !== _j02idme || String(_wdc.count) === "0") {
        $(".showmore").toggle();
    }

    // hledání v galerii widgetů
    const search = wd_el("wd_search");
    if (search) {
        search.addEventListener("input", () => {
            const q = search.value.toLowerCase().trim();
            document.querySelectorAll("#rp1_menu .wd-item").forEach(li => { li.style.display = (q === "" || li.dataset.name.includes(q)) ? "" : "none"; });
            document.querySelectorAll("#rp1_menu .wd-cat").forEach(li => { li.style.display = q === "" ? "" : "none"; });
        });
        wd_el("offcanvas_widgets")?.addEventListener("shown.bs.offcanvas", () => search.focus());
    }

    // datum DNES / klient → přenačíst plochu
    $("#d0helper").on("change", () => reload());

    reload_all_widgets();

    // automatické obnovování: jen obsah widgetů (ne celá stránka – zůstane pozice posunu)
    if (_wdc.autorefresh > 0) {
        setInterval(reload_all_widgets, _wdc.autorefresh * 1000);
    }
});

// ------------------------------------------------------------------ obsah widgetů

function wd_widget_ids() {
    return Array.from(document.querySelectorAll(".wd-board .portlet")).map(p => p.id.replace("box_", ""));
}

function reload_all_widgets() {
    wd_widget_ids().forEach(id => load_widget_content(id));     // paralelně; každý widget řeší svou chybu sám
}

async function load_widget_content(x55id) {
    const box = wd_el("box_content_" + x55id);
    if (!box) return;
    const h = Math.max(box.clientHeight, 80);
    box.innerHTML = `<div class='wd-skeleton' style='height:${h}px;'><span></span><span></span><span></span></div>`;

    let data;
    try {
        data = await wd_post("/Widgets/GetWidgetHtmlContent", {
            x55id: x55id, columnsperpage: _ColumnsPerPage, d: wd_el("d0helper")?.value || "", j02id: _j02idme, p28id: _p28id
        });
    } catch (e) {
        box.innerHTML = "";
        const msg = document.createElement("div");
        msg.className = "wd-load-error";
        msg.textContent = wd_t("loadError", "Obsah widgetu se nepodařilo načíst.") + " ";
        const again = document.createElement("a");
        again.href = "javascript:void(0)";
        again.textContent = wd_t("retry", "Zkusit znovu");
        again.addEventListener("click", () => load_widget_content(x55id));
        msg.appendChild(again);
        box.appendChild(msg);
        return;
    }

    $(box).html(data.x55Content);           // jQuery kvůli spuštění <script> v obsahu (grafy SQL widgetů)
    const head = wd_el("box_header_" + x55id);
    if (head) head.textContent = data.x55Name;
    if ((data.x55Content || "").indexOf("iframe") > 0) box.style.height = "360px";

    if (data.isUseDatatables) {
        //splněna podmínka pro práci s pluginem DataTables
        if (data.x55DataTablesButtons == 3) datatables_init_buttons_export_print_pdf(data.x55Code, "cs_CZ");
        else if (data.x55DataTablesButtons == 2) datatables_init_buttons_export_print(data.x55Code, "cs_CZ");
        else datatables_init_nobuttons(data.x55Code, "cs_CZ");
    }
}

// ------------------------------------------------------------------ rozložení

function save_dock_state() {
    // stejný formát jako jQuery UI sortable("serialize", {key:"sort"}): "sort=12&sort=15|sort=7"
    const cols = ["col1", "col2", "col3"].map(wd_el).filter(Boolean);
    const s = cols.map(col => Array.from(col.querySelectorAll(":scope > .portlet")).map(p => "sort=" + p.id.replace("box_", "")).join("&")).join("|");
    wd_post("/Widgets/SaveWidgetState", { s: s, skin: _skin, x54id: _x54id })
        .then(() => _notify_message(wd_t("saved", "Změna rozložení plochy uložena."), "info"))
        .catch(wd_err);
}

// ------------------------------------------------------------------ navigace a nastavení

function reload() {
    let url = "/Widgets/Index?skin=" + encodeURIComponent(_skin) + "&x54id=" + _x54id;
    const d = wd_el("d0helper");
    if (d) url += "&d=" + encodeURIComponent(d.value);
    const j = wd_el("j02id_me");
    if (j && j.length > 1) url += "&j02id=" + j.value;
    _redirect(url);
}

function j02id_change(j02id) { reload(); }

function x54id_onchange(cbx) {
    _notify_message("Loading...", "info");
    wd_post("/Widgets/ChangeX54ID", { x54id: cbx.value }).then(() => { _x54id = String(cbx.value); reload(); }).catch(wd_err);
}

function p28id_dropdown_onchange(cbx) { p28id_reload(cbx.value); }

function p28id_reload(p28id) {
    _notify_message("Loading...", "info");
    wd_post("/Widgets/ChangeP28ID", { p28id: p28id }).then(() => { _p28id = String(p28id); reload(); }).catch(wd_err);
}

function handle_after_close_snippet(x55id) {
    wd_post("/Widgets/RemoveWidget", { x55id: x55id, skin: _skin, x54id: _x54id }).then(reload).catch(wd_err);
}

function handle_insert_snippet(x55id) {
    wd_post("/Widgets/InsertWidget", { x55id: x55id, skin: _skin, x54id: _x54id }).then(reload).catch(wd_err);
}

function handle_pocet_sloupcu(cbx) {
    wd_post("/Widgets/SavePocetSloupcu", { x: cbx.value, skin: _skin, x54id: _x54id }).then(reload).catch(wd_err);
}

function handle_autorefresh(cbx) {
    wd_post("/Widgets/SavePageAutoRefresh", { x: cbx.value, skin: _skin, x54id: _x54id }).then(reload).catch(wd_err);
}

function handle_change_skin(cbx) {
    wd_post("/Widgets/ChangeSkin", { skin: cbx.value, x54id: _x54id })
        .then(() => location.replace(_ep("/Widgets/Index?skin=" + encodeURIComponent(cbx.value) + "&x54id=" + _x54id)))
        .catch(wd_err);
}

function widget_help(x55id) { _window_open("/x55/Help?pid=" + x55id); }

function clear_to_factory_silent() {
    wd_post("/Widgets/Clear2FactoryState", { skin: _skin, x54id: _x54id }).then(reload).catch(wd_err);
}
function clear_to_factory_confirm() { clear_to_factory_silent(); }     //zpětná kompatibilita – potvrzení dělá dvoukrokové tlačítko

function set_as_global_silent() {
    wd_post("/Widgets/SetAsDefaultGlobalBoxes", { skin: _skin, x54id: _x54id })
        .then(() => _notify_message(wd_t("globalSet", "Plocha nastavena jako výchozí pro všechny uživatele."), "info")).catch(wd_err);
}
function update2allusers_silent() {
    wd_post("/Widgets/UpdateBoxes2AllUsers", { skin: _skin, x54id: _x54id })
        .then(() => _notify_message(wd_t("allUpdated", "Obsah plochy přepsán všem uživatelům."), "info")).catch(wd_err);
}
function set_as_global() { set_as_global_silent(); }         //zpětná kompatibilita
function update2allusers() { update2allusers_silent(); }

// dvoukrokové potvrzení místo confirm(): první klik tlačítko „nabije“, druhý do 4 s provede akci
function wd_confirm(btn, fn) {
    if (btn.classList.contains("wd-armed")) {
        btn.classList.remove("wd-armed");
        btn.innerHTML = btn.dataset.orig;
        fn();
        return;
    }
    btn.dataset.orig = btn.innerHTML;
    btn.classList.add("wd-armed");
    btn.textContent = btn.getAttribute("data-confirm");
    setTimeout(() => {
        if (btn.classList.contains("wd-armed")) { btn.classList.remove("wd-armed"); btn.innerHTML = btn.dataset.orig; }
    }, 4000);
}

// ------------------------------------------------------------------ AI widgety

function aiw_pivot(x55id) { _window_open("/Widgets/AiPivot?x55id=" + x55id, 2); }

function aiw_delete(x55id) {
    wd_post("/Widgets/AiDelete", { x55id: x55id, skin: _skin, x54id: _x54id })
        .then(r => { if (r && r.flag === 0) { _notify_message(r.message); return; } reload(); })     //BO.Result: Flag Failed = 0
        .catch(wd_err);
}

async function aiw_edit(x55id) {
    let r;
    try { r = await wd_get("/Widgets/AiEditInfo", { x55id: x55id }); } catch (e) { wd_err(e); return; }
    if (!r || !r.ok) { _notify_message((r && r.error) || wd_t("error", "Chyba")); return; }

    wd_el("aiw_edit_id").value = r.x55id;
    wd_el("aiw_edit_name").value = r.name;

    const per = wd_el("aiw_edit_period");
    per.innerHTML = "";
    if (r.hasPeriod) {
        per.add(new Option(wd_t("fixedPeriod", "Pevné") + (r.fixedText ? ": " + r.fixedText : ""), "0"));
        r.periods.forEach(x => per.add(new Option(x.name + (x.interval ? " (" + x.interval + ")" : ""), String(x.pid))));
        per.value = String(r.periodPid);
        wd_el("aiw_edit_period_box").style.display = "";
    } else {
        wd_el("aiw_edit_period_box").style.display = "none";
    }

    const lim = wd_el("aiw_edit_limit");
    lim.innerHTML = "";
    const opts = [5, 10, 15, 20].filter(n => n <= r.limitMax);
    if (!opts.includes(r.limit)) opts.push(r.limit);
    opts.sort((a, b) => a - b).forEach(n => lim.add(new Option(String(n), String(n))));
    lim.value = String(r.limit);

    wd_el("aiw_edit_ask").href = "/AiAsk?editx55=" + r.x55id;
    wd_el("aiw_edit_ask_box").style.display = r.canAsk ? "" : "none";
    bootstrap.Offcanvas.getOrCreateInstance(wd_el("offcanvas_aiedit")).show();
}

async function aiw_edit_save() {
    const x55id = wd_el("aiw_edit_id").value;
    let r;
    try {
        r = await wd_post("/Widgets/AiUpdate", {
            x55id: x55id, name: wd_el("aiw_edit_name").value,
            periodPid: wd_el("aiw_edit_period").value || 0, limit: wd_el("aiw_edit_limit").value
        });
    } catch (e) { wd_err(e); return; }
    if (!r || !r.ok) { _notify_message((r && r.error) || wd_t("error", "Chyba")); return; }
    bootstrap.Offcanvas.getOrCreateInstance(wd_el("offcanvas_aiedit")).hide();
    load_widget_content(x55id);
}

// ------------------------------------------------------------------ DataTables (jQuery plugin – tabulkové SQL widgety)

function datatables_init_nobuttons(tableid, lang) {
    $("#" + tableid).DataTable({
        stateSave: true,
        language: { url: "/lib/datatables/localisation/" + lang + ".json" },
        dom: "Blfrtip",
        paging: true
    });
}
function datatables_init_buttons_export_print(tableid, lang) {
    $("#" + tableid).DataTable({
        stateSave: true,
        language: { url: "/lib/datatables/localisation/" + lang + ".json" },
        dom: "Blfrtip",
        buttons: [
            { extend: "excelHtml5", footer: true, text: "XLS" },
            { extend: "csvHtml5", footer: true },
            { extend: "copyHtml5", footer: true },
            { extend: "print", text: wd_t("print", "Tisk"), footer: true, autoPrint: true }
        ],
        paging: true
    });
}
function datatables_init_buttons_export_print_pdf(tableid, lang) {
    $("#" + tableid).DataTable({
        stateSave: true,
        language: { url: "/lib/datatables/localisation/" + lang + ".json" },
        dom: "Blfrtip",
        buttons: [
            { extend: "pdfHtml5", footer: true, orientation: "portrait", pageSize: "A4", text: wd_t("pdfPortrait", "PDF (na výšku)") },
            { extend: "pdfHtml5", footer: true, orientation: "landscape", pageSize: "A4", text: wd_t("pdfLandscape", "PDF (na šířku)") },
            { extend: "excelHtml5", footer: true, text: "XLS" },
            { extend: "csvHtml5", footer: true },
            { extend: "copyHtml5", footer: true },
            { extend: "print", text: wd_t("print", "Tisk"), footer: true, autoPrint: true }
        ],
        paging: true
    });
}
