function mycheckboxlist_checked(chk, hidden_id, value) {

    if (chk.checked === true) {

        $("#" + hidden_id).val(value);

    } else {

        $("#" + hidden_id).val("0");
    }


}
function myradiolist_checked(hidden_id, value, event_after_changevalue) {
    $("#" + hidden_id).val(value);

    if (event_after_changevalue !== "") {
        eval(event_after_changevalue + "('" + value + "')");
    }
}
function mynumber_changevalue(ctlid,newval, decimaldigits) {
    //ruční změna hodnoty čísla
    $("#num" + ctlid).val(newval);
   
    mynumber_blur(document.getElementById("num" + ctlid), decimaldigits);
    
}
function mynumber_blur(ctl, decimaldigits) {
    var num = 0;
    if (ctl.getAttribute("type") === "text") {
        var s = $(ctl).val().replace(/\s/g, '').replace(",", ".");
        num = Number(s);
    } else {
        num = Number($(ctl).val());
    }

    //val = num.toFixed(decimaldigits).replace(/\B(?=(\d{3})+(?!\d))/g, " ");
    val = num.toFixed(decimaldigits);

    var forid = $(ctl).attr("for-id");   //hidden id prvku pro spojení s hostitelským view   
    $("#" + forid).val(val.replace(".", ","));    //pro uložení na server v rámci hostitelského view, je třeba předávat desetinnou čárku a nikoliv tečku!

    //dále pro zformátování čísla navenek    
    if (val.indexOf(".") > 0) {
        arr = val.split(".");
        arr[0] = arr[0].replace(/\B(?=(\d{3})+(?!\d))/g, " ");
        val = arr[0] + "," + arr[1];
    } else {
        val = val.replace(/\B(?=(\d{3})+(?!\d))/g, " ");
    }


    val = val.replace(/[.]/g, ',');

    ctl.setAttribute("type", "text");
    ctl.value = val;





}

function formatNumber(num) {
    return num.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,')
}


function mynumber_focus(ctl) {

    var s = ctl.value;
    s = s.replace(/\s/g, '');
    s = s.replace(/[,]/g, '.');
    ctl.value = s;

    var strBrowser = getBrowser();
    if (strBrowser !== "firefox" && strBrowser !== "edge") {
        ctl.setAttribute("type", "number");
    }
    ctl.select();



}



function getBrowser() {
    var browser_name = '';
    isIE = /*@cc_on!@*/false || !!document.documentMode;
    isEdge = !isIE && !!window.StyleMedia;
    if (navigator.userAgent.indexOf("Chrome") !== -1 && !isEdge) {
        browser_name = "chrome";
    }
    else if (navigator.userAgent.indexOf("Safari") !== -1 && !isEdge) {
        browser_name = "safari";
    }
    else if (navigator.userAgent.indexOf("Firefox") !== -1) {
        browser_name = "firefox";
    }
    else if ((navigator.userAgent.indexOf("MSIE") !== -1) || (!!document.documentMode === true)) //IF IE > 10
    {
        browser_name = "ie";
    }
    else if (isEdge) {
        browser_name = "edge";
    }
    else {
        browser_name = "other-browser";
    }

    return browser_name;
}


function datepicker_init(inputid) {

    $("#" + inputid).datepicker({
        format: "dd.mm.yyyy",
        todayBtn: "linked",
        clearBtn: true,
        language: "cs",
        multidate: false,
        todayHighlight: true,
        autoclose: true
    }).on("hide", function (e) {
       
        e.stopPropagation();
        
    });

    
}

function datepicker_init_multidate(inputid) {

    $("#" + inputid).datepicker({
        format: "dd.mm.yyyy",
        todayBtn: "linked",
        clearBtn: true,
        language: "cs",
        multidate: true,
        todayHighlight: true,
        autoclose: false
    });
}

function datepicker_button_click(inputid) {
    $("#" + inputid).focus();
}

