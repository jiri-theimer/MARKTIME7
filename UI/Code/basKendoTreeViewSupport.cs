using BO.Sys;
using System.Text.Json;
using UI.Models;

namespace UI.Code
{
    public static class basKendoTreeViewSupport
    {
        public static string Get_Arrow_LastState(BL.Factory f,string strParamKey,string strDefVal= "expanded")
        {
            if (f.CBL.LoadUserParam(strParamKey, strDefVal) == "expanded")
            {
                return "arrow_drop_down";
            }

            return "arrow_right";
        }
        public static string Get_UL_LastState(BL.Factory f, string strParamKey, string strDefVal = "expanded")
        {
            if (f.CBL.LoadUserParam(strParamKey, strDefVal) == "expanded")
            {
                return "block";
            }

            return "none";
        }

        public static string Transform_To_KendoJson(List<myTreeNode> TreeDataSource)
        {
            var lisKendo = new List<kendoTreeItem>();
           
            foreach (var rec in TreeDataSource.Where(p => p.TreeLevel == 0))   //top položky stromu
            {

                lisKendo.Add(createki(rec));
            }
            int intMaxLevel = TreeDataSource.Max(p => p.TreeLevel);

            for (int intLevel = 1; intLevel <= intMaxLevel; intLevel++)   //vnořené položky stromu
            {
                foreach (var rec in TreeDataSource.Where(p => p.TreeLevel == intLevel))
                {

                    var parentki = findki(lisKendo,rec.ParentPid.ToString() + "-" + (rec.TreeLevel - 1).ToString()); //najít podle unikátního id

                    if (parentki.items == null)
                    {
                        parentki.items = new List<kendoTreeItem>();
                    }
                    parentki.items.Add(createki(rec));



                }
            }

            return GetJson(lisKendo);
            


        }

        public static string GetJson(List<kendoTreeItem> lisKendo)
        {
            return JsonSerializer.Serialize(lisKendo, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }

        private static kendoTreeItem createki(myTreeNode rec)
        {
            var c = new kendoTreeItem()
            {
                text = rec.Text,
                id =$"{rec.Pid}-{rec.TreeLevel}",   //musí být unikátní                
                recordid = rec.Pid.ToString(),   //id uchovávající pid záznamu
                parentid =$"{rec.ParentPid}-{(rec.TreeLevel - 1)}", //unikátní pid záznamu ve stromu
                imageUrl = rec.ImgUrl,
                prefix = rec.Prefix,
                cssclass = rec.CssClass,
                textocas = rec.TextOcas,
                active=rec.active
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
        private static kendoTreeItem findki(List<kendoTreeItem> lisKendo,string strFindId)
        {
            foreach (var c in lisKendo)  //projekt top úroveň stromu
            {
                if (c.id == strFindId)
                {
                    return c;
                }
                var foundki = handle_recur_findki(strFindId, c.items);   //zkusit najít v podřízených úrovních
                if (foundki != null)
                {
                    return foundki;
                }
            }

            return null;
        }
        private static kendoTreeItem handle_recur_findki(string find_unique_id, List<kendoTreeItem> nodes)
        {
            if (nodes == null)
            {
                return null;
            }
            foreach (var c in nodes)
            {
                if (c.id == find_unique_id)
                {
                    return c;
                }
                if (c.items != null)
                {
                    var cUnder = handle_recur_findki(find_unique_id, c.items);
                    if (cUnder != null)
                    {
                        return cUnder;
                    }
                }
            }
            return null;
        }



        
    }
}
