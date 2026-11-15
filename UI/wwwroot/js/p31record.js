
$(document).ready(function () {

    if (document.getElementById("cmdSaveAndClone")) {
        document.getElementById("toolbarRezerva").appendChild(document.getElementById("cmdSaveAndClone"));
    }

    if (document.getElementById("p32help"))
    {
        $('[data-toggle="tooltip"]').tooltip();
    }

    if (_device.isMobile && _theukon.p33id=="1")
    {
        $("#Rec_Value_Orig").attr("placeholder", "Hodiny");
    }

    
    
    
    
    document.getElementById("toolbarRezerva").appendChild(document.getElementById("cmdMore"));



    handle_billingflag();



    $("#cmdMore").click(function () {
        $("#divMore").toggle();
    });

    if ($("#PostbackOper").val() == "p32id" && _theukon.rec_pid == "0" && _theukon.p32id != "0" && $("#DefaultText").val() != "")
    {        
        //výchozí text aktivity – vkládat jako text (dříve .append = HTML)
        var strAppendText = $("#DefaultText").val();
        var strCurText = $("#Rec_p31Text").val();
        if (strCurText != "")
        {
            if (strCurText.indexOf(strAppendText) == -1) {
                $("#Rec_p31Text").val(strCurText + "\n" + strAppendText);
            }
        }
        else
        {
            $("#Rec_p31Text").val(strAppendText);
        }
        
        
        
    }
    
    if (_theukon.isdoclasttext != "")
    {        
        $("#Rec_p31Text").val($("#Rec_p31Text").val() + _theukon.isdoclasttext);   //text isdoc naimportovaného dokladu (jako text)
    }


    if (_theukon.p34id != "0" && (_theukon.p33id == "2" || _theukon.p33id == "5")) {
        $("#numRec_p31Calc_Pieces").on("input", function () {  //změna počtu kusů
            handle_recalc_after_pieces();
        });
        $("#numRec_p31Calc_PieceAmount").on("input", function () {  //změna počtu kusů
            handle_recalc_after_pieces();
        });
        $("#PiecePriceFlag").on("change", function () {
            var pocet = _string2number($("#numRec_p31Calc_Pieces").val());
            var cenaks = _string2number($("#numRec_p31Calc_PieceAmount").val());
            if (pocet * cenaks != 0) {
                handle_recalc_after_pieces();
            }

            _load_ajax_async("/Common/SetUserParam", { key: "p31/record-PiecePriceFlag", value: $(this).val() });
        })
    }

    

    if (_theukon.p34id != "0" && _theukon.p33id == "5") {
        $("#numRec_Amount_WithoutVat_Orig").on("input", function () {   //změna částky bez dph
            handle_recalc_after_bezdph();
        });



        $("#numRec_Amount_WithVat_Orig").on("input", function () {  //změna částky vč. dph
            handle_recalc_after_vcdph();
        });


        $("#numRec_VatRate_Orig").on("input", function () {     //změna dph sazby
            var bezdph = _string2number($("#Rec_Amount_WithoutVat_Orig").val());
            
            if (bezdph != 0) {                
                handle_recalc_after_bezdph();
                return;
            }
            handle_recalc_after_vcdph();

        });


        
    }

    
    $("#Rec_Value_Orig").keydown(function (event) {
        if (event.keyCode == 9) {
            event.preventDefault();
            $("#Rec_p31Text").focus();
        }
    })
    $("#Rec_TimeFrom").keydown(function (event) {
        if (event.keyCode == 9)
        {
            event.preventDefault();
            $("#Rec_TimeUntil").focus();                        
        }
    })
    $("#Rec_TimeUntil").keydown(function (event) {
        if (event.keyCode == 9) {
            event.preventDefault();
            $("#Rec_p31Text").focus();
        }
    })

    $("#Rec_p31Text").on("paste", function (e) {
        handle_clipboard_textarea(document.getElementById("Rec_p31Text"), e);
    });

    

    if (_theukon.p34id != "0" && _theukon.p33id == "1" && !_theukon.timesheet_by_minutes) {
       
        handle_setup_cas_intervals("Rec_TimeFrom", _theukon.casoddo_intervaly);
        handle_setup_cas_intervals("Rec_TimeUntil", _theukon.casoddo_intervaly);

        $("#Rec_TimeFrom").on("selected.xdsoft", function (e, data) {
            handle_recalc_duration("timefrom");
        });
        $("#Rec_TimeFrom").on("focus", function (e, data) {
            $(this).select();
        });
        $("#Rec_TimeUntil").on("selected.xdsoft", function (e, data) {
            handle_recalc_duration("timeuntil");
        });
        $("#Rec_TimeUntil").on("focus", function (e, data) {
            $(this).select();
        });

        $("#Rec_Value_Orig").on("change", function (e, data) {
            handle_recalc_duration("hours");
        });

        $("#Rec_Value_Orig").on("selected.xdsoft", function (e, data) {
            handle_recalc_duration("hours");
        });
        
    }


    if (_theukon.p54id != "0")
    {
        $("#Rec_p54ID").css("background-color", "orange");
    }
    if (_theukon.p40id != "0") {
        $("#Rec_p40ID_FixPrice").css("background-color", "pink");
    }


    $("#p31Datehelper").change(function () {
        if (_device.type == "Phone") {
            return;
        }
        var d = $("#p31Datehelper").val();
        $("#linkZoomDay").attr("data-rel", "/p31/Day?d=" + d + "&hover_by_reczoom=1&j02id=" + _theukon.j02id);
        _init_qtip_onpage();
        p31_daysum();
        
    });


    p31_daysum();

    if (_theukon.auto_open_project_searchbox == "True") {
        $("#cmdComboRec_p41ID").click();   //u nového záznamu automaticky otevřít searchbox projektů
    }
});



