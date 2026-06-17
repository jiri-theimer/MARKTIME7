using BL;
using Microsoft.AspNetCore.Mvc;
using UI.Models.batch;
using UI.Models.p31oper;

namespace UI.Controllers.batch
{
    public class batchdeleteController : BaseController
    {
        public IActionResult Index(string pids, string entity, int j72id, string source, string oper, string guid_pids)
        {
            if (source != null && source.Contains("/Admin") && !Factory.CurrentUser.IsAdmin)
            {
                return StopPage(true, "Chybí oprávnění administrátora.");
            }
            if (!string.IsNullOrEmpty(guid_pids))
            {
                pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            var v = new BatchDeleteViewModel() { pids = pids, j72id = j72id, entity = entity, oper = oper };
            v.prefix = BO.Code.Entity.GetPrefixDb(v.entity.Substring(0, 3));

            RefreshState(v);

            switch (v.prefix)
            {
                case "p41":
                case "p31":
                case "p28":
                case "p56":
                case "p91":
                case "p90":
                case "b05":
                case "o23":
                case "o22":
                    break;
                default:
                    if (!Factory.CurrentUser.IsAdmin)
                    {
                        return StopPage(true, "Pro tuto operaci musíte disponovat admin oprávněním.");
                    }
                    break;
            }

            return View(v);
        }

        private void RefreshState(BatchDeleteViewModel v)
        {
            var gridState = Factory.j72TheGridTemplateBL.LoadState(v.j72id, Factory.CurrentUser.pid);

            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() { j72id = v.j72id, is_enable_selecting = false, entity = gridState.j72Entity, myqueryinline = $"pids|list_int|{v.pids}", oncmclick = "", ondblclick = "" };
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load(gridState.j72Entity.Substring(0, 3), null, 0, $"pids|list_int|{v.pids}");


        }

        [HttpPost]
        public IActionResult Index(BatchDeleteViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {

                return View(v);
            }

            if (ModelState.IsValid)
            {
                int intErrs = 0; int intOks = 0;
                var arr = BO.Code.Bas.ConvertString2ListInt(v.pids);
                string strMessage = Factory.tra("Chybí vlastnické oprávnění k záznamu.");
                foreach (int pid in arr)
                {
                    bool b = true;
                    switch (BO.Code.Entity.GetPrefixDb(v.prefix))
                    {
                        case "p41":
                            var permp41 = Factory.p41ProjectBL.InhaleRecDisposition(pid);
                            if (!permp41.OwnerAccess)
                            {
                                AddMessageTranslated(strMessage);
                                b = false;
                            }
                            break;
                        case "p31":
                            var permp31 = Factory.p31WorksheetBL.InhaleRecDisposition(Factory.p31WorksheetBL.Load(pid));
                            if (!permp31.OwnerAccess)
                            {
                                AddMessageTranslated(strMessage);
                                b = false;
                            }
                            break;
                        case "p91":
                            var permp91 = Factory.p91InvoiceBL.InhaleRecDisposition(pid);
                            if (!permp91.OwnerAccess)
                            {
                                AddMessageTranslated(strMessage);
                                b = false;
                            }
                            break;
                        case "p56":
                            var permp56 = Factory.p56TaskBL.InhaleRecDisposition(pid);
                            if (!permp56.OwnerAccess)
                            {
                                AddMessageTranslated(strMessage);
                                b = false;
                            }
                            break;
                        case "p90":
                            var permp90 = Factory.p90ProformaBL.InhaleRecDisposition(pid);
                            if (!permp90.OwnerAccess)
                            {
                                AddMessageTranslated(strMessage);
                                b = false;
                            }
                            break;
                        case "p28":
                            var permp28 = Factory.p28ContactBL.InhaleRecDisposition(pid);
                            if (!permp28.OwnerAccess)
                            {
                                AddMessageTranslated(strMessage);
                                b = false;
                            }
                            break;
                        case "o23":
                            var permo23 = Factory.o23DocBL.InhaleRecDisposition(pid);
                            if (!permo23.OwnerAccess)
                            {
                                AddMessageTranslated(strMessage);
                                b = false;
                            }
                            break;
                        case "o22":
                            var permo22 = Factory.o22MilestoneBL.InhaleRecDisposition(pid);
                            if (!permo22.OwnerAccess)
                            {
                                AddMessageTranslated(strMessage);
                                b = false;
                            }
                            break;
                        case "b05":
                            var rec = Factory.WorkflowBL.GetList_b05(null, 0, 0, pid, 0).First();
                            if (rec.j02ID_Sys != Factory.CurrentUser.pid && !Factory.CurrentUser.IsAdmin && !Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_b05OwnerAll))
                            {
                                AddMessageTranslated(strMessage);
                                b = false;
                            }
                            break;
                    }
                    string s = "Chyba";
                    if (b)
                    {
                        switch (v.oper)
                        {
                            case "archive":
                                s = BO.Code.Bas.GB(Factory.CBL.Archive(v.prefix, pid));
                                break;
                            case "restore":
                                s = BO.Code.Bas.GB(Factory.CBL.Restore(v.prefix, pid));
                                break;
                            default:
                                s = Factory.CBL.DeleteRecord(v.prefix, pid);
                                break;
                        }



                    }

                    if (s != "1")
                    {
                        intErrs += 1;
                    }
                    else
                    {
                        intOks += 1;
                    }



                }
                if (intErrs > 0)
                {

                    AddMessageTranslated($"Počet odstraněných záznamů: {intOks}, počet chyb: {intErrs}.", "info");
                    RefreshState(v);
                }
                else
                {
                    v.SetJavascript_CallOnLoad(0);
                    return View(v);
                }
            }

            return View(v);
        }
    }
}
