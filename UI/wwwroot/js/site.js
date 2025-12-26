//na úvod detekce mobilního zařízení přes _device
var _device = {
    isMobile: false,
    type: "Desktop",
    availHeight: screen.availHeight,
    availWidth: screen.availWidth,
    innerWidth: window.innerWidth,
    innerHeight: window.innerHeight
}

if (screen.availHeight > screen.availWidth || screen.availWidth < 800 || screen.availHeight < 500) {   //mobilní zařízení výšku vyšší než šířku
    _device.isMobile = true;
    _device.type = "Phone";
    

}


document.onkeydown = function (e) {
    // Klávesové zkratky, ALT+W ALT+Q
    
    if (e.altKey && !e.shiftKey) {
                
        if (e.key == "w" || e.key == "W") {     
            if (parent.window.document.getElementById("fraModalContent"))                
            {
                var fra = parent.window.document.getElementById("fraModalContent");
                var okno = parent.window.document.getElementById("myModalContainer"); 
                if (fra.src.indexOf("/p31/Record") > 0 && $(okno).css("display") == "block")
                {
                    _notify_message("Jedno okno už máš otevřené.", "info");
                    return;
                }
                

            }
            
            if (window.location.href.indexOf("TheGrid/") > 0 || window.location.href.indexOf("/RecPage") > 0)
            {                
                var pid = $("#tg_selected_pid").val();
                
                var prefix = _thegrid.entity.substr(0, 3);
                if (pid != "0" && pid != "" && (prefix == "le5" || prefix == "p56" || prefix == "p28"))
                {
                    _p31_entry_from_grid();
                    return;
                }
                
                
            }
 
            _window_open("/p31/Record?pid=0", 3);
        }
        if (e.key == "q" || e.key == "Q") {
            if (parent.window.document.getElementById("cmdSaveMyToolbar")) {
                parent.window.document.getElementById("cmdSaveMyToolbar").click();
                return;
            }
            if (document.getElementById("cmdSaveMyToolbar"))
            {
                document.getElementById("cmdSaveMyToolbar").click();
                return;
            }
            
            _notify_message("Nevidím tlačítko [Uložit změny].");
        }
        if (e.key == "r" || e.key == "R" || e.key == "s" || e.key == "S") {            
            if (window.location.href.indexOf("TheGrid/") > 0 || window.location.href.indexOf("/RecPage") > 0) {                  
                var prefix = _thegrid.entity.substr(0, 3);
                
                if (prefix == "le5" || prefix == "p31" || prefix == "p28" || prefix == "p56" || prefix == "j02") {                    
                    tg_approve();
                }
                
            }

            
        }

        if ((e.key == "a" || e.key == "A") && window.location.href.indexOf("TheGrid/") > 0 && document.getElementById("tg_selected_pid")) {
            var pid = $("#tg_selected_pid").val();
            if (pid == "" || pid=="0") {
                return;
            }
            var tr = document.getElementById("r" + pid);
          
            tr.getElementsByClassName("cm")[0].click();
        }

        if ((e.key == "2" || e.key == "ě") && document.getElementById("cmdnew_mainmenu")) {
            document.getElementById("cmdnew_mainmenu").click(); 
            return;
        }
        if ((e.key == "1" || e.key == "+") && document.getElementById("cmdHome")) {
            document.getElementById("cmdHome").click();
            return;
        }
        if (e.key == "3" || e.key == "š") {
            _window_open("/p68/Index", 3);
        }

        
    }

    if (e.shiftKey && e.altKey) {   //shift+číslo barvy: zatím vypnuto       
        var prefix = _thegrid.entity.substr(0, 3);
        var pids = $("#tg_selected_pids").val();

        if ((prefix == "p31" || prefix == "le5" || prefix=="p28" || prefix=="p91" || prefix=="o23") && (e.key == "1" || e.key == "2" || e.key == "3" || e.key == "4" || e.key == "5")) {            
            if (pids === "") {
                _notify_message("Musíte zaškrtnout/označit minimálně jeden záznam.");
                return;
            }
            
            _rowcolor(prefix, pids, e.key);

            _notify_message("Barevné záznamy #" + e.key + ".", "info");
        }
        if ((prefix == "p31" || prefix == "le5" || prefix == "p28" || prefix == "p91" || prefix=="o23") && e.key == "6") {            
            if (pids === "") {
                _notify_message("Musíte zaškrtnout/označit minimálně jeden záznam.");
                return;
            }
            
            _rowcolor_clear(prefix, pids);

            _notify_message("Vyčištění barvy záznamů.", "info");
        }
    }
};