function datepicker_change(ctl) {
    var forid = $(ctl).attr("for-id");   //hidden id prvku pro spojení s hostitelským view   
    var s = ctl.value;
    if (document.getElementById(forid + "_Time")) {
        s = s + " " + document.getElementById(forid + "_Time").value;
    }
    if (s.trim() === "00:00") s = "";   //je třeba to vyčistit, aby se do db uložila null value

    $("#" + forid).val(s);    //pro uložení na server v rámci hostitelského view


}
function datepicker_time_change(ctl) {

    var forid = $(ctl).attr("for-id");   //hidden id prvku pro spojení s hostitelským view   
    var s = $("#" + forid + "helper").val() + " " + $(ctl).val();

    $("#" + forid).val(s);    //pro uložení na server v rámci hostitelského view

}
function datepicker_get_value(ctlClientID) {
    var value = $("#" + ctlClientID + "helper").datepicker("getDate");
    return (value);
}
function datepicker_set_value(ctlClientID, datValue) {
    $("#" + ctlClientID + "helper").datepicker("setDate", datValue);
}




/*_MyAutoComplete*/
function myautocomplete_init_manual(input_id, items_pipe_delimited) {
    
    var arr = items_pipe_delimited.split("|");

    $("#" + input_id).autocomplete({
        source: [arr],
        visibleLimit: 20,
        openOnFocus: true,
        highlight: false,
        autoselect: true
    });

}

function myautocomplete_init(c) {
    
    $("#" + c.controlid).on("mouseover", function () {
        
        if ($("#" + c.controlid).prop("filled") === true || $("#" + c.controlid).prop("filling") === true) return;    //datalist už bylo dříve načten
        
        $("#" + c.controlid).prop("filling", true); //momentálně běží plnění
                
        handle_load_options();
                        
    });
    $("#" + c.controlid).on("focus", function () {
        if ($("#" + c.controlid).prop("filled") === true) {
            return;
        }
        return;
        handle_load_options();

    });

    
    function handle_load_options() {
        var strCurPlaceHolder = $("#" + c.controlid).attr("placeholder");
        $("#" + c.controlid).attr("placeholder", "Loading...");
        $.post(_ep(c.posturl), { o15flag: c.o15flag }, function (data) {            
            var arr = data.split("|");
            $("#" + c.controlid).autocomplete({
                source: [arr],
                visibleLimit: 20,
                openOnFocus: true,
                highlight: false,
                autoselect: true
            });

            $("#" + c.controlid).prop("filled", true);

            $("#" + c.controlid).on("focus", function (e, data) {
                $(this).select();
            });
            
            

            $("#" + c.controlid).attr("placeholder", strCurPlaceHolder);

        });
    }
    
}

function myhours_setting() {
    _window_open(_ep("/p31hes/Index",3));
}



function myhours_init(c) {

    $("#" + c.controlid).on("mouseover", function () {
        
        if ($("#" + c.controlid).prop("filled") === true || $("#" + c.controlid).prop("filling") === true) return;    //datalist už bylo dříve načten

        $("#" + c.controlid).prop("filling", true); //momentálně běží plnění
        
        handle_setup_myhours();

    });
    $("#" + c.controlid).on("focus", function () {
        if ($("#" + c.controlid).prop("filled") === true) {
            return;
        }

        handle_setup_myhours();

    });


    function handle_setup_myhours() {        
        var arr = c.intervals.split("|");
        $("#" + c.controlid).autocomplete({
            source: [arr],
            visibleLimit: 20,
            openOnFocus: true,
            highlight: false,
            autoselect: true
        });

        $("#" + c.controlid).prop("filled", true);

        $("#" + c.controlid).on("focus", function (e, data) {
            $(this).select();
        });
    }

}




function mystitky_multiselect(event, entity) {
    var o51ids = $("#TagPids").val();
    _zoom(event, null, null, 250, "♣Oštítkovat...", "/o51/MultiSelect?entity=" + entity + "&o51ids=" + o51ids);

}


/*taghelper mycombochecklist*/
/*taghelper mycombochecklist – výběr více položek (hledání, Vše/Nic, počet). Vzhled: mcl-* v site.css.
  Hodnoty beze změny: skrytý input c.controlid = pid oddělené čárkou, c.aliasid = názvy oddělené ★ */
