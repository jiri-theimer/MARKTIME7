
function _window_open_from_grid_selected(url, pids_param_name, winflag) {    
    var pids = $("#tg_selected_pids").val();
    
    if (pids === "") {
        _notify_message(_tg_musite_vybrat_zaznam);
        return;
    }
    var spojovak_url = "&";

    if (url.indexOf("?") == -1) {
        spojovak_url = "?";
    }
    
    if (pids.length > 999) {
        $.post(_ep("/Common/Save2Temp"), { fieldname: "p85Message", fieldvalue: pids }, function (data) {
            url = url + spojovak_url + "guid_pids=" + data.p85GUID;
            if (winflag == "_blank") {
                window.open(url, "_blank");
            } else {
                _window_open(url, winflag);
            }

        });
    }
    else {
        url = url + spojovak_url + pids_param_name + "=" + pids;
        if (winflag == "_blank") {
            window.open(url, "_blank");
        } else {
            _window_open(url, winflag);
        }

    }

}

function _local_href_from_grid_selected(url, pids_param_name,isblank) {
    var pids = $("#tg_selected_pids").val();
    if (pids === "") {
        _notify_message(_tg_musite_vybrat_zaznam);
        return;
    }

    if (pids.length > 999) {
        $.post(_ep("/Common/Save2Temp"), { fieldname: "p85Message", fieldvalue: pids }, function (data) {
            url = url + "&guid_pids=" + data.p85GUID;

            if (isblank) {
                _window_open(url);
            } else {
                location.href = url;
            }
            

        });
    }
    else {
        url = url + "&" + pids_param_name + "=" + pids;
        if (isblank) {
            _window_open(url);
        } else {
            location.href = url;
        }
    }


}

function _p31text_edi(txt, p31id) {
    var s = $(txt).val();
    
    $.post(_ep("/p31misc/UpdateText"), { p31id: p31id, s: s }, function (data) {
        if (data.flag === 0) {
            _notify_message(data.flag + ", message: " + data.message);
        } else {
            _notify_message("Úpravy uloženy.", "info");
        }


    });
}

function _p31_create(p34id, p31date) {
    var url = "/p31/Record?pid=0";
    if (p34id !== undefined && p34id !=="0") {
        url = url + "&p34id=" + p34id;
    }
    if (p31date !== undefined) {
        url = url + "&p31date=" + p31date;
    }
    _window_open(url);
}

function _workflow_batch(prefix)
{    
    _window_open_from_grid_selected("/workflow_batch/Index?prefix=" + prefix, "pids", 2);
    
}

function _p31_move_to_project() {
    
    _window_open_from_grid_selected("/p31move_to_project/Index", "pids", 2);
    
}

function _p31_batch_clone() {
   
    _window_open_from_grid_selected("/p31clonebatch/Index", "pids", 2);
    
}

function _p31_batch_recalc() {
   
    _window_open_from_grid_selected("/p31recalc/Index", "pids", 2);
    
}

function tg_approve_contextmenu(prefix,pid)
{
    var millis=new Date().getTime();
    var url = "/p31approveinput/Index?prefix=" + prefix+"&millis="+millis;
    tg_approve_open_finalurl(url,prefix,pid+"");
}

function tg_approve() {
    var prefix = _tg_entity.substr(0, 3);    
    var pids = $("#tg_selected_pids").val();
    if (pids === "") {
        _notify_message(_tg_musite_vybrat_zaznam);

        return;
    }

    var b=false;

    
    var url = "/p31approveinput/Index?prefix=" + prefix;

    if (document.getElementById("_p31StateQueryHiddenValue") && $("#_p31StateQueryHiddenValue").val() !="0")
    {
        url=url+"&p31statequery="+$("#_p31StateQueryHiddenValue").val();    //filtr stav úkonů posílat pouze u akce v rámci vybraných záznamů
    }
   
    tg_approve_open_finalurl(url,prefix,pids);

    
}

function tg_approve_open_finalurl(url,prefix,pids)
{    
    if (typeof _period_get_as_string==="function")
    {        
        url=url+"&period="+_period_get_as_string(); //poslat i filtr období
    }
        

    var arr = pids.split(",");
    
    if (arr.length > 90) {
        _notify_message("Loading...", "info");
        
        $.post(_ep("/Common/SetPids2Temp"), { arr: arr }, function (data) {
            
            var millis=new Date().getTime();
            url=url+"&millis="+millis;
            _window_open(url + "&guid_pids=" + data, 2);
        });
    } 
    else 
    {
        
        var millis=new Date().getTime();
        url=url+"&millis="+millis;
        _window_open(url + "&pids=" + pids, 2);

    }
}


