
using UI.Models;

namespace UI.Menu
{
    public class TheMenuSupport
    {
        private BL.Factory _f { get; set; }
        public bool IsMobile { get; set; }
        public TheMenuSupport(BL.Factory f)
        {
            _f = f;

            
        }

        

        public string GetMainmenuEntityUrl(string prefix)
        {
            if (prefix=="p31" || prefix=="p49" || (_f.CurrentUser.j02Ping_InnerHeight<600 && _f.CurrentUser.j02Ping_InnerHeight>0)) return $"/TheGrid/FlatView?prefix={prefix}";

            if (BL.Code.UserUI.IsShowLeftPanel(prefix, _f.CurrentUser.j02UIBitStream))
            {
                return $"/Record/RecPage?prefix={prefix}";
            }

            
            return getGridUrl(prefix);
        }
       public string getGridUrl(string prefix)
        {            
            if (BL.Code.UserUI.IsShowFlatView(prefix, _f.CurrentUser.j02UIBitStream))
            {
                return $"/TheGrid/FlatView?prefix={prefix}";
            }
            else
            {
                return $"/TheGrid/MasterView?prefix={prefix}";
            }
            
        }
        

        //public List<MenuItemMyLink> getMyMenuLinks()        //vrací seznam vlastnoručně pridaných odkazů do hlavního menu ze strany uživatele
        //{
        //    return null;
        //    if (_f.CurrentUser.j02MyMenuLinks == null)
        //    {
        //        return null;
        //    }
        //    var ret = new List<MenuItemMyLink>();
        //    List<string> lis = BO.Code.Bas.ConvertString2List(_f.CurrentUser.j02MyMenuLinks, "**");int x = 1;
        //    foreach (var c in lis)
        //    {
        //        var arr = c.Split("|");
        //        ret.Add(new MenuItemMyLink() { Name = arr[0], Url = arr[1], Target = arr[2], ID = arr[3],Ordinary=x,TempGuid=BO.Code.Bas.GetGuid() });
        //        x += 1;
        //    }

        //    return ret;
        //}

        public string TryEstimateMyLinkID(string newurl)    //vrací ID html elementu A pro vlastní odkaz v hlavním menu
        {
            if (string.IsNullOrEmpty(newurl)) return null;
            if (newurl.ToLower().Contains("p31calendar")) return "p31calendar_mylink";
            if (newurl.ToLower().Contains("p31dayline")) return "p31dayline_mylink";
            if (newurl.ToLower().Contains("widgets")) return "widgets_mylink";


            return null;
            

           
        }

        private MenuItem GRD(string prefix,bool bolIsCanBeGrid11,string icon=null)
        {
            var c = new MenuItem() { ID = "cmd" + prefix,IsCanBeGrid11= bolIsCanBeGrid11,Icon=icon };
           
            switch (_f.CurrentUser.j02LangIndex)
            {
                case 1:
                    c.Name = _f.EProvider.ByPrefix(prefix).TranslateLang1; break;
                case 2:
                    c.Name = _f.EProvider.ByPrefix(prefix).TranslateLang2; break;
                case 3:
                    c.Name = _f.EProvider.ByPrefix(prefix).TranslateLang3; break;
                default:
                    c.Name = _f.EProvider.ByPrefix(prefix).AliasPlural;break;
            }
            switch (prefix)
            {
                case "j02":
                    c.Name = _f.tra("Uživatelé");
                    c.Url = "/TheGrid/FlatView?prefix=" + prefix;                   
                    break;
                case "p41":
                    c.Name = _f.getP07Level(5,false);
                    c.Url = "/TheGrid/FlatView?prefix="+prefix;
                    break;
                case "le5":
                case "le4":
                case "le3":
                case "le2":
                case "le1":
                    c.Name = _f.getP07Level(Convert.ToInt32(prefix.Substring(2,1)),false);
                    c.Url = "/TheGrid/FlatView?prefix="+prefix;
                    break;                
                case "p28":
                    c.Name = _f.tra("Kontakty");
                    c.Url = "/TheGrid/FlatView?prefix=" + prefix;
                    break;
                case "p91":                
                    c.Url = "/TheGrid/FlatView?prefix=" + prefix;
                    break;
                default:
                    c.Url = "/TheGrid/FlatView?prefix=" + prefix;
                    break;
            }

            
            
            return c;
            
        }


        

