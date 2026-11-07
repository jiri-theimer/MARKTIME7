/*
 * MARKTIME pivot (myPivot) – jednoduchá kontingenční tabulka bez externích knihoven.
 * Data: kompaktní sloupcový JSON (myPivotData.cs). Rozložení: kompatibilní s reportem WebDataRocks (slice).
 *
 *   var p = new MtPivot(element, {
 *       dataUrl: "/p31totals/PivotData?...",
 *       report: {...},                 // report WebDataRocks (j79WebDataRocksReport) nebo {}
 *       exportUrl: "/myPivot/ExportExcel",
 *       title: "Součty",
 *       txt: {...},                    // překlady (Default.cshtml)
 *       onReportChange: function (report) {},
 *       onLayout: function () {}       // po vykreslení (např. dopočet výšky)
 *   });
 *   p.getReport()  → report ve formátu WebDataRocks
 */
(function () {
    "use strict";

    var AGGS = ["sum", "average", "count", "distinctcount", "min", "max"];
    var ROW_H = 26;           // výška řádku (px) – virtualizace
    var FILTER_MAX = 500;     // max. položek ve filtru najednou

    var collator = new Intl.Collator("cs", { numeric: true, sensitivity: "base" });
    var nfCache = {};
    function nf(d) { return nfCache[d] || (nfCache[d] = new Intl.NumberFormat("cs-CZ", { minimumFractionDigits: d, maximumFractionDigits: d })); }
    function esc(s) { return String(s == null ? "" : s).replace(/[&<>"]/g, function (c) { return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" }[c]; }); }
    function lc(s) { return String(s == null ? "" : s).toLowerCase(); }
    function clone(o) { return o == null ? o : JSON.parse(JSON.stringify(o)); }

    function MtPivot(el, o) {
        this.el = el;
        this.o = o || {};
        this.txt = this.o.txt || {};
        this.report = clone(this.o.report) || {};
        this.F = null;            // pole
        this.n = 0;
        this.cnt = null;          // počet záznamů ve skupině (RowsCount)
        this.s = null;            // rozložení
        this.vis = [];            // viditelné řádky
        this._build();
        this.load();
    }
    window.MtPivot = MtPivot;
    var P = MtPivot.prototype;

    // =================================================================== kostra

    P._build = function () {
        var t = this.txt;
        function btn(act, icon, text) {
            return '<button type="button" class="btn bog" data-act="' + act + '"><span class="material-icons-outlined-btn">' + icon + "</span> " + esc(text) + "</button>";
        }
        this.el.classList.add("mtp");
        this.el.innerHTML =
            '<div class="mtp-bar">' +
            '  <div class="mtp-zones">' +
            '    <div class="mtp-zone" data-zone="filters"><span class="mtp-zl">' + esc(t.filters) + '</span><div class="mtp-chips"></div></div>' +
            '    <div class="mtp-zone" data-zone="cols"><span class="mtp-zl">' + esc(t.columns) + '</span><div class="mtp-chips"></div></div>' +
            '    <div class="mtp-zone" data-zone="rows"><span class="mtp-zl">' + esc(t.rows) + '</span><div class="mtp-chips"></div></div>' +
            '    <div class="mtp-zone" data-zone="measures"><span class="mtp-zl">' + esc(t.values) + '</span><div class="mtp-chips"></div></div>' +
            '  </div>' +
            '  <div class="mtp-tools">' +
            btn("fields", "view_list", t.fields) + btn("expand", "unfold_more", t.expandAll) + btn("collapse", "unfold_less", t.collapseAll) +
            btn("excel", "grid_on", "Excel") + btn("print", "print", t.print) +
            '    <span class="mtp-info"></span>' +
            '  </div>' +
            '  <div class="mtp-avail mtp-zone" data-zone="avail"><span class="mtp-zl">' + esc(t.available) + '</span><div class="mtp-chips"></div></div>' +
            '</div>' +
            '<div class="mtp-msg" style="display:none;"></div>' +
            '<div class="mtp-body"><table class="mtp-table"><thead></thead><tbody></tbody></table></div>';
        this.$bar = this.el.querySelector(".mtp-bar");
        this.$body = this.el.querySelector(".mtp-body");
        this.$thead = this.el.querySelector("thead");
        this.$tbody = this.el.querySelector("tbody");
        this.$msg = this.el.querySelector(".mtp-msg");
        this.$info = this.el.querySelector(".mtp-info");

        var me = this;
        this.$bar.addEventListener("click", function (e) { me._onBarClick(e); });
        this.$bar.addEventListener("change", function (e) { me._onBarChange(e); });
        this.$body.addEventListener("scroll", function () {
            if (me._raf) return;
            me._raf = requestAnimationFrame(function () { me._raf = null; me._renderBody(); });
        });
        this.$body.addEventListener("click", function (e) { me._onTableClick(e); });
        this._initDnd();
        document.addEventListener("mousedown", function (e) {
            if (me._pop && !me._pop.contains(e.target)) me._closePop();
        });
    };

    P._message = function (html, isError) {
        this.$msg.innerHTML = html;
        this.$msg.className = "mtp-msg" + (isError ? " mtp-error" : "");
        this.$msg.style.display = html ? "" : "none";
    };

    // =================================================================== data

    P.load = function () {
        var me = this;
        this._message(esc(this.txt.loading));
        fetch(this.o.dataUrl, { credentials: "same-origin" })
            .then(function (r) { if (!r.ok) throw new Error("HTTP " + r.status); return r.json(); })
            .then(function (d) {
                if (d.error) { me._message(esc(d.error), true); me.$body.style.display = "none"; return; }
                me._message("");
                me.setData(d);
            })
            .catch(function (e) { me._message(esc(me.txt.loadError) + " (" + esc(e.message) + ")", true); });
    };

    P.setData = function (d) {
        var me = this;
        this.n = d.n;
        this.cnt = d.count;
        this.F = d.fields.map(function (f, i) {
            var x = { i: i, name: f.name, type: f.type, measure: !!f.measure, decimals: f.decimals, num: null, codes: null, dict: null };
            var col = d.data[i];
            if (f.type === "number") x.num = col;
            else { x.dict = col.d; x.codes = col.i; }
            return x;
        });
        this.s = this._fromReport(this.report);
        this.recalc();
    };

    // kódy hodnot dimenze (číselné pole použité jako dimenze dostane číselník až při použití)
    P._codes = function (f) {
        if (f.codes) return f.codes;      // textová / datumová pole a virtuální pole (dategroup)
        var map = new Map(), dict = [], codes = new Int32Array(this.n);
        for (var r = 0; r < this.n; r++) {
            var v = f.num[r];
            if (v == null) { codes[r] = -1; continue; }
            var c = map.get(v);
            if (c === undefined) { c = dict.length; map.set(v, c); dict.push(v); }
            codes[r] = c;
        }
        f.dict = dict; f.codes = codes;
        return codes;
    };

    // pole pro dimenzi: datum lze seskupit po měsících / čtvrtletích / rocích (virtuální pole s vlastním číselníkem)
    P._fv = function (fi, gran) {
        var f = this.F[fi];
        if (gran === undefined && this.s) gran = this.s.dgran[fi];
        if (f.type !== "date" || !gran || gran === "day") return f;
        var key = fi + ":" + gran;
        this._vf = this._vf || {};
        if (this._vf[key]) return this._vf[key];
        var base = this._codes(f);
        var conv = f.dict.map(function (v) {
            var y = v.substr(0, 4), m = +v.substr(5, 2);
            return gran === "year" ? y : gran === "quarter" ? y + "-Q" + (Math.floor((m - 1) / 3) + 1) : y + "-" + v.substr(5, 2);
        });
        var dict = Array.from(new Set(conv)).sort(), map = new Map();
        dict.forEach(function (k, i) { map.set(k, i); });
        var codes = new Int32Array(this.n);
        for (var r = 0; r < this.n; r++) codes[r] = base[r] < 0 ? -1 : map.get(conv[base[r]]);
        return (this._vf[key] = { i: fi, name: f.name, type: "dategroup", gran: gran, dict: dict, codes: codes, decimals: 0 });
    };

    P._label = function (f, code) {
        if (code < 0) return this.txt.empty;
        var v = f.dict[code];
        switch (f.type) {
            case "dategroup":
                if (f.gran === "month") return v.substr(5, 2) + "/" + v.substr(0, 4);
                if (f.gran === "quarter") return v.substr(5) + " " + v.substr(0, 4);
                return v;
            case "date": return v.length >= 10 ? v.substr(8, 2) + "." + v.substr(5, 2) + "." + v.substr(0, 4) + (v.length > 10 ? " " + v.substr(11) : "") : v;
            case "bool": return v === "1" ? this.txt.yes : this.txt.no;
            case "number": return nf(f.decimals).format(v);
            default: return v;
        }
    };

    P._cmp = function (f, a, b) {
        if (a === b) return 0;
        if (a < 0) return 1; if (b < 0) return -1;            // prázdné na konec
        var x = f.dict[a], y = f.dict[b];
        if (f.type === "number") return x - y;
        if (f.type === "date" || f.type === "dategroup") return x < y ? -1 : 1;
        return collator.compare(x, y);
    };

    // =================================================================== rozložení (report WebDataRocks)

    P._field = function (name) {
        var k = lc(name);
        for (var i = 0; i < this.F.length; i++) if (lc(this.F[i].name) === k) return this.F[i];
        return null;
    };

    P._fromReport = function (rep) {
        var me = this, sl = (rep && rep.slice) || {}, opt = (rep && rep.options && rep.options.grid) || {};
        var s = { filters: [], rows: [], cols: [], measures: [], excl: {}, sort: {}, dgran: {}, valueSort: null, expandAll: false, expRows: new Set(), expCols: new Set(), showTotals: opt.showTotals || "on", showGrand: opt.showGrandTotals || "on", pendingExp: null };

        function take(list, arr) {
            (list || []).forEach(function (it) {
                if (!it || !it.uniqueName || it.uniqueName === "Measures") return;
                var f = me._field(it.uniqueName);
                if (!f || s.rows.indexOf(f.i) >= 0 || s.cols.indexOf(f.i) >= 0 || s.filters.indexOf(f.i) >= 0) return;
                arr.push(f.i);
                if (it.sort === "desc" || it.sort === "asc") s.sort[f.i] = it.sort;
                if (f.type === "date" && ["month", "quarter", "year"].indexOf(it.mtGranularity) >= 0) s.dgran[f.i] = it.mtGranularity;
                if (it.filter) me._filterFromReport(me._fv(f.i, s.dgran[f.i] || null), it.filter, s);
            });
        }
        take(sl.reportFilters, s.filters);
        take(sl.rows, s.rows);
        take(sl.columns, s.cols);

        (sl.measures || []).forEach(function (m) {
            var f = m && me._field(m.uniqueName);
            if (!f) return;
            var agg = lc(m.aggregation || (f.type === "number" ? "sum" : "count"));
            if (AGGS.indexOf(agg) < 0) agg = f.type === "number" ? "sum" : "count";
            if (f.type !== "number" && agg !== "count" && agg !== "distinctcount") agg = "count";
            if (s.measures.some(function (x) { return x.f === f.i; })) return;     // stejné pole v hodnotách jen jednou
            s.measures.push({ f: f.i, agg: agg, format: m.format || null });
        });

        // bez uloženého rozložení: první dimenze do řádků, všechna měřítka do hodnot
        if (!s.rows.length && !s.cols.length && !s.measures.length) {
            var dim = this.F.filter(function (f) { return !f.measure; })[0];
            if (dim) s.rows.push(dim.i);
            this.F.filter(function (f) { return f.measure && f.type === "number"; }).forEach(function (f) { s.measures.push({ f: f.i, agg: "sum" }); });
        }

        var ex = sl.expands || {};
        s.expandAll = !!ex.expandAll;
        if (ex.rows && ex.rows.length) s.pendingExp = ex.rows;

        var so = sl.sorting && sl.sorting.column;
        if (so && so.measure && (!so.tuple || !so.tuple.length)) {
            var f = this._field(so.measure.uniqueName || so.measure);
            if (f) {
                var mi = s.measures.findIndex(function (m) { return m.f === f.i && (!so.measure.aggregation || lc(so.measure.aggregation) === m.agg); });
                if (mi >= 0) s.valueSort = { m: mi, dir: so.type === "asc" ? "asc" : "desc" };
            }
        }
        return s;
    };

    // člen WebDataRocks "pole.hodnota" → kód hodnoty
    P._memberCode = function (f, member) {
        var m = String(member), pre = lc(f.name) + ".";
        if (lc(m).indexOf(pre) === 0) m = m.substr(pre.length);
        var k = lc(m);
        if (!f.codes) this._codes(f);
        for (var c = 0; c < f.dict.length; c++) {
            if (lc(f.dict[c]) === k || lc(this._label(f, c)) === k) return c;
        }
        return null;
    };

    P._filterFromReport = function (f, flt, s) {
        var me = this, set = new Set();
        if (!f.codes) this._codes(f);
        if (flt.exclude) {
            flt.exclude.forEach(function (m) { var c = me._memberCode(f, m); if (c != null) set.add(c); });
        } else if (flt.members) {
            var inc = new Set();
            flt.members.forEach(function (m) { var c = me._memberCode(f, m); if (c != null) inc.add(c); });
            for (var c = -1; c < f.dict.length; c++) if (!inc.has(c)) set.add(c);
        }
        if (set.size) s.excl[f.i] = set;
    };

    /** Aktuální rozložení ve formátu WebDataRocks (pro uložení do j79WebDataRocksReport / localStorage). */
    P.getReport = function () {
        var me = this, s = this.s, rep = clone(this.report) || {};
        if (!s) return rep;
        function item(fi) {
            var f = me._fv(fi), it = { uniqueName: f.name };
            if (s.sort[fi]) it.sort = s.sort[fi];
            if (f.gran) it.mtGranularity = f.gran;           // vlastní vlastnost MARKTIME pivotu (WebDataRocks ji ignoruje)
            var ex = s.excl[fi];
            if (ex && ex.size) {
                var mem = function (c) { return f.name + "." + (c < 0 ? "" : f.dict[c]); };
                if (ex.size * 2 > f.dict.length + 1) {     // většina vyloučena → uložit vybrané hodnoty
                    it.filter = { members: [] };
                    for (var c = -1; c < f.dict.length; c++) if (!ex.has(c)) it.filter.members.push(mem(c));
                } else {
                    it.filter = { exclude: [] };
                    ex.forEach(function (c) { it.filter.exclude.push(mem(c)); });
                }
            }
            return it;
        }
        rep.slice = rep.slice || {};
        rep.slice.reportFilters = s.filters.map(item);
        rep.slice.rows = s.rows.map(item);
        rep.slice.columns = s.cols.map(item).concat([{ uniqueName: "Measures" }]);
        rep.slice.measures = s.measures.map(function (m) {
            var x = { uniqueName: me.F[m.f].name, aggregation: m.agg };
            if (m.format) x.format = m.format;
            return x;
        });
        var exp = { expandAll: s.expandAll };
        if (!s.expandAll && this._rowRoot) {
            var tuples = [];
            this._walk(this._rowRoot, function (n) {
                if (n.level > 0 && s.expRows.has(n.key)) tuples.push({ tuple: me._tuple(n, s.rows) });
            });
            if (tuples.length) exp.rows = tuples;
        }
        rep.slice.expands = exp;
        if (s.valueSort && s.measures[s.valueSort.m]) {
            var m = s.measures[s.valueSort.m];
            rep.slice.sorting = { column: { type: s.valueSort.dir, tuple: [], measure: { uniqueName: this.F[m.f].name, aggregation: m.agg } } };
        } else delete rep.slice.sorting;
        rep.options = rep.options || {};
        rep.options.grid = rep.options.grid || {};
        rep.options.grid.type = "compact";
        rep.options.grid.showTotals = s.showTotals;
        rep.options.grid.showGrandTotals = s.showGrand;
        return rep;
    };

    P._tuple = function (n, fields) {
        var arr = [];
        for (var x = n; x && x.level > 0; x = x.parent) { var f = this._fv(fields[x.level - 1]); arr.unshift(f.name + "." + (x.code < 0 ? "" : f.dict[x.code])); }
        return arr;
    };

    P._changed = function () {
        this.recalc();
        if (this.o.onReportChange) this.o.onReportChange(this.getReport());
    };

    // =================================================================== výpočet

    P.recalc = function () {
        if (!this.F) return;
        var t0 = performance.now();
        var me = this, s = this.s, n = this.n, F = this.F, cnt = this.cnt;
        var rowF = s.rows.map(function (i) { return me._fv(i); }), colF = s.cols.map(function (i) { return me._fv(i); });
        var rowC = rowF.map(function (f) { return me._codes(f); }), colC = colF.map(function (f) { return me._codes(f); });
        var M = s.measures.length;
        var mF = s.measures.map(function (m) { return F[m.f]; });
        var mVals = mF.map(function (f) { return f.type === "number" ? f.num : me._codes(f); });

        // filtry
        var exKeys = Object.keys(s.excl).filter(function (k) { return s.excl[k] && s.excl[k].size; }).map(Number);
        var exCodes = exKeys.map(function (k) { return me._codes(me._fv(k)); }), exSets = exKeys.map(function (k) { return s.excl[k]; });

        // stromy řádků a sloupců
        function root() { return { key: "", code: null, level: 0, parent: null, kids: new Map(), id: 0 }; }
        var rRoot = root(), cRoot = root(), rNodes = [rRoot], cNodes = [cRoot];
        var recRow = new Int32Array(n), recCol = new Int32Array(n), used = new Uint8Array(n);

        function node(parent, code, nodes) {
            var k = parent.kids.get(code);
            if (!k) {
                k = { key: parent.key + "\u0001" + code, code: code, level: parent.level + 1, parent: parent, kids: new Map(), id: nodes.length };
                parent.kids.set(code, k);
                nodes.push(k);
            }
            return k;
        }

        var shown = 0;
        outer: for (var r = 0; r < n; r++) {
            for (var e = 0; e < exCodes.length; e++) if (exSets[e].has(exCodes[e][r])) continue outer;
            used[r] = 1; shown++;
            var nd = rRoot;
            for (var a = 0; a < rowC.length; a++) nd = node(nd, rowC[a][r], rNodes);
            recRow[r] = nd.id;
            nd = cRoot;
            for (var b = 0; b < colC.length; b++) nd = node(nd, colC[b][r], cNodes);
            recCol[r] = nd.id;
        }

        // agregace do řídkých buněk (řádkový uzel × sloupcový uzel)
        var CN = cNodes.length, slot = new Map(), sCnt = [], sSum = [], sN = [], sMin = [], sMax = [], sSet = [];
        var hasDistinct = s.measures.some(function (m) { return m.agg === "distinctcount"; });
        function getSlot(ri, ci) {
            var key = ri * CN + ci, x = slot.get(key);
            if (x === undefined) {
                x = sCnt.length; slot.set(key, x); sCnt.push(0);
                for (var m = 0; m < M; m++) { sSum.push(0); sN.push(0); sMin.push(Infinity); sMax.push(-Infinity); if (hasDistinct) sSet.push(null); }
            }
            return x;
        }
        for (var r2 = 0; r2 < n; r2++) {
            if (!used[r2]) continue;
            var w = cnt ? cnt[r2] : 1;
            for (var rn = rNodes[recRow[r2]]; rn; rn = rn.parent) {
                for (var cn = cNodes[recCol[r2]]; cn; cn = cn.parent) {
                    var x = getSlot(rn.id, cn.id);
                    sCnt[x] += w;
                    for (var m = 0; m < M; m++) {
                        var j = x * M + m, agg = s.measures[m].agg, v = mVals[m][r2];
                        if (agg === "distinctcount") {
                            if (v != null && v !== -1) { (sSet[j] || (sSet[j] = new Set())).add(v); }
                            continue;
                        }
                        if (v == null || mF[m].type !== "number") continue;
                        sSum[j] += v; sN[j] += w;
                        if (v < sMin[j]) sMin[j] = v;
                        if (v > sMax[j]) sMax[j] = v;
                    }
                }
            }
        }
        this._agg = { slot: slot, CN: CN, M: M, cnt: sCnt, sum: sSum, n: sN, min: sMin, max: sMax, set: sSet };
        this._rowRoot = rRoot; this._colRoot = cRoot; this._rowF = rowF; this._colF = colF;
        this._rNodes = rNodes; this._cNodes = cNodes;

        // rozbalení z uloženého reportu (tuple "pole.hodnota")
        if (s.pendingExp) {
            s.pendingExp.forEach(function (t) {
                var nd = rRoot;
                (t.tuple || []).forEach(function (mem, lvl) {
                    if (!nd || lvl >= rowF.length) { nd = null; return; }
                    var c = me._memberCode(rowF[lvl], mem);
                    nd = c == null ? null : nd.kids.get(c);
                });
                if (nd) s.expRows.add(nd.key);
            });
            s.pendingExp = null;
        }

        this._sortTree(rRoot, rowF, true);
        this._sortTree(cRoot, colF, false);
        this._layout();
        this._renderBar();
        this._renderHead();
        this._renderBody();
        if (this.o.onLayout) this.o.onLayout();
        this.$info.textContent = (this.txt.records || "") + " " + nf(0).format(shown) + (shown < n ? " / " + nf(0).format(n) : "") + " · " + Math.round(performance.now() - t0) + " ms";
    };

    // hodnota agregace v buňce (řádkový uzel, sloupcový uzel, index měřítka) – null = prázdná buňka
    P._val = function (rn, cn, m) {
        var A = this._agg, x = A.slot.get(rn.id * A.CN + cn.id);
        if (x === undefined) return null;
        var j = x * A.M + m, agg = this.s.measures[m].agg;
        switch (agg) {
            case "count": return A.cnt[x];
            case "distinctcount": return A.set[j] ? A.set[j].size : 0;
            case "average": return A.n[j] ? A.sum[j] / A.n[j] : null;
            case "min": return A.n[j] ? A.min[j] : null;
            case "max": return A.n[j] ? A.max[j] : null;
            default: return A.n[j] ? A.sum[j] : null;
        }
    };

    P._decimals = function (m) {
        var me = this.s.measures[m], f = this.F[me.f];
        if (me.agg === "count" || me.agg === "distinctcount") return 0;
        var fm = this.report.formats || [];
        var named = me.format ? fm.find(function (x) { return x.name === me.format; }) : null;
        var def = fm.find(function (x) { return !x.name; });
        var d = named && named.decimalPlaces != null ? named.decimalPlaces : (def && def.decimalPlaces != null ? def.decimalPlaces : f.decimals);
        return d < 0 ? f.decimals : d;
    };

    P._fmt = function (v, m) { return v == null ? "" : nf(this._decimals(m)).format(v); };

    P._sortTree = function (rootNode, fields, isRows) {
        var me = this, s = this.s, vs = isRows ? s.valueSort : null, croot = this._colRoot;
        (function sortNode(nd) {
            if (!nd.kids.size) { nd.list = []; return; }
            var f = fields[nd.level], list = Array.from(nd.kids.values());
            if (vs && s.measures[vs.m]) {
                var dir = vs.dir === "asc" ? 1 : -1;
                list.sort(function (a, b) {
                    var x = me._val(a, croot, vs.m), y = me._val(b, croot, vs.m);
                    if (x == null) return 1; if (y == null) return -1;
                    return (x - y) * dir;
                });
            } else {
                var d = s.sort[f.i] === "desc" ? -1 : 1;
                list.sort(function (a, b) { return me._cmp(f, a.code, b.code) * d; });
            }
            nd.list = list;
            list.forEach(sortNode);
        })(rootNode);
    };

    P._walk = function (nd, fn) { fn(nd); (nd.list || Array.from(nd.kids.values())).forEach(function (k) { P._walk(k, fn); }); };

    // rozbaleno: v režimu „vše rozbaleno“ se pamatují sbalené uzly (prefix "!"), jinak rozbalené
    P._isExp = function (nd, isRows) {
        var s = this.s, set = isRows ? s.expRows : s.expCols;
        if (!nd.kids.size) return false;
        return s.expandAll ? !set.has("!" + nd.key) : set.has(nd.key);
    };

    // viditelné řádky a sloupce
    P._layout = function (all) {
        var me = this, s = this.s, rows = [], cols = [];
        var subCols = s.showTotals === "on" || s.showTotals === "columns";
        (function addRow(nd) {
            nd.list.forEach(function (k) {
                var ex = k.kids.size > 0 && (all || me._isExp(k, true));
                rows.push({ n: k, exp: ex });
                if (ex) addRow(k);
            });
        })(this._rowRoot);
        if (!this._rowF.length || s.showGrand === "on" || s.showGrand === "rows") rows.push({ n: this._rowRoot, grand: true });

        (function addCol(nd) {
            nd.list.forEach(function (k) {
                var ex = k.kids.size > 0 && (all || me._isExp(k, false));
                if (ex) { addCol(k); if (subCols) cols.push({ n: k, sub: true }); }
                else cols.push({ n: k });
            });
        })(this._colRoot);
        if (!this._colF.length || s.showGrand === "on" || s.showGrand === "columns") cols.push({ n: this._colRoot, grand: true });

        if (all) return { rows: rows, cols: cols };
        this.vis = rows; this.visCols = cols;
    };

    // =================================================================== vykreslení – panel polí

    P._chip = function (fi, zone, mi) {
        var f = this.F[fi], s = this.s, t = this.txt, h = '<span class="mtp-chip' + (f.measure ? " mtp-m" : "") + '" draggable="true" data-f="' + fi + '" data-zone="' + zone + '"' + (mi != null ? ' data-mi="' + mi + '"' : "") + '>';
        if (zone === "measures") {
            var m = s.measures[mi];
            h += '<span class="mtp-cl">' + esc(f.name) + '</span><select class="mtp-agg" data-mi="' + mi + '">';
            AGGS.forEach(function (a) {
                if (f.type !== "number" && a !== "count" && a !== "distinctcount") return;
                h += '<option value="' + a + '"' + (a === m.agg ? " selected" : "") + ">" + esc(t.agg[a] || a) + "</option>";
            });
            h += "</select>";
        } else {
            h += '<span class="mtp-cl">' + esc(f.name) + "</span>";
            if (zone !== "avail" && f.type === "date") {
                var g = s.dgran[fi] || "day";
                h += '<select class="mtp-gran" data-f="' + fi + '">';
                ["day", "month", "quarter", "year"].forEach(function (x) { h += '<option value="' + x + '"' + (x === g ? " selected" : "") + ">" + esc(t.gran[x]) + "</option>"; });
                h += "</select>";
            }
            if (zone !== "avail") {
                var ex = s.excl[fi] && s.excl[fi].size;
                h += '<span class="mtp-ic mtp-flt' + (ex ? " on" : "") + '" data-act="filter" title="' + esc(t.filter) + '">⧩</span>';
                if (zone !== "filters") h += '<span class="mtp-ic" data-act="sort" title="' + esc(t.sort) + '">' + (s.sort[fi] === "desc" ? "↓" : "↑") + "</span>";
            }
        }
        if (zone !== "avail") h += '<span class="mtp-ic mtp-x" data-act="remove" title="' + esc(t.remove) + '">×</span>';
        return h + "</span>";
    };

    P._renderBar = function () {
        var me = this, s = this.s, z = {};
        this.$bar.querySelectorAll(".mtp-zone").forEach(function (el) { z[el.getAttribute("data-zone")] = el.querySelector(".mtp-chips"); });
        z.filters.innerHTML = s.filters.map(function (fi) { return me._chip(fi, "filters"); }).join("") || '<span class="mtp-hint">' + esc(this.txt.dropHere) + "</span>";
        z.cols.innerHTML = s.cols.map(function (fi) { return me._chip(fi, "cols"); }).join("") || '<span class="mtp-hint">' + esc(this.txt.dropHere) + "</span>";
        z.rows.innerHTML = s.rows.map(function (fi) { return me._chip(fi, "rows"); }).join("") || '<span class="mtp-hint">' + esc(this.txt.dropHere) + "</span>";
        z.measures.innerHTML = s.measures.map(function (m, i) { return me._chip(m.f, "measures", i); }).join("") || '<span class="mtp-hint">' + esc(this.txt.dropHere) + "</span>";
        var placed = new Set(s.filters.concat(s.rows, s.cols, s.measures.map(function (m) { return m.f; })));
        z.avail.innerHTML = this.F.filter(function (f) { return !placed.has(f.i); }).map(function (f) { return me._chip(f.i, "avail"); }).join("");
    };

    P._onBarClick = function (e) {
        var a = e.target.closest("[data-act]");
        if (!a) return;
        var act = a.getAttribute("data-act"), chip = a.closest(".mtp-chip");
        switch (act) {
            case "fields": var av = this.$bar.querySelector(".mtp-avail"); av.style.display = av.style.display === "none" ? "" : "none"; return;
            case "expand": this.expandAll(); return;
            case "collapse": this.collapseAll(); return;
            case "excel": this.exportExcel(); return;
            case "print": this.print(); return;
        }
        if (!chip) return;
        var fi = +chip.getAttribute("data-f"), zone = chip.getAttribute("data-zone");
        if (act === "remove") { this._move(fi, zone, "avail", null, chip.getAttribute("data-mi")); return; }
        if (act === "sort") { this.s.sort[fi] = this.s.sort[fi] === "desc" ? "asc" : "desc"; this.s.valueSort = null; this._changed(); return; }
        if (act === "filter") { this._openFilter(fi, a); return; }
    };

    P._onBarChange = function (e) {
        if (e.target.classList.contains("mtp-gran")) {
            var fi = +e.target.getAttribute("data-f");
            if (e.target.value === "day") delete this.s.dgran[fi]; else this.s.dgran[fi] = e.target.value;
            delete this.s.excl[fi];          // kódy hodnot se změnily
            this.s.expRows.clear(); this.s.expCols.clear();
            this._changed();
            return;
        }
        if (e.target.classList.contains("mtp-agg")) {
            this.s.measures[+e.target.getAttribute("data-mi")].agg = e.target.value;
            this._changed();
        }
    };

    // přesun pole mezi zónami (before = index cílové pozice nebo null = na konec)
    P._move = function (fi, from, to, before, mi) {
        var s = this.s, f = this.F[fi];
        if (from === "measures" && to === "measures") {
            var m = s.measures.splice(+mi, 1)[0];
            if (before != null && before > +mi) before--;
            s.measures.splice(before == null ? s.measures.length : before, 0, m);
        } else {
            if (from === "measures") s.measures.splice(+mi, 1);
            else if (from !== "avail") { var arr0 = s[from]; arr0.splice(arr0.indexOf(fi), 1); }
            if (to === "measures") {
                if (s.measures.some(function (m) { return m.f === fi; })) return;     // pole je v hodnotách už jednou
                var nm = { f: fi, agg: f.type === "number" ? "sum" : "count" };
                s.measures.splice(before == null ? s.measures.length : before, 0, nm);
            } else if (to !== "avail") {
                ["filters", "rows", "cols"].forEach(function (z) { var i = s[z].indexOf(fi); if (i >= 0 && z !== from) s[z].splice(i, 1); });
                var arr = s[to];
                var cur = arr.indexOf(fi);
                if (cur >= 0) { arr.splice(cur, 1); if (before != null && before > cur) before--; }
                arr.splice(before == null ? arr.length : before, 0, fi);
            }
            if (to === "avail" || to === "filters") { /* filtr zůstává i v zóně Filtry */ }
            if (to === "avail" && from !== "measures") { delete s.excl[fi]; delete s.dgran[fi]; }
        }
        s.expRows.clear(); s.expCols.clear();
        if (s.valueSort && !s.measures[s.valueSort.m]) s.valueSort = null;
        this._changed();
    };

    P._initDnd = function () {
        var me = this, drag = null;
        this.$bar.addEventListener("dragstart", function (e) {
            var c = e.target.closest && e.target.closest(".mtp-chip");
            if (!c) return;
            drag = { f: +c.getAttribute("data-f"), zone: c.getAttribute("data-zone"), mi: c.getAttribute("data-mi") };
            e.dataTransfer.effectAllowed = "move";
            try { e.dataTransfer.setData("text/plain", "mtp"); } catch (x) { }
            c.classList.add("mtp-drag");
        });
        this.$bar.addEventListener("dragend", function (e) {
            drag = null;
            me.$bar.querySelectorAll(".mtp-drag,.mtp-over").forEach(function (x) { x.classList.remove("mtp-drag", "mtp-over"); });
        });
        this.$bar.addEventListener("dragover", function (e) {
            var z = e.target.closest(".mtp-zone");
            if (!drag || !z) return;
            e.preventDefault();
            me.$bar.querySelectorAll(".mtp-over").forEach(function (x) { x.classList.remove("mtp-over"); });
            z.classList.add("mtp-over");
        });
        this.$bar.addEventListener("drop", function (e) {
            var z = e.target.closest(".mtp-zone");
            if (!drag || !z) return;
            e.preventDefault();
            var to = z.getAttribute("data-zone"), target = e.target.closest(".mtp-chip"), before = null;
            if (target && target.getAttribute("data-zone") === to && to !== "avail") {
                before = to === "measures" ? +target.getAttribute("data-mi") : me.s[to].indexOf(+target.getAttribute("data-f"));
            }
            var d = drag; drag = null;
            if (d.zone === to && to === "avail") return;
            me._move(d.f, d.zone, to, before, d.mi);
        });
        // dvojklik na dostupné pole: měřítko → Hodnoty, ostatní → Řádky
        this.$bar.addEventListener("dblclick", function (e) {
            var c = e.target.closest(".mtp-chip");
            if (!c || c.getAttribute("data-zone") !== "avail") return;
            var fi = +c.getAttribute("data-f");
            me._move(fi, "avail", me.F[fi].measure ? "measures" : "rows", null, null);
        });
    };

    // =================================================================== filtr hodnot

    P._closePop = function () { if (this._pop) { this._pop.remove(); this._pop = null; } };

    P._openFilter = function (fi, anchor) {
        this._closePop();
        var me = this, f = this._fv(fi), t = this.txt;
        this._codes(f);
        var ex = new Set(this.s.excl[fi] || []);
        var all = []; for (var c = 0; c < f.dict.length; c++) all.push(c);
        all.sort(function (a, b) { return me._cmp(f, a, b); });
        all.push(-1);

        var pop = document.createElement("div");
        pop.className = "mtp-pop";
        pop.innerHTML = '<div class="mtp-pop-h">' + esc(f.name) + '</div>' +
            '<input type="search" class="form-control form-control-sm" placeholder="' + esc(t.search) + '" />' +
            '<div class="mtp-pop-a"><a href="#" data-a="all">' + esc(t.selectAll) + '</a> · <a href="#" data-a="none">' + esc(t.selectNone) + '</a></div>' +
            '<div class="mtp-pop-list"></div>' +
            '<div class="mtp-pop-b"><button type="button" class="btn btn-sm btn-primary" data-a="ok">OK</button> <button type="button" class="btn btn-sm bog" data-a="cancel">' + esc(t.cancel) + "</button></div>";
        document.body.appendChild(pop);
        var r = anchor.getBoundingClientRect();
        pop.style.left = Math.max(4, Math.min(r.left + window.scrollX, window.scrollX + document.documentElement.clientWidth - 300)) + "px";
        pop.style.top = (r.bottom + window.scrollY + 4) + "px";
        this._pop = pop;

        var $list = pop.querySelector(".mtp-pop-list"), $q = pop.querySelector("input"), shownCodes = [];
        function draw() {
            var q = lc($q.value), h = [], k = 0;
            shownCodes = [];
            for (var i = 0; i < all.length; i++) {
                var lab = me._label(f, all[i]);
                if (q && lc(lab).indexOf(q) < 0) continue;
                if (k++ >= FILTER_MAX) { h.push('<div class="mtp-pop-more">' + esc(t.filterMore) + "</div>"); break; }
                shownCodes.push(all[i]);
                h.push('<label><input type="checkbox" data-c="' + all[i] + '"' + (ex.has(all[i]) ? "" : " checked") + "> " + esc(lab) + "</label>");
            }
            $list.innerHTML = h.join("");
        }
        draw();
        $q.addEventListener("input", draw);
        $q.focus();
        $list.addEventListener("change", function (e) {
            var c = +e.target.getAttribute("data-c");
            if (e.target.checked) ex.delete(c); else ex.add(c);
        });
        pop.addEventListener("click", function (e) {
            var a = e.target.getAttribute("data-a");
            if (!a) return;
            e.preventDefault();
            if (a === "all") { shownCodes.forEach(function (c) { ex.delete(c); }); draw(); }
            if (a === "none") { shownCodes.forEach(function (c) { ex.add(c); }); draw(); }
            if (a === "cancel") me._closePop();
            if (a === "ok") {
                if (ex.size >= all.length) return;      // aspoň jedna hodnota musí zůstat
                if (ex.size) me.s.excl[fi] = ex; else delete me.s.excl[fi];
                me._closePop();
                me._changed();
            }
        });
    };

    // =================================================================== vykreslení – tabulka

    P._colLabel = function (cn) {
        return this._label(this._colF[cn.level - 1], cn.code);
    };

    P._headHtml = function (L, printMode) {
        var me = this, s = this.s, t = this.txt, M = s.measures.length, C = this._colF.length, cols = L.cols;
        var rowsTitle = this._rowF.map(function (f) { return f.name; }).join(" / ");
        var h = [];
        var hdrRows = C + (M ? 1 : 0);
        function ancAt(nd, lvl) { while (nd.level > lvl) nd = nd.parent; return nd; }
        for (var lvl = 1; lvl <= C; lvl++) {
            var tr = ["<tr>"];
            if (lvl === 1) tr.push('<th class="mtp-corner" rowspan="' + hdrRows + '">' + esc(rowsTitle) + "</th>");
            var span = Math.max(M, 1), i = 0;
            while (i < cols.length) {
                var e = cols[i], nd = e.n;
                if (e.grand) {
                    if (lvl === 1) tr.push('<th class="mtp-gt" colspan="' + span + '" rowspan="' + C + '">' + esc(t.grandTotal) + "</th>");
                    i++; continue;
                }
                if (nd.level < lvl) { i++; continue; }          // pokryto buňkou s rowspan z vyšší úrovně
                if (nd.level === lvl) {
                    var tg = !printMode && nd.kids.size ? '<span class="mtp-tg" data-c="' + nd.id + '">' + (e.sub ? "−" : "+") + "</span>" : "";
                    tr.push('<th colspan="' + span + '" rowspan="' + (C - lvl + 1) + '"' + (e.sub ? ' class="mtp-sub"' : "") + ">" + tg + esc(e.sub ? t.total + " " + this._colLabel(nd) : this._colLabel(nd)) + "</th>");
                    i++; continue;
                }
                // předek na úrovni lvl – sloučit souvislou skupinu jeho potomků
                var anc = ancAt(nd, lvl), j = i;
                while (j < cols.length && !cols[j].grand && cols[j].n.level > lvl && ancAt(cols[j].n, lvl) === anc) j++;
                var tg2 = !printMode ? '<span class="mtp-tg" data-c="' + anc.id + '">−</span>' : "";
                tr.push('<th colspan="' + (j - i) * span + '">' + tg2 + esc(this._colLabel(anc)) + "</th>");
                i = j;
            }
            tr.push("</tr>");
            h.push(tr.join(""));
        }
        if (M) {
            var tm = ["<tr>"];
            if (!C) tm.push('<th class="mtp-corner">' + esc(rowsTitle) + "</th>");
            cols.forEach(function (e) {
                s.measures.forEach(function (m, mi) {
                    var arrow = "";
                    if (e.grand && s.valueSort && s.valueSort.m === mi) arrow = s.valueSort.dir === "asc" ? " ↑" : " ↓";
                    tm.push('<th class="mtp-mh' + (e.grand ? " mtp-vs" : "") + '"' + (e.grand ? ' data-vs="' + mi + '" title="' + esc(t.sortByValue) + '"' : "") + ">" + esc(me._mCaption(mi)) + arrow + "</th>");
                });
            });
            tm.push("</tr>");
            h.push(tm.join(""));
        }
        return h.join("");
    };

    P._mCaption = function (mi) {
        var m = this.s.measures[mi], f = this.F[m.f];
        return m.agg === "sum" ? f.name : (this.txt.agg[m.agg] || m.agg) + " – " + f.name;
    };

    P._renderHead = function () {
        this.$thead.innerHTML = this._headHtml({ cols: this.visCols }, false);
        this.$body.style.display = this.s.measures.length ? "" : "none";
        if (!this.s.measures.length) this._message(esc(this.txt.noMeasures));
        else if (this.$msg.className.indexOf("mtp-error") < 0) this._message("");
    };

    P._rowHtml = function (row, cols, printMode) {
        var nd = row.n, s = this.s, M = s.measures.length;
        var lab, cls = "";
        if (row.grand) { lab = this.txt.grandTotal; cls = "mtp-gtr"; }
        else {
            var tg = !printMode && nd.kids.size ? '<span class="mtp-tg" data-r="' + nd.id + '">' + (row.exp ? "−" : "+") + "</span>" : '<span class="mtp-tg0"></span>';
            lab = tg + esc(this._label(this._rowF[nd.level - 1], nd.code));
            if (row.exp) cls = "mtp-subr";
        }
        var drill = !printMode && this.o.onDrill;
        var icon = drill ? '<span class="mtp-di material-icons-outlined-btn" title="' + esc(this.txt.drill) + '">list_alt</span>' : "";
        var h = ['<tr class="' + cls + '"' + (printMode ? "" : ' data-rn="' + nd.id + '"') + '><td class="mtp-rl" style="padding-left:' + ((drill ? 28 : 6) + (row.grand ? 0 : (nd.level - 1) * 16)) + 'px">' + icon + (row.grand ? esc(lab) : lab) + "</td>"];
        for (var i = 0; i < cols.length; i++) {
            var e = cols[i], ccls = e.grand ? "mtp-gtc" : (e.sub ? "mtp-subc" : "");
            for (var m = 0; m < M; m++) h.push('<td class="' + ccls + '">' + this._fmt(this._val(nd, e.n, m), m) + "</td>");
        }
        h.push("</tr>");
        return h.join("");
    };

    P._renderBody = function () {
        if (!this.vis) return;
        var total = this.vis.length, top = this.$body.scrollTop, hgt = this.$body.clientHeight || 600;
        var headH = this.$thead.offsetHeight || 0;
        var from = Math.max(0, Math.floor((top - headH) / ROW_H) - 10), to = Math.min(total, from + Math.ceil(hgt / ROW_H) + 20);
        var h = [];
        // řádek celkového součtu se vykresluje vždy (přilepený dole), ostatní jen ve viditelné oblasti
        var grand = total && this.vis[total - 1].grand ? this.vis[total - 1] : null, last = grand ? total - 1 : total;
        to = Math.min(to, last);
        if (from > 0) h.push('<tr class="mtp-sp" style="height:' + from * ROW_H + 'px"><td colspan="999"></td></tr>');
        for (var i = from; i < to; i++) h.push(this._rowHtml(this.vis[i], this.visCols, false));
        if (to < last) h.push('<tr class="mtp-sp" style="height:' + (last - to) * ROW_H + 'px"><td colspan="999"></td></tr>');
        if (grand) h.push(this._rowHtml(grand, this.visCols, false));
        this.$tbody.innerHTML = h.join("");
    };

    P._onTableClick = function (e) {
        var tg = e.target.closest(".mtp-tg"), s = this.s;
        if (tg) {
            var rid = tg.getAttribute("data-r"), cid = tg.getAttribute("data-c"), me = this;
            if (rid != null) this._toggle(this._rNodes[+rid], true);
            if (cid != null) this._toggle(this._cNodes[+cid], false);
            return;
        }
        var di = e.target.closest(".mtp-di");
        if (di && this.o.onDrill) {
            this._drill(this._rNodes[+di.closest("tr").getAttribute("data-rn")], this._colRoot);   // řádek přes všechny sloupce
            return;
        }
        var vs = e.target.closest("[data-vs]");
        if (vs) {
            var mi = +vs.getAttribute("data-vs");
            s.valueSort = s.valueSort && s.valueSort.m === mi ? (s.valueSort.dir === "desc" ? { m: mi, dir: "asc" } : null) : { m: mi, dir: "desc" };
            this._changed();
        }
    };

    P._toggle = function (nd, isRows) {
        if (!nd) return;
        var s = this.s, set = isRows ? s.expRows : s.expCols;
        if (s.expandAll) {
            // v režimu „vše rozbaleno“ se pamatují sbalené uzly s prefixem "!"
            if (set.has("!" + nd.key)) set.delete("!" + nd.key); else set.add("!" + nd.key);
        } else {
            if (set.has(nd.key)) set.delete(nd.key); else set.add(nd.key);
        }
        this._layout(); this._renderHead(); this._renderBody();
        if (this.o.onReportChange) this.o.onReportChange(this.getReport());
    };

    // =================================================================== zdrojové záznamy buňky (drill-down)

    // podmínka pro server: pole, hodnoty (u seskupeného data rozsahy [od, do)), prázdná hodnota
    P._cond = function (f, codes, mode) {
        var c = { f: f.name, mode: mode, empty: false, values: [], ranges: [] };
        codes.forEach(function (code) {
            if (code < 0) { c.empty = true; return; }
            var v = f.dict[code];
            if (f.type !== "dategroup") { c.values.push(v); return; }
            var y = +v.substr(0, 4), m1, m2;
            if (f.gran === "year") { m1 = 0; m2 = 12; }
            else if (f.gran === "quarter") { m1 = (+v.substr(6) - 1) * 3; m2 = m1 + 3; }
            else { m1 = +v.substr(5, 2) - 1; m2 = m1 + 1; }
            var iso = function (yy, mm) { yy += Math.floor(mm / 12); mm = mm % 12; return yy + "-" + String(mm + 1).padStart(2, "0") + "-01"; };
            c.ranges.push([iso(y, m1), iso(y, m2)]);
        });
        return c;
    };

    P._drill = function (rn, cn) {
        if (!rn || !cn) return;
        var me = this, s = this.s, conds = [], label = [];
        [[rn, this._rowF], [cn, this._colF]].forEach(function (x) {
            var lab = [];
            for (var nd = x[0]; nd && nd.level > 0; nd = nd.parent) {
                var f = x[1][nd.level - 1];
                conds.push(me._cond(f, [nd.code], "in"));
                lab.unshift(f.name + ": " + me._label(f, nd.code));
            }
            label = label.concat(lab);
        });
        Object.keys(s.excl).forEach(function (k) {
            if (s.excl[k] && s.excl[k].size) conds.push(me._cond(me._fv(+k), Array.from(s.excl[k]), "notin"));
        });
        this.o.onDrill({ conds: conds, label: label.join(" · ") || this.txt.grandTotal });
    };

    P.expandAll = function () { this.s.expandAll = true; this.s.expRows.clear(); this.s.expCols.clear(); this._changed(); };
    P.collapseAll = function () { this.s.expandAll = false; this.s.expRows.clear(); this.s.expCols.clear(); this._changed(); };

    // =================================================================== export a tisk (vždy vše rozbalené)

    P._fullLayout = function () { return this._layout(true); };

    P.exportExcel = function () {
        if (!this.F) return;
        var me = this, s = this.s, L = this._fullLayout(), M = s.measures.length, C = this._colF.length;
        var headers = [], rowsTitle = this._rowF.map(function (f) { return f.name; }).join(" / ");
        for (var lvl = 1; lvl <= C; lvl++) {
            var hr = [lvl === 1 ? rowsTitle : ""];
            L.cols.forEach(function (e) {
                var nd = e.n, txt = "";
                if (e.grand) txt = lvl === 1 ? me.txt.grandTotal : "";
                else if (nd.level === lvl) txt = e.sub ? me.txt.total + " " + me._colLabel(nd) : me._colLabel(nd);
                else if (nd.level > lvl) { var a = nd; while (a.level > lvl) a = a.parent; txt = me._colLabel(a); }
                for (var m = 0; m < M; m++) hr.push(m === 0 ? txt : "");
            });
            headers.push(hr);
        }
        var mh = [C ? "" : rowsTitle];
        L.cols.forEach(function () { for (var m = 0; m < M; m++) mh.push(me._mCaption(m)); });
        headers.push(mh);
        var decimals = [];
        L.cols.forEach(function () { for (var m = 0; m < M; m++) decimals.push(me._decimals(m)); });
        var rows = L.rows.map(function (r) {
            var v = [];
            L.cols.forEach(function (e) { for (var m = 0; m < M; m++) { var x = me._val(r.n, e.n, m); v.push(x == null ? null : Math.round(x * 1e6) / 1e6); } });
            return { l: r.grand ? 0 : r.n.level - 1, t: r.grand ? me.txt.grandTotal : me._label(me._rowF[r.n.level - 1], r.n.code), b: !!r.grand, v: v };
        });
        var btn = this.$bar.querySelector('[data-act="excel"]');
        btn.disabled = true;
        fetch(this.o.exportUrl || "/myPivot/ExportExcel", {
            method: "POST", credentials: "same-origin",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ title: this.o.title || "", headers: headers, rows: rows, decimals: decimals })
        }).then(function (r) {
            if (!r.ok) throw new Error("HTTP " + r.status);
            var fn = (/filename\*?=(?:UTF-8'')?"?([^";]+)/i.exec(r.headers.get("Content-Disposition") || "") || [])[1] || "Soucty.xlsx";
            return r.blob().then(function (b) { return { b: b, fn: decodeURIComponent(fn) }; });
        }).then(function (x) {
            var a = document.createElement("a");
            a.href = URL.createObjectURL(x.b); a.download = x.fn;
            document.body.appendChild(a); a.click();
            setTimeout(function () { URL.revokeObjectURL(a.href); a.remove(); }, 1000);
        }).catch(function (e) {
            alert(me.txt.loadError + " (" + e.message + ")");
        }).finally(function () { btn.disabled = false; });
    };

    P.print = function () {
        if (!this.F) return;
        var me = this, L = this._fullLayout();
        var body = L.rows.map(function (r) { return me._rowHtml(r, L.cols, true); }).join("");
        var css = "body{font-family:Segoe UI,Arial,sans-serif;font-size:11px;margin:10px}h3{margin:0 0 8px}" +
            "table{border-collapse:collapse}th,td{border:1px solid #bbb;padding:2px 6px}th{background:#eee}" +
            "td{text-align:right;white-space:nowrap}td.mtp-rl{text-align:left}.mtp-gtr td,.mtp-gtc,.mtp-subr td{font-weight:bold}" +
            ".mtp-tg0{display:none}thead{display:table-header-group}tr{page-break-inside:avoid}";
        var w = window.open("", "_blank");
        if (!w) return;
        w.document.write('<!DOCTYPE html><html><head><meta charset="utf-8"><title>' + esc(this.o.title || "") + "</title><style>" + css + "</style></head><body>" +
            (this.o.title ? "<h3>" + esc(this.o.title) + "</h3>" : "") +
            "<table><thead>" + this._headHtml(L, true) + "</thead><tbody>" + body + "</tbody></table></body></html>");
        w.document.close();
        w.focus();
        setTimeout(function () { w.print(); }, 300);
    };
})();