function tg_append2invoice() {
   
    _window_open_from_grid_selected("/p31invoice/Append2Invoice", "pids",2);

    
}



function _change_grid(j72id) {
    var strKey = "flatview";
    var url = location.pathname.toLowerCase();

    if (location.href.indexOf("masterview") > 0) {
        strKey = "masterview";
    }
    if (location.href.indexOf("slaveview") > 0) {
        strKey = "slaveview";
    }
    $.post(_ep("/Common/SetUserParam"), { key: strKey+"-j72id-" + _pageprefix, value: j72id }, function (data) {
        _redirect(location.href);

    });
}


function _p31_entry_from_grid() {       //vykázat úkon z gridu projektu nebo klienta projektu
    var pid = $("#tg_selected_pid").val();
    var prefix = _thegrid.entity.substr(0, 3);

    //alert("pid: " + pid + ", prefix: " + prefix);
    //if (prefix == "p31" && _thegrid.master_entity=="")
    //{
    //    _window_open("/p31/Record?pid=0", 3);
    //    return;
    //}
    
    if (prefix == "p31") {
        if (_thegrid.master_entity != "") {
            prefix = _thegrid.master_entity.substr(0, 3);
            pid = _thegrid.master_pid;
        }
        else
        {
            if (pid == "" || pid == "0")
            {
                _window_open("/p31/Record?pid=0", 3);
                return;
            }
            
            
        }
        
    }

    
    
    if (pid == "" || pid=="0")
    {
        _notify_message("Musíte vybrat záznam v Přehledu.", "error");
        return;
    }
    var url = "/p31/Record?pid=0&newrec_prefix=" + prefix + "&newrec_pid=" + pid;

    if (prefix == "p91")
    {
        url="/p31/Record?pid=0&p91id=" + pid;
    }
    
    _window_open(url,3);
}

function _recpage_from_grid(prefix) {       //přejít na stránku záznamu vybraného z gridu
    var pid = $("#tg_selected_pid").val();
    var prefix = _thegrid.entity.substr(0, 3);

    if (pid == "" || pid == "0") {
        _notify_message("Musíte vybrat záznam v Přehledu.", "error");
        return;
    }
    location.href = "/Record/RecPage?prefix=" + prefix + "&pid=" + pid;
    
    
}


function _update_flattab_badge(badgeid, val) {    
    //aktualizace hodnoty záložky ve Flatview gridu
    $("#" + badgeid).text(val);
    if (val === "" || val === "0") {
        $("#" + badgeid).css("visibility", "hidden");
    } else {
        $("#" + badgeid).css("visibility", "visible");
    }
}

function _get_url_alias(url)
{
    
    if (url.indexOf("ReportNoContextFramework")>0)
    {
        return "Pevné tiskové sestavy"
    }
    if (url.indexOf("?prefix=p90") > 0) return "Zálohy"
    if (url.indexOf("?prefix=p58") > 0) return "Opakované úkoly"
    if (url.indexOf("?prefix=p51") > 0) return "Ceníky sazeb"
    if (url.indexOf("?prefix=p40") > 0) return "Opakované odměny"
    if (url.indexOf("?prefix=o22") > 0) return "Termíny/Lhůty/Události"
    if (url.indexOf("?prefix=p15") > 0) return "Pojmenované lokality"
    if (url.indexOf("?prefix=o43") > 0) return "Inbox"
    if (url.indexOf("?prefix=o51") > 0) return "Položky štítků"
    if (url.indexOf("?prefix=o53") > 0) return "Štítky"
    if (url.indexOf("workflow_designer") > 0) return "Workflow návrhář"
    if (url.indexOf("AdminOneWebpage") > 0) return "Návrhář stránek"
    if (url.indexOf("captml") > 0) return "Kapacitní plánování"


    return null;
}