function mycombochecklist_init(c) {
    var $menu = $("#divDropdown" + c.controlid);
    var $data = $("#divData" + c.controlid);
    var $btn = $("#cmdCombo" + c.controlid);
    var $search = $menu.find(".mcl-search");
    var $count = $menu.find(".mcl-count");
    var $onlysel = $menu.find(".mcl-onlysel-chk");
    var container = document.getElementById("divDropdownContainer" + c.controlid);
    var filled = false, loading = false;

    function norm(s) { return (s || "").toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g, ""); }
    function esc(s) { return $("<div>").text(s).html(); }

    function render_button(lbls) {
        var $t = $btn.find(".mcl-text"), $b = $btn.find(".mcl-badge");
        if (lbls.length === 0) {
            $t.text(c.placeholder).removeClass("mcl-has");
            $btn.attr("title", "");
            $b.hide();
        } else {
            var s = lbls.join(", ");
            $t.text(s).addClass("mcl-has");
            $btn.attr("title", lbls.join("\n"));
            if (lbls.length > 1) { $b.text(lbls.length).show(); } else { $b.hide(); }
        }
    }

    //výchozí text tlačítka z aliasu (★ i čárka = oddělovač, jak bylo dosud)
    var deftext = $("#" + c.aliasid).val();
    render_button(deftext ? deftext.split(/[★,]/).map(function (x) { return x.trim(); }).filter(function (x) { return x !== ""; }) : []);

    $menu.on("click.bs.dropdown", function (e) { e.stopPropagation(); });   //klik uvnitř nezavírá

    function items() { return $data.find("label.mcl-item"); }

    function update_count() {
        var $all = $data.find("input:checkbox[name='chk" + c.controlid + "']");
        var n = $all.filter(":checked").length;
        $count.text(n + " z " + $all.length);
    }

    function apply_filter() {
        var words = norm($search.val()).split(/\s+/).filter(function (w) { return w !== ""; });
        var onlysel = $onlysel.prop("checked");
        items().each(function () {
            var $sp = $(this).find("span").first();
            var txt = $sp.data("txt");
            if (txt === undefined) { txt = $sp.text(); $sp.data("txt", txt); }
            var n = norm(txt), ok = true;
            for (var i = 0; i < words.length; i++) { if (n.indexOf(words[i]) < 0) { ok = false; break; } }
            if (ok && onlysel && !$(this).find("input").prop("checked")) ok = false;
            $(this).toggle(ok);
            $sp.html(ok && words.length > 0 ? mark(txt, words) : esc(txt));
        });
        $data.find(".mcl-group").each(function () {     //skupina bez viditelných položek se skryje
            var vis = $(this).nextUntil(".mcl-group", "label.mcl-item").filter(function () { return this.style.display !== "none"; }).length;
            $(this).toggle(vis > 0);
        });
        $data.find(".mcl-empty").remove();
        if (items().filter(function () { return this.style.display !== "none"; }).length === 0 && items().length > 0) {
            $data.find(".mcl-list").append("<div class='mcl-empty'>Nic nenalezeno</div>");
        }
    }

    function mark(txt, words) {       //zvýraznění bez ohledu na diakritiku (normalizace zachová délku po odstranění znamének → mapa indexů)
        var map = [], n = "";
        for (var i = 0; i < txt.length; i++) {
            var ch = norm(txt[i]);
            for (var k = 0; k < ch.length; k++) { n += ch[k]; map.push(i); }
        }
        var hit = new Array(txt.length).fill(false);
        words.forEach(function (w) {
            var p = n.indexOf(w);
            while (p >= 0) { for (var j = p; j < p + w.length; j++) hit[map[j]] = true; p = n.indexOf(w, p + w.length); }
        });
        var out = "", open = false;
        for (var i2 = 0; i2 < txt.length; i2++) {
            if (hit[i2] && !open) { out += "<mark class='mtc-mark'>"; open = true; }
            if (!hit[i2] && open) { out += "</mark>"; open = false; }
            out += esc(txt[i2]);
        }
        if (open) out += "</mark>";
        return out;
    }

    function commit() {         //zápis hodnot + text tlačítka + jedno volání on_after_change
        var vals = [], lbls = [];
        $data.find("input:checkbox[name='chk" + c.controlid + "']:checked").each(function () {
            vals.push($(this).val());
            var $sp = $(this).closest("label").find("span").first();
            lbls.push($sp.data("txt") !== undefined ? $sp.data("txt") : $sp.text());
        });
        $("#" + c.controlid).val(vals.join(","));
        $("#" + c.aliasid).val(lbls.join("★"));
        render_button(lbls);
        update_count();
        if (c.on_after_change !== "") {
            eval(c.on_after_change + "('" + vals.join(",") + "')");
        }
    }

    function sort_selected_first() {     //při otevření vybrané nahoru (jen bez skupin, aby se nerozbilo seskupení)
        var $list = $data.find(".mcl-list");
        if ($list.find(".mcl-group").length > 0) return;
        var $sel = items().filter(function () { return $(this).find("input").prop("checked"); });
        $list.prepend($sel);
    }

    function load() {
        if (filled || loading) return;
        loading = true;
        $data.html("<div class='mcl-empty'>Načítám…</div>");
        $.post(_ep(c.posturl), { controlid: c.controlid, entity: c.entity, selectedvalues: c.selectedvalues, masterprefix: c.masterprefix, masterpid: c.masterpid, myqueryinline: c.myqueryinline }, function (data) {
            $data.html(data);
            filled = true;
            loading = false;
            //aktuální stav skrytého inputu má přednost (hodnoty mohly být změněny v klientovi)
            var cur = ($("#" + c.controlid).val() || "").split(",");
            $data.find("input:checkbox[name='chk" + c.controlid + "']").each(function () {
                this.checked = cur.indexOf(this.value) >= 0;
            });
            sort_selected_first();
            update_count();
            apply_filter();
        }).fail(function () { loading = false; $data.html("<div class='mcl-empty'>Chyba načtení</div>"); });
    }

    $data.on("change", "input:checkbox[name='chk" + c.controlid + "']", function () {
        commit();
        if ($onlysel.prop("checked")) apply_filter();
    });

    function bulk(state) {
        var changed = false;
        items().filter(function () { return this.style.display !== "none"; }).find("input:checkbox").each(function () {
            if (this.checked !== state) { this.checked = state; changed = true; }
        });
        if (changed) commit();
        if ($onlysel.prop("checked")) apply_filter();
    }
    $menu.find(".mcl-all").on("click", function () { bulk(true); });
    $menu.find(".mcl-none").on("click", function () { bulk(false); });
    $onlysel.on("change", apply_filter);
    $search.on("input", apply_filter);

    $search.on("keydown", function (e) {
        if (e.key === "ArrowDown") {
            e.preventDefault();
            items().filter(function () { return this.style.display !== "none"; }).first().find("input").focus();
        } else if (e.key === "Enter") {
            e.preventDefault();
            var $vis = items().filter(function () { return this.style.display !== "none"; });
            if ($vis.length === 1) { var ch = $vis.find("input")[0]; ch.checked = !ch.checked; commit(); }
        }
    });
    $data.on("keydown", "input:checkbox", function (e) {
        var $vis = items().filter(function () { return this.style.display !== "none"; });
        var i = $vis.index($(this).closest("label"));
        if (e.key === "ArrowDown" && i < $vis.length - 1) { e.preventDefault(); $vis.eq(i + 1).find("input").focus(); }
        else if (e.key === "ArrowUp") { e.preventDefault(); if (i > 0) { $vis.eq(i - 1).find("input").focus(); } else { $search.focus(); } }
        else if (e.key === "Enter") { e.preventDefault(); this.checked = !this.checked; commit(); }
    });

    container.addEventListener("show.bs.dropdown", function () { load(); });
    container.addEventListener("shown.bs.dropdown", function () {
        if (filled) { sort_selected_first(); }
        $search.trigger("focus");
    });
    container.addEventListener("hidden.bs.dropdown", function () {
        if ($search.val() !== "" || $onlysel.prop("checked")) { $search.val(""); $onlysel.prop("checked", false); apply_filter(); }
    });
}