function hardrefresh(pid, flag) {

    _postback();
}



function levelindex_change(cbx) {
    _postback("levelindex");
}
function j02id_change(j02id) {
    _postback("j02id");
}
function p34id_change(p34id) {
    
    $("#Rec_p32ID").val("0");
    _postback("p34id");
}
function p32id_change(p32id) {
    _postback("p32id");
}
function p41id_change(p41id) {
    _postback("p41id");
}

function p56id_change(p56id) {
    
    _postback("p56id");
}


function handle_recalc_after_bezdph() {
    if (_theukon.p33id != "5") return;

    bezdph = _string2number($("#numRec_Amount_WithoutVat_Orig").val());
    var dphsazba = _string2number($("#numRec_VatRate_Orig").val());
    var vcdph = _roundnum(bezdph + (bezdph * dphsazba / 100), 2);

    var dphcastka = _roundnum(vcdph - bezdph, 2);

    mynumber_changevalue("Rec_Amount_WithVat_Orig", vcdph, 2);
    mynumber_changevalue("Rec_Amount_Vat_Orig", dphcastka, 2);

}
function handle_recalc_after_vcdph() {
    if (_theukon.p33id != "5") return;

    var vcdph = _string2number($("#numRec_Amount_WithVat_Orig").val());
    var dphsazba = _string2number($("#numRec_VatRate_Orig").val());
    var bezdph = _roundnum(vcdph / (1 + dphsazba / 100), 2);

    var dphcastka = _roundnum((vcdph - bezdph), 2);

    mynumber_changevalue("Rec_Amount_WithoutVat_Orig", bezdph, 2);
    mynumber_changevalue("Rec_Amount_Vat_Orig", dphcastka, 2);

}

function handle_recalc_after_pieces() {
    var pocet = _string2number($("#numRec_p31Calc_Pieces").val());
    var cenaks = _string2number($("#numRec_p31Calc_PieceAmount").val());
    var celkem = _roundnum(pocet * cenaks, 2);

    if ($("#PiecePriceFlag").val() == "1") {
        //kusová cena bez DPH
        mynumber_changevalue("Rec_Amount_WithoutVat_Orig", celkem, 2);
        handle_recalc_after_bezdph();
    }
    if ($("#PiecePriceFlag").val() == "2" && _theukon.p33id == "5") {
        //kusová cena vč DPH
        mynumber_changevalue("Rec_Amount_WithVat_Orig", celkem, 2);
        handle_recalc_after_vcdph();
    }

}

function simulation() {
    var d = $("#p31Datehelper").val();

    _window_open("/p31misc/RateSimulation?d=" + d + "&p41id=" + _theukon.p41id + "&p32id=" + _theukon.p32id + "&j02id=" + _theukon.j02id, 3);


}

function setting() {
    _window_open("/p31hes/Index?pagesource=timesheet", 3);
}



function handle_setup_cas_intervals(controlid, intervals) {
    var arr = intervals.split("|");
    $("#" + controlid).autocomplete({
        source: [arr],
        limit:30,
        visibleLimit: 30,
        openOnFocus: true,
        highlight: false,
        autoselect: true
    });

    $("#" + controlid).prop("filled", true);

    $("#" + controlid).on("focus", function (e, data) {
        $(this).select();
    });
}