function _ep(url)   //vrací relativní cestu z url výrazu
{
    if (url.indexOf("//") > 0) {
        return url;
    }
    if (_relpath === "/" && url.substring(0, 1) === "/") {
        return url;
    }
    if (url.substring(0, 1) === "/") {
        url = url.substring(1, url.length - 1);
    }
    return _relpath + url;
}
function _redirect(url) {
    location.href = url;
    //location.replace(_ep(url));
}

///vrací číslo
function _string2number(s) {
    s = s.replace(/\s/g, '');
    s = s.replace(/[,]/g, '.');
    var num = 0;
    num = Number(s);
    return (num);
}
function _roundnum(n, decimals) {

    return (n.toFixed(decimals));
}

function _format_number(val) {
    if (val === null) {
        return ("");
    }

    var s = val.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, '$1,');
    return (s);
}
function _format_number_int(val) {
    if (val === null) {
        return ("");
    }
    var s = val.toFixed(0).replace(/(\d)(?=(\d{3})+\.)/g, '$1,');
    return (s);
}

function _format_date(d, is_include_time) {

    var month = '' + (d.getMonth() + 1);
    var day = '' + d.getDate();
    var year = d.getFullYear();
    var hour = d.getHours();
    var minute = d.getMinutes();

    if (is_include_time === true) {
        return (day + "." + month + "." + year + " " + hour + ":" + minute);
    } else {
        return (day + "." + month + "." + year);
    }

}

function _format_time(d) {
    var hour = d.getHours();
    var minute = d.getMinutes();

    if (minute < 10) {
        minute = "0" + minute;
    }

    if (hour < 10) {
        hour = "0" + hour;
    }

    return (hour + ":" + minute);

}

function _rowcolor(prefix, pids, colorindex) {
   
    $.post("/Common/UpdateRowColor", { prefix: prefix, pids: pids,colorindex:colorindex }, function (data) {
        
        var arr = pids.split(",");        
        for (var i = 0; i < arr.length; i++) {
            var pid = arr[i];
            $("#r" + pid).removeClass("trbg1");
            $("#r" + pid).removeClass("trbg2");
            $("#r" + pid).removeClass("trbg3");
            $("#r" + pid).removeClass("trbg4");
            $("#r" + pid).removeClass("trbg5");
            $("#r" + pid).addClass("trbg" + colorindex);
        }                
        
        
    });

}

function _rowcolor_clear(prefix, pids) {
    $.post("/Common/ClearRowColor", { prefix: prefix, pids: pids }, function (data) {

        var arr = pids.split(",");
        for (var i = 0; i < arr.length; i++) {
            var pid = arr[i];
            $("#r" + pid).removeClass("trbg1");
            $("#r" + pid).removeClass("trbg2");
            $("#r" + pid).removeClass("trbg3");
            $("#r" + pid).removeClass("trbg4");
            $("#r" + pid).removeClass("trbg5");
        }     
                
    });
}



function _delete(prefix, pid) {
  
    if (confirm("Opravdu nenávratně odstranit tento záznam?")) {        
        $.post("/Common/DeleteRecord", { entity: prefix, pid: pid }, function (data) {
           
            if (data == "1")
            {                
                if (window !== top)
                {   //voláno uvnitř iframe
                    window.parent.document.location.href = window.parent.document.location.href;
                }
                else
                {
                    location.replace(window.location.href);
                }
            }
            else
            {
                alert(data);        //byla vrácena chyba
            }
            
        });
        
    }
}

function _edit(controller, pid, header,rez) {
    
    var url = "";
    var winflag = 1;
    if (controller.substring(0, 2) === "le") {
        _window_open("/p41/Record?pid=" + pid + "&levelprefix=" + controller, winflag, header);
        return;
    }
    
    switch (controller) {
        case "x40":
            url = "/Mail/Record?pid=" + pid;
            break;        
        case "j90":
        case "j92":
        case "p11":
        case "p55":
        case "j95":
            _notify_message("Pro tento záznam neexistuje stránka detailu.", "info");
            return;
        default:
            url = "/" + controller + "/record?pid=" + pid;
            if (typeof rez !== "undefined")
            {
                url = url + "&rez=" + rez;
            }
            
            break;
    }
    if (controller == "p31" || controller=="p56" || controller=="o22") {
        winflag = 3;
    }
    if (controller === "p51" || controller == "o43") {
        winflag = 2;
    }

    _window_open(url, winflag, header);

}



function _clone(controller, pid, header) {
    var winflag = 1;
    if (controller == "p31") winflag = 3;
    
    var url = "/" + controller + "/Record?isclone=true&pid=" + pid;

    if (controller == "p91") {
        url = "/p91Clone/Index?pid=" + pid;
    }

    _window_open(url, winflag, header);
        
    

}

function _edit_code(prefix, pid) {
    _window_open("/Record/RecordCode?prefix=" + prefix + "&pid=" + pid, 1);
}

