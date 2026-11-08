// Taghelper mycombo (a mySearch na horní liště): výběr záznamu z nabídky.
// Zachované rozhraní: mycombo_init(c), ID prvků cmdCombo*, cmdClear*, cmdPop*, divData*, data-id value_* / text_*, explicit_columns*,
// callback c.on_after_change (s #pid# nebo jako název funkce), u mySearch řádky s data-prefix.
// filterflag "0" = data se načtou celá a filtruje se v prohlížeči; jinak se filtruje na serveru (SearchString + TOP).
// Hledání: bez ohledu na velikost písmen a diakritiku, po slovech (všechna slova musí být v řádku); shody se zvýrazní.
function mycombo_init(c) {

    var _controlid = c.controlid;
    var _combo_currentFocus = -1;

    var _tabid = "tab1" + _controlid;
    var _tabbodyid = _tabid + "_tbody";
    var _cmdcombo = $("#cmdCombo" + _controlid);
    var _searchbox = _cmdcombo;
    var _inputEl = document.getElementById("cmdCombo" + _controlid);
    var _dropdownid = "divDropdownContainer" + _controlid;
    var _divData = $("#divData" + _controlid);

    var _event_after_change = c.on_after_change;
    var _filterflag = c.filterflag;
    var _server_timeout;
    var _xhr = null;                // běžící požadavek – při dalším psaní se zruší (jinak by starší odpověď přepsala novější)
    var _masterprefix = c.masterprefix;
    var _masterpid = c.masterpid;
    var _typed = false;             // uživatel psal od otevření nabídky (lokální filtr se použije i na data, která dorazí později)

    var TXT_LOADING = "Hledám…";
    var TXT_EMPTY = "Nic nenalezeno";

    if (c.defvalue !== "") {    //výchozí vyplněná hodnota comba
        if (c.deftext !== "") {
            $(_cmdcombo).val(c.deftext);
        }
        $("[data-id=value_" + _controlid + "]").val(c.defvalue);
        $("[data-id=text_" + _controlid + "]").val(c.deftext);
    }

    var myDropdown = document.getElementById(_dropdownid);

    // ikony × a ▾ jsou uvnitř pole (padding mtc-input) – šířka pole se už nezmenšuje
    if ($(_searchbox).width() > 0 && $(_searchbox).width() < 100) {
        $("#cmdClear" + _controlid).css("display", "none");
    }
    if (_device.isMobile) {
        $("#cmdPop" + _controlid).css("display", "none");
        $("#cmdClear" + _controlid).css("right", "2px");
    }
    handle_update_state();

    $(myDropdown).on("click", function (e) {
        if (e.target === _inputEl) $(_searchbox).select();
    });

    // ------------------------------------------------------------------ otevření nabídky = načtení dat

    myDropdown.addEventListener("show.bs.dropdown", function () {
        _typed = false;
        var curpid = "";
        if (_filterflag == "1") {
            curpid = $("[data-id=value_" + _controlid + "]").val();
        }
        load({ entity: c.entity, o15flag: "", pids: curpid, tableid: _tabid, myqueryinline: c.myqueryinline, filterflag: _filterflag, searchstring: $(_searchbox).val(), masterprefix: _masterprefix, masterpid: _masterpid }, function () {
            $("#" + _dropdownid).prop("filled", true);
            setTimeout(function () { _inputEl.focus(); }, 50);

            // nastavit kurzor na aktuálně vybraný záznam
            var rows = rows_all();
            _combo_currentFocus = -1;
            var current_value = $("#" + _controlid).val();
            if (current_value != "0" && current_value != "") {
                for (var i = 0; i < rows.length; i++) {
                    if ($(rows[i]).attr("data-v") == current_value && !$(rows[i]).attr("data-skip")) { _combo_currentFocus = i; break; }
                }
            }
            if (_combo_currentFocus === -1) _combo_currentFocus = get_first_visible_rowindex(0, "down");
            update_selected_row(true);
            if (_typed && (_filterflag === "0" || _filterflag === "")) local_filter($(_searchbox).val());
        });
    });

    // načte HTML nabídky ze serveru; předchozí neukončený požadavek zruší
    function load(data, done) {
        if (_xhr) { try { _xhr.abort(); } catch (e) { } }
        show_status(TXT_LOADING, true);
        _xhr = $.ajax({ url: c.posturl, type: "POST", data: data });
        var me = _xhr;
        _xhr.done(function (html) {
            if (me !== _xhr) return;      // mezitím odešel novější požadavek
            _xhr = null;
            _divData.html(html);
            $("#" + _tabid + " .txz").on("click", function () {
                record_was_selected(this);
            });
            if (done) done();
            if (_filterflag !== "0" && _filterflag !== "") highlight(_searchbox.val()); // server už vyfiltroval – jen zvýraznit
            check_empty();
        }).fail(function (x, status) {
            if (status !== "abort" && me === _xhr) { _xhr = null; show_status("Chyba načtení nabídky.", false); }
        });
    }

    function show_status(text, spinner) {
        _divData.html("<div class='mtc-status'>" + (spinner ? "<span class='spinner-border spinner-border-sm me-2' role='status'></span>" : "") + text + "</div>");
    }

    function check_empty() {
        _divData.find(".mtc-empty").remove();
        if (!document.getElementById(_tabbodyid)) return;
        var any = rows_all().filter(function () { return !$(this).attr("data-skip") && this.style.display !== "none"; }).length > 0;
        if (!any) _divData.append("<div class='mtc-status mtc-empty'>" + TXT_EMPTY + "</div>");
    }

    // ------------------------------------------------------------------ psaní

    $(_searchbox).on("change", function () {
        if ($(this).val() == "") handle_clear_value();
    });

    $(_searchbox).on("input", function (e) {
        ensure_open();
        _typed = true;
        if (_filterflag !== "0" && _filterflag !== "") {      //filtruje se na straně serveru
            clearTimeout(_server_timeout);
            _server_timeout = setTimeout(function () { handle_server_filtering(e); }, 250);
            return;
        }
        local_filter($(this).val());                          //filtruje se v prohlížeči
    });

    $(_cmdcombo).on("keydown", function (e) {
        handle_keydown(e);
    });

    function handle_keydown(e) {
        if (e.keyCode === 8 || e.keyCode == 46) {  //DELETE nebo BACKSPACE nad celým označeným textem = vyčistit hodnotu
            var startPos = _inputEl.selectionStart, endPos = _inputEl.selectionEnd;
            if (startPos == 0 && endPos == _inputEl.value.length && _inputEl.value.length > 0) {
                e.stopPropagation();
                e.preventDefault();
                $(_searchbox).val("");
                $("[data-id=value_" + _controlid + "]").val("0");
                $("[data-id=text_" + _controlid + "]").val("");
                handle_update_state();
                if (_filterflag !== "0" && _filterflag !== "") handle_server_filtering(); else local_filter("");
                ensure_open();
            } else if (e.keyCode == 8 && _inputEl.value == "") {
                ensure_open();
            }
            return;
        }

        if (e.keyCode === 27) {  //ESC
            if (is_open()) { e.preventDefault(); e.stopPropagation(); hide(); }
            return;
        }

        if (e.keyCode === 13) {  //ENTER
            e.preventDefault();
            var row = $("#" + _tabbodyid).find(".combo_selrow");
            if (is_open() && row.length > 0 && !$(row).attr("data-skip") && row[0].style.display !== "none") {
                record_was_selected(row[0]);
            } else {
                ensure_open();
            }
            return;
        }

        if (e.keyCode === 9) {  //TAB
            hide();
            return;
        }

        if (e.keyCode === 40 || e.keyCode === 38) {   //šipky
            e.preventDefault();
            if (!is_open()) { ensure_open(); return; }
            var rows_count = rows_all().length;
            var dest;
            if (e.keyCode === 40) {
                dest = get_first_visible_rowindex(_combo_currentFocus + 1, "down");
                if (dest === -1) dest = get_first_visible_rowindex(0, "down");
            } else {
                dest = get_first_visible_rowindex(_combo_currentFocus - 1, "up");
                if (dest === -1) dest = get_first_visible_rowindex(rows_count - 1, "up");
            }
            if (dest === -1) return;
            _combo_currentFocus = dest;
            update_selected_row(false);
        }
    }

    $("#cmdClear" + _controlid).on("click", function () {
        $(_cmdcombo).val("");
        handle_clear_value();
        _inputEl.focus();
    });

    $("#cmdPop" + _controlid).on("click", function () {
        var div = $("#" + _dropdownid).find(".dropdown-menu").first();
        var w = $(div).width() - 30;
        var offset_btn = $(this).offset();
        if ((w + offset_btn.left) < $(window).width()) {
            $(div).css("left", (w * -1) + "px");
        }
    });

    // ------------------------------------------------------------------ otevření / zavření

    function dd() { return bootstrap.Dropdown.getOrCreateInstance(_inputEl); }
    function is_open() { return $("#" + _dropdownid + " .dropdown-menu").hasClass("show"); }
    function ensure_open() { if (!is_open()) dd().show(); }
    function hide() {
        if (!is_open()) return;
        dd().hide();
        var pop = document.getElementById("cmdPop" + _controlid);
        if (pop) { var i = bootstrap.Dropdown.getInstance(pop); if (i) i.hide(); }
    }

    // ------------------------------------------------------------------ hledání v prohlížeči (filterflag 0)

    function norm(s) {
        return String(s || "").normalize("NFD").replace(/[̀-ͯ]/g, "").toLowerCase();
    }
    function words(q) {
        return norm(q).split(/\s+/).filter(function (w) { return w.length > 0; });
    }

    function local_filter(q) {
        var ws = words(q);
        var rows = rows_all();
        rows.each(function () {
            if ($(this).attr("data-skip")) return;
            var t = this._mtcText || (this._mtcText = norm(this.textContent));
            var ok = ws.every(function (w) { return t.indexOf(w) > -1; });
            this.style.display = ok ? "" : "none";
        });
        // záhlaví skupin: zobrazit, jen když skupina obsahuje viditelný řádek
        var header = null, visible = false;
        rows.each(function () {
            if ($(this).attr("data-skip")) {
                if (header) header.style.display = visible ? "" : "none";
                header = this; visible = false;
            } else if (this.style.display !== "none") {
                visible = true;
            }
        });
        if (header) header.style.display = visible ? "" : "none";

        highlight(q);
        check_empty();
        _combo_currentFocus = get_first_visible_rowindex(0, "down");
        update_selected_row(true);
    }

    // zvýraznění shod <mark> – jen v buňkách s čistým textem (buňky s HTML se nemění)
    function highlight(q) {
        var ws = words(q);
        rows_all().each(function () {
            if ($(this).attr("data-skip") || this.style.display === "none") return;
            $(this).children("td").each(function () {
                if (this._mtcOrig === undefined) {
                    if (this.children.length > 0) { this._mtcOrig = null; return; }   //buňka s HTML – nezvýrazňovat
                    this._mtcOrig = this.textContent;
                }
                if (this._mtcOrig === null) return;
                this.innerHTML = mark(this._mtcOrig, ws);
            });
        });
    }

    function esc(s) { return String(s).replace(/[&<>"]/g, function (x) { return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" }[x]; }); }

    function mark(text, ws) {
        if (!ws.length || !text) return esc(text);
        // normalizovaný text s mapou pozic na původní znaky
        var n = "", map = [];
        for (var i = 0; i < text.length; i++) {
            var x = norm(text[i]);
            for (var k = 0; k < x.length; k++) { n += x[k]; map.push(i); }
        }
        var on = new Array(text.length).fill(false), any = false;
        ws.forEach(function (w) {
            var p = n.indexOf(w);
            while (p > -1) {
                for (var j = p; j < p + w.length && j < map.length; j++) on[map[j]] = true;
                any = true;
                p = n.indexOf(w, p + w.length);
            }
        });
        if (!any) return esc(text);
        var out = "", open = false;
        for (var m = 0; m < text.length; m++) {
            if (on[m] && !open) { out += "<mark class='mtc-mark'>"; open = true; }
            if (!on[m] && open) { out += "</mark>"; open = false; }
            out += esc(text[m]);
        }
        if (open) out += "</mark>";
        return out;
    }

    // ------------------------------------------------------------------ řádky a výběr

    function rows_all() { return $("#" + _tabbodyid).find("tr"); }

    function handle_clear_value() {
        $("[data-id=value_" + _controlid + "]").val("0");
        $("[data-id=text_" + _controlid + "]").val("");
        handle_update_state();
        fire_change("0", null);
    }

    function fire_change(v, prefix) {
        if (_event_after_change === "" || _event_after_change == null) return;
        if (prefix) {   //mySearch – řádek s data-prefix
            eval(_event_after_change + "('" + v + "','" + prefix + "')");
            return;
        }
        if (_event_after_change.indexOf("#pid#") === -1) {
            eval(_event_after_change + "('" + v + "')");
        } else {
            eval(_event_after_change.replace("#pid#", v));
        }
    }

    function get_first_visible_rowindex(fromindex, direction) {
        var rows = rows_all(), i;
        if (direction === "down") {
            for (i = Math.max(0, fromindex); i < rows.length; i++) {
                if ($(rows[i]).attr("data-skip")) continue;   //záhlaví skupiny (mySearch)
                if (rows[i].style.display !== "none") return i;
            }
        } else {
            for (i = Math.min(fromindex, rows.length - 1); i >= 0; i--) {
                if ($(rows[i]).attr("data-skip")) continue;
                if (rows[i].style.display !== "none") return i;
            }
        }
        return -1;
    }

    function update_selected_row(center) {
        var rows = rows_all();
        rows.removeClass("combo_selrow");
        if (_combo_currentFocus > -1 && rows[_combo_currentFocus]) {
            var row = rows[_combo_currentFocus];
            $(row).addClass("combo_selrow");
            row.scrollIntoView({ block: center ? "center" : "nearest", inline: "nearest" });
        }
    }

    function handle_update_state() {
        var strValue = $("[data-id=value_" + _controlid + "]").val();
        var w = $(_searchbox).width();
        var show = (w > 100 || w == -20) && strValue !== "" && strValue !== "0" && strValue != null;
        $("#cmdClear" + _controlid).css("display", show ? "" : "none");
    }

    function record_was_selected(row) {
        var v = $(row).attr("data-v");
        var t = $(row).attr("data-t");
        if (t === undefined) {
            var td = $(row).find("td:first")[0];
            t = td ? (td._mtcOrig != null ? td._mtcOrig : td.textContent) : "";
        }
        $(_cmdcombo).val(t);
        $("[data-id=value_" + _controlid + "]").val(v);
        $("[data-id=text_" + _controlid + "]").val(t);
        handle_update_state();
        hide();

        if (typeof _toolbar_warn2save_changes === "function") _toolbar_warn2save_changes();
        fire_change(v, $(row).attr("data-prefix"));
    }

    // ------------------------------------------------------------------ hledání na serveru (filterflag >= 1)

    function handle_server_filtering() {
        var explicit_columns = $("#explicit_columns" + _controlid).val();
        load({ entity: c.entity, o15flag: "", tableid: _tabid, myqueryinline: c.myqueryinline, filterflag: _filterflag, searchstring: $(_searchbox).val(), masterprefix: _masterprefix, masterpid: _masterpid, explicit_columns: explicit_columns }, function () {
            $("[data-id=value_" + _controlid + "]").val("0");
            $("[data-id=text_" + _controlid + "]").val("");
            handle_update_state();
            var columnsinfo = $("#columnsinfo" + _tabid).val();
            $("#explicit_columns" + _controlid).val(columnsinfo);
            _combo_currentFocus = get_first_visible_rowindex(0, "down");
            update_selected_row(true);
        });
    }
}
