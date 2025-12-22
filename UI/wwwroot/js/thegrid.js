var _j72id;
var _tg_url_data;
var _tg_url_handler;
var _tg_url_filter;
var _tg_url_export;
var _tg_entity;
var _tg_filterinput_timeout;
var _tg_filter_is_active;
var _tg_go2pid;
var _tg_master_entity;
var _tg_master_pid;
var _tg_oncmclick;
var _tg_ondblclick;
var _tg_tablerows;
var _tg_drag_is_active = false;
var _tg_drag_tbodyrows = [];
var _tg_rowindex = 0;
var _tg_mousedown_active = false;
var _tg_current_pid;
var _tg_is_enable_clipboard = true;
var _tg_is_enable_selecting = true;    //zda je povoleno označování řádek myší
var _tg_fixedcolumns;
var _tg_viewstate;
var _tg_rez;
var _tg_myqueryinline;
var _tg_isperiodovergrid;
var _tg_langindex = 0;
var _tg_reczoomflag=0;
var _tg_musite_vybrat_zaznam = "Musíte vybrat minimálně jeden záznam.";
const event_thegridbound = new Event("thegrid_rebound");
var _tg_last_resizer_when = null;
var _tg_operatorflag = 0;
var _tg_go2grouprow = 0;
var _tg_current_groupfield = null;
var _j72id_query = 0;


function tg_init(c) {
    _tg_entity = c.entity;
    _j72id = c.j72id;
    _tg_url_data = c.dataurl;
    _tg_url_handler = c.handlerurl;
    _tg_url_filter = c.filterurl;
    _tg_url_export = c.exporturl;
    _tg_go2pid = parseInt(c.go2pid);
    _tg_master_entity = c.master_entity;
    _tg_master_pid = c.master_pid;
    _tg_oncmclick = c.oncmclick;
    _tg_ondblclick = c.ondblclick;
    _tg_fixedcolumns = c.fixedcolumns;
    _tg_viewstate = c.viewstate;
    _tg_rez = c.rez;
    _tg_myqueryinline = c.myqueryinline;
    _tg_isperiodovergrid = c.isperiodovergrid;
    _tg_langindex = c.langindex;
    _tg_reczoomflag = c.reczoomflag;
    _tg_operatorflag = c.operatorflag;
    _tg_current_groupfield = c.groupbyfield;
    _j72id_query = c.j72id_query;
    

    if (c.is_enable_selecting == "True" || c.is_enable_selecting == "true" || c.is_enable_selecting==true) {
        _tg_is_enable_selecting = true;
    } else {
        _tg_is_enable_selecting = false;
    }
    
    
  
    $("#container_grid").scroll(function () {
        $("#container_vScroll").width($("#container_grid").width() + $("#container_grid").scrollLeft());
    });

    

    $("#tabgrid0 .sortcolumn").on("click", function () {
        
        if (_tg_last_resizer_when != null && _tg_last_resizer_when+1000 > Date.now())
        {
            return; //před okamžikem uživatel prováděl resize sloupce - nejedná se o třídění
        }
        var field = this.id.replace("th_", "");
        tg_post_handler("sorter", "sortfield", field);
    });
    
    tg_setup_checkbox_handler();

    //$("#container_grid").css("visibility", "visible");

    var parentElement = document.getElementById("container_grid").parentNode;
    if (parentElement.id !== "splitter_panel1")
    {                
        tg_adjust_for_screen(); //bude voláno až po inicializaci splitteru v mateřské stránce gridu
    }
    
    tg_adjust_parts_width();

    var basewidth = $("#tabgrid0").width();
    $("#tabgrid1").width(basewidth);
    $("#tabgrid2").width(basewidth);

    tg_setup_selectable();  //inicializace selectable

    
    if (_tg_current_groupfield != null && _tg_current_groupfield != "")
    {
        tg_init_grouprow_dblclick();
    }

    
   

    
    if (_tg_go2pid !== null && _tg_go2pid !== 0) {
        tg_go2pid(_tg_go2pid);  //automaticky přejít na záznam _tg_go2pid        
    } else {
        
        if (_tg_current_groupfield != null && _tg_current_groupfield !="") {

            
            if (document.getElementsByClassName("group_row_active").length>0)
            {
                var row = document.getElementsByClassName("group_row_active")[0];
                var rowindex = $(row).attr("id").replace("rg", "");                
                tg_go2grouprowindex(rowindex);
            }
        }
    }


    $("#tabgrid1_thead .query_textbox").on("focus", function (e) {
        $(this).select();
    });

    $(".groupbycolumn").on("mouseover", function (e) {
        tg_groupbycolumn_mouseover($(this));        
    });
    $(".groupbycolumn").on("mouseout", function (e) {
        tg_groupbycolumn_mouseout($(this));
    });
    
    

    $("#tabgrid1_thead .query_textbox").on("input", function (e) {
        var txt1 = this;
        if (typeof _tg_filterinput_timeout !== "undefined") {
            clearTimeout(_tg_filterinput_timeout);
        }
        _tg_filterinput_timeout = setTimeout(function () {

            //your ajax stuff , 500ms zpoždění, aby se počkalo na víc stringů od uživatele 
            var field = txt1.id.replace("txtqry_", "");
            if (txt1.value !== "") {
                $("#hidqry_" + field).val(txt1.value);
                $("#hidoper_" + field).val("3")    //natvrdo doplnit operátor OBSAHUJE
                $("#txtqry_" + field).css("background-color", "red");
                $("#txtqry_" + field).css("color", "white");
            } else {
                $("#hidqry_" + field).val("");
                $("#txtqry_" + field).css("background-color", "");
                $("#txtqry_" + field).css("color", "");
            }


            tg_filter_send2server();



        }, 500);


    });

    $("#tabgrid1_thead button.query_button").on("click", function (e) {
        var field = this.id.replace("cmdqry_", "");
        var caption = $("#th_" + field).text();
        var coltypename = normalize_coltype_name($("#th_" + field).attr("columntypename"));

        if ($("#tg_div_filter_field").val() === field && $("#tg_div_filter").css("display") === "block") {
            tg_filter_hide_popup(); //toto filter je již otevřeno
            return;
        }
        var ofs = $(this).offset();
        var l = 10 + $(this).width() + ofs.left - $("#tg_div_filter").width();
        if (l < 0) l = 0;
        $("#tg_div_filter").css("left", l);
        $("#tg_div_filter").css("top", $(this).height() + ofs.top + 1);
        $("#tg_div_filter_header").text(caption);
        $("#tg_div_filter_field").val(field);

        $("#tg_div_filter").css("display", "block");
        tg_filter_prepare_popup(field, coltypename);
    });


    _tg_filter_is_active = tg_is_filter_active();

    $(".query_button").css("visibility", "hidden");

    
    $(".query_th").mouseover(function () {
        var btn = $(this).find(".query_button");
        var offset = $(this).offset();
        
        $(btn).css("top", offset.top+3);
        $(btn).css("left", offset.left + $(this).width() - $(btn).width()-4);
        $(btn).css("visibility", "visible");

       // $("#cmdFilterClear").css("display", "block");
        //$("#cmdFilterClear").css("top", offset.top-20);
        //$("#cmdFilterClear").css("left", offset.left + $(this).width() - $(btn).width() - 4);
    });
    $(".query_th").mouseout(function () {
        $(this).find(".query_button").css("visibility", "hidden");
        //$("#cmdFilterClear").css("display", "none");
    });

    
    
}