/*taghelper mysearch*/
function mysearch_init(c) {
    var _mysearch_serverfiltering_timeout;

    $("#divDropdown" + c.controlid).on("click.bs.dropdown", function (e) {
        e.stopPropagation();                                    //click na dropdown oblast nemá zavírat dropdown div
    });
    $("#" + c.controlid).click(function () {
        $("#cmdCombo" + c.controlid).dropdown("toggle");        //click na textbox se má chovat stejně jako click na tlačítko cmdCombo
        $("#" + c.controlid).focus();
    });

    $("#divDropdownContainer" + c.controlid).on("show.bs.dropdown", function (e) {

        $("#divDropdown" + c.controlid).css("margin-left", 25 + $("#divDropdown" + c.controlid).width() * -1);      //šírka dropdown oblasti má být zleva 100% jako celý usercontrol
        
        var expr = $("#" + c.controlid).val();
        if (expr !== "") {

            return;
        }
        $.post(_ep(c.posturl) + "Setting", { controlid: c.controlid, entity: c.entity }, function (data) {

            $("#divData" + c.controlid).html(data);

        });

        $("#divDropdown" + c.controlid).css("top", "500px");

    });

    $("#" + c.controlid).focus(function () {
        //$(this).select();
    });

    $("#" + c.controlid).on("input", function (e) {
        if ($("#divDropdown" + c.controlid).css("display") === "none") {
            $("#divDropdown" + c.controlid).dropdown("show");   //zobrazit dropdown, pokud je skrytý
        }


        if (typeof _mysearch_serverfiltering_timeout !== "undefined") {
            clearTimeout(_mysearch_serverfiltering_timeout);
        }
        _mysearch_serverfiltering_timeout = setTimeout(function () {
            //čeká se 500ms až uživatel napíše všechny znaky
            handle_mysearch_server_filtering();

        }, 500);


    });

    function handle_mysearch_server_filtering() {
        var expr = $("#" + c.controlid).val();

        $.post(_ep(c.posturl), { entity: c.entity, searchstring: expr }, function (data) {
            
            $("#divData" + c.controlid).html(data);
            
            var xx = document.getElementById("divData" + c.controlid);
            var ofs = $(xx).offset();

            if (xx.scrollHeight > xx.clientHeight && $(xx).height() < 500 && window.innerHeight - ofs.top > 500) {        //je vidět scrollbara -> zvýšit výšku divData                      
                $("#divData" + c.controlid).css("height", "500px");
            }
            

            $("#hovado .txs").on("click", function () {                
                var pid = $(this).attr("data-v");
                
                if (c.on_after_search !== "") {
                    eval(c.on_after_search + "('" + pid + "')");
                } else {
                    location.replace(_ep("/" + c.entity.substring(0, 3) + "/Recpage?pid=" + pid));
                }

                
            });


        });



    }


}

