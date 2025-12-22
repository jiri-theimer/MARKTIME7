function setup_ds() {
    
    var ds = new DragSelect({
        draggability: false,
        immediateDrag: false,
        selectables: document.getElementsByClassName("tabrow"),
        area: document.getElementById("tab1"),
        selectedClass: "tabrow_selected_bydrag",
        multiSelectMode: false
    });
   
    ds.subscribe("callback", (callback_object) => {
        if (callback_object.items) {
            //volá se po označení řádek
            
            if (event.target.name != undefined && event.target.name == "selrow") {
                //kliknul na checkbox
                return;
            }
            
            $(".tabrow").removeClass("tabrow_selected_bycheck");

            uncheck_all();
            
            $(callback_object.items).each(function () {
                var pid = $(this).prop("id").replace("tr", "");
                
                
                $("#chk" + pid).prop("checked", true);
            });

            
            handle_check_pids();
            refresh_highlight_all_checked();

            refresh_selected_rowscount_info();
        }
    })



    

}

function init_controls()
{
    

    $(".cm").click(function (event) {
        rcm(event, $(this).attr("data-id"));

    });

    

    $("input[name='selrow']").change(function (event) {
        
        onchecked($(this));
        
    });

    $(".editable").change(function (event) {
        var tr = $(this).closest("tr");
        var pid = $(tr).attr("id").replace("tr", "");
        var x = $(tr).attr("data-x");
        
        handle_rec_change_inline(x,pid);

    });

    $(".editable").focus(function (event)
    {
        
        if (event.target.tagName == "INPUT")
        {
            $(this).select();
            
        }
        

    });

    if (_IsInterniHodiny == "True")
    {
        $(".hodinyinterni").attr("title", "Interně schválené hodiny");
    }
    if (_IsHodinyPausal == "True")
    {
        $(".hodinypausal").attr("title", "Hodiny v paušálu");
    }
    

    if (_IsUrovenSchvalovani == "True")
    {
        $(".uroven").attr("title", "Úroveň schvalování");

        
    }

    if ($("#cbxP72ID").val() != "0")
    {
        $("#cbxP72ID").css("background-color", "red");
    }
    
    

   
    $(".cm").each(function () {
        $(this).html("<span class='material-icons-outlined'>menu</span>");
    });
    

    
}

function onchecked(chk)
{
    handle_check_pids();
    refresh_highlight_all_checked();
    
}

function edit_source_rec() {
    var pid = $("#Rec_pid").val();
    _window_open("/p31/Record?pid=" + pid + "&approve_guid=" + _guid);

}

function batch_select(cbx)
{
    if (cbx.value == "" || cbx.value == null) return;
    batch_select_what(cbx.value);
}

function batch_select_what(trcssclass) {
    

    uncheck_all();
    
    

    if (trcssclass == "check_all")
    {
        check_all();
        return;
    }
    if (trcssclass == "uncheck_all") {        
        return;
    }
    
    var rows = $("#tab1 ." + trcssclass);
    var firstpid = null;
    for (var i = 0; i < rows.length; i++) {
        $(rows[i]).addClass("tabrow_selected_bycheck");
        var pid = rows[i].id.replace("tr", "");
       
        $("#chk" + pid).prop("checked", true);

        if (firstpid == null) firstpid = pid;

    }
   
    handle_check_pids();
    refresh_highlight_all_checked();

    if (firstpid != null)
    {
        scroll_to_row(firstpid);
    }
}

function scroll_to_row(pid)
{
    if (document.getElementById("tr" + pid))
    {
        var row2scroll = document.getElementById("tr" + pid);
        row2scroll.scrollIntoView({ block: "start", behavior: "smooth" });
    }
    
}

function check_all() {
    $.each($("input[name='selrow']"), function () {
        $(this).prop("checked", true);
    });
    handle_check_pids();
    refresh_highlight_all_checked();
}
function uncheck_all() {
    $("#CheckedPids").val("");
    $.each($("input[name='selrow']:checked"), function () {
        $(this).prop("checked", false);
    });
    refresh_highlight_all_checked();
    refresh_selected_rowscount_info();
}