function tg_post_data() {

    $.post(_tg_url_data, { tgi: get_all_tgi_params(null,null,null), pathpars: get_all_path_values() }, function (data) {

        refresh_environment_after_post("first_data", data);

        if (_tg_go2pid !== null && _tg_go2pid !== 0) {
            tg_go2pid(_tg_go2pid);
        }                
        
    });

}

function tg_refresh_sorter(sortfield, sortdir) {
    
    $("#tabgrid0 .sortcolumn").each(function () {
        //$(this).text(this.title);
        $(this).text($(this).attr("data-header"));
    });
    var ths = $("#th_" + sortfield);
    
    if (sortdir === "asc") {
        $(ths).html($(ths).html() + "<span class='material-icons-outlined-btn'>arrow_drop_up</span>");
    }
    if (sortdir === "desc") {
        $(ths).html($(ths).html() + "<span class='material-icons-outlined-btn'>arrow_drop_down</span>");
    }
    $(".sortcolumn").css("background-color","");
    $(ths).css("background-color", "lightblue");
}

function tg_hardrefresh(pid)    //voláno z _layout pro načtení gridu po aktualizaci spodního panelu slaveview
{
    _tg_go2pid = parseInt(pid);
    tg_post_handler("reload_hardrefresh", "reload_hardrefresh", null);

}

function tg_post_handler(strOper, strKey, strValue) {
   
    var params = get_all_tgi_params(strOper, strKey, strValue);

    if (_tg_viewstate !== "") {
        params.viewstate = _tg_viewstate.split("|");
    }
    
    
    $("#tabgrid1_tbody").html("<b>Loading...</b>");
    $.post(_tg_url_handler, { tgi: params, pathpars: get_all_path_values() }, function (data) {
        // _notify_message("vrátilo se: oper: " + strOper + ", key: " + strKey + ", value: " + strValue);

        refresh_environment_after_post(strOper, data);

        document.dispatchEvent(event_thegridbound);

        if (strOper == "reload_hardrefresh")
        {
            tg_go2pid(_tg_go2pid);
        }
        
        
    })
        .fail(function (error) {
            $("#tabgrid1_tbody").html("<code>" + error.responseJSON + "</code>");
            alert(error.responseJSON)
        });

}


function refresh_environment_after_post(strOper, data) {
    $("#thegrid_message").text(data.message);
    $("#tabgrid1_tbody").html(data.body);
    $("#tabgrid1_tfoot").html(data.foot);
    $("#divPager").html(data.pager);

    if (strOper === "sorter" || strOper === "first_data") {
        tg_refresh_sorter(data.sortfield, data.sortdir);
    }
    if (document.getElementById("TheGridRows")) {        
        $("#TheGridRows").text(_format_number_int(data.virtualrowscount));
        
    }
    
    tg_adjust_parts_width();

    var basewidth = $("#tabgrid0").width();
    $("#tabgrid1").width(basewidth);
    $("#tabgrid2").width(basewidth);

    

    _tg_tablerows = $("#tabgrid1_tbody").find("tr");
    tg_setup_selectable();

    if (strOper == "filter")    //sloupcový filter
    {
        var trrows = $("#tabgrid1_tbody .selectable");
        if (trrows.length > 0)
        {
            var trfirst = $(trrows).first();    //najít první řádek v gridu
            var pid = $(trfirst).attr("id").replace("r", "");   //hodnota pid prvního řádku v gridu
            tg_go2pid(pid); //označit první řádek

            var event_thegridrowselect = new CustomEvent("thegrid_rowselect", { detail: { pid: pid, pid_pre: pid } });
            document.dispatchEvent(event_thegridrowselect); //vyvolat událost, která souvisí se změnou záznamu v gridu

        }
        
    }
    
    if (strOper==="sorter" && _tg_current_pid > 0) {
        tg_go2pid(_tg_current_pid);
    }

    if (strOper == "groupbyval" && _tg_go2grouprow>0) {
        tg_go2grouprowindex(_tg_go2grouprow);       //skočit na aktuální řádek groupby souhrnů
        
    }

    if (_tg_reczoomflag != "0" && _tg_reczoomflag != "") {
        _init_qtip_onpage();    //v gridu reczoom íčko
    }
    
    _tg_operatorflag = data.operatorflag;    
    tg_refresh_operatorflag();

    if (_tg_current_groupfield != null && _tg_current_groupfield != "") {
        tg_init_grouprow_dblclick();
    }
}

function tg_adjust_parts_width() {

    $("#container_vScroll").width($("#container_grid").width() + $("#container_grid").scrollLeft());
    if ($("#container_vScroll").width() == 0)
    {        
        $("#container_grid").css("width", _device.availWidth-50);
        
        
        
        
    }
    
}