function handle_recalc_duration(caller)     //caller: timefrom/timeuntil/hours
{    
    var t1 = $("#Rec_TimeFrom").val();
    var t2 = $("#Rec_TimeUntil").val();
    var hours = $("#Rec_Value_Orig").val();
    if (hours.indexOf(".") > -1) {
        $("#Rec_Value_Orig").val(hours.replace(".", ","));
        hours = $("#Rec_Value_Orig").val();
    }
    

    var p41id = $("#Rec_p41ID").val();

    
    
    $("#hours_message").text("");
    $.post(_ep("/p31/Record_RecalcDuration"), { caller: caller, hours: hours, timefrom: t1, timeuntil: t2, p41id: p41id, hoursformat: _theukon.hoursformat }, function (data) {
        //spočítat hodiny z t1 - t2
        if (data.error != null) {
            $("#hours_message").text(data.info);
            _notify_message(data.error);
            return;
        }
        
        $("#Rec_TimeFrom").val(data.t1);
        $("#Rec_TimeUntil").val(data.t2);        
        $("#hours_message").text(data.info);

        if (data.update_orig_hours == "true")
        {
            $("#Rec_Value_Orig").val(data.duration);            
        }
        //if (_theukon.hoursformat == "T" || caller != "hours" || (data.t1 != "00:00" && data.t2 != "00:00"))
        //{
        //    $("#Rec_Value_Orig").val(data.duration);    //data.duration vrací server vždy ve formátu HH:mm
            
        //}
        
    });
}

function set_p31date_today()
{
    var d = new Date();    
    //$("#p31Datehelper").val(_format_date(d, false));
    
    $("#p31Date").val(_format_date(d, false));

    $("#p31Datehelper").datepicker("update", d);

    if (document.getElementById("KopirovatZaznamDialog"))
    {
        $("#KopirovatZaznamDialog").css("display", "none");
    }
}

function p54id_onchange(cbx)
{
    if (cbx.value == "0") {
        $(cbx).css("background-color", "");
    } else
    {
        $(cbx).css("background-color", "orange");
    }
}

function p40id_onchange(cbx) {
    if (cbx.value == "0") {
        $(cbx).css("background-color", "");
    } else {
        $(cbx).css("background-color", "pink");
    }
}

function handle_saveandclone() {
    form1.action = "/p31/Record?oper=saveandclone";
    form1.submit();
    
}


function Rec_Value_Trimmed_onchange(txt, p33id) {
    if (p33id != undefined && p33id != 1) {
        return;
    }
    var s = $(txt).val();
    if (s.length >= 4 && !s.includes(":") && !s.includes(","))
    {
        s = s.substring(0, 2) + ":" + s.substring(2, 4);
        $(txt).val(s);
    }

    
}

