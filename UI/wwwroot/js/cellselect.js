//Hromadné označování buněk v měsíčním kalendáři (/p31calendar/Index) a v dayline (/p31dayline/Index)
//  tažení myší   = souvislý rozsah (v dayline obdélník osoby x dny)
//  Ctrl+klik     = přidat/odebrat jednu buňku
//  Shift+klik    = rozsah od naposledy označené (nebo naposledy kliknuté) buňky
//  Esc, klik     = zrušit výběr
//V rozsahu (tažení, Shift) se vynechávají nepracovní dny (data-nw='1': víkend, svátek), přes Ctrl+klik je lze přidat jednotlivě.
//Nad výběrem jde i nastavit nebo vyčistit barvu dne (limit maxcolorcells, na serveru FutBatchResult.MaxColorCells).
//Počet označených buněk je omezen (maxcells, na serveru FutBatchResult.MaxCells): nad limit lišta akce zablokuje.
//Lišta s akcemi nad výběrem: ~/Views/Shared/_CellSelectBar.cshtml
var mtCellSelect = (function () {
    var cfg = null;
    var sel = [];           //označené buňky (elementy TD)
    var anchor = null;      //výchozí buňka pro Shift+klik
    var drag = null;        //{start, last, active}
    var busy = false;

    function init(options) {
        cfg = Object.assign({ selector: "", grid: false, j02id: null, maxcells: 40, maxcolorcells: 400, onsaved: null }, options);
        if (!document.getElementById("csbar")) return;     //lišta není na stránce (uživatel nemá modul úkonů)

        document.addEventListener("mousedown", on_mousedown);
        document.addEventListener("mouseover", on_mouseover);
        document.addEventListener("mouseup", on_mouseup);
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && sel.length > 0) clear();
        });
    }

    function cell_of(target) {
        return (target && target.closest) ? target.closest(cfg.selector) : null;
    }
    function is_interactive(target) {
        return target.closest("a,button,input,select,textarea,label") !== null;
    }

    function on_mousedown(e) {
        if (e.button !== 0) return;
        var td = cell_of(e.target);
        if (!td || is_interactive(e.target)) return;

        if (e.shiftKey && anchor) {
            e.preventDefault();     //jinak prohlížeč označuje text
            var r = range(anchor, td);
            set((e.ctrlKey || e.metaKey) ? union(sel, r) : r);
            return;
        }
        if (e.ctrlKey || e.metaKey) {
            e.preventDefault();
            toggle(td);
            anchor = td;
            return;
        }
        drag = { start: td, last: td, active: false };
    }

    function on_mouseover(e) {
        if (!drag) return;
        if ((e.buttons & 1) === 0) {    //tlačítko bylo puštěno mimo okno
            end_drag();
            return;
        }
        var td = cell_of(e.target);
        if (!td || td === drag.last) return;
        drag.last = td;
        if (!drag.active) {
            drag.active = true;
            document.body.classList.add("cs-dragging");
            var s = window.getSelection();
            if (s) s.removeAllRanges();
        }
        set(range(drag.start, td));
    }

    function on_mouseup(e) {
        if (!drag) return;
        var wasdrag = drag.active;
        anchor = drag.start;
        end_drag();
        if (wasdrag) return;
        if (sel.length > 0 && !e.shiftKey && !e.ctrlKey && !e.metaKey) clear(true);   //prostý klik na buňku ruší výběr
    }

    function end_drag() {
        drag = null;
        document.body.classList.remove("cs-dragging");
    }

    //buňky mezi a + b: kalendář = souvislá řada dnů, dayline = obdélník
    function range(a, b) {
        if (a === b) return [a];
        var all = Array.prototype.slice.call(document.querySelectorAll(cfg.selector));
        var ret;
        if (!cfg.grid) {
            var i1 = all.indexOf(a), i2 = all.indexOf(b);
            ret = all.slice(Math.min(i1, i2), Math.max(i1, i2) + 1);
        } else {
            var rows = [];
            all.forEach(function (td) {
                if (rows[rows.length - 1] !== td.parentNode) rows.push(td.parentNode);
            });
            var r1 = rows.indexOf(a.parentNode), r2 = rows.indexOf(b.parentNode);
            var rmin = Math.min(r1, r2), rmax = Math.max(r1, r2);
            var cmin = Math.min(a.cellIndex, b.cellIndex), cmax = Math.max(a.cellIndex, b.cellIndex);
            ret = [];
            for (var r = rmin; r <= rmax; r++) {
                var tds = rows[r].cells;
                for (var c = cmin; c <= cmax && c < tds.length; c++) {
                    if (tds[c].matches(cfg.selector)) ret.push(tds[c]);
                }
            }
        }
        return ret.filter(function (td) { return td.getAttribute("data-nw") !== "1"; });
    }

    function union(a, b) {
        var ret = a.slice();
        b.forEach(function (td) { if (ret.indexOf(td) < 0) ret.push(td); });
        return ret;
    }

    function toggle(td) {
        var i = sel.indexOf(td);
        var lis = sel.slice();
        if (i >= 0) lis.splice(i, 1); else lis.push(td);
        set(lis);
    }

    function set(lis) {
        sel.forEach(function (td) { if (lis.indexOf(td) < 0) td.classList.remove("cs-sel"); });
        lis.forEach(function (td) { td.classList.add("cs-sel"); });
        sel = lis;
        render_bar();
    }

    function clear(keepanchor) {
        set([]);
        if (keepanchor !== true) anchor = null;
    }

    //označené buňky jako [{j02id, d}], d ve tvaru dd.MM.yyyy
    function cells() {
        return sel.map(function (td) {
            return { j02id: String(td.getAttribute("data-j02id") || cfg.j02id), d: td.getAttribute("data-d") };
        });
    }
    function distinct(arr) {
        return arr.filter(function (x, i) { return arr.indexOf(x) === i; });
    }
    function datekey(d) {   //dd.MM.yyyy -> yyyyMMdd
        var a = d.split(".");
        return a[2] + a[1] + a[0];
    }

    function render_bar() {
        var bar = document.getElementById("csbar");
        if (sel.length === 0) {
            bar.style.display = "none";
            menu(false);
            return;
        }
        var c = cells();
        var persons = distinct(c.map(function (x) { return x.j02id; }));
        var s = "Označeno dnů: " + c.length;
        if (cfg.grid) {
            s = "Označeno buněk: " + c.length;
            if (persons.length > 1) s += " (osob: " + persons.length + ")";
        }
        var over = is_over();
        if (over) s += " – najednou lze nejvýše " + cfg.maxcells;
        var info = document.getElementById("csbar_info");
        info.textContent = s;
        info.classList.toggle("csbar-over", over);

        var cmd = document.getElementById("csbar_p31");
        cmd.disabled = (over || persons.length !== 1);
        cmd.title = over ? over_message() : (cmd.disabled ? "Formulář úkonu lze otevřít jen pro označené dny jedné osoby" : "Otevřít formulář úkonu pro označené dny");

        var cmdfut = document.getElementById("csbar_fut");
        if (cmdfut) {
            cmdfut.disabled = over;
            cmdfut.title = over ? over_message() : "";
        }
        if (over) menu(false, "fut");

        var cmdc24 = document.getElementById("csbar_c24");
        if (cmdc24) {
            var overc = sel.length > cfg.maxcolorcells;
            cmdc24.disabled = overc;
            cmdc24.title = overc ? "Najednou lze obarvit nejvýše " + cfg.maxcolorcells + " buněk." : "";
            if (overc) menu(false, "c24");
        }

        bar.style.display = "flex";
    }

    function is_over() {
        return sel.length > cfg.maxcells;
    }
    function over_message() {
        return "Najednou lze vykázat nejvýše " + cfg.maxcells + " buněk, označeno je " + sel.length + ". Zmenšete výběr.";
    }

    //nabídky lišty: which = "fut" (nepřítomnost, výchozí) nebo "c24" (barva dne); menu(false) zavře obě
    function menu(show, which) {
        var ids = { fut: "csbar_menu", c24: "csbar_menu_c24" };
        if (show === false && !which) {
            Object.keys(ids).forEach(function (k) { var x = document.getElementById(ids[k]); if (x) x.style.display = "none"; });
            return;
        }
        which = which || "fut";
        var m = document.getElementById(ids[which]);
        if (!m) return;
        if (show === undefined) show = (m.style.display === "none");
        if (show) menu(false);      //otevřená je vždy jen jedna nabídka
        m.style.display = show ? "block" : "none";
    }

    //barva dne do všech označených buněk (c24id = 0: bez barvy)
    function color(c24id) {
        var c = cells();
        if (c.length === 0 || busy) return;
        menu(false);
        if (c.length > cfg.maxcolorcells) {
            _notify_message("Najednou lze obarvit nejvýše " + cfg.maxcolorcells + " buněk.", "info");
            return;
        }
        busy = true;
        $.post(_ep("/p31calendar/SaveC23Batch"), { c24id: c24id, cells: c.map(function (x) { return x.j02id + "|" + x.d; }) }, function (data) {
            busy = false;
            if (data.errors && data.errors.length > 0) {
                alert(data.errors.join("\n"));
                return;
            }
            if (cfg.onsaved) cfg.onsaved();
        }).fail(function () {
            busy = false;
            alert("Barvu dne se nepodařilo odeslat na server.");
        });
    }

    //formulář úkonu předvyplněný označenými dny (režim "multi" v p31/Record)
    function record(p32id) {
        var c = cells();
        if (c.length === 0) return;
        if (is_over()) {
            _notify_message(over_message(), "info");
            return;
        }
        var persons = distinct(c.map(function (x) { return x.j02id; }));
        if (persons.length !== 1) {
            _notify_message("Formulář úkonu lze otevřít jen pro označené dny jedné osoby.", "info");
            return;
        }
        var ds = distinct(c.map(function (x) { return x.d; })).sort(function (a, b) { return datekey(a).localeCompare(datekey(b)); });
        var url = "/p31/Record?j02id=" + persons[0] + "&ds=" + ds.join(",");
        if (p32id) url += "&p32id=" + p32id;
        menu(false);
        _window_open(url, 3);
    }

    //rychlé vykázání aktivity nepřítomnosti do všech označených buněk
    function fut(p32id, ctl) {
        var c = cells();
        if (c.length === 0 || busy) return;
        var name = ctl ? ctl.getAttribute("data-name") : "";
        menu(false);
        if (is_over()) {
            _notify_message(over_message(), "info");
            return;
        }
        if (!confirm("Vykázat [" + name + "] do označených dnů?\n\nPočet úkonů: " + c.length)) return;

        busy = true;
        $.post(_ep("/p31calendar/SaveFutBatch"), { p32id: p32id, cells: c.map(function (x) { return x.j02id + "|" + x.d; }) }, function (data) {
            busy = false;
            if (data.manual) {
                //aktivita nemá výchozí hodnotu nebo projekt: zadat přes formulář
                if (distinct(c.map(function (x) { return x.j02id; })).length === 1) {
                    _notify_message("V nastavení aktivity není uvedena výchozí hodnota úkonu nebo projekt - proto nelze vykazovat úplně automaticky", "info");
                    record(p32id);
                } else {
                    alert("V nastavení aktivity není uvedena výchozí hodnota úkonu nebo projekt.\n\nTakovou aktivitu lze hromadně vykázat jen přes formulář úkonu, tedy pro označené dny jedné osoby.");
                }
                return;
            }
            if (data.errors && data.errors.length > 0) {
                alert("Zapsáno úkonů: " + data.saved + " z " + c.length + "\n\nNezapsáno:\n" + data.errors.join("\n"));
            }
            if (data.saved > 0 && cfg.onsaved) cfg.onsaved();
        }).fail(function () {
            busy = false;
            alert("Hromadné vykázání se nepodařilo odeslat na server.");
        });
    }

    return { init: init, clear: clear, cells: cells, record: record, fut: fut, color: color, menu: menu };
})();