function tg_setup_selectable() {
    
    if (!_tg_is_enable_selecting) return;

    _tg_tablerows = $("#tabgrid1_tbody").find("tr");


    $(_tg_tablerows).mousedown(function (e) {        

        if (e.target.tagName === "INPUT" || this.className === "trgroup" || this.className == "trgroup group_row_active" || e.target.className === "link_in_grid") {
            return; //kliknutí na checkbox nebo grid není selectable nebo kliknutí na vložený link do buňky
        }

        if (e.target.className === "td0") {
            //kliknutí těsně vedle checkboxu
            return;
            
        }
        
        var pid = this.id.replace("r", "");
        _tg_rowindex = this.rowIndex;
        
        _tg_current_pid = pid;

        var pid_pre = $("#tg_selected_pid").val();

        if (pid !== pid_pre) {
            var event_thegridrowselect = new CustomEvent("thegrid_rowselect", { detail: { pid: pid, pid_pre: pid_pre } });
            document.dispatchEvent(event_thegridrowselect);

            $("#gridcurrow").text(" #" + (_tg_rowindex+1));
            
        }
        if (e.ctrlKey) {
            
            this.classList.add("selrow");
            $("#" + this.id + " input:checkbox").prop("checked", true);
            tg_save_selected_pids(null);
        } else {
            tg_select_one_row(this, pid);
            tg_save_selected_pids(pid);
        }



        _tg_mousedown_active = true;

        if (_tg_is_enable_clipboard === false) {   //pokud je true, pak uživatelé mohou v gridu označovat text
            e.preventDefault(); // this prevents text selection from happening
        }


    });

    $(_tg_tablerows).mousemove(function (e) {
        if (!_tg_is_enable_selecting) return;
        if (_tg_mousedown_active === false || this.className === "trgroup" || this.className == "trgroup group_row_active" || e.ctrlKey) {
            return;
        }

        if (_tg_drag_is_active === false) {
            //nastartovat drag režim
            _tg_drag_is_active = true;
            _tg_drag_tbodyrows = $("#tabgrid1_tbody").find("tr");
            first_y = e.pageY;
            first_rowindex = this.rowIndex;
            last_rowindex = this.rowIndex;

            $("#tabgrid1_tbody").find("tr.selrow").removeClass("selrow");
            $("#tabgrid1_tbody").find("tr.highlight").removeClass("highlight");
            $("#tabgrid1_tbody").find("input:checkbox").prop("checked", false);

            $(this).addClass("highlight");
            

        }
        last_y = e.pageY, last_rowindex = this.rowIndex;

        if (this.classList.contains("highlight") === false) {
            this.classList.add("highlight");
        }

       

    });

    $(_tg_tablerows).mouseout(function (e) {
        if (!_tg_is_enable_selecting) return;
        $(this).css("background-color", "");    //hover řádky
        if (_tg_drag_is_active === false || this.className === "trgroup" || this.className == "trgroup group_row_active") return;
        
        if (!_tg_is_enable_selecting) return;

        if (first_rowindex < last_rowindex) {
            if (e.pageY < last_y) {
                _tg_drag_tbodyrows[last_rowindex].classList.remove("highlight");
            }
        }
        if (first_rowindex > last_rowindex) {
            if (e.pageY > last_y) {
                _tg_drag_tbodyrows[last_rowindex].classList.remove("highlight");
            }
        }
        
        if (first_rowindex != last_rowindex) {
            //zrušit označení textu, pokud je označeno více řádků
            document.getSelection().collapseToEnd();
        }
        
        //$("#tabgrid1_cmd_sel").html("<i class='fas fa-check-double fai_button'></i> " + $(".highlight").length);

    });

    $(_tg_tablerows).mouseover(function (e) {        
        $(this).css("background-color", "#ECECEC"); //hover řádky            
    });

    $(document).mouseup(function (ev) {
        if (!_tg_is_enable_selecting) return;
        _tg_mousedown_active = false;

        if (_tg_drag_is_active === false) return;

        var rows = $("#tabgrid1_tbody").find("tr.highlight");

        if (rows.length > 0) {
            var rows_all = $("#tabgrid1_tbody").find("tr");
            var start_index = first_rowindex, end_index = last_rowindex;
            if (first_rowindex > last_rowindex) {
                start_index = last_rowindex;
                end_index = first_rowindex;
            }
            for (var i = start_index; i <= end_index; i++) {
                rows_all[i].classList.add("highlight");
            }
            rows = $("#tabgrid1_tbody").find("tr.highlight");
        }
        var chks = $("#tabgrid1_tbody").find("tr.highlight input:checkbox");

        $("#tabgrid1_tbody").find("tr.highlight").removeClass("highlight");
        $("#tabgrid1_tbody").find("tr.selrow").removeClass("selrow");
        $("#tabgrid1_tbody").find("input:checkbox").prop("checked", false);

        $(chks).prop("checked", true);
        $(rows).addClass("selrow");

        tg_save_selected_pids(null);	//je třeba dodělat!
        _tg_drag_is_active = false;
        _tg_drag_tbodyrows = null;
        
        if (last_rowindex != first_rowindex) {               
            //označeno více řádků - zrušit označený text v buňkách, protože to vypadá blbě a označit text pouze v prvním nebo posledním řádku
            var onerow = $(rows[rows.length - 1]);
            if (last_rowindex < first_rowindex) {
                onerow = $(rows[0]);
            }
            
            var selection = window.getSelection();
            var range = document.createRange();
            range.setStartBefore(onerow.first()[0]);
            range.setEndAfter(onerow.last()[0]);
            selection.removeAllRanges();
            selection.addRange(range);

            //document.getSelection().collapseToEnd();
        }
        

    });


}





