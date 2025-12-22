
using DocumentFormat.OpenXml.Drawing;
using UI.Models;

namespace UI.Menu
{
    public abstract class BaseContextMenu
    {
        protected BL.Factory _f;
        protected int pid { get; set; }
        private List<MenuItem> _lis;

        public string device { get; set; }

        public BaseContextMenu(BL.Factory f, int pid)
        {
            _lis = new List<MenuItem>();
            _f = f;
        }

        public List<MenuItem> GetItems()
        {
            return _lis;
        }

        public MenuItem AMI_NoDisposition()
        {
            return AMI(_f.tra("Menu není k dispozici"), null);
        }

        public MenuItem AMI(string strName, string strUrl, string icon = null, string strParentID = null, string strID = null, string strTarget = null, string strAfterName = null, bool bolTranslate = true)
        {
            var c = new MenuItem() { Url = strUrl, Target = strTarget, ID = strID, ParentID = strParentID, Icon = icon,Name=strName };
            if (bolTranslate)
            {
                c.Name = _f.tra(strName);
            }
            

            if (strAfterName != null)
            {
                c.Name = $"{c.Name} <span class='badge-light'>{BO.Code.Bas.OM2(strAfterName, 20)}</span>";
            }
            _lis.Add(c);
            return c;
        }

        public MenuItem AMI_RecPage(string strName, string prefix, int pid, string parentid = null, string icon = null, string strAfterName = null)
        {
            
            strName = BO.Code.Bas.OM2(strName,36);
            if (icon == null)
            {
                icon = get_menu_icon(prefix);
            }
            if (device == "Phone")
            {
                return AMI(strName, $"/Record/RecPageMobile?prefix={prefix}&pid={pid}", icon, parentid, null, "_top",null,false);
            }
            else
            {
                return AMI(strName, $"/Record/RecPage?prefix={prefix}&pid={pid}", icon, parentid, null, "_top", strAfterName,false);
            }

        }
        public MenuItem AMI_RecGrid(string strName, string prefix, int pid, int rez = 0,bool bolTranslate=true)
        {
            string s = $"/TheGrid/MasterView?prefix={prefix}&go2pid={pid}";
            if (!_f.CBL.LoadUserParamBool($"grid-{prefix}-show11", true))
            {
                s = $"/TheGrid/FlatView?prefix={prefix}&go2pid={pid}";
            }

            if (this.device == "Phone")
            {
                s = $"/TheGrid/MobileView?prefix={prefix}&go2pid={pid}";
            }

            if (rez > 0) s += "&rez=" + rez.ToString();

            return AMI(strName, s, "grid_view", null, null, "_top",null,bolTranslate);
        }

        public void AMI_RowColor(string prefix, int pid)
        {
            if (_f.CurrentUser.IsMobileDisplay())
            {
                return;
            }
            AMI("Barva záznamu", null, "palette", null, "rowcolor");

            AMI("Zelená", $"javascript:_rowcolor('{prefix}','{pid}',1)", "filter_1", "rowcolor");
            AMI("Žlutá", $"javascript:_rowcolor('{prefix}','{pid}',2)", "filter_2", "rowcolor");
            AMI("Fialová", $"javascript:_rowcolor('{prefix}','{pid}',3)", "filter_3", "rowcolor");
            AMI("Modrá", $"javascript:_rowcolor('{prefix}','{pid}',4)", "filter_4", "rowcolor");
            AMI("Stříbrná", $"javascript:_rowcolor('{prefix}','{pid}',5)", "filter_5", "rowcolor");
            AMI("Vyčistit barvu", $"javascript:_rowcolor_clear('{prefix}','{pid}')", "mop", "rowcolor");


        }

        public void AMI_Clone(string prefix, int pid, bool onlycopyurl = false)
        {
            string strParentID = null;
            if (!onlycopyurl)
            {
                strParentID = "clone";
                AMI("Kopírovat", $"javascript:_clone('{prefix}',{pid})", "content_copy", null, "clone");
                AMI("Zkopírovat do nového záznamu", $"javascript:_clone('{prefix}',{pid})", "content_copy", "clone");
                AMI("Zkopírovat do schránky", $"javascript:tg_export('clipboard_tabs','selected')", "file_copy", "clone");
            }


            if (prefix == "p41" || prefix == "p91" || prefix == "p56" || prefix == "p28" || prefix == "o23" || prefix == "o22" || prefix == "j02")
            {
                AMI($"Zkopírovat url stránky záznamu", $"javascript:_copy_recpage_to_clipboard('{prefix}',{pid})", "link", strParentID);
            }


        }
        public MenuItem AMI_Report(string prefix, int pid, string parentmenuid = null)
        {
            if (_f.x31ReportBL.GetList(new BO.myQueryX31() { entity = prefix, MyRecordsDisponible = true }).Count() > 0)
            {
                return AMI("Tisková sestava", $"javascript: _window_open('/ReportsClient/ReportContext?pid={pid}&prefix={prefix}')", "print", parentmenuid, "ami_report");





            }
            return null;

        }
        public void AMI_SendMail(string prefix, int pid, string parentmenuid = "more",string strDefJ61Name=null)
        {

            //if (lisJ61.Count() > 0 && parentmenuid == null)
            //{
            //    DIV(null, parentmenuid);
            //}

           

            AMI($"Odeslat e-mail", $"javascript: _window_open('/Mail/SendMail?record_entity={prefix}&record_pid={pid}',2)", "alternate_email", parentmenuid,"SendMail",null,strDefJ61Name);
            
           
            if (strDefJ61Name==null && (prefix == "p91" || prefix == "p90" || prefix=="p84"))
            {
                var lisJ61 = _f.j61TextTemplateBL.GetList(new BO.myQueryJ61() { entity = prefix, MyRecordsDisponible = true });
                foreach (var c in lisJ61)
                {
                    AMI("E-mail šablona", $"javascript: _window_open('/Mail/SendMail?record_entity={prefix}&record_pid={pid}&j61id={c.pid}',2)", "alternate_email", "SendMail", null, null, c.j61Name);
                }
            }
            


        }

