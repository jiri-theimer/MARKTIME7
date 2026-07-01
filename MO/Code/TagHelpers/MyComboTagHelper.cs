using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Text.Encodings.Web;

namespace MO.Code.TagHelpers
{
    /// <summary>
    /// Multi-column combo s client-side filtrem. Pro velké seznamy (1500+ projektů, atd.).
    /// Použití:
    ///   &lt;mycombo asp-for="p41ID" selected-text="@Model.SelectedProjectText"
    ///            items="@Model.ProjectComboItems"
    ///            placeholder="vyberte projekt"
    ///            event-after-change="loadTasksForProject" /&gt;
    /// </summary>
    [HtmlTargetElement("mycombo")]
    public class MyComboTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("selected-text")]
        public string SelectedText { get; set; }

        [HtmlAttributeName("items")]
        public IEnumerable<MO.Models.ComboItem> Items { get; set; }

        [HtmlAttributeName("placeholder")]
        public string Placeholder { get; set; }

        /// <summary>Název JS funkce, která se zavolá po změně hodnoty. Dostane argument = nový pid (string).</summary>
        [HtmlAttributeName("event-after-change")]
        public string EventAfterChange { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "mc-wrap relative");

            var name = For.Name;                                  // posílá se s formulářem
            var ctlId = "mc_" + name.Replace(".", "_").Replace("[", "_").Replace("]", "_");
            var modalId = ctlId + "_modal";
            var selectedValue = "0";
            if (For.Model != null) selectedValue = For.Model.ToString();
            var placeholder = Placeholder ?? "vyberte...";
            var encoder = HtmlEncoder.Default;

            // Pomocné: počet položek pro UI
            var items = (Items ?? Enumerable.Empty<MO.Models.ComboItem>()).ToList();

            var sb = new StringBuilder();

            // Hidden input - drží hodnotu pro POST
            sb.Append($"<input type=\"hidden\" id=\"{ctlId}\" name=\"{encoder.Encode(name)}\" value=\"{encoder.Encode(selectedValue)}\" />");

            // Trigger button - vypadá jako input
            var hasSelection = !string.IsNullOrEmpty(SelectedText) && selectedValue != "0";
            var triggerTextCls = hasSelection ? "" : "text-base-content/40";
            sb.Append($"<button type=\"button\" class=\"input input-bordered w-full flex items-center justify-between gap-1 pr-1\" onclick=\"mc_open('{ctlId}')\">");
            sb.Append($"  <span class=\"mc-trigger-text truncate text-left {triggerTextCls}\" id=\"{ctlId}_text\" data-placeholder=\"{encoder.Encode(placeholder)}\">");
            sb.Append(encoder.Encode(hasSelection ? SelectedText : placeholder));
            sb.Append("  </span>");
            sb.Append("  <span class=\"flex items-center gap-0 shrink-0\">");
            if (hasSelection)
            {
                sb.Append($"    <span class=\"material-icons-outlined text-base-content/40 hover:text-base-content cursor-pointer\" style=\"font-size:20px;\" onclick=\"event.stopPropagation();mc_clear('{ctlId}')\">close</span>");
            }
            sb.Append("    <span class=\"material-icons-outlined text-base-content/60\" style=\"font-size:20px;\">arrow_drop_down</span>");
            sb.Append("  </span>");
            sb.Append("</button>");

            // Modal - full-screen na mobilu. Standardní daisyUI vzor: <dialog> + showModal()/close().
            sb.Append($"<dialog id=\"{modalId}\" class=\"modal\">");
            sb.Append("  <div class=\"modal-box max-w-full w-full h-full max-h-full rounded-none p-0 flex flex-col\">");

            // Hlavička s X
            sb.Append("    <header class=\"navbar bg-base-100 border-b border-base-300 min-h-12 px-2\">");
            sb.Append($"      <div class=\"flex-1 font-semibold truncate\">{encoder.Encode(placeholder)}</div>");
            sb.Append($"      <button type=\"button\" class=\"btn btn-ghost btn-sm btn-circle\" onclick=\"mc_close('{ctlId}')\">");
            sb.Append("        <span class=\"material-icons-outlined\" style=\"font-size:22px;\">close</span>");
            sb.Append("      </button>");
            sb.Append("    </header>");

            // Search
            sb.Append("    <div class=\"p-2 border-b border-base-300\">");
            sb.Append($"      <input type=\"search\" class=\"input input-bordered w-full\"");
            sb.Append("              placeholder=\"hledat...\" autocomplete=\"off\" autocorrect=\"off\"");
            sb.Append("              autocapitalize=\"off\" spellcheck=\"false\" />");
            sb.Append("    </div>");

            // Seznam položek - s volitelným seskupováním (GroupBy)
            sb.Append("    <div class=\"mc-list flex-1 overflow-y-auto\">");

            var useGroups = items.Any(i => !string.IsNullOrEmpty(i.GroupBy));
            string lastGroup = null;

            foreach (var it in items)
            {
                // Skupinová hlavička - vloží se při každé změně GroupBy hodnoty
                if (useGroups)
                {
                    var grp = it.GroupBy ?? "";
                    if (grp != lastGroup)
                    {
                        lastGroup = grp;
                        sb.Append($"      <div class=\"mc-group-header\" data-group=\"{encoder.Encode(grp)}\"");
                        sb.Append("           style=\"position:sticky;top:0;z-index:1;\">");
                        if (!string.IsNullOrEmpty(grp))
                        {
                            sb.Append($"        <div class=\"px-3 py-1 text-xs font-bold uppercase tracking-wide\" style=\"background:#c0c0c0;color:#666;\">{encoder.Encode(grp)}</div>");
                        }
                        sb.Append("      </div>");
                    }
                }

                var haystack = ((it.Code ?? "") + " " + (it.Text ?? "") + " " + (it.Meta ?? "") + " " + (it.GroupBy ?? "")).ToLowerInvariant();
                var isSelected = it.Id.ToString() == selectedValue;
                var itemCls = "mc-item px-3 py-2 border-b border-base-200 active:bg-base-300"
                    + (isSelected ? " mc-item-selected bg-primary/10 border-l-4 border-l-primary" : " border-l-4 border-l-transparent")
                    + (useGroups && !string.IsNullOrEmpty(it.GroupBy) ? $" data-group=\"{encoder.Encode(it.GroupBy)}\"" : "");

                sb.Append($"      <div class=\"{itemCls}\"");
                sb.Append($"           data-id=\"{it.Id}\"");
                sb.Append($"           data-text=\"{encoder.Encode(it.Text ?? "")}\"");
                sb.Append($"           data-haystack=\"{encoder.Encode(haystack)}\"");
                if (useGroups && !string.IsNullOrEmpty(it.GroupBy))
                    sb.Append($"           data-group=\"{encoder.Encode(it.GroupBy)}\"");
                sb.Append("           >");
                sb.Append("        <div class=\"flex gap-2 items-baseline\">");
                if (!string.IsNullOrEmpty(it.Code))
                {
                    sb.Append($"          <span class=\"font-mono text-xs text-base-content/60 shrink-0\" style=\"min-width:6em;\">{encoder.Encode(it.Code)}</span>");
                }
                sb.Append($"          <span class=\"font-medium truncate\">{encoder.Encode(it.Text ?? "")}</span>");
                sb.Append("        </div>");
                if (!string.IsNullOrEmpty(it.Meta))
                {
                    sb.Append($"        <div class=\"text-xs text-base-content/60 pl-[calc(6em+0.5rem)] truncate\">{encoder.Encode(it.Meta)}</div>");
                }
                sb.Append("      </div>");
            }
            sb.Append($"      <div class=\"mc-empty hidden p-6 text-center text-base-content/50\">Žádné výsledky.</div>");
            sb.Append("    </div>");

            // Footer s počtem
            sb.Append("    <footer class=\"text-xs text-base-content/50 px-3 py-1 border-t border-base-300 text-right\">");
            sb.Append($"      <span class=\"mc-count\">{items.Count}</span> položek");
            sb.Append("    </footer>");

            sb.Append("  </div>");
            sb.Append("</dialog>");

            // Wiring event-after-change
            if (!string.IsNullOrEmpty(EventAfterChange))
            {
                sb.Append($"<script>document.getElementById('{ctlId}').addEventListener('change', function(){{ {EventAfterChange}(this.value); }});</script>");
            }

            output.Content.AppendHtml(sb.ToString());
        }
    }
}