function tg_setup_checkbox_handler() {
    if (!_tg_is_enable_selecting) return;   //není zapnuto označování záznamů
    $("#tabgrid1").on("change", "input:checkbox", function () {

        var tr_row = this.parentNode.parentNode;
        if (this.checked === true) {
            tr_row.classList.add("selrow");
        } else {
            tr_row.classList.remove("selrow");
        }

        tg_save_selected_pids(null);

    });

    $("#tabgrid1").on("click", ".td0", function (e) {
        
        if (e.target.tagName === "INPUT") {
            return; //kliknutí na checkbox řeší jiná událost
        }
        
        var tr_row = this.parentNode;

        var ischecked=$("#" + this.parentNode.id + " input:checkbox").prop("checked");
        if (ischecked)
        {
            //$("#" + this.parentNode.id + " input:checkbox").prop("checked", false);
            //tr_row.classList.remove("selrow");
            
        }
        else
        {
            $("#" + this.parentNode.id + " input:checkbox").prop("checked", true);
            tr_row.classList.add("selrow");
        }

        tg_save_selected_pids(null);

        

    });
}

function tg_exist_pid(pid) {
    
    if (document.getElementById("r" + pid)) {        
        return true
    }

    return false;
}

function tg_go2grouprowindex(rowindex) {
    if (document.getElementById("rg" + rowindex))
    {
        var row = document.getElementById("rg" + rowindex);
        tg_scrollinto_row(row);
        //row.scrollIntoView({ block: "start", behavior: "smooth" });
        
    }
    
}
function tg_go2pid(pid) {       //již musí být ze serveru odstránkováno!
    if (tg_exist_pid(pid))
    {
        
        var row = document.getElementById("r" + pid);
        tg_select_one_row(row, pid);
        
        //row.scrollIntoView({ block: "start", behavior: "smooth" });
        tg_scrollinto_row(row);
        
    }
}

function tg_select_explicit_pids(pids, scroll_into_view) {
    
    if (pids === null || pids === "" || !_tg_is_enable_selecting) {
        return;
    }
    tg_clear_selection();
    var arr = [];
    var rows = $("#tabgrid1_tbody tr");
    pids = ","+pids + ",";

    for (var i = 0; i < rows.length; i++) {
        var pid = rows[i].id.replace("r", "");
        if (pids.indexOf(","+pid+",") > -1) {
            $(rows[i]).addClass("selrow");
            arr.push(pid);
            $("#chk" + pid).prop("checked", true);
        }
                
        tg_save_selected_pids(arr.join(","));
    }

    if (scroll_into_view && arr.length>0)
    {
        var row = document.getElementById("r" + arr[0]);
        //row.scrollIntoView({ block: "start", behavior: "smooth" });
        tg_scrollinto_row(row);
    }
}

function tg_select(records_count) {     //označí prvních X (records_count) záznamů
    tg_clear_selection();
    var arr = [];
    
    var rows = $("#tabgrid1_tbody tr").not(".trgroup");
    if (records_count > rows.length) {
        records_count = rows.length;
    }
    for (var i = 0; i < records_count; i++) {
        $(rows[i]).addClass("selrow");
        var pid = rows[i].id.replace("r", "");
        arr.push(pid);

        $("#chk" + pid).prop("checked", true);
        
    }
    tg_save_selected_pids(arr.join(","));
    tg_div_close_synthetic_divs();

   
    
    
}

function tg_select_bycss(trcssclass) {     //označí řádky podle css třídy
    tg_clear_selection();
    var arr = [];    
    var rows = $("#tabgrid1_tbody ." + trcssclass);

    if (trcssclass == "hours") {
        //taková class neexistuje :not(.trexp)
        rows = $("#tabgrid1_tbody .selectable:not(.trexp):not(.trpc):not(.trfee)");
       
    }
    
    for (var i = 0; i < rows.length; i++) {
        $(rows[i]).addClass("selrow");
        var pid = rows[i].id.replace("r", "");
        arr.push(pid);

        $("#chk" + pid).prop("checked", true);
        
    }
    tg_save_selected_pids(arr.join(","));
    tg_div_close_synthetic_divs();
    if (rows.length>0 && arr.length > 0) {
        var row = document.getElementById("r" + arr[0]);       
        //row.scrollIntoView({ block: "start", behavior: "smooth" });        
        tg_scrollinto_row(row);
    }

    _notify_message("Počet záznamů: " + arr.length, "info");

}

function tg_pager(pageindex) {  //změna stránky
    tg_post_handler("pager", "pagerindex", pageindex);

}
function tg_pagesize(ctl) {//změna velikosti stránky
    tg_post_handler("pager", "pagesize", ctl.value);
}






function tg_is_filter_active() {
    var b = new Boolean;
    $("#cmdDestroyFilter").css("display", "none");
    $("#cmdOperatorFlag").css("display", "none");
    
    $("#tr_header_query").find("input:hidden").each(function () {
        if (this.id.indexOf("hidqry_") > -1 && this.value !== "") {

            $("#cmdDestroyFilter").css("display", "block");
            $("#cmdOperatorFlag").css("display", "block");
            tg_refresh_operatorflag();
            b = true;
            return b;
        }
        if (this.id.indexOf("hidoper_") > -1 && this.value !== "3" && this.value !== "" && this.value !== "0") {

            $("#cmdDestroyFilter").css("display", "block");
            $("#cmdOperatorFlag").css("display", "block");
            tg_refresh_operatorflag();
            b = true;
            return b;
        }
    });
    return b;
}

function tg_refresh_operatorflag(){
    if (_tg_operatorflag == "1") {
        $("#cmdOperatorFlag").text("O");
        $("#cmdOperatorFlag").attr("title", "Sloupcový filtr v režimu [OR]. Vrátit do režimu [AND]");
        $("#cmdOperatorFlag").addClass("btn-warning");
        $("#cmdOperatorFlag").removeClass("btn-light");
    } else {
        $("#cmdOperatorFlag").text("A");
        $("#cmdOperatorFlag").attr("title", "Sloupcový filtr v režimu [AND]. Přepnout do režiu [OR]");
        $("#cmdOperatorFlag").addClass("btn-light");
        $("#cmdOperatorFlag").removeClass("btn-warning");
        
    }
}


//filtrování z MT6:
function tg_qryval_keydown(e) {
    if (e.keyCode === 13) {
        tg_filter_ok();
        return false;
    }
    if (e.keyCode === 27) {
        tg_filter_hide_popup();
        return false;
    }

}