        public void AMI_Workflow(string prefix, int pid, int b02id, int b01id, string parentmenuid = null)
        {

            BO.b01WorkflowTemplate recB01 = null;
            if (b01id > 0)
            {
                recB01 = _f.b01WorkflowTemplateBL.Load(b01id);
            }
            if (recB01 == null && b02id > 0)
            {
                var recB02 = _f.b02WorkflowStatusBL.Load(b02id);
                recB01 = _f.b01WorkflowTemplateBL.Load(recB02.b01ID);
            }
            if (b02id > 0 || (recB01 != null && recB01.b01PrincipleFlag == BO.b01PrincipleFlagEnum.StatusOnly))
            {
                AMI("Posunout workflow stav", null, "task_alt", null, "wrk");


                if (recB01.b01PrincipleFlag == BO.b01PrincipleFlagEnum.Step)    //stavový mechanismus
                {
                    var lisB06 = _f.b06WorkflowStepBL.GetList(new BO.myQuery("b06")).Where(p => p.b02ID == b02id && p.b06AutoRunFlag == BO.b06AutoRunFlagEnum.UzivatelskyKrok).ToList();

                    foreach (var c in lisB06)
                    {
                        if (_f.WorkflowBL.IsStepAvailable4Me(c, pid, prefix))
                        {
                            //var mi = AMI(c.NameWithTargetStatus, $"javascript: _window_open('/workflow_dialog/Index?record_prefix={prefix}&record_pid={pid}&run_b06id={c.pid}')", "task_alt", "wrk");
                            var mi = AMI(c.NameWithTargetStatus, $"javascript: _window_open('/workflow_dialog/Index?record_prefix={prefix}&record_pid={pid}&b06id={c.pid}')", "task_alt", "wrk");
                            mi.Color = c.b02Color;
                            c.UserInsert = "ok1";

                        }
                    }


                }
                else
                {   //není stavový mechanismus
                    var lisB02 = _f.b02WorkflowStatusBL.GetList(new BO.myQuery("b02")).Where(p => p.b01ID == recB01.pid && p.b02AutoRunFlag == BO.b02AutoRunFlagEnum.Standard).OrderBy(p => p.b02Ordinary);
                    foreach (var c in lisB02)
                    {
                        if (c.pid == b02id)
                        {
                            var mi = AMI(c.b02Name, null, "check_box", "wrk");
                            mi.Color = c.b02Color;
                        }
                        else
                        {
                            var mi = AMI(c.b02Name, $"javascript: _window_open('/workflow_dialog/Index?record_prefix={prefix}&record_pid={pid}&run_b02id={c.pid}')", "done", "wrk");
                            mi.Color = c.b02Color;
                            c.UserInsert = "ok1";
                        }
                    }

                }

                AMI("Doplnit poznámku", $"javascript: _window_open('/workflow_dialog/Index?record_prefix={prefix}&record_pid={pid}')", "speaker_notes", "wrk");


            }
            else
            {
                AMI("Doplnit poznámku", $"javascript: _window_open('/workflow_dialog/Index?record_prefix={prefix}&record_pid={pid}')", "speaker_notes", parentmenuid);
            }






        }
        public MenuItem AMI_Doc(string prefix, int pid)
        {
            MenuItem ret = null;
            var lis = _f.o18DocTypeBL.GetList_DocumentCreate(prefix);
            if (lis.Count() > 0)
            {
                ret = AMI("Nový dokument", null, null, null, "docnew");
                foreach (var c in lis)
                {
                    AMI(c.o18Name, $"javascript: _window_open('/o23/Record?prefix={prefix}&pid=0&recpid={pid}&o18id={c.pid}')", "file_present", "docnew");
                }
                return null;
            }
            return ret;

            //return AMI("Nový záznam dokumentu", $"javascript: _window_open('/o23/Record?prefix={prefix}&pid=0&recpid={pid}')", "file_present", parentmenuid);
        }

