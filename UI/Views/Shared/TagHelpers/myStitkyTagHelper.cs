using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace UI.Views.Shared.TagHelpers
{
    //štítky záznamu: čipy s × + rozbalovací výběr s hledáním (mystitky_init v mycontrols.js, vzhled mst-* v site.css)
    //hodnoty beze změny: skrytý TagPids = o51ID oddělené čárkou, TagHtml = HTML štítků (kvůli kompatibilitě)
    [HtmlTargetElement("mystitky")]
    public class myStitkyTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private readonly BL.Factory _f;

        public myStitkyTagHelper(BL.Factory f)
        {
            _f = f;
        }

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("entity")]
        public string Entity { get; set; }

        [HtmlAttributeName("buttontext")]
        public string ButtonText { get; set; }

        [HtmlAttributeName("tagnames")]
        public string SelectedTagNames { get; set; }

        [HtmlAttributeName("taghtml")]
        public string SelectedTagHtml { get; set; }

        private System.Text.StringBuilder _sb;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            _sb = new System.Text.StringBuilder();
            string strSelectedValues = this.For.Model as string;

            //vybrané štítky se načtou podle TagPids (ne z HTML poslaného prohlížečem)
            List<int> pids = BO.Code.Bas.ConvertString2ListInt(strSelectedValues).Where(p => p > 0).ToList();
            IEnumerable<BO.o51Tag> tags = new List<BO.o51Tag>();
            if (pids.Count > 0)
            {
                tags = _f.o51TagBL.GetTagging(pids).Tags ?? new List<BO.o51Tag>();
            }
            strSelectedValues = string.Join(",", tags.Select(p => p.pid));

            _sb.Append($"<input type='hidden' id='TagPids' name='TagPids' value='{Html(strSelectedValues)}' />");
            _sb.Append($"<input type='hidden' id='TagHtml' name='TagHtml' value='{Html(this.SelectedTagHtml)}' />");

            _sb.Append("<div id='mstTagging' class='mst dropdown'>");
            _sb.Append($"<button id='cmdTagging' type='button' class='btn btn-sm btn-light mst-add' data-bs-toggle='dropdown' data-bs-auto-close='outside' aria-expanded='false'><span class='material-icons-outlined-btn'>local_offer</span> {Html(this.ButtonText)}</button>");

            _sb.Append("<div id='divTagHtml' class='mst-chips'>");
            foreach (var c in tags)
            {
                _sb.Append(Chip(c.pid, c.o51Name, c.o53Name, c.o51IsColor ? c.o51BackColor : null, c.o51IsColor ? c.o51ForeColor : null));
            }
            _sb.Append("</div>");

            _sb.Append("<div class='dropdown-menu mst-menu p-0' aria-labelledby='cmdTagging'>");
            _sb.Append("<div class='mst-tools'><input type='search' class='form-control form-control-sm mst-search' placeholder='Hledat štítek…' autocomplete='off' /></div>");
            _sb.Append("<div class='mst-data'></div>");
            _sb.Append("<div class='mst-foot'><button type='button' class='btn btn-sm btn-light mst-clear'>Odebrat všechny štítky</button><span class='mst-count'></span></div>");
            _sb.Append("</div>");   //dropdown-menu
            _sb.Append("</div>");   //mst

            _sb.Append($"<script type='text/javascript'>mystitky_init({{entity:'{Js(this.Entity)}'}});</script>");

            output.Content.AppendHtml(_sb.ToString());
        }

        //stejný formát čipu generuje i JS (mystitky_init → chip)
        private static string Chip(int pid, string name, string group, string bg, string fg)
        {
            string style = "";
            if (!string.IsNullOrEmpty(bg))
            {
                style = $" style='background-color:{Html(bg)};color:{Html(fg)};border-color:{Html(bg)};'";
            }
            return $"<span class='mst-chip' data-pid='{pid}' title='{Html(group)}'{style}><span class='mst-name'>{Html(name)}</span><button type='button' class='mst-x' tabindex='-1' title='Odebrat' aria-label='Odebrat'>×</button></span>";
        }

        private static string Html(string s) => System.Net.WebUtility.HtmlEncode(s ?? "");
        private static string Js(string s) => System.Web.HttpUtility.JavaScriptStringEncode(s ?? "");
    }
}