function tg_filter_operator() {
    if (_tg_operatorflag == "1") {
        tg_post_handler("operatorflag", "operatorflag", "0");       //přepnout na filtr [AND]
    } else {
        tg_post_handler("operatorflag", "operatorflag", "1");       //přepnout na filtr [OR]
    }
    
    
}

function tg_filter_clear() {
    $("#tr_header_query").find("input:hidden").each(function () {
        if (this.id.substr(0, 3) === "hid") {
            this.value = "";
            var field = this.id.replace("hidqry_", "");
            if (document.getElementById("qryalias_" + field)) {
                $("#qryalias_" + field).html("");
            }
            if (document.getElementById("txtqry_" + field)) {
                $("#txtqry_" + field).val("");
                $("#txtqry_" + field).css("background-color", "");
            }


        }
    });



    tg_filter_send2server();

}
function normalize_coltype_name(coltypename) {
    if (coltypename === "num0" || coltypename === "num3" || coltypename === "num") coltypename = "number";
    if (coltypename === "datetime") coltypename = "date";
    return (coltypename);
}
function tg_filter_ok() {
    $("#cmdDestroyShowOnlyPID").css("display", "none");

    if ($("input[name='chlfilter']:checked").length === 0) {
        switch (_tg_langindex) {
            case 2:
                _notify_message("Ви повинні перевірити одного з операторів фільтра.", "warning"); break;
            default:
                _notify_message("Musíte zaškrtnout jeden z filtrovacích operátorů.", "warning"); break;
        }

        return;
    }
    var c1 = document.getElementById("qryval1");
    var c2 = document.getElementById("qryval2");
    var field = $("#tg_div_filter_field").val();
    var coltypename = normalize_coltype_name($("#th_" + field).attr("columntypename"));


    var operator = $("input[name='chlfilter']:checked").val();
    var av = tg_filter_operator_as_alias(operator);

    var fv = "";

    if (coltypename === "bool") {
        fv = operator;
    }
    if (coltypename === "string") {
        fv = c1.value;
        if (fv === "" && (operator === "3" || operator === "4" || operator === "5" || operator === "6" || operator === "7")) {
            switch (_tg_langindex) {
                case 2:
                    _notify_message("Ви повинні заповнити вираз фільтра.", "warning"); break;
                default:
                    _notify_message("Musíte vyplnit filtrovací výraz.", "warning"); break;
            }
            c1.focus();
            return;
        }
        if (operator === "0" || operator === "1" || operator === "2") fv = "";
    }
    if (coltypename === "number" || coltypename === "date") {
        if (operator === "4" && (c1.value === "" || c2.value === "")) {
            switch (_tg_langindex) {
                case 2:
                    _notify_message("Ви повинні заповнити значення від - до.", "warning"); break;
                default:
                    _notify_message("Musíte vyplnit hodnoty od - do.", "warning"); break;
            }
            c1.focus();
            return;
        }
    }

    var filter_before = tg_is_filter_active();

    $("#qryalias_" + field).css("display", "block");

    if (coltypename === "number") {
        fv = c1.value + "|" + c2.value;
        if (operator === "0" || operator === "1" || operator === "2" || operator === "10" || operator === "11") fv = "";
        if (av === "" && operator !== "0") av = c1.value + " - " + c2.value;
    }
    if (coltypename === "date") {
        fv = c1.value + "|" + c2.value;
        if (operator === "0" || operator === "1" || operator === "2") fv = "";
        if (av === "" && operator !== "0") av = c1.value + "<br>" + c2.value;
    }
    if (coltypename === "string") {
        if (av === "" && operator !== "0") av = c1.value;
        if (operator === "5") av = "[*=] " + av;
        if (operator === "6") av = "[=] " + av;
        if (operator === "7") av = "[<>] " + av;

        $("#txtqry_" + field).css("display", "block");
        if (operator === "3" || operator === "0") {
            $("#qryalias_" + field).css("display", "none");
            $("#txtqry_" + field).css("display", "block");
            $("#txtqry_" + field).val(fv);
        } else {
            $("#txtqry_" + field).css("display", "none");
        }
        if (operator === "3" && fv !== "") {
            $("#txtqry_" + field).css("background-color", "red");
            $("#txtqry_" + field).css("color", "white");
        } else {
            $("#txtqry_" + field).css("background-color", "");
            $("#txtqry_" + field).css("color", "");
        }

    }

    if (operator === "0") {
        $("#hidqry_" + field).val("");
    } else {
        $("#hidqry_" + field).val(fv);
    }

    $("#qryalias_" + field).html(av);


    $("#hidoper_" + field).val(operator);

    _tg_filter_is_active = tg_is_filter_active();

    if (_tg_filter_is_active === false && filter_before === true) {
        tg_filter_clear();


    }

    tg_filter_hide_popup();
    tg_filter_send2server();

}

function tg_get_qry_value(field, coltypename) {
    var ret = {
        operator: -1,
        filtervalue: "",
        c1value: "",
        c2value: "",
        aliasvalue: ""
    }

    ret.operator = parseInt($("#hidoper_" + field).val());
    var s = $("#hidqry_" + field).val();
    ret.filtervalue = s;
    if (s === "") {
        return ret;
    }


    if ((coltypename === "date" || coltypename === "number") && ret.filtervalue !== "") {
        arr = ret.filtervalue.split("|");
        ret.c1value = arr[0];
        ret.c2value = arr[1];
        ret.aliasvalue = ret.c1value + " - " + ret.c2value;
    }
    if (coltypename === "string") {
        ret.aliasvalue = ret.filtervalue;
    }

    ret.aliasvalue = tg_filter_operator_as_alias(ret.operator);

    return ret;
}