function mysearch_save_setting() {
    var strTopRecs = $("#mysearch_toprecs").val();
    var keys = [];
    var vals = [];

    keys.push("mysearch_toprecs");
    vals.push($("#mysearch_toprecs").val());
    keys.push("mysearch_closedrecs");
    vals.push($("#mysearch_closedrecs").val());

    $.post(_re("Common/SetUserParams"), { keys: keys, values: vals }, function (data) {

        _notify_message("Nastavení bylo uloženo.", "info");

    });

    alert(s);
}


function mytree_init(expandall) {

    var toggler = document.getElementsByClassName("caret");
    var i;
    var s = $("#treeState1").val();
    var arr = [];
    if (s !== "") {
        arr = s.split(",");
    }


    for (i = 0; i < toggler.length; i++) {
        toggler[i].addEventListener("click", function () {
            if (event.target.id === "") {
                return; //je třeba klikat na id=rx...
            }
            this.parentElement.querySelector(".branch_nested").classList.toggle("branch_active");

            this.classList.toggle("caret-down");


            var intIndex = arr.indexOf(this.id);

            if ($(this).hasClass("caret-down")) {
                if (intIndex === -1) {
                    arr.push(this.id);
                }
            } else {
                if (intIndex > -1) {
                    arr.splice(intIndex, 1);
                }

            }

            $("#treeState1").val(arr.join(","));


        });
    }

    if (expandall === true) {
        $(".caret").each(function () {

            $(this).click();


        });
    }
    


}