        public MenuItem AMI_ChangeLog(string prefix, int pid, string parentmenuid = "more")
        {
            return AMI("CHANGE-LOG", $"javascript: _window_open('/ChangeLog/{prefix}?pid={pid}',2)", "timeline", parentmenuid);
        }

        public void AMI_Vykazat(BO.p41Project rec, int p56id = 0, int p91id = 0)
        {
            if (rec.isclosed || !_f.CurrentUser.j04IsModule_p31)
            {
                return; //není oprávnění a podmínky pro vykazování úkonů v projektu
            }
            var lisP34 = _f.p34ActivityGroupBL.GetList_WorksheetEntryIn_OneProject(rec, _f.CurrentUser.pid);
            if (lisP34.Count() > 0)
            {
                if (p91id > 0)
                {
                    AMI("Přidat do vyúčtování", null, "more_time", null, "p31create");
                }
                else
                {
                    string strBaseUrl = $"javascript: _window_open('/p31/Record?pid=0',3)";
                    if (rec != null && rec.pid > 0)
                    {
                        if (p56id > 0)
                        {
                            strBaseUrl = $"javascript: _window_open('/p31/Record?newrec_prefix=p56&newrec_pid={p56id}',3)";
                        }
                        else
                        {
                            strBaseUrl = $"javascript: _window_open('/p31/Record?newrec_prefix=p41&newrec_pid={rec.pid}',3)";
                        }
                    }
                    AMI("Vykázat úkon", strBaseUrl, "more_time", null, "p31create");
                }

                foreach (BO.p34ActivityGroup c in lisP34.Take(10))
                {
                    string strIcon = "more_time";
                    switch (c.p33ID)
                    {
                        case BO.p33IdENUM.Kusovnik:
                            strIcon = "swipe"; break;
                        case BO.p33IdENUM.PenizeBezDPH:
                        case BO.p33IdENUM.PenizeVcDPHRozpisu:
                            strIcon = "paid"; break;
                    }
                    string url = $"javascript: _window_open('/p31/Record?newrec_prefix=p41&newrec_pid={rec.pid}&p34id={c.pid}',3)";
                    if (p56id > 0)
                    {
                        url = $"javascript: _window_open('/p31/Record?newrec_prefix=p56&newrec_pid={p56id}&p34id={c.pid}',3)";
                    }
                    if (p91id > 0)
                    {
                        url = $"javascript: _window_open('/p31/Record?newrec_prefix=p41&newrec_pid={rec.pid}&p34id={c.pid}&p91id={p91id}',3)";
                    }
                    AMI(c.p34Name, url, strIcon, "p31create");
                }

                if (p91id > 0)
                {
                    DIV(null, "p31create");
                    var lisP31 = _f.p31WorksheetBL.GetList(new BO.myQueryP31() { p91id = p91id });
                    if (lisP31.Count() > 0)
                    {
                        string strP41IDs = string.Join(",", lisP31.Select(p => p.p41ID).Distinct());
                        //lisP31 = _f.p31WorksheetBL.GetList(new BO.myQueryP31() { p41ids = lisP31.Select(p => p.p41ID).Distinct().ToList(),p31statequery=3 });

                        string strURL = $"javascript: _window_open('/p31approveinput/Index?prefix=p41&pids={strP41IDs}&p91id={p91id}')";
                        AMI("Přidat nevyúčtované úkony", strURL, "approval", "p31create");
                    }

                }
            }
        }
        public void DIV(string strName = null, string strParentID = null)
        {
            _lis.Add(new MenuItem() { IsDivider = true, Name = BO.Code.Bas.OM2(strName, 30), ParentID = strParentID });
        }
        public void DIV_TRANS(string strName = null)
        {
            _lis.Add(new MenuItem() { IsDivider = true, Name = BO.Code.Bas.OM2(_f.tra(strName), 30) });
        }
        public void HEADER(string strName)
        {
            _lis.Add(new MenuItem() { IsHeader = true, Name = BO.Code.Bas.OM2(strName, 50) });
        }


        public string get_menu_icon(string prefix)
        {
            switch (prefix)
            {

                case "p91":
                    return "receipt_long";
                case "j02":
                    return "face";
                case "p41":
                    return "work_outline";
                case "le1":
                case "le2":
                case "le3":
                case "le4":
                case "le5":
                    return "work_outline";
                case "p28":
                    return "contacts";
                case "p56":
                    return "task";
                
                case "o23":
                    return "file_present";
                default:
                    return "maps_home_work";
            }
        }

        public string CompleteText_SchvalitVyuctovat(DateTime? d1, DateTime? d2, int intWipCount, int intApprovedCount)
        {
            var s = $" ({intWipCount}x + {intApprovedCount}x)";
            if (d1.HasValue && d1.Value.Year > 1900)
            {
                s = $" ({intWipCount}x + {intApprovedCount}x)<br><span style='margin-left:30px;'>{d1.Value.ToString("dd.MM.yyyy")} - {d2.Value.ToString("dd.MM.yyyy")}</span>";
            }
            else
            {
                s = $"{intWipCount}x + {intApprovedCount}x";
            }

            return s;
        }
    }
}