function _get_request_param(name) {
    var results = new RegExp("[\?&]" + name + "=([^&#]*)").exec(window.location.href);
    return results ? decodeURIComponent(results[1].replace(/\+/g, '%20')) : null;
}



function _notify_message_hide()
{
    if (!document.getElementById("notify_container"))
    {
        return;
    }
    $("#notify_container").html("");
    
}


function _notify_message(strMessage, strTemplate = "error", milisecs = "3000") {
    if (document.getElementById("notify_container")) {
        //notify div na stránce již existuje          
    } else {
        var el = document.createElement("DIV");
        $(el).css("position", "absolute");
        $(el).css("top", "0px");
        $(el).css("z-index", "9999");
        if (screen.availWidth > 500) $(el).css("left", window.innerWidth - 500);

        el.id = "notify_container";
        document.body.appendChild(el);
    }
    if (strTemplate) {
        if (strTemplate === "") strTemplate = "info";
    } else {
        strTemplate = "info";
    }


    var img = "info", intTimeoutSeconds = 5000;
    if (strMessage.length > 250) intTimeoutSeconds = 10000;

    if (strTemplate === "error") {
        img = "exclamation-triangle";
        strTemplate = "danger";
    }
    if (strTemplate === "warning") img = "exclamation";
    if (strTemplate === "success") img = "thumbs-up";
    var toast_id = "toast" + parseInt(100000 * Math.random());

    var node = document.createElement("DIV");
    node.id = "box" + parseInt(100000 * Math.random());
    var w = "400px";
    if (screen.availWidth < 400) w = "95%";

    var s = "<div id='" + toast_id + "' class='toast' role='alert' arial-live='assertive' aria-atomic='true' data-autohide='true' data-delay='" + milisecs + "' data-animation='true' style='margin-top:10px;min-width:" + w + ";'>";
    s = s + "<div class='toast-header text-dark bg-" + strTemplate + "'><i class='fas fa-" + img + "'></i>";
    //s = s + "<strong class='mr-auto' style='color:black;'>Toast Header</strong>";
    s = s + "<div style='width:90%;'> " + strTemplate + "</div><button type='button' class='btn-close' data-bs-dismiss='toast' aria-label='close'></button>";
    s = s + "</div>";
    s = s + "<div class='toast-body' style='font-size:130%;'>";
    s = s + strMessage;
    s = s + "</div>";
    s = s + "</div>";


    $(node).html(s);
    document.getElementById("notify_container").appendChild(node);

    if (typeof is_permanent !== "undefined") {
        if (is_permanent === true) return;
    }

    $("#" + toast_id).toast("show");



}