// ------------------------------------------------------------------
// "Najít text": hledání v textech dříve vykázaných úkonů a vložení do popisu úkonu.
// Panel se vytváří až při prvním otevření a patří pod <body>, takže přežije částečné překreslení formuláře.
// Data: /p31/FindTexts. Texty rozhraní (přeložené) nese tlačítko #cmdFindText v atributech data-*.
// ------------------------------------------------------------------
var p31ft = (function () {
    var _panel = null, _items = [], _active = -1, _seq = 0, _timer = null, _scope = "all", _words = [];

    function T(key, def) {
        var b = document.getElementById("cmdFindText");
        return (b && b.getAttribute("data-" + key)) || def;
    }
    function txt() { return document.getElementById("Rec_p31Text"); }
    function val(id) { var c = document.getElementById(id); return c ? (parseInt(c.value, 10) || 0) : 0; }
    function esc(s) { return String(s == null ? "" : s).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;"); }
    function fold(s) { return s.normalize("NFD").replace(/[̀-ͯ]/g, "").toLowerCase(); }   //bez diakritiky, malá písmena

    function is_open() { return _panel !== null && _panel.style.display !== "none"; }

    function build() {
        _panel = document.createElement("div");
        _panel.id = "p31ft";
        _panel.className = "p31ft";
        _panel.setAttribute("role", "dialog");
        _panel.innerHTML =
            "<div class='p31ft-head'><span class='material-icons-outlined p31ft-ico' aria-hidden='true'>search</span>"
            + "<input type='search' id='p31ft_q' class='p31ft-q' autocomplete='off' spellcheck='false'>"
            + "<button type='button' class='btn-close p31ft-close' tabindex='-1' title='Esc'></button></div>"
            + "<div class='p31ft-scope'>"
            + "<button type='button' class='p31ft-chip' data-scope='all'></button>"
            + "<button type='button' class='p31ft-chip' data-scope='client'></button>"
            + "<button type='button' class='p31ft-chip' data-scope='project'></button>"
            + "<span class='p31ft-info' id='p31ft_info'></span></div>"
            + "<div class='p31ft-list' id='p31ft_list'></div>"
            + "<div class='p31ft-foot'></div>";
        document.body.appendChild(_panel);

        var q = _panel.querySelector(".p31ft-q");
        q.addEventListener("input", function () {
            clearTimeout(_timer);
            _timer = setTimeout(load, 250);
        });
        q.addEventListener("keydown", on_keydown);
        _panel.querySelector(".p31ft-close").addEventListener("click", function () { close(true); });
        _panel.querySelector(".p31ft-scope").addEventListener("click", function (e) {
            var chip = e.target.closest(".p31ft-chip");
            if (!chip || chip.disabled) return;
            set_scope(chip.getAttribute("data-scope"));
            load();
            q.focus();
        });
        _panel.querySelector(".p31ft-list").addEventListener("click", function (e) {
            var row = e.target.closest(".p31ft-item");
            if (row) choose(parseInt(row.getAttribute("data-i"), 10), e.ctrlKey || e.metaKey);
        });
        document.addEventListener("mousedown", function (e) {
            if (!is_open()) return;
            if (_panel.contains(e.target) || (e.target.closest && e.target.closest("#cmdFindText"))) return;
            close(false);   //klik mimo panel
        });
        window.addEventListener("resize", function () { if (is_open()) place(); });
    }

    function set_scope(scope) {
        if (scope !== "client" && scope !== "project") scope = "all";
        if (scope !== "all" && val("Rec_p41ID") === 0) scope = "all";     //bez vybraného projektu není podle čeho zúžit
        _scope = scope;
        try { localStorage.setItem("p31ft_scope", scope); } catch (e) { }
        var chips = _panel.querySelectorAll(".p31ft-chip");
        for (var i = 0; i < chips.length; i++) chips[i].classList.toggle("active", chips[i].getAttribute("data-scope") === _scope);
    }

    function place() {     //pod pole textu, ve stejné šířce; když dole není místo, přisadit k dolnímu okraji okna
        var t = txt();
        if (!t) return;
        var r = t.getBoundingClientRect();
        var w = Math.max(Math.min(r.width, 820), Math.min(420, window.innerWidth - 16));
        var left = Math.max(8, Math.min(r.left, window.innerWidth - w - 8));
        var maxh = 460;
        var top = r.bottom + 4;
        if (top + 280 > window.innerHeight) top = Math.max(8, window.innerHeight - Math.min(maxh, window.innerHeight - 16) - 8);
        maxh = Math.max(200, Math.min(maxh, window.innerHeight - top - 8));
        _panel.style.width = w + "px";
        _panel.style.left = (left + window.scrollX) + "px";
        _panel.style.top = (top + window.scrollY) + "px";
        _panel.style.maxHeight = maxh + "px";
    }

    function open() {
        if (!txt()) return;
        if (_panel === null) build();

        _panel.querySelector(".p31ft-q").placeholder = T("ph", "Hledat v textech dříve vykázaných úkonů…");
        _panel.querySelector(".p31ft-foot").textContent = T("hint", "Klik nebo Enter = nahradit text · Ctrl+klik nebo Ctrl+Enter = připojit na konec textu");
        var chips = _panel.querySelectorAll(".p31ft-chip");
        chips[0].textContent = T("all", "Vše");
        chips[1].textContent = T("client", "Tento klient");
        chips[2].textContent = T("project", "Tento projekt");
        var hasproject = val("Rec_p41ID") > 0;
        chips[1].disabled = !hasproject;
        chips[2].disabled = !hasproject;

        var scope = "all";
        try { scope = localStorage.getItem("p31ft_scope") || "all"; } catch (e) { }
        _panel.style.display = "flex";
        set_scope(scope);
        place();

        var q = _panel.querySelector(".p31ft-q");
        q.value = "";
        q.focus();
        load();
    }

    function close(focus_text) {
        if (!is_open()) return;
        clearTimeout(_timer);
        _seq++;     //rozpracovaná odpověď serveru se už nevykreslí
        _panel.style.display = "none";
        if (focus_text && txt()) txt().focus();
    }

    function toggle() {
        if (is_open()) close(true); else open();
    }

    function status(text, spinner) {
        _items = [];
        _active = -1;
        document.getElementById("p31ft_info").textContent = "";
        document.getElementById("p31ft_list").innerHTML = "<div class='p31ft-status'>" + (spinner ? "<span class='spinner-border spinner-border-sm me-2' role='status'></span>" : "") + esc(text) + "</div>";
    }

    function load() {
        var q = _panel.querySelector(".p31ft-q").value.trim();
        var seq = ++_seq;
        if (_items.length === 0) status(T("loading", "Načítám…"), true);

        var btn = document.getElementById("cmdFindText");
        var pars = {
            q: q,
            j02id: val("Rec_j02ID") || (btn ? parseInt(btn.getAttribute("data-j02id"), 10) || 0 : 0),
            p41id: val("Rec_p41ID"),
            scope: _scope,
            exclude_pid: btn ? parseInt(btn.getAttribute("data-pid"), 10) || 0 : 0
        };
        $.post(_ep("/p31/FindTexts"), pars, function (data) {
            if (seq !== _seq || !is_open()) return;     //mezitím uživatel psal dál nebo panel zavřel
            _words = fold(q).split(/\s+/).filter(function (w) { return w.length >= 2; });
            render(data, q);
        }).fail(function () {
            if (seq !== _seq || !is_open()) return;
            status(T("error", "Texty se nepodařilo načíst."), false);
        });
    }

    function render(data, q) {
        _items = (data && data.items) ? data.items : [];
        _active = -1;
        var list = document.getElementById("p31ft_list");
        if (_items.length === 0) {
            status(q === "" ? T("norecent", "Zatím žádné úkony s textem.") : T("empty", "Žádný text neodpovídá hledání."), false);
            return;
        }
        document.getElementById("p31ft_info").textContent = (q === "" ? T("recent", "Naposledy použité texty") : T("found", "Nalezeno") + ": " + _items.length + (data.more ? "+" : ""));

        var s = [];
        for (var i = 0; i < _items.length; i++) {
            var c = _items[i];
            var meta = [esc(c.date)];
            var kde = [c.client, c.project].filter(function (x) { return x; }).join(" – ");
            if (kde) meta.push(esc(kde));
            if (c.activity) meta.push(esc(c.activity));
            s.push("<div class='p31ft-item' data-i='" + i + "'><div class='p31ft-text'>" + mark(c.text) + "</div>"
                + "<div class='p31ft-meta'>" + meta.join(" · ") + (c.count > 1 ? "<span class='p31ft-count' title='" + esc(T("counttitle", "Kolikrát byl text použit")) + "'>" + c.count + "×</span>" : "") + "</div></div>");
        }
        list.innerHTML = s.join("");
        list.scrollTop = 0;
    }

    //zvýraznění hledaných slov bez ohledu na diakritiku a velikost písmen (stejně hledá server)
    function mark(text) {
        if (_words.length === 0) return esc(text);
        var folded = "", map = [];
        for (var i = 0; i < text.length; i++) {
            var f = fold(text.charAt(i));
            for (var k = 0; k < f.length; k++) { folded += f.charAt(k); map.push(i); }
        }
        var on = new Array(text.length);
        _words.forEach(function (w) {
            var pos = folded.indexOf(w);
            while (pos > -1) {
                for (var m = pos; m < pos + w.length; m++) on[map[m]] = true;
                pos = folded.indexOf(w, pos + w.length);
            }
        });
        var out = "", open = false;
        for (var j = 0; j < text.length; j++) {
            if (on[j] && !open) { out += "<mark>"; open = true; }
            if (!on[j] && open) { out += "</mark>"; open = false; }
            out += esc(text.charAt(j));
        }
        if (open) out += "</mark>";
        return out;
    }

    function set_active(i) {
        var rows = _panel.querySelectorAll(".p31ft-item");
        if (rows.length === 0) return;
        if (i < 0) i = 0;
        if (i > rows.length - 1) i = rows.length - 1;
        if (_active > -1 && rows[_active]) rows[_active].classList.remove("active");
        _active = i;
        rows[i].classList.add("active");
        rows[i].scrollIntoView({ block: "nearest" });
    }

    function on_keydown(e) {
        if (e.key === "ArrowDown") { e.preventDefault(); set_active(_active + 1); }
        else if (e.key === "ArrowUp") { e.preventDefault(); set_active(_active - 1); }
        else if (e.key === "Enter") {
            e.preventDefault();     //Enter nesmí odeslat formulář
            if (_items.length > 0) choose(_active > -1 ? _active : 0, e.ctrlKey || e.metaKey);
        }
        else if (e.key === "Escape") { e.preventDefault(); e.stopPropagation(); close(true); }
    }

    //vložení textu: append=false nahradí celý popis, append=true připojí na nový řádek. Přes insertText, aby šlo vrátit Ctrl+Z.
    function choose(i, append) {
        var c = _items[i], t = txt();
        if (!c || !t) return;
        var text = c.text;
        close(false);
        t.focus();
        if (append && t.value.trim() !== "") {
            text = (/\n$/.test(t.value) ? "" : "\n") + text;
            t.setSelectionRange(t.value.length, t.value.length);
        } else {
            t.setSelectionRange(0, t.value.length);
        }
        var before = t.value, ok = false;
        try { ok = document.execCommand("insertText", false, text); } catch (e) { ok = false; }
        if (!ok || t.value === before) {
            t.value = append && before.trim() !== "" ? before + text : text;
            t.dispatchEvent(new Event("input", { bubbles: true }));
        }
        t.setSelectionRange(t.value.length, t.value.length);
        if (typeof _toolbar_warn2save_changes === "function") _toolbar_warn2save_changes();
    }

    return { toggle: toggle, open: open, close: close };
})();


function isdoc_upload()
{
    form1.enctype = "multipart/form-data";
    form1.action = "/p31/Record?oper=isdoc_upload";
    form1.submit();
}


function handle_billingflag()
{
    var textarea = document.getElementById("Rec_p31Text");
    var billingDiv = document.getElementById("divBillingLang");

    if (!textarea || !billingDiv) return;

    // vytvoření wrapperu
    var wrapper = document.createElement("div");
    wrapper.style.position = "relative";

    // vložení wrapperu před textarea
    textarea.parentNode.insertBefore(wrapper, textarea);

    // přesunutí textarea do wrapperu
    wrapper.appendChild(textarea);

    // přesunutí billingDiv do wrapperu
    wrapper.appendChild(billingDiv);

    // nastavení stylu billingDiv
    billingDiv.style.position = "absolute";
    billingDiv.style.right = "0";
    billingDiv.style.top = "-22px";
    billingDiv.style.zIndex = "10";
}


function report(prefix,pid) {
    _window_open("/ReportsClient/ReportContext/?prefix="+prefix+"&pid=" + pid, 3);
}


function zrusit_p34id_for_p31_entry()
{
    
   
    _postback("zrusit_p34id_for_p31_entry");
    
}


// naposledy použitá kombinace projekt + sešit + aktivita: vyplnit vše a jedním postbackem načíst (oper "recent")
function p31_recent(p41id, p34id, p32id) {
    const form = document.getElementById("form1");
    const setVal = (name, val) => {
        let el = form.querySelector("input[name='" + name + "']");
        if (!el) {          //aktivita bez comba (sešit ještě nevybraný) – pole doplnit
            el = document.createElement("input");
            el.type = "hidden";
            el.name = name;
            form.appendChild(el);
        }
        el.value = val;
    };
    setVal("Rec.p41ID", p41id);
    setVal("Rec.p34ID", p34id);
    setVal("Rec.p32ID", p32id);
    ["SelectedComboProject", "SelectedComboP34Name", "SelectedComboP32Name"].forEach(n => setVal(n, ""));     //názvy doplní server
    _postback("recent");
}

// souhrn dne u data (počet úkonů a hodiny vykazované osoby) – na pozadí
function p31_daysum() {
    const el = document.getElementById("p31_daysum");
    const d = $("#p31Datehelper").val();
    if (!el || !d) return;
    const t = _theukon.daysum_texts || {};
    $.get(_ep("/p31/Record_DaySum"), { d: d, j02id: _theukon.j02id }, function (r) {
        if (!r) { el.textContent = ""; return; }
        el.innerHTML = "";
        el.append((t.day || "Vykázáno") + ": ");
        const b = document.createElement("b");
        b.textContent = String(r.hours).replace(".", ",") + " " + (t.h || "h");
        el.append(b);
        el.append(" (" + (t.count || "úkonů") + ": " + r.count + ")");      //bez skloňování: „úkonů: 4“
    });
}
