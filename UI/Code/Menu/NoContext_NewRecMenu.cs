

namespace UI.Menu
{
    public class NoContext_NewRecMenu: BaseNonContextMenu
    {
        public NoContext_NewRecMenu(BL.Factory f):base(f)
        {
            
            AMI("Dokument", "javascript:_window_open('/o23/SelectDocType')", "file_present");

            if (f.CurrentUser.j04IsModule_p31)
            {
                DIV();
                var lisP34 = f.p34ActivityGroupBL.GetList_WorksheetEntry_InAllProjects(f.CurrentUser.pid);
                if (lisP34.Count() < 10)
                {
                    foreach (var recP34 in lisP34)
                    {
                        var strIcon = "more_time";
                        if ((recP34.p33ID == BO.p33IdENUM.PenizeBezDPH || recP34.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu) && recP34.p34IncomeStatementFlag == BO.p34IncomeStatementFlagENUM.Vydaj)
                        {
                            strIcon = "price_change";
                        }
                        if ((recP34.p33ID == BO.p33IdENUM.PenizeBezDPH || recP34.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu) && recP34.p34IncomeStatementFlag == BO.p34IncomeStatementFlagENUM.Prijem)
                        {
                            strIcon = "price_check";
                        }
                        if (recP34.p33ID == BO.p33IdENUM.Kusovnik)
                        {
                            strIcon = "calculate";
                        }
                        AMI(recP34.p34Name, $"javascript:_p31_create({recP34.pid})", strIcon);
                    }
                }
                else
                {
                    AMI("Vykázat úkon", $"javascript:_p31_create()", "more_time");
                }
                
            }


            if (f.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Creator))
            {
                if (f.p07LevelsCount == 1)
                {
                    AMI(f.getP07Level(5, true), "javascript:_edit('p41',0)", "work_outline"); //nový projekt, pouze jedna vertikální úroveň
                }
                else
                {
                    for (int i = 1; i <= 5; i++)   //v systému více vertikálních úrovní
                    {
                        if (f.getP07Level(i, true) != null)
                        {
                            AMI(f.getP07Level(i, true), $"javascript:_edit('le{i}',0)", "work_outline");
                        }
                    }
                    DIV();
                }
            }
            if (f.CurrentUser.TestPermission(BO.PermValEnum.GR_p28_Creator))
            {
                AMI("Kontakt", "javascript:_edit('p28',0)", "business");
            }

            if (f.CurrentUser.j04IsModule_p90)
            {
                DIV();
                AMI("Záloha", "javascript:_edit('p90',0)", "receipt");
            }

            
            
            
            
            if (f.CurrentUser.IsAdmin)
            {
                AMI("Uživatelský účet", "javascript:_window_open('/j02/Record?pid=0', 1)", "person");
            }
            
        }
    }
}