//vyvolání kontextového menu
function _cm(e, entity, pid, menu_source, master_entity) {

    if (menu_source === undefined) menu_source = null;
    if (master_entity === undefined) master_entity = null;

    var ctl = e.target;

    var w = $(window).width();
    var pos_left = e.clientX + $(window).scrollLeft();

    var cssname = "cm_left2right";
    
    if (_device.type === "Phone") {
        cssname = "cm_mobile";
        
    }
    var menuid = "cm_" + entity + "_" + pid;

    if (!document.getElementById(menuid)) {        
        //div na stránce neště existuje
        var el = document.createElement("DIV");
        el.id = menuid;
        el.className = cssname;
        el.style.display = "none";
        document.body.appendChild(el);
    }


    if (ctl.getAttribute("menu_je_inicializovano") === "1") {
        return; // kontextové menu bylo již u tohoto elementu inicializováno - není třeba to dělat znovu.
    }

    var menuLoadByServer = true;

    if (document.getElementById("divContextMenuStatic")) {
        var data = $("#divContextMenuStatic").html();   //na stránce se nachází preferované UL statického menu v divu id=divContextMenuStatic -> není třeba ho volat ze serveru
        data = data.replace(/#pid#/g, pid);  //místo #pid# replace pravé pid hodnoty
        $("#" + menuid).html(data);

        menuLoadByServer = false;

    } else {
        //načíst menu později dynamicky ze serveru   
        $("#" + menuid).html("Loading...");
        menuLoadByServer = true;

    }

    $(ctl).contextMenu({
        menuSelector: "#" + menuid,
        menuClicker: ctl,
        menuEntity: entity,
        menuPid: pid,
        menuSource: menu_source,
        menuMasterEntity: master_entity,
        menuLoadByServer: menuLoadByServer

    });
    ctl.setAttribute("menu_je_inicializovano", "1");




}



function FindByAttributeValue(attribute, value, element_type) {
    element_type = element_type || "*";
    var All = document.getElementsByTagName(element_type);
    for (var i = 0; i < All.length; i++) {
        if (All[i].getAttribute(attribute) == value) { return All[i]; }
    }

    return null;
}

function _mainmenu_highlight_current(linkID, orlinkID) {

    var url = window.location.pathname + window.location.search;
    var link = FindByAttributeValue("data-url", url, "a");
    if (link != null) {
        $(link).addClass("active");
       
    }

    if (document.getElementById(linkID)) {
        $("#" + linkID).addClass("active");  //označit odkaz v hlavním menu
       
    }

    if (orlinkID !== undefined && document.getElementById(orlinkID)) {
        $("#" + orlinkID).addClass("active");  //označit odkaz v hlavním menu
    }
    
    if (linkID == "cmdo22" || linkID == "cmdx31" || linkID == "cmdp15")
    {        
        $("#cmdmisc_mainmenu").addClass("active");
    }
    if (linkID == "cmdo53" || linkID == "cmdo51") {
        $("#cmdadmin").addClass("active");
    }
}










function _removeUrlParam(key, sourceURL) {
    var rtn = sourceURL.split("?")[0],
        param,
        params_arr = [],
        queryString = (sourceURL.indexOf("?") !== -1) ? sourceURL.split("?")[1] : "";
    if (queryString !== "") {
        params_arr = queryString.split("&");
        for (var i = params_arr.length - 1; i >= 0; i -= 1) {
            param = params_arr[i].split("=")[0];
            if (param === key) {
                params_arr.splice(i, 1);
            }
        }
        if (params_arr.length > 0) {
            rtn = rtn + "?" + params_arr.join("&");
        }

    }
    return rtn;
}


function _resize_textareas() {
    $("textarea").each(function () {
        this.style.height = "auto";
        this.style.height = (this.scrollHeight) + "px";

        $(this).on("input", function () {
            this.style.height = "auto";
            this.style.height = (this.scrollHeight) + "px";
        });
    });
}



////z elementu se stane draggable:
function _make_element_draggable(elmnt, inner_elmnt_4hide) {
    var pos1 = 0, pos2 = 0, pos3 = 0, pos4 = 0;
    if (document.getElementById(elmnt.id + "header")) {
        /* if present, the header is where you move the DIV from:*/
        document.getElementById(elmnt.id + "header").onmousedown = dragMouseDown;
    } else {
        /* otherwise, move the DIV from anywhere inside the DIV:*/
        elmnt.onmousedown = dragMouseDown;
    }

    function dragMouseDown(e) {


        e = e || window.event;
        e.preventDefault();
        // get the mouse cursor position at startup:
        pos3 = e.clientX;
        pos4 = e.clientY;
        document.onmouseup = closeDragElement;
        // call a function whenever the cursor moves:
        document.onmousemove = elementDrag;
    }

    function elementDrag(e) {
        inner_elmnt_4hide.style.display = "none";
        e = e || window.event;
        e.preventDefault();
        // calculate the new cursor position:
        pos1 = pos3 - e.clientX;
        pos2 = pos4 - e.clientY;
        pos3 = e.clientX;
        pos4 = e.clientY;
        // set the element's new position:
        elmnt.style.top = (elmnt.offsetTop - pos2) + "px";
        elmnt.style.left = (elmnt.offsetLeft - pos1) + "px";
    }

    function closeDragElement() {
        /* stop moving when mouse button is released:*/
        inner_elmnt_4hide.style.display = "block";
        $("#inner_elmnt_4hide").attr("disabled", "");
        document.onmouseup = null;
        document.onmousemove = null;
    }
}


function _init_elements_for_mobile()
{
    //osekat html na mobilní telefony
    $("label").prop("for", null);
    
    if (document.getElementById("form1")) {
        
        $("#form1").css("overflow-x", "hidden");
    }

    $(".div-over-table-scrolling").css("width", _device.availWidth);
    $(".div-over-table-scrolling").css("overflow", "scroll");
    
}

//inicializovat na formuláři všechny qtips
function _init_qtip_onpage() {
    //qtip:
        
    
    var iframeWidth = 800;
    var maxwidth = $(window).innerWidth();
    if (maxwidth < 800) {
        iframeWidth = maxwidth;
    }

    if (window !== top && _get_request_param("hover_by_reczoom") === "1") {   //voláno uvnitř qtip iframe: zde už reczoom schovat
        $("a.reczoom").each(function () {
            $(this).css("display", "none");
            $(this).removeClass("reczoom");
        });
        return;
    }

    $("a.reczoom").each(function () {

        var mywidth = iframeWidth;
        var $this = $(this);
        var myurl = $this.attr("data-rel");
        var myheight = $this.attr("data-height");
        var mywidth = iframeWidth + "px";
        var myinitvisibility = $this.attr("data-visibility");

        if ($this.attr("data-width") !== undefined) {
            mywidth = $this.attr("data-width");
        }

        if (myheight === undefined || myheight === null || myheight === "") {
            myheight = 270;
        }
        var mytitle = $this.attr("data-title");
        if (mytitle === null || mytitle === "") {
            mytitle = "Detail";
        }
        if ($this.attr("data-maxwidth") === "1") {
            mywidth = $(window).innerWidth() - 10;     //okno má mít maximální šířku            
        }

        if (myinitvisibility !== undefined) {
            $(this).css("visibility", myinitvisibility);
        }

        var mypos_topleft = "top center";   //pozice top left, výchozí hodnota: top center
       
        if ($this.attr("data-pos")!== undefined) {
            mypos_topleft = $this.attr("data-pos");
        }


        $this.qtip({
            content: {
                text: "<iframe id='fraRecZoom' framemargin='0' style='height:" + myheight + "px;width:" + mywidth + ";' src='" + myurl + "'></iframe>",
                title: {
                    text: mytitle
                },

            },
            position: {
                my: mypos_topleft,  // Position my top left..., default: top center
                at: "bottom center", // at the bottom right of...
                viewport: $(window),
                adjust: {
                    method: "shift"
                }
            },

            hide: {
                fixed: true,
                delay: 100
            },
            style: {
                classes: "qtip-tipped",
                width: mywidth + 30,
                height: parseInt(myheight) + 30,
                
            }
        });
    });
}


//upozornění uživatele na editaci prvku na formuláři
function _toolbar_warn2save_changes(message) {
    if (typeof message === "undefined") {
        message = "Změny potvrďte tlačítkem [Uložit změny].";
    }
    if ($("#toolbar_changeinfo").length) {
        $("#toolbar_changeinfo").text(message);
    }

}

//spustit hardrefresh na volající stránkce

function _reload_layout_and_close(pid, flag) { 
    
    if (flag === null || flag === "") {
        flag = window.location.href;
    }
    if (window !== top) {   
        
        window.parent.hardrefresh(pid, flag);
        window.parent._window_close();
    } else {
        
            try {
           
                hardrefresh(pid, flag);
            } catch (err)
            {
               
                window.close();
            }
        
    }
}

function _close_and_reload_parentsite(url) {
    if (window !== top) {
        window.parent.location.replace(url);
        window.parent._window_close();
    } else {
        location.replace(url);
    }
}


//vyvolání zoom info okna
function _zoom(e, entity, pid, dialog_width, header, url, nochange) {     //nochange=true: obsah bude vždy načten znovu
    
    var ctl = e.target;
    if (typeof nochange === "undefined")
    {
        nochange=false
    }
    
    var maxwidth = $(window).innerWidth();
    var maxheight = $(window).innerHeight();
    var minwidth = 300;

    if (maxwidth > 1000) maxwidth = 1000;

    var w = 600;
    var h = 600;

    if (typeof dialog_width !== "undefined") {
        w = dialog_width;
        minwidth = w;
        
    }
    if (w > maxwidth) {
        w = maxwidth - 10;
    }
    if (h > maxheight) {
        h = maxheight - 10;
    }
    
    
    if ((w + e.pageX) > $(window).innerWidth()) {        
        w = $(window).innerWidth() - e.pageX - 10;
        if (w > maxwidth) {
            w = maxwidth - 20;
        }
        if (w < minwidth) {
            w = minwidth;
        }
    }


    if (typeof url === "undefined") {
        url = "/" + entity + "/Info?pid=" + pid;
    }
    if (typeof header === "undefined") {
        header = "INFO";
    }

    var menuid = "zoom_okno";
   
    if (!document.getElementById(menuid)) {
        //div na stránce neště existuje
        var el = document.createElement("DIV");
        el.id = menuid;
        el.style.display = "none";
        el.style.zIndex = "99999";
        document.body.appendChild(el);
    }

    

    $("#" + menuid).width(w);
    $("#" + menuid).height(h);

    
    //document.getElementById("frazoom").contentDocument.location.reload(true);

    var s = "<div id='divZoomContainer' style=';border:solid 1px silver;width:" + w + "px;' orig_w='" + w + "' orig_h='" + h + "'>";
    s += "<div id='divZoomHeader' style='cursor: move;height:30px;background-color:lightsteelblue;padding:3px;' ondblclick='_zoom_toggle()'>" + header;
    s += "<a href='javascript:_zoom_close()' style='margin-left:auto;margin-right:6px;float:right;'><span class='material-icons-outlined-btn'>close</span></a>";
    s += "</div>";
    s += "<div id='divZoomFrame'>";
    s += "<iframe id='frazoom' src = '" + url + "' style = 'width:100%;height: " + (h - 31) + "px;' frameborder=0></iframe >";
    s += "</div>";
    s += "</div>";

    $("#" + menuid).html(s);

    _make_element_draggable(document.getElementById(menuid), document.getElementById("divZoomFrame")); //předávání nefunguje přes jquery

    if (ctl.getAttribute("menu_je_inicializovano") === "1")
    {
        
        
        return; // kontextové menu bylo již u tohoto elementu inicializováno - není třeba to dělat znovu.
    }

    

    $(ctl).contextMenu({
        menuSelector: "#" + menuid,
        menuClicker: ctl,
        menuLoadByServer: false,
        menuWithoutCache: nochange      //obsah menu se bude vždy načítat znovu

    });

    ctl.setAttribute("menu_je_inicializovano", "1");
}

function _zoom_close()
{
    $("#divZoomContainer").css("display", "none");
}

function _zoom_toggle()
{    
    var okno = $("#divZoomContainer");
    var offset = $(okno).offset();

    var w = $(window).width() - offset.left - 10;
    var h = $(window).height() - offset.top - 10;

    if ($(window).width() - offset.left - $(okno).width() < 30) {
        w = $("#divZoomContainer").attr("orig_w");
        h = $("#divZoomContainer").attr("orig_h");
    }

    $(okno).width(w);
    $(okno).height(h);
    $("#divZoomFrame").height(h - 31);
    $("#frazoom").height(h - 31);

}

function _helppage() {
    var s = document.title.replace(" - MARKTIME", "");
    var viewurl = window.location.pathname.split('?')[0];
    var fullurl = viewurl+ window.location.search;
    try {        
        _window_open("/x51/Index?viewurl=" + viewurl + "&fullurl=" + fullurl, 1, "Help");
    } catch (err) {
        window.open("/x51/Index?viewurl=" + viewurl + "&pagetitle=" + s+"&fullurl="+fullurl, "_blank");     //pokud na stránce není definice metody _window_open (např. reporting)
    }
}

function _helppage_layout() {
    var s = document.title.replace(" - MARKTIME", "");

    var viewurl = window.location.pathname.split('?')[0] + window.location.search;
    _window_open("/x51/Index?viewurl=" + viewurl, 1, "Help");
}


function _resize_iframe_onpage(iframe_id) {
    var offset = $("#" + iframe_id).offset();
    var remain_height = _device.innerHeight - offset.top;
    remain_height = parseInt(remain_height) - 20;
    if (_device.type === "Phone") {
        h_vertical = 400;
    }
    if (remain_height < 100)
    {
        remain_height = 100;
    }
    $("#" + iframe_id).css("height", remain_height + "px");
}

function _location_replace_top(url) {
    if (window !== top) {   //voláno uvnitř iframe
        window.open(url, "_top");
    } else {
        location.replace(url);
    }

}



//--------------------- funkce pro volání ze splitter stránek --------------------------

function _splitter_init(splitterLayout, prefix) {    //splitterLayout 1 - horní a spodní panel, 2 - levý a pravý panel
    
    
    var cont = document.getElementById("splitter_container");
    var offset = $(cont).offset();
    var h = _device.innerHeight - offset.top - 2;       //proč je třeba odečíst 2 nevím
    $(cont).height(h);

    if (splitterLayout === "2" && cont.className !== "splitter-container-left2right") {
        $("#splitter_container").attr("class", "splitter-container-left2right");
        $("#splitter_resizer").attr("class", "splitter-resizer-left2right");
        $("#splitter_panel1").attr("class", "splitter-panel-left");
        $("#splitter_panel2").attr("class", "splitter-panel-right");
    }
    if (splitterLayout === "1" && cont.className !== "splitter-container-top2bottom") {
        $("#splitter_container").attr("class", "splitter-container-top2bottom");
        $("#splitter_resizer").attr("class", "splitter-resizer-top2bottom");
        $("#splitter_panel1").attr("class", "splitter-panel-top");
        $("#splitter_panel2").attr("class", "splitter-panel-bottom");
    }

    if (splitterLayout === "2") {
        document.getElementById("fra_subgrid").height = h - 1;
    }


    $("#splitter_panel1").resizable({
        handleSelector: "#splitter_resizer",

        onDragStart: function (e, $el, opt) {
            //resizeHeight: false
            //$("#splitter_panel2").html("<h6>Velikost panelu ukládám do vašeho profilu...</h6>");

            return true;
        },
        onDragEnd: function (e, $el, opt) {     //splitterLayout 1 - horní a spodní panel, 2 - levý a pravý panel
            var id = $el.attr("id");
            var panel1_size = $el.height();
            var key = prefix + "_panel1_size";

            if (splitterLayout === "2") {
                panel1_size = $el.width();
            }


            _notify_message("Velikost panelu ukládám do vašeho profilu.<hr>" + key + ": " + panel1_size + "px", "info");
            localStorage.setItem(key, panel1_size);

            if (document.getElementById("tabgrid1")) {
                tg_adjust_for_screen("splitter_panel1");
            }
            if (document.getElementById("fra_subgrid")) {
                document.getElementById("fra_subgrid").contentDocument.location.reload(true);
            }

            //run_postback(key, panel1_size);          //velikost panelu se uloží přes postback             

        }
    });
}

function _splitter_resize_after_init(splitterLayout, defaultPanel1Size) {   //splitterLayout 1 - horní a spodní panel, 2 - levý a pravý panel
    var win_size = $("#splitter_container").height();
    var splitter_size = $("#splitter_resizer").height();
    var panel1_size = parseInt(defaultPanel1Size);

    if (splitterLayout === "1") {
        //výšku iframe přepočítávat pouze v režimu horní+spodní    
        if (panel1_size === 0) panel1_size = 500;
        $("#splitter_panel1").height(panel1_size);
        //alert("panel1: " + $("#splitter_panel1").height() + ", panel2: " + $("#splitter_panel2").height());

        document.getElementById("fra_subgrid").height = win_size - panel1_size - splitter_size;
    } else {
        document.getElementById("fra_subgrid").height = win_size - 1;
        if (panel1_size === 0) panel1_size = 300;
        $("#splitter_panel1").width(panel1_size);

    }
}

function _loading_show(flag)
{    
    if (flag === "undefined")
    {
        flag = "topleft";        
    }
    var strClass = "loader_" + flag;

    if (flag == "postback")
    {
        strClass = "loader_topleft";
    }
    if (!document.getElementById("loading_container")) {
        var el = document.createElement("DIV");
        $(el).addClass(strClass);
        el.id = "loading_container";
        document.body.appendChild(el);
    }
    else
    {
        document.getElementById("loading_container").classList.add(strClass);
    }

    if (flag == "postback")
    {
        document.getElementById("loading_container").classList.add("loader_postback");
    }

}
function _loading_hide() {
    
    if (!document.getElementById("loading_container")) {
        return;
    }
    if ($("#loading_container").hasClass("loader_topleft")) {
        document.getElementById("loading_container").classList.remove("loader_topleft");
    }
    if ($("#loading_container").hasClass("loader_middle")) {
        document.getElementById("loading_container").classList.remove("loader_middle");
    }
    if ($("#loading_container").hasClass("loader_white")) {
        document.getElementById("loading_container").classList.remove("loader_white");
    }
    
}

function _showloading() {
    var index = "1";
    if (window !== top) {   //voláno uvnitř iframe
        index = "2";
    }
    if (document.getElementById("#site_loading" + index)) {
        $("#site_loading" + index).css("display", "block");
    }

}


function _load_ajax_async(strHandlerUrl, params, callback) {
    _load_ajax_data(strHandlerUrl, params, callback, true, "json");
}

///vrací ajax výsledek
function _load_ajax_data(strHandlerUrl, params, callback, is_async, data_type) {
    var is_success = false;
    var ret = null;
    var is_done = false;
    if (is_async === undefined || is_async === null) is_async = false;
    if (data_type === undefined || data_type === null) data_type = "json";

    $.ajax({
        url: strHandlerUrl,
        type: "POST",
        dataType: data_type,
        async: is_async,
        cache: false,
        data: params,
        complete: function (result) {
            is_success = true;
            if (callback !== undefined) {
                callback(ret);
            } else {
                return (ret);
            }

        },
        success: function (result) {
            is_done = true;
            is_success = true;

            ret = result;

        },
        error: function (e, b, error) {
            is_done = true;
            if (strHandlerUrl != "/Home/UpdateCurrentUserPing") {
                alert("load_ajax_data: " + error + "/" + strHandlerUrl);
            }

        }
    });

    if (is_async === false) {
        return (ret);
    }

}



function handle_clipboard_textarea(ctl, e) { //očištění html znaků z clipboard schránky u textarea kontrolů    

    var s_source = "";
    if (window.clipboardData && window.clipboardData.getData) { // IE        
        s_source = window.clipboardData.getData("Text");
    }
    else if (e.originalEvent.clipboardData && e.originalEvent.clipboardData.getData) { // other browsers
        s_source = e.originalEvent.clipboardData.getData("text/plain");
    }

    var s_dest = $("<div>").html(s_source).text();

    if (s_source.length - s_dest.length > 3) {
        e.preventDefault();    // cancel paste   
        _notify_message("Text vložený ze schránky byl očištěn o několik nepovolených HTML znaků, které by MARKTIME odmítl uložit.", "info");
    } else {
        return; //nechat fungovat standardní paste funkcionalitu
    }

    $(ctl).val($(ctl).val() + s_dest);
}


function close_offcanvas(offcanvas_id) {
    const el = document.getElementById(offcanvas_id);
    bootstrap.Offcanvas.getInstance(el)?.hide();
}

function register_menu(cmd_id, ul_id, method) {
    if (!document.getElementById(cmd_id)) {
        return;
    }

    var myDropdown = document.getElementById(cmd_id);
    myDropdown.addEventListener("show.bs.dropdown", function () {
        render_menu(cmd_id, ul_id, method);
        $("#" + cmd_id).css("background-color", "#5641e1");
    })
    myDropdown.addEventListener("hide.bs.dropdown", function () {
        //když se dropdown bootstrap zavře
        
        
        $("#" + cmd_id).css("background-color", "");
    })
}

function render_menu(cmd_id, ul_id, method) {
    if ($("#" + cmd_id).attr("nacteno") == "1") {
        return;
    }
    var url = method;
    if (url.indexOf("?") > 0)
    {
        url = url + "&userdevice=" + _device.type;
    }
    else
    {
        url = url + "?userdevice=" + _device.type;
    }


    $.post(url, function (data) {
        $("#" + ul_id).html(data);
        $("#" + cmd_id).attr("nacteno", "1");
    })
        .fail(function (response) {
            if (!response.responseText || 0 === response.responseText.length) {
                alert("Načtený obsah je prázdný!");
                //alert("Z důvodu neaktivity pravděpodobně vypršel čas přihlášení v systému.Musíte se znovu přihlásit.");
                //_redirect("/Login/UserLogin");
            }

        });

}

function _copy_element_to_clipboard(element_id)
{           
    var el = document.getElementById(element_id);

    var div0 = document.createElement("div");   
    $(div0).html($(el).html());

    document.body.appendChild(div0);
    $(div0).find(".cm").each(function (e) {
        $(this).css("display", "none"); //vyhodit z html hamburger menu
    });

    
    var text = div0.innerText || div0.textContent; // Získání textového obsahu
    

    var textarea = document.createElement("textarea");
    
    textarea.value = text;
    document.body.appendChild(textarea);
    textarea.select();
    document.execCommand("copy");

    document.body.removeChild(textarea);
    document.body.removeChild(div0);

    _notify_message("Zkopírováno.", "info");

    
}

function _html2mail(element_id, mailsubject,kostra) {
    var htmlbody = document.getElementById(element_id).outerHTML;
    
    $.post(_ep("/Common/SaveMessage2Temp"), { s: htmlbody }, function (data) {
        var url = "/Mail/SendMail?record_entity=p85&mailsubject=" + mailsubject + "&record_pid=" + data;
        if (kostra != undefined) {
            url = url + "&kostra=" + kostra;
        }
        _window_open(url);

    });
}

function _adjust_divheight_small_display(divID){

    if (_device.isMobile){
        return; //pro mobilní zařízení neaplikovat
    }
    var offset = $("#" + divID).offset();     
    
    if ($("#" + divID).height() + offset.top > window.innerHeight)
    {
        
        $("#" + divID).css("overflow", "auto");
        $("#" + divID).height(window.innerHeight - offset.top-20);
        
            
    }
}

function _p31_new_from_everywhere()
{
    if (window.location.href.indexOf("TheGrid/") > 0 || window.location.href.indexOf("/RecPage") > 0) {
        var pid = $("#tg_selected_pid").val();

        var prefix = _thegrid.entity.substr(0, 3);
        if (pid != "0" && pid != "" && (prefix == "le5" || prefix == "p56" || prefix == "p28" || prefix=="p41")) {
            _p31_entry_from_grid();
            return;
        }


    }
    if (window.location.href.indexOf("TreePage") > 0)
    {
        var p41id = $("#pid").val();
        var url = "/p31/Record?pid=0&newrec_prefix=p41&newrec_pid=" + p41id;
        
        _window_open(url, 3);
        return;

    }


    _window_open("/p31/Record?pid=0", 3);
}





function _treeview_toggle(ctl,param_key) {
    
    var ul1 = $(ctl).siblings("ul:first");

    if ($(ul1)) {
        $(ul1).toggle();
    }

    var kontejner_sipka = $(ctl).parent().first();  //default je nadřízené LI
    if ($(kontejner_sipka).children(".mytreeview-toggle-sipka").length == 0)
    {
        kontejner_sipka = $(ctl);   //zkusit element .mytreeview-toggle-kontejner
    }
    
    
    var param_value = null;

    if ($(kontejner_sipka).children(".mytreeview-toggle-sipka").length > 0)
    {
        var span_sipka = $(kontejner_sipka).children(".mytreeview-toggle-sipka").first();
        if ($(ul1).css("display") == "none") {
            $(span_sipka).html("arrow_right");
            param_value = "collapsed";
        } else {
            $(span_sipka).html("arrow_drop_down");
            param_value = "expanded";
        }
    }
    
    if (typeof param_key !== "undefined")
    {
        $.post(_ep("/Common/SetUserParam"), { key: param_key, value: param_value }, function (data) {
            //nic

        });
    }
    
    

}