function _update_user_ping() {

    var devicetype = "Desktop";
    if (screen.availHeight > screen.availWidth || screen.width < 800 || screen.height < 600) {   //mobilní zařízení výšku vyšší než šířku
        devicetype = "Phone";
    }

    var log = {
        j92BrowserUserAgent: navigator.userAgent,
        j92BrowserAvailWidth: screen.availWidth,
        j92BrowserAvailHeight: screen.availHeight,
        j92BrowserInnerWidth: window.innerWidth,
        j92BrowserInnerHeight: window.innerHeight,
        j92BrowserDeviceType: devicetype,
        j92RequestURL: location.href.replace(location.host, "").replace(location.protocol, "").replace("///", "")
    }

    _load_ajax_async("/Home/UpdateCurrentUserPing", { c: log });


}


function tg_init_resizer(j72id,j02id)
    {
        $("#tr_header_headline").find("th").not(".th0")
            .css({
                position: "relative"
            })
            .prepend("<div class='resizer'></div>")
            .resizable({
                resizeHeight: false,
                handleSelector: "",
                onDragStart: function (e, $el, opt) {
                    // only drag resizer
                    if (!$(e.target).hasClass("resizer"))
                        return false;
                    
                    return true;
                },
                onDragEnd: function (e, $el, opt) {                    
                    var id = $el.attr("id");
                    if (typeof id === "undefined")
                    {
                        return;
                    }
                    _tg_last_resizer_when = Date.now();
                    
                    var w=$el.width();
                    
                    //jedna tabulka: šířku sloupce určuje buňka hlavičky, tělo a součty ji přebírají samy

                    w = parseInt($el.width());

                    $.post(_ep("/Common/SaveResizedColumn"), { j72id: j72id, j02id: j02id, colid: id,colwidth: w }, function (data) {
        
                        //nová šířka sloupce uložena
                        location.replace(location.href);
                        
                        

                    });


                }
            });
}

function _livechat(zapnout) {
    
    $.post(_ep("/Home/Livechat"), { zapnout: zapnout }, function (data) {
        location.replace(location.href);


    });
}

function _stopky() {
  
    _window_open("/p68/Index",3);
}