function tg_filter_operator_as_alias(operator) {
    operator = String(operator);

    if (_tg_langindex === 2) {
        if (operator === "1") return "пусто";
        if (operator === "2") return "не є порожнім";
        if (operator === "8") return "ТАК";
        if (operator === "9") return "НІ";
        if (operator === "10") return "&gt;0";
        if (operator === "11") return "0 або порожній";
    }
    if (operator === "1") return "Je prázdné";
    if (operator === "2") return "Není prázdné";
    if (operator === "8") return "ANO";
    if (operator === "9") return "NE";
    if (operator === "10") return "&gt;0";
    if (operator === "11") return "0 nebo prázdné";
    return "";
}


function tg_filter_radio_change(ctl) {
    $("#hidoper_" + $("#tg_div_filter_field").val()).val(ctl.value);

    if (ctl.value === "0" || ctl.value === "1" || ctl.value === "2" || ctl.value === "8" || ctl.value === "9" || ctl.value === "10" || ctl.value === "11") {


        tg_filter_hide_popup();
        tg_filter_ok();
    }
    if (ctl.value === "3" || ctl.value === "4" || ctl.value === "5" || ctl.value === "6" || ctl.value === "7" || ctl.value === "33") {
        $("#tg_div_filter_inputs").css("visibility", "visible");
        document.getElementById("qryval1").focus();
        document.getElementById("qryval1").select();
    }

}


function tg_filter_prepare_popup(field, coltypename) {
    var c1 = document.getElementById("qryval1");
    var c2 = document.getElementById("qryval2");
    var curqryvalue = tg_get_qry_value(field, coltypename);

    $(c1).datepicker("destroy");
    $(c2).datepicker("destroy");
    c1.attributes["type"].value = "text"
    c2.attributes["type"].value = "text";
    $(c1).css("display", "block");
    $(c2).css("display", "block");
    c1.value = ""
    c2.value = "";

    $("#tg_div_filter_radios").find("div").css("display", "none");
    $("#tg_div_filter_inputs").css("visibility", "visible");
    $("#tg_div_filter_inputs").find("label").css("visibility", "visible");

    $("#tg_div_filter input:radio").prop("checked", false);

    if (curqryvalue.operator !== -1) {
        $("#chkf" + curqryvalue.operator).prop("checked", true);
    }

    if (coltypename === "date") {

        $("#tg_div_filter [cdate|='1']").css("display", "block");
        if (curqryvalue.operator === -1) {
            $("#chkf4").prop("checked", true)
        } else {
            c1.value = curqryvalue.c1value;
            c2.value = curqryvalue.c2value;
        }

        $(c1).datepicker({
            format: "dd.mm.yyyy",
            todayBtn: "linked",
            clearBtn: true,
            language: "cs",
            todayHighlight: true,
            autoclose: true
        });
        $(c2).datepicker({
            format: "dd.mm.yyyy",
            todayBtn: "linked",
            clearBtn: true,
            language: "cs",
            todayHighlight: true,
            autoclose: true
        });
    }

    if (coltypename === "number") {
        $("#tg_div_filter [cnumber|='1']").css("display", "block");
        c1.attributes["type"].value = "number";
        c2.attributes["type"].value = "number";

        if (curqryvalue.operator === -1) {
            $("#chkf4").prop("checked", true)
        } else {
            c1.value = curqryvalue.c1value;
            c2.value = curqryvalue.c2value;
        }
        c1.focus(), c1.select();
    }

    if (coltypename === "string") {
        if (curqryvalue.operator === -1) $("#chkf3").prop("checked", true);
        c1.value = curqryvalue.filtervalue;
        $("#tg_div_filter [cstring|='1']").css("display", "block");
        $("#tg_div_filter_inputs").find("label").css("visibility", "hidden");
        $(c2).css("display", "none");
        c1.focus(), c1.select();
    }
    if (coltypename === "bool") {
        if (curqryvalue.operator === -1) $("#chkf0").prop("checked", true);
        $("#tg_div_filter_inputs").css("visibility", "hidden");
        $("#tg_div_filter [cbool|='1']").css("display", "block");

        $("#tg_div_filter_inputs").find("label").css("visibility", "hidden");
        $(c2).css("display", "none");

    }

    //var operator = $("input[name='chlfilter']:checked").val();


    if (curqryvalue.operator === 0 || curqryvalue.operator === 1 || curqryvalue.operator === 2) {
        $("#tg_div_filter_inputs").css("visibility", "hidden");
    }


}


function tg_filter_hide_popup() {
    $("#tg_div_filter").css("display", "none");
    $("#tg_div_filter_header").text("");


}


function tg_filter_send2server() {
    //odeslat filtrovací podmínku na server, na serveru musí odpovídat třídě BO.TheGridColumnFilter
    var rec;
    var ret = [];
    $("#tr_header_query").find("input[type='hidden']").each(function () {
        if (this.id.indexOf("hidqry_") >= 0) {
            var fieldname = this.id.replace("hidqry_", "");
            var coltypename = normalize_coltype_name($("#th_" + fieldname).attr("columntypename"));
            var oper = $("#hidoper_" + fieldname).val();
            var val = $(this).val();

            if (coltypename === "string") {
                if (val !== "" && oper === "") oper = "3";
            }


            if (val !== "" || parseInt(oper) > 0) {
                rec = {
                    field: fieldname,
                    value: val,
                    oper: oper
                }
                //alert("val: "+val+", rec.value: " + rec.value + ", oper: " + rec.oper + ", field: " + rec.field);

                ret.push(rec);
            }
        }


    });

    $("#tabgrid1_tbody").html("<b>Loading...</b>");
    $.post(_tg_url_filter, { tgi: get_all_tgi_params(null,null,null), pathpars: get_all_path_values(), filter: ret }, function (data) {
        
        refresh_environment_after_post("filter", data);


        _tg_filter_is_active = tg_is_filter_active();
        if (_tg_filter_is_active === true) {
            $("#cmdDestroyFilter").css("display", "block");
            $("#cmdOperatorFlag").css("display", "block");

        } else {
            $("#cmdDestroyFilter").css("display", "none");
            $("#cmdOperatorFlag").css("display", "none");

        }

        document.dispatchEvent(event_thegridbound);

    })
        .fail(function (error) {
            $("#tabgrid1_tbody").html("<code>" + error.responseJSON + "</code>");
            alert(error.responseJSON)
        });



}

