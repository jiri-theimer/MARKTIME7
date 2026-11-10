/* Notepad editor (lehká Froala) – jediné místo konfigurace.
   Používá Views/Shared/_Notepad.cshtml (skripty načtené se stránkou) a _HtmlMemo.cshtml (editor až po kliknutí).
   Editor nenahrává soubory ani obrázky – přílohy patří do FILEBOXu záznamu. */
(function () {
    "use strict";

    const PLUGINS = ["align", "code_beautifier", "colors", "code_view", "fullscreen", "line_breaker", "link", "lists", "paragraph_format", "quote", "table", "url", "word_paste"];
    const KEY = "DUA2yE2D2C2A3B8C3B2qYFd1UQRFQIVb1MSMc2IWPNe1IFg1yD4C3D2F2C4B1F1H4B2B1==";

    const np = {
        // texty a jazyk doplní Razor (_Notepad / _HtmlMemo) přes mtNotepad.setup({...})
        texts: { placeholder: "Text…", normal: "Normální", heading: "Nadpis", subheading: "Podnadpis", noFiles: "Obrázky a soubory vkládejte do přílohy (FILEBOX), ne do textu." },
        lang: "",
        version: "",

        setup(o) {
            if (o.texts) Object.assign(np.texts, o.texts);
            if (o.lang !== undefined) np.lang = o.lang;
            if (o.version !== undefined) np.version = o.version;
        },

        options(extra) {
            const btns = {
                moreText: { buttons: ["bold", "italic", "underline", "textColor", "strikeThrough", "backgroundColor", "clearFormatting"], buttonsVisible: 4 },
                moreParagraph: { buttons: ["formatUL", "formatOL", "paragraphFormat", "quote", "alignLeft", "alignCenter", "alignRight", "outdent", "indent"], buttonsVisible: 3 },
                moreRich: { buttons: ["insertLink", "insertTable", "insertHR"], buttonsVisible: 2 },
                moreMisc: { buttons: ["undo", "redo", "fullscreen", "html"], align: "right", buttonsVisible: 2 }
            };
            const btnsXS = {
                moreText: { buttons: ["bold", "italic", "underline", "textColor", "formatUL", "formatOL", "insertLink", "clearFormatting"], buttonsVisible: 4 },
                moreMisc: { buttons: ["undo", "redo", "fullscreen"], align: "right", buttonsVisible: 1 }
            };
            const o = {
                key: KEY,
                attribution: false,
                pluginsEnabled: ["align", "codeBeautifier", "colors", "codeView", "fullscreen", "lineBreaker", "link", "lists", "paragraphFormat", "quote", "table", "url", "wordPaste"],
                toolbarButtons: btns, toolbarButtonsMD: btns, toolbarButtonsSM: btns, toolbarButtonsXS: btnsXS,
                toolbarInline: false,
                toolbarSticky: true,
                quickInsertEnabled: false,
                charCounterCount: false,
                heightMin: 140,
                placeholderText: np.texts.placeholder,
                language: np.lang,
                // barvy: malá paleta (text / zvýraznění), bez vlastního HEX
                colorsText: ["#000000", "#59636E", "#B02A37", "#C2410C", "#B7791F", "#198754", "#0B4B92", "#6F42C1", "REMOVE"],
                colorsBackground: ["#FFF3B0", "#FDE2E1", "#D1F2DD", "#DCEBFF", "#EDE4FF", "#EEF1F4", "REMOVE"],
                colorsStep: 9,
                colorsHEXInput: false,
                paragraphFormat: { N: np.texts.normal, H2: np.texts.heading, H3: np.texts.subheading },
                linkAlwaysBlank: true,
                linkEditButtons: ["linkOpen", "linkEdit", "linkRemove"],
                // vložení z Wordu/Outlooku/webu: bez stylů a tříd; nahrávání souborů a obrázků vypnuto
                pasteDeniedAttrs: ["class", "style", "id"],
                pasteDeniedTags: ["img", "iframe", "video", "audio", "object", "embed", "form", "input", "button", "script", "style"],
                wordPasteModal: false,              // bez dotazu „ponechat formátování?“
                wordPasteKeepFormatting: false,
                imagePaste: false,
                imageUpload: false,
                fileUpload: false,
                videoUpload: false,
                events: {
                    "paste.afterCleanup": function (html) {
                        // obrázky (i base64 z Wordu/Outlooku) do textu nepouštět
                        return html.replace(/<img\b[^>]*>/gi, "");
                    },
                    "paste.after": function () {
                        // pomocné id, které Froala po vložení nechává v HTML
                        this.el.querySelectorAll("#isPasted").forEach(el => el.removeAttribute("id"));
                    },
                    "paste.before": function (e) {
                        const cd = e && (e.clipboardData || (e.originalEvent && e.originalEvent.clipboardData));
                        if (cd && cd.files && cd.files.length > 0 && !cd.getData("text/html") && !cd.getData("text/plain")) {
                            if (typeof _notify_message === "function") _notify_message(np.texts.noFiles, "info");
                            return false;
                        }
                    },
                    "drop": function (e) {
                        const dt = e && (e.dataTransfer || (e.originalEvent && e.originalEvent.dataTransfer));
                        if (dt && dt.files && dt.files.length > 0) return false;
                    }
                }
            };
            if (extra) {
                const ev = extra.events; delete extra.events;
                Object.assign(o, extra);
                if (ev) {
                    // vlastní události přidat k našim (stejný název → zavolat obě)
                    for (const k of Object.keys(ev)) {
                        const mine = o.events[k], theirs = ev[k];
                        o.events[k] = mine ? function () { const r = mine.apply(this, arguments); theirs.apply(this, arguments); return r; } : theirs;
                    }
                }
            }
            return o;
        },

        create(selector, extra) {
            return new FroalaEditor(selector, np.options(extra));
        },

        // Bezpečné zobrazení uloženého HTML ve stránce (bez iframe): parsuje se v neaktivním dokumentu
        // (skripty se nespustí, obrázky se nenačtou), ponechají se jen povolené značky a atributy.
        safeHtml(html) {
            const ALLOWED = new Set(["P", "BR", "DIV", "SPAN", "B", "STRONG", "I", "EM", "U", "S", "STRIKE", "SUB", "SUP", "SMALL", "MARK", "FONT",
                "H1", "H2", "H3", "H4", "H5", "H6", "UL", "OL", "LI", "BLOCKQUOTE", "PRE", "CODE", "HR",
                "TABLE", "THEAD", "TBODY", "TFOOT", "TR", "TD", "TH", "CAPTION", "COLGROUP", "COL", "A", "IMG"]);
            const DROP = new Set(["SCRIPT", "STYLE", "IFRAME", "FRAME", "OBJECT", "EMBED", "FORM", "INPUT", "BUTTON", "SELECT", "TEXTAREA", "LINK", "META", "BASE", "SVG", "MATH", "TEMPLATE", "NOSCRIPT", "AUDIO", "VIDEO"]);
            const ATTRS = new Set(["href", "src", "alt", "title", "style", "colspan", "rowspan", "align", "width", "height", "color", "target"]);
            const okUrl = (v, img) => /^(https?:|mailto:|\/|#)/i.test(v) || (img && /^data:image\/(png|jpe?g|gif|webp);/i.test(v)) || !/^[a-z][a-z0-9+.-]*:/i.test(v);
            const doc = new DOMParser().parseFromString("<!doctype html><body>" + (html || "") + "</body>", "text/html");
            const clean = (node) => {
                for (const el of Array.from(node.children)) {
                    if (DROP.has(el.tagName)) { el.remove(); continue; }
                    clean(el);
                    if (!ALLOWED.has(el.tagName)) { el.replaceWith(...el.childNodes); continue; }   // neznámá značka → jen obsah
                    for (const a of Array.from(el.attributes)) {
                        const n = a.name.toLowerCase(), v = a.value.trim();
                        if (!ATTRS.has(n)) el.removeAttribute(a.name);
                        else if ((n === "href" || n === "src") && !okUrl(v, n === "src")) el.removeAttribute(a.name);
                        else if (n === "style" && /expression|url\s*\(|javascript:|@import|position\s*:\s*(fixed|absolute)/i.test(v)) el.removeAttribute(a.name);
                    }
                    if (el.tagName === "A") { el.setAttribute("target", "_blank"); el.setAttribute("rel", "noopener noreferrer"); }
                }
            };
            clean(doc.body);
            const frag = document.createDocumentFragment();
            for (const n of Array.from(doc.body.childNodes)) frag.appendChild(document.importNode(n, true));
            return frag;
        },

        // načtení skriptů a stylů až při prvním použití (stránky bez _Notepad)
        _loading: null,
        load() {
            if (window.FroalaEditor && FroalaEditor.PLUGINS && FroalaEditor.PLUGINS.colors) return Promise.resolve();
            if (np._loading) return np._loading;
            const v = np.version ? "?v=" + encodeURIComponent(np.version) : "";
            const css = href => new Promise(res => {
                if (document.querySelector(`link[data-np="${href}"]`)) return res();
                const l = document.createElement("link"); l.rel = "stylesheet"; l.href = href + v; l.dataset.np = href;
                l.onload = l.onerror = () => res(); document.head.appendChild(l);
            });
            const js = src => new Promise((res, rej) => {
                const s = document.createElement("script"); s.src = src + v; s.async = false;
                s.onload = () => res(); s.onerror = () => rej(new Error(src)); document.head.appendChild(s);
            });
            np._loading = Promise.all([
                css("/lib/froala-editor/css/froala_editor.pkgd.min.css"),
                css("/css/notepad.css"),
                js("/lib/froala-editor/js/froala_editor.min.js")
                    .then(() => Promise.all(PLUGINS.map(p => js(`/lib/froala-editor/js/plugins/${p}.min.js`))
                        .concat(np.lang ? [js(`/lib/froala-editor/js/languages/${np.lang}.js`)] : [])))
            ]).catch(err => { np._loading = null; throw err; });
            return np._loading;
        }
    };

    window.mtNotepad = np;
})();
