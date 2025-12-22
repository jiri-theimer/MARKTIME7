using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("myicon")]
    public class myIconTagHelper : TagHelper
    {
        [HtmlAttributeName("symbol")]
        public string Symbol { get; set; }

        [HtmlAttributeName("perc")]
        public int Percentage { get; set; }

        [HtmlAttributeName("cssclass")]
        public string CssClass { get; set; }
        [HtmlAttributeName("color")]
        public string ForeColor { get; set; }
        
        [HtmlAttributeName("prefix")]
        public string Prefix { get; set; }
        



        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (this.Symbol == "extension")
            {
                output.Content.AppendHtml("<span class='material-icons' style='font-size:100%;color:gray;'>extension</span>");
                return;
            }
            if (!string.IsNullOrEmpty(this.Symbol) && this.Symbol.Length == 3)
            {
                this.Prefix = this.Symbol;
                this.Symbol = null;
            }
           
            if (string.IsNullOrEmpty(this.Symbol))
            {
                switch (this.Prefix)
                {
                    case "p28":
                        this.Symbol = "contacts";
                        break;
                    case "p91":
                        this.Symbol = "receipt_long";
                        break;
                    case "p31":
                        this.Symbol = "more_time";
                        break;
                    case "p56":
                        this.Symbol = "task";
                        break;
                    case "p58":
                        this.Symbol = "published_with_changes";
                        break;
                    case "p41":
                    case "le5":
                    case "le4":
                    case "le3":
                    case "le2":
                    case "le1":
                        this.Symbol = "work_outline";
                        break;
                    case "j02":
                        this.Symbol = "face";
                        break;
                    case "o22":
                        this.Symbol = "today";
                        break;
                    case "o23":
                        this.Symbol = "file_present";
                        break;
                    case "p90":
                        this.Symbol = "receipt";
                        break;
                    case "o53":
                        this.Symbol = "local_offer"; break;
                    case "o38":
                        this.Symbol = "location_city"; break;
                    case "o51":
                        this.Symbol = "local_offer"; break;
                    case "p51":
                        this.Symbol = "price_change"; break;
                    case "j11":
                        this.Symbol = "groups"; break;
                    case "p11":
                        this.Symbol = "hourglass_top";
                        break;
                    case "b05":
                        this.Symbol = "speaker_notes";
                        break;
                    case "p84":
                        this.Symbol = "gavel";
                        break;
                    default:
                        this.Symbol = "dvr";
                        break;
                }
            }
            

            if (this.CssClass == null)
            {
                this.CssClass = "material-icons-outlined-btn";
            }
            if (this.ForeColor == null && this.Percentage==0)
            {
                output.Content.AppendHtml($"<span class='{this.CssClass}'>{this.Symbol}</span>");
            }
            else
            {
                string strStyle = null;
                if (this.Percentage > 0)
                {
                    strStyle = $"font-size:{this.Percentage}%;";
                }
                                
                if (this.ForeColor != null)
                {
                    strStyle += "color: " + this.ForeColor;
                }
                output.Content.AppendHtml($"<span class='{this.CssClass}' style='{strStyle}'>{this.Symbol}</span>");
            }
            
            

            
        }
    }
}