function handle_check_pids() {
    var pids = [];
    $.each($("input[name='selrow']:checked"), function () {
        pids.push($(this).val());
        
    });

    $("#CheckedPids").val(pids.join(","));

    refresh_selected_rowscount_info();

}

function refresh_highlight_all_checked() {
    $(".tabrow").removeClass("tabrow_selected_bydrag");
    $(".tabrow").removeClass("tabrow_selected_bycheck");

    $.each($("input[name='selrow']:checked"), function () {
        var pid = $(this).val();

        $("#tr" + pid).addClass("tabrow_selected_bycheck");

        
    });
}



function rcm(e, pid)
{

    if (pid == "" || pid == "0") {
        _notify_message("Musíte vybrat záznam z nadřízeného panelu.");
        return;
    }

    e.target.setAttribute("menu_je_inicializovano", null);  //vyčistit paměť o inicializaci menu v rámci tohoto tlačítka

    _cm(e, "p31approve", pid, "grid", $("#p31guid").val());
}

function p31_create() {
    _window_open("/p31/Record?pid=0&approve_guid=" + _guid);

}


function handle_rec_load(pid, p31guid) {
    $.post(_ep("/p31approve/LoadGridRecord"), { p31id: pid, guid: p31guid }, function (data) {
        if (data.errormessage != null) {
            _notify_message(data.errormessage);
            return;
        }
        $("#Rec_pid").val(pid);



        $("#Rec_p33id").val(data.p33id);

        $("#chkp71ID_" + data.p71id).prop("checked", true);
        if (data.p72id > 0) {
            $("#chkp72ID_" + data.p72id).prop("checked", true);
        }


        $("#Rec_Datum").html(data.datum + data.emotion);
        $("#Rec_Jmeno").text(data.jmeno);

        $("#Rec_Projekt").text(data.projekt);
        $("#Rec_Aktivita").text(data.aktivita);
        $("#Rec_Aktivita").attr("title", data.sesit);
        if (data.fakturovatelne) {
            $("#Rec_Aktivita").css("color", "green");
        } else {
            $("#Rec_Aktivita").css("color", "red");
        }
        $("#Rec_Vykazano").text(data.vykazano);
        $("#Rozdil_Vykazano").text(data.rozdil_vykazano_schvaleno_hodnota);
        $("#Rozdil_Sazba").text(data.rozdil_vykazano_schvaleno_sazba);
        $("#Rec_Popis").val(data.popis);
        $("#Rec_uroven").val(data.uroven);
        $("#j27code_bezdph").text("(" + data.j27code + "):");
        $("#j27code_sazba").text("(" + data.j27code + "):");


        if (data.p33id == 1) {
            $("#Rec_hodiny").val(data.hodiny);
            $("#Rec_hodinypausal").val(data.hodinypausal);
            $("#Rec_hodinyinterni").val(data.hodinyinterni);
        }
        if (data.p33id == 1 || data.p33id == 3) {
            mynumber_changevalue("Rec_sazba", data.sazba, 2);
            $("#spanPuvodniHodiny4").text("(" + data.vykazano + "):");
            $("#spanPuvodniHodiny6").text("(" + data.vykazano + "):");
            $("#Rec_Sazba").text(data.vykazano_sazba);
            if (data.vykazano_sazba.substring(0, 1) == "0") {
                $("#Rec_Sazba").css("color", "red");
            } else {
                $("#Rec_Sazba").css("color", "green");
            }
        }
        else {

            mynumber_changevalue("Rec_dphsazba", data.dphsazba, 0);
            mynumber_changevalue("Rec_bezdph", data.bezdph, 2);
        }

        $("#timestamp_insert").text(data.timestamp_insert);
        if (data.timestamp_insert != data.timestamp_update) {
            $("#timestamp_update").text(data.timestamp_insert);
        }

        handle_rec_change();


        //$("#tdProjekt").attr("title", data.pl);
        _resize_textareas();
    });
}



