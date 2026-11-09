using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;


namespace UI.Views.Shared.TagHelpers
{
    /// <summary>
    /// &lt;mylabel text="…" tooltip="…" explicit-for="…" bez-dvojtecky="true" class="…"&gt;
    /// Vykreslí &lt;label&gt;; všechny ostatní atributy (class, style, for, id, title…) zůstávají.
    /// text = prostý text (escapuje se), tooltip = HTML (záměrně: &lt;hr&gt;, ikony…), zobrazí se při najetí myší nebo fokusu (Tab / dotyk).
    /// </summary>
    [HtmlTargetElement("mylabel")]
    public class myLabelTagHelper : TagHelper
    {
        private readonly HtmlEncoder _enc;
        public myLabelTagHelper(HtmlEncoder enc)
        {
            _enc = enc;
        }

        [HtmlAttributeName("text")]
        public string LabelText { get; set; }

        /// <summary>HTML nápovědy (není escapované – zadává vývojář, ne uživatel).</summary>
        [HtmlAttributeName("tooltip")]
        public string Tooltip { get; set; }

        [HtmlAttributeName("explicit-for")]
        public string ExplicitFor { get; set; }

        [HtmlAttributeName("bez-dvojtecky")]
        public bool IsBezDvojtecky { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "label";
            output.TagMode = TagMode.StartTagAndEndTag;
            if (!string.IsNullOrEmpty(this.ExplicitFor))
            {
                output.Attributes.SetAttribute("for", this.ExplicitFor);
            }

            string text = this.LabelText ?? "";
            if (!this.IsBezDvojtecky && text.Length > 0 && !text.TrimEnd().EndsWith(":"))
            {
                text += ":";
            }

            if (string.IsNullOrEmpty(this.Tooltip))
            {
                output.Content.SetContent(text);    //SetContent = escapovaný text
                return;
            }

            //tabindex: nápověda jde zobrazit i klávesnicí (Tab) a klepnutím na dotykovém zařízení (CSS :focus)
            output.Content.SetHtmlContent($"<span class='mylabelhelp' tabindex='0'>{_enc.Encode(text)}<span class='mylabelhelptext' role='tooltip' style='color:black'>{this.Tooltip}</span></span>");
        }
    }
}