function myselect_focus(e, cbx, entity, textfield,orderfield) { //zpracování událost focus pro taghelper mySelect
    
    if ($(cbx).prop("naplneno")) {
        return; //dropdown byl již dříve naplněn daty
    }

    var firstrow = "";
    var curval = $(cbx).val();
    
    if (cbx.options.length > 0) {
        firstrow = "<option value='" + cbx.options[0].value + "'>" + cbx.options[0].text + "</option>"; //hack kvůli vizualiazci dropdown
    }

    
    for (var i = 1; i <= 10; i++) {
        $(cbx).append(new Option("Loading..."));
        
    }   
    $(cbx).val("0");
    
  
    $.post(_re("TheCombo/GetMySelectHtmlOptions"), { textfield: textfield, entity: entity, orderfield: orderfield }, function (data) {

        $(cbx).html(firstrow + data);


        $(cbx).prop("naplneno", "1");               

        $(cbx).html(firstrow + data);

        $(cbx).val(curval);
       
        var x = $(cbx).find(":selected").index();
        //$(cbx).attr("size", 10);
        cbx.options[x].scrollIntoView(false);
        //$(cbx).attr("size", 1);

        
        
    });
    

}



function _autocomplete_setup(txtid, datasource, delimiter) {
    
    var arr = datasource.split(delimiter);

    $("#" + txtid).autocomplete({
        source: [arr],
        visibleLimit: 20,
        openOnFocus: true,
        highlight: false,
        autoselect: true
    });

    $("#" + txtid).on("focus", function (e, data) {
        $(this).select();
    });
}

function _showmore_toggle(cmd,classname_target)
{
    $("." + classname_target).toggle();
    if ($(cmd).html().indexOf("arrow_right") > 0)
    {
        $(cmd).html("<span class='material-icons'>keyboard_double_arrow_left</span>");
    } else
    {
        $(cmd).html("<span class='material-icons'>keyboard_double_arrow_right</span>");
    }
    
    
    
}

function _showmorevertical_toggle(cmd, classname_target,label) {
    $("." + classname_target).toggle();
    if ($(cmd).html().indexOf("arrow_up") > 0) {
        $(cmd).html("<span>"+label+"</span><span class='material-icons'>keyboard_arrow_down</span>");
    } else {
        $(cmd).html("<span>"+label+"</span><span class='material-icons'>keyboard_arrow_up</span>");
    }



}

function _hrefnic()
{

}

function _najit_text(txt,event, field,j02id) {    
    var ofs = $(txt).offset();
    var w = _device.availWidth - ofs.left;

    _zoom(event, null, null, w, "Najít text...", "/Record/NajitText?field=" + field+"&j02id="+j02id);

}

function _html_editor(txt, event, textareaid, j02id) {
    var ofs = $(txt).offset();
    var w = _device.availWidth - ofs.left;
    const s = document.getElementById(textareaid).value
    
    _zoom(event, null, null, w, "Html editor...", "/Record/HtmlEditor?textareaid=" + textareaid + "&j02id=" + j02id+"&html="+s);

}



function _detect_textarea_content(textarea) {
    const text = (typeof textarea === 'string' ? textarea : textarea.value).trim();

    if (!text) return 'empty';

    const htmlPattern = /<([a-zA-Z][a-zA-Z0-9]*)\b[^>]*>(.*?)<\/\1>|<([a-zA-Z][a-zA-Z0-9]*)\b[^>]*\/>/s;
    const htmlEntities = /&(amp|lt|gt|nbsp|quot|apos|#\d+|#x[0-9a-fA-F]+);/;
    const doctype = /<!DOCTYPE\s+html/i;
    const htmlComment = /<!--[\s\S]*?-->/;

    let score = 0;

    if (htmlPattern.test(text)) score += 3;
    if (htmlEntities.test(text)) score += 2;
    if (doctype.test(text)) score += 3;
    if (htmlComment.test(text)) score += 2;

    // Počet tagů
    const tagCount = (text.match(/<[a-zA-Z][^>]*>/g) || []).length;
    if (tagCount >= 2) score += 2;
    if (tagCount >= 5) score += 2;

    return score >= 3 ? 'html' : 'plaintext';
}