function handle_rec_change() {

    var p71id = parseInt($("input[name='Rec.p71id']:checked").val());
    var p72id = parseInt($("input[name='Rec.p72id']:checked").val());
    var p33id = parseInt($("#Rec_p33id").val());


    showhideid("trP72ID", "visible");
    showhideid("trUrovenSchvalovani", "visible");
    showhide($("[data-p33id=1]"), "visible");
    showhide($("[data-p33id=13]"), "visible");
    showhide($("[data-p33id=25]"), "visible");



    if (p71id == 0 || p71id == 2) {
        showhideid("trP72ID", "hidden");
        showhide($("[data-p33id=1]"), "hidden");
        showhide($("[data-p33id=13]"), "hidden");
        showhide($("[data-p33id=25]"), "hidden");
        showhideid("trSazba", "visible");
    }

    if (p71id == 1) {
        showhideid("trPopis", "visible");
        showhideid("trUrovenSchvalovani", "visible");
        showhideid("trUpravitZdrojovyUkon", "visible");
    } else {
        showhideid("trPopis", "hidden");
        showhideid("trUrovenSchvalovani", "hidden");
        showhideid("trUpravitZdrojovyUkon", "hidden");
    }

    if (p71id == 0) {
        showhideid("trUrovenSchvalovani", "hidden");

    }

   

    //následně už pouze skrývat (hidden) nepotřebné, protože aktuálně je vše viditelné!

    if (p33id == 1 || p33id == 3 || p72id == 6 || p72id == 3 || p72id == 2) {
        showhideid("trDphSazba", "hidden");
        showhideid("trBezDph", "hidden");


    }
    if (p33id != 1) {
        showhideid("trHodinyKFakturaci", "hidden");
        showhideid("trHodinyInterni", "hidden");

    }
    if (p33id != 1 && p33id != 3) {

        showhideid("trSazbaKFakturaci", "hidden");
        showhideid("trSazba", "hidden");

    }

    if ((p33id == 1 && p72id != 6) || p33id > 1) {
        showhideid("trHodinyVPausalu", "hidden");

    }

    if ((p33id == 1 || p33id == 3) && (p72id == 6 || p72id == 3 || p72id == 2)) {
        showhideid("trSazbaKFakturaci", "hidden");
        showhideid("trHodinyKFakturaci", "hidden");

    }

}


function showhide(c, visible) {
    if (visible == "visible" || visible == "block") {
        $(c).css("visibility", "visible");
    } else {
        $(c).css("visibility", "hidden");
    }

}

function showhide_display(c, visible) {
    
    if (visible == "visible" || visible == "block") {        
        $(c).css("display", "block");        
    } else {
        $(c).css("display", "none");
    }

}

function showhideid(element_id, visible) {
    if (!document.getElementById(element_id)) return;

    

    showhide($("#" + element_id), visible);

}



function save_temp_record(pid, p31guid) {

    var p71id = $("input[name='Rec.p71id']:checked").val();
    var p72id = $("input[name='Rec.p72id']:checked").val();
    var p33id = $("#Rec_p33id").val();

    var c = {
        p31id: pid,
        p71id: p71id,
        p72id: p72id,
        p33id: p33id,
        uroven: $("#Rec_uroven").val(),
        hodiny: $("#Rec_hodiny").val(),
        hodinyinterni: $("#Rec_hodinyinterni").val(),
        sazba: $("#numRec_sazba").val(),
        hodinypausal: $("#Rec_hodinypausal").val(),
        popis: $("#Rec_Popis").val(),
        bezdph: $("#numRec_bezdph").val(),
        dphsazba: $("#numRec_dphsazba").val()

    };

    $.post(_ep("/p31approve/SaveTempRecord"), { rec: c, p31id: pid, guid: p31guid }, function (data) {

        if (data.message != null) {
            _notify_message(data.message);
            return;
        }

        if (document.getElementById("hidTotoJeGrid")) {
            tg_post_handler("refresh");

        }
        if (document.getElementById("hidTotoJeRecord")) {
            var selpid = window.parent.document.getElementById("tg_selected_pid").value;

            window.parent.document.getElementById("batchpids").value = pid;

            if (selpid != pid) {

                window.parent.tg_go2pid(pid);
            }

            window.parent.tg_post_handler("refresh");
            window.parent.thegrid_handle_rowselect(pid);
            render_record();
        }


    });
}