        //vrátí HTML pro kontextové menu
        public string FlushResult_UL(List<MenuItem> menuitems, bool bolSupportIcons, bool bolRenderUlContainer, string source = null,string device=null)
        {
            var sb = new System.Text.StringBuilder();
            if (menuitems.Count()>19 && device == "Phone")
            {
                sb.Append($"<button class='btn cog' style='width:90%;text-align:center;' type='button'><span class='material-icons-outlined-btn'>close</span>Zavřít</button>");
            }
            if (bolRenderUlContainer)
            {
                sb.AppendLine("<ul class='cm_menu' style='border:0px;'>");
            }
            var qry = menuitems.Where(p => p.ParentID == null);
            if (source == "recpage")
            {
                qry = qry.Where(p => p.IsHeader == false);
            }
            foreach (var c in qry)
            {
                if (c.IsDivider == true)
                {
                    if (c.Name == null)
                    {
                        sb.Append("<li><hr></li>");  //divider
                    }
                    else
                    {
                        sb.Append("<div class='hr-text'>" + c.Name + "</div>");
                    }

                }
                else
                {
                    if (c.IsHeader)
                    {
                        sb.Append("<div style='color:silver;font-style: italic;'>" + c.Name + "</div>");
                    }
                    else
                    {
                        string strStyle = null;
                        string strImg = "<span style='margin-left:10px;'></span>";
                        string strCssClass = "dropdown-item";
                        if (c.CssClass != null)
                        {
                            strCssClass = c.CssClass;
                        }
                        if (bolSupportIcons)
                        {
                            strImg = "<span style='width:30px;'></span>";
                            if (c.Icon != null)
                            {
                                if (c.Icon.Length == 1)
                                {
                                    strImg = $"<span style='margin-left:10px;margin-right:5px;font-size:150%;color:royalblue;'>{c.Icon}</span>";     //1 unicode character
                                }
                                else
                                {    
                                    if (c.Icon.Contains("/images"))
                                    {
                                        strImg = $"<span style='width:30px'><img src='{c.Icon}'/></span>";   //images
                                    }
                                    else
                                    {
                                        strImg = $"<span class='material-icons-outlined-btn' style='width:30px'>{c.Icon}</span>";   //google icon
                                    }
                                    
                                }

                            }
                            
                        }

                        if (c.IsActive == true)
                        {
                            strStyle = " style='background-color: #ADD8E6;' id='menu_active_item'";
                        }
                        bool bolHasChilds = false;
                        int intCount = menuitems.Where(p => p.ParentID == c.ID).Count();
                        if (c.ID != null && intCount > 0)
                        {
                            bolHasChilds = true;
                                                       
                            if (!this.IsMobile)
                            {
                                
                                if (c.ID == "p41")
                                {
                                    //natvrdo menu Kontakt->Projekty klienta
                                    c.Name += "<span class='material-icons-outlined-btn' style='color:red;position:absolute;right:0px;bottom:10px;' >arrow_right</span>";
                                }
                                else
                                {
                                    c.Name += $" ({intCount})<span class='material-icons-outlined-btn' style='color:red;position:absolute;right:0px;bottom:10px;' >arrow_right</span>";
                                }
                            }


                        }
                        

                        if (c.Url == null)
                        {
                            if (bolHasChilds)
                            {
                                sb.Append($"<li{strStyle}><span class='material-icons-outlined-btn' style='width:30px;'>more_horiz</span>{c.Name}");
                            }
                            else
                            {
                                sb.Append($"<li{strStyle}><a>{c.Name}</a>");
                            }

                        }
                        else
                        {
                            if (c.Target != null) c.Target = $" target='{c.Target}'";
                            sb.Append($"<li{strStyle}><a class='{strCssClass} px-0' href=\"{c.Url}\"{c.Target}>{strImg}{c.Name}</a>");                            

                        }
                        if (bolHasChilds)
                        {
                            //podřízené nabídky -> druhá úroveň »
                            sb.Append("<ul class='cm_submenu'>");
                            foreach (var cc in menuitems.Where(p => p.ParentID == c.ID))
                            {
                                if (cc.IsDivider)
                                {
                                    sb.Append("<li><hr></li>");  //divider
                                }
                                else
                                {
                                    if (cc.Target != null) cc.Target = " target='" + cc.Target + "'";
                                    strImg = "<span style='margin-left:10px;'></span>";
                                    if (bolSupportIcons)
                                    {
                                        strImg = "<span style='width:30px;'></span>";
                                        if (cc.Icon != null)
                                        {
                                            if (cc.Icon.Length == 1)
                                            {
                                                strImg = $"<span class='cm_submenuicon' style='margin-left:10px;margin-right:5px;font-size:150%;'>{cc.Icon}</span>";     //1 unicode character
                                            }
                                            else
                                            {
                                                if (cc.Icon.Contains("/images"))
                                                {
                                                    strImg = $"<span class='material-icons-outlined-btn cm_submenuicon' style='width:30px'><img src='{cc.Icon}'/></span>";   //images
                                                }
                                                else
                                                {
                                                    strImg = $"<span class='material-icons-outlined-btn cm_submenuicon' style='width:30px;'>{cc.Icon}</span>";   //google icon
                                                }
                                                
                                            }

                                        }
                                    }
                                    if (cc.Url != null)
                                    {
                                        sb.Append($"<li><a class='dropdown-item' href=\"{cc.Url}\"{cc.Target}>{strImg}{cc.Name}</a></li>");
                                    }
                                    else
                                    {
                                        sb.Append($"<li><a class='dropdown-item'>{strImg}{cc.Name}</a></li>");
                                    }
                                    
                                                                        
                                    
                                }

                            }
                            sb.Append("</ul>");
                        }

                        sb.Append("</li>");
                    }

                }

            }

            if (bolRenderUlContainer)
            {
                sb.Append("</ul>");
            }
            if (device == "Phone")
            {
                sb.Append($"<hr><button class='btn cog' style='width:90%;text-align:center;' type='button'><span class='material-icons-outlined-btn'>close</span>Zavřít</button>");
            }
            return sb.ToString();
        }

    }
}