// stav stopek v horní liště: barva ikony + běžící čas (H:MM). Volá se při načtení stránky, když stopky běží,
// a z okna stopek po každé změně (window.parent._stopky_refresh).
var _stopky_timer = null;
function _stopky_refresh() {
    if (!document.getElementById("stopky_icon")) return;
    $.get(_ep("/p68/Running"), function (r) {
        if (_stopky_timer) { clearInterval(_stopky_timer); _stopky_timer = null; }
        var $i = $("#stopky_icon"), $t = $("#stopky_time");
        $i.css("color", r.flag === 1 ? "lime" : (r.flag === 2 ? "orange" : "white"));
        $i.attr("title", r.flag === 1 ? ("Stopky běží" + (r.project ? ": " + r.project : "")) : (r.flag === 2 ? "Stopky obsahují záznamy" : "Stopky"));
        if (r.flag !== 1) { $t.text(""); return; }
        var t0 = Date.now(), base = r.elapsed || 0;
        var show = function () {
            var s = base + Math.floor((Date.now() - t0) / 1000);
            var h = Math.floor(s / 3600), m = Math.floor(s / 60) % 60;
            $t.text(" " + h + ":" + (m < 10 ? "0" : "") + m);
        };
        show();
        _stopky_timer = setInterval(show, 15000);
    });
}
// ---------------------------------------------------------------- interní chat (ChatController, chat.js)
// okno chatu; volitelně rovnou konkrétní konverzace
function _chat(u01id) {
    _window_open("/Chat/Index" + (u01id ? "?u01id=" + u01id : ""), 3, "Chat");
}
// přímá konverzace s uživatelem
function _chat_user(j02id) {
    _window_open("/Chat/Index?j02id=" + j02id, 3, "Chat");
}
// konverzace k záznamu: prefix p41, p56, p28, p91 nebo o23
function _chat_record(prefix, pid) {
    _window_open("/Chat/Index?entity=" + prefix + "&pid=" + pid, 3, "Chat");
}
// počet nepřečtených zpráv u ikony v horní liště (volá i okno chatu)
function _chat_badge(n) {
    var c = $("#cmdChatCount");
    if (!c.length) return;
    if (n > 0) { c.text(n > 99 ? "99+" : n).show(); } else { c.hide(); }
}
function _chat_badge_refresh() {
    $.ajax({ url: _ep("/Chat/UnreadCount"), type: "GET", dataType: "json", cache: false }).done(function (data) { _chat_badge(data.count); });
}
// je okno chatu právě vidět? (modální okno se při zavření jen skrývá)
function _chat_window_visible() {
    try {
        var fra = document.getElementById("fraModalContent");
        return !!(fra && fra.getClientRects().length > 0 && fra.contentWindow && fra.contentWindow.ChatApp);
    } catch (e) {
        return false;
    }
}
// zvuk nové zprávy: krátké dvoutónové pípnutí přes Web Audio; vypíná se v okně chatu (localStorage chat_sound)
var _chat_audio = null;
function _chat_beep() {
    try {
        if (window.localStorage.getItem("chat_sound") === "0") return;
        var AC = window.AudioContext || window.webkitAudioContext;
        if (!AC) return;
        if (!_chat_audio) _chat_audio = new AC();
        if (_chat_audio.state === "suspended") _chat_audio.resume();   // prohlížeč povolí zvuk až po prvním kliknutí uživatele
        var t = _chat_audio.currentTime;
        [[880, 0], [1175, 0.13]].forEach(function (n) {
            var o = _chat_audio.createOscillator(), g = _chat_audio.createGain();
            o.type = "sine"; o.frequency.value = n[0];
            g.gain.setValueAtTime(0.0001, t + n[1]);
            g.gain.exponentialRampToValueAtTime(0.16, t + n[1] + 0.02);
            g.gain.exponentialRampToValueAtTime(0.0001, t + n[1] + 0.22);
            o.connect(g); g.connect(_chat_audio.destination);
            o.start(t + n[1]); o.stop(t + n[1] + 0.24);
        });
    } catch (e) { }
}
// horní lišta: načíst počet nepřečtených a přes SignalR poslouchat nové zprávy
var _chat_timer = null;
function _chat_init(j02id_me) {
    if (!document.getElementById("cmdChat")) return;
    _chat_badge_refresh();
    // prohlížeč dovolí zvuk až po zásahu uživatele: při prvním kliknutí nebo stisku klávesy si zvukový výstup odemknout
    $(document).one("pointerdown keydown", function () {
        try {
            var AC = window.AudioContext || window.webkitAudioContext;
            if (!AC) return;
            if (!_chat_audio) _chat_audio = new AC();
            if (_chat_audio.state === "suspended") _chat_audio.resume();
        } catch (e) { }
    });
    $.ajax({ url: _ep("/lib/signalr/signalr.min.js"), dataType: "script", cache: true }).done(function () {
        if (typeof signalR === "undefined") return;
        var hub = new signalR.HubConnectionBuilder().withUrl(_ep("/chathub")).withAutomaticReconnect([0, 5000, 15000, 30000, 60000]).build();
        hub.on("chat", function (e) {
            if (e.type === "ping") return;     // zkušební událost diagnostiky patří oknu chatu
            if (_chat_timer) window.clearTimeout(_chat_timer);
            _chat_timer = window.setTimeout(_chat_badge_refresh, 600);
            if (e.type === "message" && e.message && e.actorid !== j02id_me && !e.muted && !_chat_window_visible()) {
                var s = $("<div>").text((e.message.sender || "") + ": " + (e.message.text || "📎")).html();
                _notify_message("<a href='javascript:_chat(" + e.u01id + ")' style='color:inherit;'>" + s + "</a>", "info");
                _chat_beep();
            }
        });
        hub.onreconnected(_chat_badge_refresh);
        hub.start().catch(function () { });   // bez spojení zůstává počet z načtení stránky
    });
}

function _bells() {

    _window_open("/Home/Bells", 3);
}
function _news() {
    $.post(_ep("/Home/UpdateNews_Timestamp"), function (data) {
        
        window.open("https://www.marktime.cz/novinky", "_blank");

    });

    
    
}

function _wrkdialog(prefix,pid)
{
    _window_open("/workflow_dialog/Index?record_prefix="+prefix+"&record_pid=" + pid, 3);
}

function _newrec_from_grid(prefix, rez) {
    
    if (prefix == "p56") {
        _window_open("/p56/NewBianco", 3);
        return;
    }
    if (prefix == "p55") {
        _window_open("/p56/TodoList", 3);
        return;
    }

    _edit(prefix, 0, null, rez);
}


