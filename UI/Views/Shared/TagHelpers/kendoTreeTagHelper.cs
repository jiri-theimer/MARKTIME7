using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

using System.Text.Json;

using UI.Models;
using System.Drawing.Text;
using DocumentFormat.OpenXml.Office2013.PowerPoint;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("kendotree")]
    public class kendoTreeTagHelper : TagHelper
    {
        private System.Text.StringBuilder _sb;

        [HtmlAttributeName("tree-datasource")]
        public List<myTreeNode> TreeDataSource { get; set; }            //pokud je zadáno, tak se toto vnitřně převede na KendoDataSource

        [HtmlAttributeName("kendo-datasource")]
        public List<kendoTreeItem> KendoDataSource { get; set; }        //primární zdroj pro treeview

        [HtmlAttributeName("clientid")]
        public string ClientId { get; set; } = "tree1";


        private kendoTreeItem createki(myTreeNode rec)
        {
            var c = new kendoTreeItem()
            {
                text = rec.Text,
                id = rec.Pid.ToString()+"-"+rec.TreeLevel.ToString(),   //musí být unikátní                
                recordid= rec.Pid.ToString(),   //id uchovávající pid záznamu
                parentid = rec.ParentPid.ToString() + "-" + (rec.TreeLevel - 1).ToString(), //unikátní pid záznamu ve stromu
                imageUrl = rec.ImgUrl,                
                prefix = rec.Prefix,               
                cssclass=rec.CssClass,
                textocas=rec.TextOcas
            };
            
            if (rec.Expanded)
            {
                c.expanded = true;
            }
            if (rec.Checked)
            {
                c.@checked = true;
            }
            return c;
        }
        private kendoTreeItem findki(string strFindId)
        {           
            foreach(var c in this.KendoDataSource)  //projekt top úroveň stromu
            {
                if (c.id == strFindId)
                {
                    return c;
                }
                var foundki= handle_recur_findki(strFindId, c.items);   //zkusit najít v podřízených úrovních
                if (foundki != null)
                {
                    return foundki;
                }
            }

            return null;
        }
        private kendoTreeItem handle_recur_findki(string find_unique_id,List<kendoTreeItem> nodes)
        {
            if (nodes == null)
            {
                return null;
            }
            foreach(var c in nodes)
            {
                if (c.id == find_unique_id)
                {
                    return c;
                }
                if (c.items != null)
                {                    
                    var cUnder= handle_recur_findki(find_unique_id, c.items);
                    if (cUnder != null)
                    {
                        return cUnder;
                    }
                }
            }
            return null;
        }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            _sb = new System.Text.StringBuilder();

            if (this.TreeDataSource != null)
            {
                //převést na KendoDataSource rekurzně
                this.KendoDataSource = new List<kendoTreeItem>();
                
                foreach (var rec in this.TreeDataSource.Where(p=>p.TreeLevel<=1))   //top položky stromu
                {                   
                    
                    this.KendoDataSource.Add(createki(rec));
                }
              
                for (int intLevel = 2; intLevel <= 9; intLevel++)   //vnořené položky stromu
                {
                    foreach (var rec in this.TreeDataSource.Where(p => p.TreeLevel == intLevel))
                    {
                        
                        var parentki = findki(rec.ParentPid.ToString() + "-" + (rec.TreeLevel - 1).ToString()); //najít podle unikátního id

                        if (parentki.items == null)
                        {
                            parentki.items = new List<kendoTreeItem>();
                        }
                        parentki.items.Add(createki(rec));



                    }
                }

                
            }

            

            string strJsonDataSource = JsonSerializer.Serialize(this.KendoDataSource, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition= System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull                
            });

            sb(string.Format("<div id='{0}'></div>", this.ClientId));

            sbl("");
            sbl("<script type='text/javascript'>");
            sbl("");

            sb(string.Format("var ds{0}=", this.ClientId));
            sb(strJsonDataSource);
            sb(";");


            sbl("");
            sbl("</script>");


            output.Content.AppendHtml(_sb.ToString());
        }



        private void sb(string s)
        {
            _sb.Append(s);
        }
        private void sbl(string s)
        {
            _sb.AppendLine(s);
        }
    }
}
