
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

    $("#cmdMore").click(function () {
        $("#divMore").toggle();
    });

    if ($("#PostbackOper").val() == "p32id" && _theukon.rec_pid == "0" && _theukon.p32id != "0" && $("#DefaultText").val() != "")
    {        
        var strAppendText = $("#DefaultText").val();
        var strCurText = $("#Rec_p31Text").text();
        if (strCurText != "")
        {
            if (strCurText.indexOf(strAppendText) == -1) {
                $("#Rec_p31Text").text(strCurText + "\n");
                $("#Rec_p31Text").append(strAppendText);
            }
            
            
        }
        else
        {
            $("#Rec_p31Text").append(strAppendText);
        }
        
        
        
    }
    
    if (_theukon.isdoclasttext != "")
    {        
        $("#Rec_p31Text").append(_theukon.isdoclasttext);//text isdoc naimportovaného dokladu
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
        
    });


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
    //nic
    //_postback("p56id");
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
   
    $("#p31Datehelper").val(_format_date(d, false));
    $("#p31Date").val(_format_date(d, false));

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

function p31text_mouseover(txt) {
    $("#cmdFindText").css("visibility", "visible");
}

function p31text_mouseout(txt) {

    var btn = document.getElementById("cmdFindText");

    if (btn.matches(":hover")) {
        return; //myš je nad tlačítkem
    }

    $("#cmdFindText").css("visibility", "hidden");
}



function cmdFindText_mouseout(cmd) {
    $("#cmdFindText").css("visibility", "hidden");
}


function isdoc_upload()
{
    form1.enctype = "multipart/form-data";
    form1.action = "/p31/Record?oper=isdoc_upload";
    form1.submit();
}