function _handle_mywrk_mouseover(td)
{
    if (document.getElementById("cmdWrkButton"))
    {
        $("#cmdWrkButton").css("visibility", "visible");
    }
    if (document.getElementById("cmdCopyRecPageUrl")) {
        $("#cmdCopyRecPageUrl").css("visibility", "visible");
    }    
    if (document.getElementById("cmdWrkReport")) {
        $("#cmdWrkReport").css("visibility", "visible");
    }
    if (document.getElementById("cmdWrkEmail")) {
        $("#cmdWrkEmail").css("visibility", "visible");
    }
    if (document.getElementById("cmdWrkTotals")) {
        $("#cmdWrkTotals").css("visibility", "visible");
    }
  
    if (document.getElementById("cmdWrkQuickStat")) {
        $("#cmdWrkQuickStat").css("visibility", "visible");
    }
    if (document.getElementById("cmdWrkRecPage")) {
        var prefix = $("#cmdWrkRecPage").attr("data-prefix");
        var pid = $("#cmdWrkRecPage").attr("data-pid");
        
        $("#cmdWrkRecPage").attr("href", "/Record/RecPage?prefix=" + prefix + "&pid=" + pid);
        $("#cmdWrkRecPage").css("visibility", "visible");
        
        
    }

    
}
function _handle_mywrk_mouseout(td) {
    if (document.getElementById("cmdWrkButton")) {
        $("#cmdWrkButton").css("visibility", "hidden");
    }
    if (document.getElementById("cmdCopyRecPageUrl")) {
        $("#cmdCopyRecPageUrl").css("visibility", "hidden");
    }
    if (document.getElementById("cmdWrkReport")) {
        $("#cmdWrkReport").css("visibility", "hidden");
    }
    if (document.getElementById("cmdWrkEmail")) {
        $("#cmdWrkEmail").css("visibility", "hidden");
    }
    if (document.getElementById("cmdWrkRecPage")) {
        $("#cmdWrkRecPage").css("visibility", "hidden");
    }
    if (document.getElementById("cmdWrkTotals")) {
        $("#cmdWrkTotals").css("visibility", "hidden");
    }
    
    if (document.getElementById("cmdWrkQuickStat")) {
        $("#cmdWrkQuickStat").css("visibility", "hidden");
    }
}

function _copy_recpage_to_clipboard(prefix, pid)
{
    var s = window.location.origin;

    s = s + "/Record/RecPage?prefix=" + prefix + "&pid=" + pid;


    navigator.clipboard.writeText(s).then(() => {
        _notify_message("Do schránky zkopírováno: " + s,"info");

    });
}

function _p55_record_rename(p55id) {
    var s = prompt("Zadejte nový název.");
    if (s.length > 0) {
        $.post("/p56/RenameToDoList", { newname: s, p55id: p55id }, function (data) {
            location.replace(location.href);

        });

    }
}

function _handle_onetab_mouseover_switch_layout(tab) {
    if (!document.getElementById("cmdQuickPageLayoutSwitch")) {
        return;
    }
    var offset = $(tab).offset();
    var btn = document.getElementById("cmdQuickPageLayoutSwitch");
        
    $(btn).css("top", offset.top + $(tab).height());
    $(btn).css("left", offset.left);
    $(btn).css("display", "block");
}
function _handle_onetab_mouseout_switch_layout(cmd) {
    if (!document.getElementById("cmdQuickPageLayoutSwitch")) {
        return;
    }
    
    var btn = document.getElementById("cmdQuickPageLayoutSwitch");
    if (btn.matches(":hover")) {
        return; //myš je nad tlačítkem
    }

    $(btn).css("display", "none");
}
function _handle_quick_switch_page_layout(prefix,isleftpanel) {

    $.post(_ep("/Home/SaveCurrentUserUI_LeftPanel"), { leftpanel: isleftpanel, prefix: prefix }, function (data) {

        if (isleftpanel == true)
        {
            location.replace("/Record/RecPage?prefix=" + prefix);
        }
        else
        {
            location.replace("/TheGrid/MasterView?prefix=" + prefix);
        }
        


    });


    
}

function _search1_on_change(pid, prefix) {

    if (pid == undefined || prefix == undefined || prefix=="" || pid=="0") {
       
        return;
    }
    location.href = "/Record/RecPage?prefix=" + prefix + "&pid=" + pid;
    
}