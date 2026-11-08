/* Našeptávač e-mailových adres (náhrada Kendo AutoComplete) – SendMail.
   mailrecipients_init({ inputs: ["#Recipient", ...], url: "/Mail/AutoCompleteSource", recent: ["a@b.cz", ...], recentTitle: "...", emptyText: "..." })
   - více adres oddělených čárkou, našeptává se k právě psané (poslední) adrese
   - hledá v adrese i jménu (bez ohledu na diakritiku), zvýrazní shodu
   - prázdné pole: nabídne naposledy použité adresy
   - klávesy ↑ ↓ Enter Tab Esc */
function mailrecipients_init(o) {
    var list = null, loading = null;
    var $pop = $("<div class='mr-pop' role='listbox' style='display:none;'></div>").appendTo("body");
    var $cur = null, items = [], active = -1, open = false;

    function norm(s) { return (s || "").toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, ""); }
    function esc(s) { return $("<div>").text(s == null ? "" : s).html(); }

    function load() {
        if (list || loading) return loading;
        loading = $.getJSON(_ep(o.url)).done(function (data) {
            var seen = {};
            list = [];
            (data || []).forEach(function (r) {
                var a = (r.address || r.Address || "").trim();
                if (a === "" || seen[a.toLowerCase()]) return;
                seen[a.toLowerCase()] = true;
                var t = r.text || r.Text || "";
                list.push({ a: a, t: t, n: norm(a + " " + t) });
            });
        }).always(function () { loading = null; });
        return loading;
    }

    function token($i) {        //právě psaná adresa = text za poslední čárkou/středníkem
        var v = $i.val(), m = v.match(/^(.*[,;]\s*)?([^,;]*)$/);
        return { head: m && m[1] ? m[1] : "", word: m ? m[2].trim() : v.trim() };
    }

    function mark(txt, w) {
        if (!w) return esc(txt);
        var n = norm(txt), p = n.indexOf(w);
        if (p < 0 || n.length !== txt.length) return esc(txt);      //normalizace změnila délku → bez zvýraznění
        return esc(txt.substr(0, p)) + "<mark class='mtc-mark'>" + esc(txt.substr(p, w.length)) + "</mark>" + esc(txt.substr(p + w.length));
    }

    function show($i) {
        $cur = $i;
        var tk = token($i), w = norm(tk.word), html = [];
        items = [];
        if (w === "") {
            if ($i.val().trim() === "" && o.recent && o.recent.length) {
                html.push("<div class='mr-head'>" + esc(o.recentTitle) + "</div>");
                o.recent.forEach(function (a) { items.push(a); html.push("<div class='mr-item' data-i='" + (items.length - 1) + "'><span class='mr-addr'>" + esc(a) + "</span></div>"); });
            }
        } else if (list) {
            var already = norm($i.val());
            for (var k = 0; k < list.length && items.length < 12; k++) {
                var r = list[k];
                if (r.n.indexOf(w) < 0) continue;
                if (already.indexOf(norm(r.a) + ",") >= 0) continue;     //už je v poli
                items.push(r.a);
                html.push("<div class='mr-item' data-i='" + (items.length - 1) + "'><span class='mr-addr'>" + mark(r.a, w) + "</span>" + (r.t ? "<span class='mr-name'>" + mark(r.t, w) + "</span>" : "") + "</div>");
            }
        }
        if (items.length === 0) { hide(); return; }
        $pop.html(html.join(""));
        var off = $i.offset();
        $pop.css({ top: off.top + $i.outerHeight() + 2, left: off.left, width: Math.max($i.outerWidth(), 280) }).show();
        open = true;
        active = -1;
    }

    function hide() { $pop.hide().empty(); open = false; items = []; active = -1; }

    function pick(i) {
        if (!$cur || i < 0 || i >= items.length) return;
        var tk = token($cur);
        $cur.val(tk.head + items[i] + ", ").trigger("change");
        var el = $cur[0]; el.focus(); el.setSelectionRange(el.value.length, el.value.length);
        hide();
    }

    function move(d) {
        if (!items.length) return;
        active = (active + d + items.length) % items.length;
        var $it = $pop.find(".mr-item").removeClass("mr-active").eq(active).addClass("mr-active");
        var it = $it[0]; if (it) it.scrollIntoView({ block: "nearest" });
    }

    o.inputs.forEach(function (sel) {
        var $i = $(sel);
        if (!$i.length) return;
        $i.attr("autocomplete", "off");
        $i.on("focus", function () { var p = load(); if (p) p.done(function () { if (document.activeElement === $i[0]) show($i); }); show($i); });
        $i.on("input", function () { var p = load(); if (p) p.done(function () { show($i); }); else show($i); });
        $i.on("blur", function () { setTimeout(function () { if ($cur && document.activeElement !== $cur[0]) hide(); }, 150); });
        $i.on("keydown", function (e) {
            if (!open) {
                if (e.key === "ArrowDown") { show($i); move(1); e.preventDefault(); }
                return;
            }
            if (e.key === "ArrowDown") { move(1); e.preventDefault(); }
            else if (e.key === "ArrowUp") { move(-1); e.preventDefault(); }
            else if (e.key === "Enter") { if (active >= 0) { pick(active); e.preventDefault(); } else if (items.length === 1) { pick(0); e.preventDefault(); } }
            else if (e.key === "Tab") { if (active >= 0) { pick(active); e.preventDefault(); } }
            else if (e.key === "Escape") { hide(); e.stopPropagation(); e.preventDefault(); }
        });
    });

    $pop.on("mousedown", ".mr-item", function (e) { e.preventDefault(); pick(parseInt($(this).data("i"), 10)); });
    $pop.on("mousemove", ".mr-item", function () { active = parseInt($(this).data("i"), 10); $pop.find(".mr-item").removeClass("mr-active"); $(this).addClass("mr-active"); });
    $(window).on("resize", hide);
    $(document).on("scroll", hide);
}