function showhide_inline(element_class, pid, visible) {

    $("#tr" + pid + " ." + element_class).css("visibility", visible);

    

}

function handle_rec_change_inline(x,pid) {
    
    var p71id = parseInt($("#tr" + pid+" .p71id").val());
    var p72id = parseInt($("#tr" + pid + " .p72id").val());
    var p33id = parseInt($("#tr" + pid + " .p33id").val());

    if (p71id == 1)
    {
        showhide_inline("p72id", pid, "visible");
    } else
    {
        showhide_inline("p72id", pid, "hidden");
    }

    inline_save_temp_record(x,pid,false,true);

    inline_update_colors();
    
}


function inline_refresh_stat()
{
    $.post(_ep("/p31approve/LoadStat"), { guid: _guid }, function (data) {
        
        show_stat_val("statHodiny0", data.hodiny0);
        show_stat_val("statPenize0CZK", data.penize_czk0);
        show_stat_val("statPenize0EUR", data.penize_eur0);

        show_stat_val("statHodiny4", data.hodiny4);
        show_stat_val("statHodiny6", data.hodiny6);
        show_stat_val("statHodiny2", data.hodiny2);
        show_stat_val("statHodiny3", data.hodiny3);
        show_stat_val("statHodiny7", data.hodiny7);
        show_stat_val("statHodinyMinus", data.hodiny_minus);

        show_stat_val("statBezDPH4CZK", data.bezdph4czk);
        show_stat_val("statBezDPH4EUR", data.bezdph4eur);
        show_stat_val("statPenizeCZKMinus", data.penize_czk_minus);
        show_stat_val("statPenizeEURMinus", data.penize_eur_minus);
        
        show_stat_val("statHodiny9", data.hodiny9);
        show_stat_val("statPenizeHonorar9", data.honorar_hodiny9);
    });
}

function inline_update_colors()
{
    var rows = $(".tabrow");
    
    for (var x = 0; x < rows.length; x++)
    {
        var tr = $(rows[x]);
        var trid = $(tr).attr("id");
        var pid = $(tr).attr("id").replace("tr", "");
        var x = $(tr).attr("data-x");
       
        
        var p71id = parseInt($("#" + trid + " .p71id").val());
        var p72id = parseInt($("#" + trid + " .p72id").val());
        var p33id = parseInt($("#" + trid + " .p33id").val());

        if (p71id == 1) {
            showhide_inline("p72id", pid, "visible");
            showhide_inline("hodiny", pid, "visible");
            showhide_inline("hodinyinterni", pid, "visible");

            showhideid("divSazba" + x, "visible");
            showhideid("divCena" + x, "visible");
            showhideid("honorar_schvaleno" + x, "visible");


        }
        if (p71id == 2 || p71id == 0) {
            showhide_inline("p72id", pid, "hidden");
            showhide_inline("hodiny", pid, "hidden");
            showhide_inline("hodinypausal", pid, "hidden");
            showhide_inline("hodinyinterni", pid, "hidden");
            showhideid("divSazba" + x, "hidden");
            showhideid("divCena" + x, "hidden");
            showhideid("honorar_schvaleno" + x, "hidden");
        }

        if (p71id == 0) {
            showhide_inline("uroven", pid, "hidden");
        } else {
            showhide_inline("uroven", pid, "visible");
        }
        
        if (p72id == 2 || p72id == 3 || p72id == 6) {
            showhide_inline("hodiny", pid, "hidden");
            showhideid("divSazba"+x, "hidden");
            showhideid("divCena"+x, "hidden");
            
            
            showhideid("honorar_schvaleno"+x, "hidden");

        }

        if (p72id == 6) {
            $("#" + trid + " .p72id").css("background-color", "pink");
            $("#" + trid + " .p72id").css("color", "black");
            showhide_inline("hodinypausal", pid, "visible");
        } else {
            showhide_inline("hodinypausal", pid, "hidden");
        }

        if (p72id == 4) {
            $("#" + trid + " .p72id").css("background-color", "green");
            $("#" + trid + " .p72id").css("color", "white");
        }
        if (p72id == 7) {
            $("#" + trid + " .p72id").css("background-color", "gold");
            $("#" + trid + " .p72id").css("color", "black");
        }
        if (p72id == 2) {
            $("#" + trid + " .p72id").css("background-color", "red");
            $("#" + trid + " .p72id").css("color", "white");
        }
        if (p72id == 3) {
            $("#" + trid + " .p72id").css("background-color", "brown");
            $("#" + trid + " .p72id").css("color", "white");
        }
        
    }

}