function tg_cm(e) {     //vyvolání kontextového menu k vybranému záznamu    
    var link = e.target;
    var pid = link.parentNode.parentNode.id.replace("r", "");
    
    _cm(e, _tg_entity, pid,"grid",_tg_master_entity);
}


function tg_adjust_for_screen(strParentElementID)
{
    
    if (!document.getElementById("tabgrid0")) return;

    
    
    var w0 = 0;
    if (typeof strParentElementID !== "undefined") {
        //výška gridu bude odvozená podle nadřízeného elementu strParentElementID
        var parentElement = document.getElementById(strParentElementID);
        var hh = $(parentElement).height() - $("#tabgrid0").height() - $("#tabgrid2").height() - $("#divPagerContainer").height() - 2;

        if ($("#tabgrid2").height() <= 2) {
            hh = hh - 35;   //rezerva pro tfoot s TOTALS            
        } else {
            //hh = hh - $("#tabgrid1_tfoot").height();
        }


        w0 = $(parentElement).width();

        if ($("#tabgrid0").width() > w0) {
                   
        }

        var div = document.getElementById("container_grid");
        var hasHorizontalScrollbar = div.scrollWidth > div.clientWidth;
        if (hasHorizontalScrollbar)
        {
            hh = hh - 20;   //je vidět horizontální scrollbara, ubereme výšku, aby byla vidět, 20: odhad výšky scrollbary     
        }

        $("#container_vScroll").css("height", hh + "px");



    }
    else
    {
        //režim 1 panelu, bez splitter
        var div_horizontal = $("#container_grid");
        var offset = $(div_horizontal).offset();
        var h_vertical = _device.innerHeight - offset.top - $("#tabgrid0").height() - $("#tabgrid2").height() - $("#divPagerContainer").height();

        //w0 = document.getElementById("container_grid").scrollWidth; //nefunguje přes jquery
        var w_tbody = document.getElementById("tabgrid1_tbody").scrollWidth;
        
        //var w1 = $(div_horizontal).width();
        
        if (w_tbody+20 > _device.innerWidth)
        {
            
            h_vertical = h_vertical - 20;   //je vidět horizontální scrollbara, ubereme výšku, aby byla vidět
           
        }
        if ($("#tabgrid1_tfoot").height() <= 2) {
            h_vertical = h_vertical - 35;   //rezerva pro tfoot s TOTALS
        }
        
        h_vertical = parseInt(h_vertical) - 4;  //ty 4 nevím proč
        
        $("#container_vScroll").css("height", h_vertical + "px");
        $(document.body).css("overflow-y", "hidden");

    }

    $("#container_vScroll").width($("#container_grid").width() + $("#container_grid").scrollLeft());


    //$("#container_grid").css("overflow-x", "auto");
    
    

    //var basewidth = $("#tabgrid0").width();
    //$("#tabgrid1").width(basewidth);
    //$("#tabgrid2").width(basewidth);



}



function tg_dblclick(row) {
    var prefix = _tg_entity.substr(0, 3);
    var pid = row.id.replace("r", "");
    
    if (prefix === "p41" || prefix === "p28" || prefix==="p91" || prefix === "j02") {
        var url = _ep("/" + prefix + "/RecPage?pid=" + pid);
        if (window !== top) {   //voláno uvnitř iframe
            window.open(url, "_top");
        } else {
            location.replace(url);
        }
        return;
    }
    
    
    _edit(prefix, pid);
}

function tg_export(format, scope) {
    var pids = "";
    if (scope === "selected") {
        pids = $("#tg_selected_pids").val();
        if (pids === "") {
            _notify_message(_tg_musite_vybrat_zaznam);
            return;
        }
    }
   

    $.post(_tg_url_export, { format: format, pids: pids, tgi: get_all_tgi_params(null,null,null), pathpars: get_all_path_values() }, function (data) {

        if (data.contenttype == "clipboard")
        {
            var copyText = data.clipboardcontent;
            navigator.clipboard.writeText(copyText).then(() => {
                // Alert the user that the action took place.
                if (scope == "selected") {
                    alert("Počet záznamů zkopírovaných do schránky: " + pids.split(",").length);
                }
                
            });

            return;
        }

        location.replace(_ep("/FileUpload/FileDownloadTempFileNDB?tempfilename=" + data.tempfilename + "&contenttype=" + data.contenttype + "&downloadfilename=" + data.downloadfilename));


    });


}


function tg_batchupdate(prefix) {
    var pids = $("#tg_selected_pids").val();
    if (pids === "") {
        _notify_message(_tg_musite_vybrat_zaznam);
        return;
    }
    var url = "/BatchUpdate/" + prefix + "?pids=" + pids;

    _window_open(url, 2);

}





function tg_clear_selection() {
    $("#tabgrid1_tbody").find("tr.selrow").removeClass("selrow");
    $("#tabgrid1_tbody").find("input:checkbox").prop("checked", false);

    $("#tg_selected_pid").val("");
    $("#tg_selected_pids").val("");
}

function tg_save_selected_pids(explicit_pids) {
    var pids = [];
    if (explicit_pids !== null) {
        pids = explicit_pids.split(",");
    } else {
        var rows = $("#tabgrid1_tbody").find("tr.selrow");
        if (rows.length > 0) {
            for (i = 0; i < rows.length; i++) {
                pid = rows[i].id.replace("r", "");
                pids.push(pid);
                $("#chk" + pid).prop("checked", true);
            }
        }
    }

    if (pids.length > 0) {
        $("#tg_selected_pid").val(pids[0]);
        $("#tg_selected_pids").val(pids.join(","));
    } else {
        $("#tg_selected_pid").val("");
        $("#tg_selected_pids").val("");
    }

    if (document.getElementById("exportSelPocet"))     //menu_selected - nadpis
    {
        $("#exportSelPocet").text("(" + pids.length + ")");
    }
    if (document.getElementById("akceSelPocet"))     //menu_selected - nadpis
    {
        $("#akceSelPocet").text(pids.length);
    }
    
}

