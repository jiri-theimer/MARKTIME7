function mycombo_init(c) {
    
    var _controlid = c.controlid;
    var _combo_currentFocus = -1;
   
    var _tabid = "tab1" + _controlid;
    var _tabbodyid = _tabid + "_tbody";
    var _cmdcombo = $("#cmdCombo" + _controlid);
    var _searchbox = $("#cmdCombo" + _controlid);    
    var _lookup_value = $(_searchbox).val();
    var _dropdownid = "divDropdownContainer" + _controlid;
    
    var _event_after_change = c.on_after_change;
    var _filterflag = c.filterflag;
    var _searchbox_serverfiltering_timeout;    
    var _masterprefix = c.masterprefix;
    var _masterpid = c.masterpid;
        
    
    if (c.defvalue !== "") {    //výchozí vyplněná hodnota comba
        if (c.deftext !== "") {
            $(_cmdcombo).val(c.deftext);
            $(_cmdcombo).css("color", "navy");
            
        }

        $("[data-id=value_" + _controlid + "]").val(c.defvalue);
        $("[data-id=text_" + _controlid + "]").val(c.deftext);
        handle_update_state();
    }

    var myDropdown = document.getElementById(_dropdownid);

    if ($(_searchbox).width() > 0)
    {
        if (!_device.isMobile) {
            $(_searchbox).css("width", ($(_searchbox).width() - 11) + "px");
        }
        

        if ($(_searchbox).width() < 100) {
            $("#cmdClear" + _controlid).css("display", "none");
        }
        
    }
    

   
    if (_device.isMobile) {
        $("#cmdPop" + _controlid).css("display", "none");
        $("#cmdClear" + _controlid).css("right", "0px");
        $("#cmdClear" + _controlid).css("border-radius", "3px");
    }

    
    

    $(myDropdown).on("click", function () {
        
        $(_searchbox).select();
    })
    
    myDropdown.addEventListener("show.bs.dropdown", function () {
        
        //if (c.viewflag === "2") {//bez zobrazení search
            //$("#cmdCombo" + _controlid).css("display", "block");
            
            
        //}
        if ($("#" + _dropdownid).prop("filled") === true && _filterflag === "0") {
            setTimeout(function () {
                //focus až po 300ms
                document.getElementById("cmdCombo" + _controlid).focus();
                //document.getElementById("cmdCombo" + _controlid).select();

                //$("#cmdCombo" + _controlid).focus();
                //$("#cmdCombo" + _controlid).select();
                
                return;    //data už byla dříve načtena a filtruje se na straně klienta, protože _filterflag=0
            }, 200);
            
        }
        $("#divData" + c.controlid).html("Loading...");

        var curpid = "";
        if (_filterflag == "1") {
            curpid = $("[data-id=value_" + _controlid + "]").val();
        }
        
        $.post(c.posturl, { entity: c.entity, o15flag: "",pids:curpid, tableid: _tabid, myqueryinline: c.myqueryinline, filterflag: _filterflag, searchstring: $(_searchbox).val(), masterprefix: _masterprefix,masterpid: _masterpid }, function (data) {
            $("#divData"+c.controlid).html(data);
            
            $("#"+_dropdownid).prop("filled", true);
            
            $("#" + _tabid + " .txz").on("click", function () {
                
                record_was_selected(this);
                
                _toolbar_warn2save_changes();
            });

            
            setTimeout(function () {
                //focus až po 300ms
                document.getElementById("cmdCombo" + _controlid).focus();
                

            }, 200);

            
            var rows_count = $("#" + _tabbodyid).find("tr").length;            
            if (rows_count > 0)
            {
                _combo_currentFocus = 0;

                var current_value = $("#" + _controlid).val();
                
                if (current_value != "0" && current_value != "") {  //skočit na aktuálně vybranou řádku
                    for (i = 1; i < rows_count; i++)
                    {
                        var row = $("#" + _tabbodyid).find("tr").eq(i);
                        
                        var val = $(row).attr("data-v");
                        if (val == current_value) {
                            _combo_currentFocus = i;
                        }
                    }

                    
                }

                
                update_selected_row();
            }
            
        });
    })
   

    

    
    $(_searchbox).on("change", function (e) {
        var val = $(this).val();
        if (val == "")
        {
            //vyčistit hodnotu
            handle_clear_value();
        }
        
    });
    

    
    $(_searchbox).on("input", function (e) {
        
        var isvisible = $("#divData" + c.controlid).is(":visible");
        if (!isvisible)
        {
            $(myDropdown).dropdown("toggle");
        }
        
        

        if (_filterflag !== "0" && _filterflag !== "") {  //_filterflag>=1 nebo string: filtruje se na straně serveru
            if (typeof _searchbox_serverfiltering_timeout !== "undefined") {
                clearTimeout(_searchbox_serverfiltering_timeout);
            }
            _searchbox_serverfiltering_timeout = setTimeout(function () {
                //čeká se 500ms až uživatel napíše všechny znaky
                
                handle_server_filtering(e);
                
                
            }, 500);
            
        }


    })

    $(_cmdcombo).on("keydown", function (e) {
        
        handle_keydown(e);
    });

   
    
    

    function handle_keydown(e) {

        if ((e.keyCode === 8 || e.keyCode == 46) ) {  //DELETE nebo BACKSPACE
                      
            var startPos = document.getElementById("cmdCombo" + _controlid).selectionStart;
            var endPos = document.getElementById("cmdCombo" + _controlid).selectionEnd;
            if (startPos == 0 && endPos == document.getElementById("cmdCombo" + _controlid).value.length)
            {

                e.stopPropagation();
                e.preventDefault();

                $(_searchbox).val("");
                $(_searchbox).text("");

                $("[data-id=value_" + _controlid + "]").val("0");
                $("[data-id=text_" + _controlid + "]").val("");
                handle_update_state();

                handle_server_filtering();

                var isvisible = $("#divData" + c.controlid).is(":visible");
                if (!isvisible) {
                    $(myDropdown).dropdown("toggle");   //zobrazit dropdown nabídku
                }
                
            }

            
            return;
        }
        
        if (e.keyCode === 27 && e.target.id === "cmdCombo"+_controlid) {  //ESC
            $("#divDropdown"+_controlid).dropdown("hide");
        }
        var rows_count = $("#"+_tabbodyid).find("tr").length;
        

        if (e.keyCode === 13 && e.target.id === "cmdCombo" + _controlid) {//ENTER
            
            var row = $("#" + _tabbodyid).find(".combo_selrow");
            if (row.length > 0) {

                record_was_selected(row[0]);
                
                //$(myDropdown).dropdown("toggle");
                //$(myDropdown).dropdown("toggle");
                //$(myDropdown).dropdown("hide");
                
                
            } else {
                //vyrolovat seznam, protože je hodnota je prázdná
                $(myDropdown).dropdown("toggle");   //zobrazit dropdown nabídku
            }


        }

        if (e.keyCode === 9 && e.target.id === "cmdCombo" + _controlid) {//TAB
            $(myDropdown).dropdown("hide");

        }
        
        if (e.keyCode === 40 || e.keyCode === 38)   //šipka dolu nebo nahoru - test, zda rozbalit dropdown nabídku
        {
            var isvisible = $("#divData" + c.controlid).is(":visible");
            if (!isvisible) {
                $(myDropdown).dropdown("toggle");   //zobrazit dropdown nabídku
            }
        }

        
        
        if (e.keyCode == 8 && $(_searchbox).val() == "")
        {         
            
            var isvisible = $("#divData" + c.controlid).is(":visible");
            if (!isvisible)
            {                
                $(myDropdown).dropdown("toggle");   //backspace+prázdný searchbox: zobrazit dropdown nabídku
            }                        
        }
        
        var destrowindex;
        if (e.keyCode === 40) {  //down
            
            if (rows_count - 1 <= _combo_currentFocus) {
                _combo_currentFocus = 0

            } else {
                _combo_currentFocus++;

            }
            destrowindex = get_first_visible_rowindex(_combo_currentFocus, "down");
            if (destrowindex === -1) destrowindex = get_first_visible_rowindex(0, "down");
            _combo_currentFocus = destrowindex;
            if (destrowindex === -1) return;

            update_selected_row();

        }
        if (e.keyCode === 38) {  //up            
            _combo_currentFocus--;
            destrowindex = get_first_visible_rowindex(_combo_currentFocus, "up");
            if (destrowindex === -1) destrowindex = get_first_visible_rowindex(0, "down");
            _combo_currentFocus = destrowindex;
            if (destrowindex === -1) _combo_currentFocus = 0;


            update_selected_row();
        }

        
    }

    $(_searchbox).on("keyup", function (e) {
        
       
        if (_filterflag === "1") return;
        //zde se filtruje podle lokálních dat:

        if (e.keyCode===27) {            
            return;
        }    
        if (e.keyCode == 38 || e.keyCode == 40) {            
            return;
        }  
        
        
               
        var value = $(this).val().toLowerCase();
        var x = 0;
        var rows_count = $("#" + _tabbodyid).find("tr").length;
        
        if ($(_searchbox).val() !== _lookup_value) {
            
            $("#" + _tabbodyid+" tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
                x++;
                if (x === rows_count) {
                    recovery_selected();


                }

            });
        }
        
        _lookup_value = $(_searchbox).val();
    });

    $("#cmdClear" + _controlid).on("click", function () {
        
       
        $(_cmdcombo).css("font-weight", "normal");
        $(_cmdcombo).css("color", "gray");
        $(_cmdcombo).val("");

        handle_clear_value();
        
    })

    $("#cmdPop" + _controlid).on("click", function (e) {
        
        var div = $("#"+_dropdownid).find(".dropdown-menu").first();
        var w = $(div).width() - 30;
        
        //var offset = $(div).offset();
        var offset_btn = $(this).offset();

        //alert("left: " + offset.left + ", w: " + w + ", avaiil: " + $(window).width() + ", btn-left: " + offset_btn.left);

        if ((w + offset_btn.left) < $(window).width())
        {
            $(div).css("left", (w * -1) + "px");
        }
       
        
    });

    

    function handle_clear_value()
    {
        $("[data-id=value_" + _controlid + "]").val("0");
        $("[data-id=text_" + _controlid + "]").val("");
        handle_update_state();

        if (_event_after_change !== "") {
            if (_event_after_change.indexOf("#pid#") === -1) {
                eval(_event_after_change + "('0')");
            } else {

                eval(_event_after_change.replace("#pid#", "0"));
            }


        }
    }

    function get_first_visible_rowindex(fromindex, direction) {        
        var rows_count = $("#" + _tabbodyid).find("tr").length;
        var row;
        if (direction === "down") {
            for (i = fromindex; i < rows_count; i++) {
                row = $("#" + _tabbodyid).find("tr").eq(i);
                if ($(row).attr("data-skip")) {                    
                    continue;   //přeskočit záhlaví bloku řádek - mySearch1
                }
                if ($(row).css("display") !== "none") {
                    return i;
                }
            }
        }
        if (direction === "up") {
            for (i = fromindex; i >= 0; i--) {
                row = $("#" + _tabbodyid).find("tr").eq(i);
                if ($(row).attr("data-skip")) {
                    continue;   //přeskočit záhlaví bloku řádek - mySearch1
                }
                if ($(row).css("display") !== "none") {
                    return i;
                }
            }
        }

        return -1;
    }

    
    function update_selected_row() {

        $("#" + _tabbodyid).find("tr").removeClass("combo_selrow");
        if (_combo_currentFocus > -1) {
            var row = $("#" + _tabbodyid).find("tr").eq(_combo_currentFocus);

            $(row).addClass("combo_selrow");


            var element = row[0];
            element.scrollIntoView({ block: "end", inline: "nearest" });

            
        }

    }

    function handle_update_state() {
        var strValue = $("[data-id=value_" + _controlid + "]").val();
        
        if ($(_searchbox).width() > 100 || $(_searchbox).width() == -20) {
            
            if (strValue !== "" && strValue !== "0") {
                $("#cmdClear" + _controlid).css("display", "block");
            } else {
                $("#cmdClear" + _controlid).css("display", "none");
            }
        } else {
            $("#cmdClear" + _controlid).css("display", "none");
        }
        
        
    }


    function recovery_selected() {
        //handle_update_state();
        _combo_currentFocus = get_first_visible_rowindex(0, "down");
        
        update_selected_row();
    }

    function record_was_selected(row) {
        
        var v = $(row).attr("data-v");
        var t = $(row).attr("data-t");
        if (t === undefined) {
            t = $(row).find("td:first").text();
        }
        
        
        $(_cmdcombo).css("color", "red");        
        $(_cmdcombo).val(t);

        $("[data-id=value_" + _controlid + "]").val(v);
        $("[data-id=text_" + _controlid + "]").val(t);
        
          
        handle_update_state();

        if ($(row).attr("data-prefix"))
        {
            //prvek mySearch1 pro fulltext hledání - v TR je atribut data-prefix            
            eval(_event_after_change + "('" + v + "','" + $(row).attr("data-prefix") + "')");
            return;
        }

        if (_event_after_change !== "") {
            if (_event_after_change.indexOf("#pid#") === -1) {
                eval(_event_after_change + "('" + v + "')");
            } else {
                
                eval(_event_after_change.replace("#pid#", v));
            }                        
        }

        
        var isvisible = $(_dropdownid).is(":visible");
        if (!isvisible) {
            $(myDropdown).dropdown("toggle");
        }
        
        
    }

    

    function handle_server_filtering(e) {


        var s = $(_searchbox).val();
        
        $("#divData" + c.controlid).html("Loading...");
        
        var explicit_columns=$("#explicit_columns"+c.controlid).val();
        
        $.post(c.posturl, { entity: c.entity, o15flag: "", tableid: _tabid, myqueryinline: c.myqueryinline, filterflag: _filterflag, searchstring: s, masterprefix: _masterprefix, masterpid: _masterpid,explicit_columns: explicit_columns }, function (data) {
            $("#divData" + c.controlid).html(data);            
            
            $("[data-id=value_" + _controlid + "]").val("0");
            $("[data-id=text_" + _controlid + "]").val("");
            handle_update_state();

           
            
            $("#" + _tabid + " .txz").on("click", function () {
                
                record_was_selected(this);

                _toolbar_warn2save_changes();
            });

           

            
            var columnsinfo=$("#columnsinfo"+_tabid).val();
            $("#explicit_columns"+c.controlid).val(columnsinfo);

            var rows_count = $("#" + _tabbodyid).find("tr").length;
            if (rows_count > 0)
            {
                //destrowindex = get_first_visible_rowindex(_combo_currentFocus, "down");
                _combo_currentFocus = get_first_visible_rowindex(0,"down");
                if (_combo_currentFocus == -1) {
                    _combo_currentFocus = 0;
                }
                update_selected_row();
            }
            
        });
    }

    

}