function getvalue(pid, element_class,default_value)
{
    var arr = $("#tr" + pid + " ." + element_class);
    if (arr.length > 0)
    {
        return $(arr[0]).val();
    }
    else
    {
        return default_value;
    }
}

function show_stat_val(element_id, val)
{
    var c = $("#" + element_id);
    if (val == "" || val=="0" || val=="h" || val=="-" || val==null) {
        $(c).css("display", "none");
    } else {
        val = val.replace("&#xA0;", " ");
        $(c).text(val);
        $(c).css("display", "block");
    }
}





function inline_save_temp_record(x,pid,isbatch,isupdate_stat) {
    var p31guid = $("#p31guid").val();
    
    var p71id = parseInt($("#tr" + pid + " .p71id").val());
    var p72id = parseInt($("#tr" + pid + " .p72id").val());
    var p33id = parseInt($("#tr" + pid + " .p33id").val());
    
    var hodiny = $("#tr" + pid + " .hodiny").val();
    var hodiny_interni = hodiny;
    var hodiny_pausal = 0;
    var uroven = 0;

    hodiny_interni = getvalue(pid, "hodinyinterni", hodiny_interni);
    hodiny_pausal = getvalue(pid, "hodinypausal", hodiny_pausal);
    uroven = getvalue(pid, "uroven", uroven);
    
  
    var c = {
        p31id: pid,
        p71id: p71id,
        p72id: p72id,
        p33id: p33id,
        uroven: uroven,
        hodiny: hodiny,
        hodinyinterni: hodiny_interni,
        sazba: getvalue(pid, "sazba", null),
        hodinypausal: hodiny_pausal,
        popis: getvalue(pid,"popis",null),
        bezdph: getvalue(pid,"bezdph",null),
        dphsazba: getvalue(pid,"dphsazba",null)

    };
    //alert("pid: " + pid + ", guid: " + p31guid + ", popis: " + c.popis);
    
    $.post(_ep("/p31approve/SaveTempRecord"), { rec: c, p31id: pid, guid: p31guid,isbatch: isbatch }, function (data) {

        if (data.message != null) {
            _notify_message(data.message);
            return;
        }
               
        if (isupdate_stat) {
            inline_refresh_stat();
        }

        $.post(_ep("/p31approve/LoadGridRecord"), { p31id: pid, guid: p31guid }, function (data) {
            
            if (c.p33id = 1 || c.p33id == 3)
            {                
                $("#honorar_schvaleno" + x).text(data.honorar_schvaleno);
                
                $("#tr" + pid + " .rozdil_bezdph").text(data.rozdil_vykazano_schvaleno_bezdph);
                $("#tr" + pid + " .rozdil_hodnota").text(data.rozdil_vykazano_schvaleno_hodnota);
                $("#tr" + pid + " .rozdil_sazba").text(data.rozdil_vykazano_schvaleno_sazba);

                if (isbatch)
                {
                    $("#tr" + pid + " .sazba").val(data.sazba);
                    $("#tr" + pid + " .hodiny").val(data.hodiny);
                    
                    $("#tr" + pid + " .schvaleno").text(data.honorar_schvaleno);
                }
                
            }
            
            if (c.p33id == 2 || c.p33id == 5)
            {
                
                $("#tr" + pid + " .rozdil_bezdph").text(data.rozdil_vykazano_schvaleno_bezdph);
                if (isbatch)
                {
                    $("#tr" + pid + " .bezdph").val(data.bezdph);
                    
                    $("#tr" + pid + " .dphsazba").val(data.dphsazba);

                    $("#tr" + pid + " .schvaleno").text(data.bezdph);
                }
            }
        });


    });
}