function tg_select_one_row(ctl, pid) {    
    if (!_tg_is_enable_selecting) return;   //není zapnuto označování záznamů
    tg_clear_selection();
    

    if (ctl.classList.contains("selrow") === false) {
        ctl.classList.add("selrow");
    }

    $("#chk" + pid).prop("checked", true);

    $("#tg_selected_pid").val(pid);
    $("#tg_selected_pids").val(pid);
}


function get_all_tgi_params(strOper,strKey, strValue) {

    var record_bin_query = "";
    var p31_tab_query = "";
    var period_value = "";
    var period_field = "";
    var d1_iso = $("#d1_iso").val();
    var d2_iso = $("#d2_iso").val();
    var p31_state_query = "";

    if (typeof _RecordBinQuery_Value !== "undefined") record_bin_query = _RecordBinQuery_Value;
    if (typeof _p31TabQuery_Value !== "undefined") p31_tab_query = _p31TabQuery_Value;
    if (typeof _p31StateQuery_Value !== "undefined") p31_state_query = _p31StateQuery_Value;
    if (typeof _periodfield !== "undefined") period_field = _periodfield;
    if (typeof _periodvalue !== "undefined") period_value = _periodvalue;
    
    
    var params = {
        entity: _tg_entity,
        oper: strOper,
        key: strKey,
        value: strValue,
        j72id: _j72id,
        go2pid: _tg_go2pid,
        master_entity: _tg_master_entity,
        oncmclick: _tg_oncmclick,
        ondblclick: _tg_ondblclick,
        fixedcolumns: _tg_fixedcolumns,
        viewstate: [],
        master_pid: _tg_master_pid,
        myqueryinline: _tg_myqueryinline,
        rez:_tg_rez,
        isperiodovergrid: _tg_isperiodovergrid,
        pathname: location.pathname,
        currentpid: _tg_current_pid,
        reczoomflag: _tg_reczoomflag,
        is_enable_selecting: _tg_is_enable_selecting,
        record_bin_query: record_bin_query,
        p31_tab_query: p31_tab_query,
        period_value: period_value,
        period_field: period_field,
        d1_iso: d1_iso,
        d2_iso: d2_iso,
        p31_state_query: p31_state_query,
        j72id_query: _j72id_query
    }

    if (_tg_viewstate !== "") {
        params.viewstate = _tg_viewstate.split("|");
    }
    
    //alert(_periodUserParamKey + ": " + d1_iso + " - " + d2_iso + ", _RecordBinQuery_Value: " + record_bin_query + ", _p31TabQuery_Value: " + p31_tab_query + ", _p31StateQuery_Value: " + p31_state_query + ", _periodfield: " + period_field);
    return params;
}

function get_all_path_values() {    //rozloží kompletní querystring do pole vhodné pro BO.StringPair
    var onepar;
    var pars = [];
    var in_url_str = window.location.search.replace("?", "").split("&");
    $.each(in_url_str, function (kay, val) {
        var v = val.split("=");
        onepar = {
            Key: v[0],
            Value: v[1]
        }
        pars.push(onepar);
    });

    return pars;
}


function tg_select_all_toggle() {
    var pids = $("#tg_selected_pids").val();
    if (pids.indexOf(",") === -1) {
        tg_select(1000);    //zaškrtnout vše
    } else {
        tg_clear_selection();   //odškrtnout vše
    }
}






function tg_div_close_synthetic_divs() {
    if (document.getElementById("tg_div_setting")) $("#tg_div_setting").css("display", "none");
    if (document.getElementById("tg_div_selecting")) $("#tg_div_selecting").css("display", "none");
    if (document.getElementById("tg_div_print")) $("#tg_div_print").css("display", "none");


}




function tg_groupbycolumn_mouseover(th) {
    if (!document.getElementById("cmdGridColumnGroupBy")) {
        return;
    }
    var offset = $(th).offset();
    var btn = document.getElementById("cmdGridColumnGroupBy");
    $(btn).attr("field", $(th).attr("id"));
    
    
    $(btn).css("top", offset.top + ($(th).height() - $(btn).height()) / 2);
    if ($(th).width() > 100) {
        $(btn).css("left", offset.left + $(th).width() - $(btn).width()-40);
    } else {
        $(btn).css("left", offset.left + $(th).width() - $(btn).width());
    }

    
    if ($(th).attr("id") == "th_"+_tg_current_groupfield) {
        $(btn).css("background-color", "red");        
    }
    
    $(btn).css("display", "block");
}
function tg_groupbycolumn_mouseout(th) {
    if (!document.getElementById("cmdGridColumnGroupBy")) {
        return;
    }
    
    var btn = document.getElementById("cmdGridColumnGroupBy");    

    if (btn.matches(":hover")) {
        return; //myš je nad tlačítkem
    }

    $(btn).css("background-color", "");
    
    $(btn).css("display", "none");
}

function tg_groupby_click(cmd) {
    var field = $(cmd).attr("field").replace("th_","");

    _tg_current_groupfield = field;
    
    tg_post_handler("groupby", "groupfield", field);
    
}
function tg_groupbyval_click(cmd,group_row_index) {
    var val = $(cmd).attr("data-val");
    if (val == "") {
        val = "null";
    }
    _tg_go2grouprow = group_row_index;
    
    tg_post_handler("groupbyval", "groupvalue", val);
    

}

function tg_init_grouprow_dblclick()
{    
    $("#tabgrid1 .trgroup").on("dblclick", function () {
        var rowid = $(this).attr("id");
        var group_row_index = $(this).attr("id").replace("rg", "");
        var cmd = $("#" + rowid + " .groupvallink")[0];
        var val = $(cmd).attr("data-val");

        _tg_go2grouprow = group_row_index;

        tg_post_handler("groupbyval", "groupvalue", val);

    });
}


function tg_scrollinto_row(row)
{
    $("#container_vScroll").animate(
        {
            scrollTop: $(row).offset().top - $("#container_vScroll").offset().top,
        },
        500
    );


    //dřív to bylo takhle:
    //row.scrollIntoView({ block: "start", behavior: "smooth" });
}