function batch(cmd, p71id, p72id) {
    var pids = $("#CheckedPids").val();
    if (pids === "") {
        _notify_message(_tg_musite_vybrat_zaznam);
        return;
    }
    $("#batchpids").val(pids);

    _loading_show("postback");
   

    var rows = $(".tabrow");
    var xx = 0;
    for (var x = 0; x < rows.length; x++)
    {
        var tr = $(rows[x]);        
        var pid = $(tr).attr("id").replace("tr", "");
        var x = $(tr).attr("data-x");

        if ($("#chk" + pid).prop("checked"))
        {
            $("#tr" + pid + " .p71id").val(p71id);
            $("#tr" + pid + " .p72id").val(p72id);

            if (p71id == 1)
            {
                showhide_inline("p72id", pid, "visible");
            }
            else
            {
                showhide_inline("p72id", pid, "hidden");
            }

            
            inline_save_temp_record(x, pid,true,false);


            xx = xx + 1;
        }
    }

    setTimeout(() => {
        //počkej 400 milisekund
        inline_refresh_stat();

        inline_update_colors();

        _loading_hide();

    }, 400);

    
    

    
    
}

function refresh_selected_rowscount_info()
{
    let ret = 0;
    const rows = $(".tabrow");
    for (var x = 0; x < rows.length; x++) {

        var tr = $(rows[x]);
        var pid = $(tr).attr("id").replace("tr", "");
       
        if ($("#chk" + pid).prop("checked")) {
            ret += 1;


        }
    }

    $("#selected_rowscount_info").text(ret);
}

function batch_uroven()
{
    var uroven = $("#BatchApproveLevel").val();
    var pids = $("#CheckedPids").val();
    if (pids === "") {
        _notify_message(_tg_musite_vybrat_zaznam);
        return;
    }
   
    var rows = $(".tabrow");
    
    for (var x = 0; x < rows.length; x++) {
        var tr = $(rows[x]);
        var pid = $(tr).attr("id").replace("tr", "");
        var x = $(tr).attr("data-x");

        if ($("#chk" + pid).prop("checked"))
        {            
            $("#tr" + pid + " .uroven").val(uroven);
          
            inline_save_temp_record(x, pid, true,false);


        }
    }

    inline_update_colors();
    inline_refresh_stat();
    
}

function batch_sazba() {
    var sazba = $("#BatchInvoiceRate").val();
    var pids = $("#CheckedPids").val();
    if (pids === "") {
        _notify_message(_tg_musite_vybrat_zaznam);
        return;
    }

    var rows = $(".tabrow");

    for (var x = 0; x < rows.length; x++) {
        var tr = $(rows[x]);
        var pid = $(tr).attr("id").replace("tr", "");
        var x = $(tr).attr("data-x");

        if ($("#chk" + pid).prop("checked")) {
            $("#tr" + pid + " .sazba").val(sazba);

            inline_save_temp_record(x, pid, true,false);


        }
    }

    inline_update_colors();
    inline_refresh_stat();

}

function statistika(cmd,e,guid)
{    
    
    
    _zoom(e, null, null, 900, "Rychlá statistika", "/p31approve/Stat?p31guid=" + guid,true);
    
}

function memos(cmd, e, guid)
{
    _zoom(e, null, null, 900, "Fakturační poznámky", "/p31approve/Memos?p31guid=" + guid, true);
}

function invoice_info(cmd, e, p91id) {
    _zoom(e, null, null, 900, "Přidat do vyúčtování", "/p91/info?pid=" + p91id, true);
}


function go2grid(p31guid, p91id) {
    $("#linkGo2Grid").text("Processing...")
    $("#linkGo2Grid").attr("disabled", true);
    _loading_show("postback");
    location.replace("/p31approve/Grid?p31guid=" + p31guid + "&p91id=" + p91id);
}

function wrk(p31id)
{
    _window_open("/workflow_dialog/Index?record_prefix=p31&record_pid="+p31id,3);
}

function report(guid) {
    _window_open("/ReportsClient/ReportContext/?prefix=app&p31guid=" + guid, 3);
}