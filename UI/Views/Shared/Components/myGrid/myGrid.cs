
using BL;
using Microsoft.AspNetCore.Mvc;


namespace UI.Views.Shared.Components.myGrid
{
    public class myGrid : ViewComponent
    {
        private readonly BL.Factory _f;
        private readonly BL.TheColumnsProvider _colsProvider;
        public myGrid(BL.Factory f, BL.TheColumnsProvider cp)
        {
            _f = f;
            _colsProvider = cp;
        }



        public IViewComponentResult Invoke(myGridInput input)
        {
            var ret = new myGridViewModel();

            ret.GridInput = input;


            if (ret.GridInput.query == null)
            {
                ret.GridInput.query = new BO.InitMyQuery(_f.CurrentUser).Load(ret.GridInput.entity);

            }


            if (ret.GridInput.j72id > 0)
            {
                ret.GridState = _f.j72TheGridTemplateBL.LoadState(ret.GridInput.j72id, _f.CurrentUser.pid);
            }
            if (ret.GridState == null)
            {
                ret.GridState = _f.j72TheGridTemplateBL.LoadState(ret.GridInput.entity, _f.CurrentUser.pid, ret.GridInput.master_entity, ret.GridInput.rez);  //výchozí, systémový grid: j72SystemFlag=1, pokud tedy již existuje
            }


            if (ret.GridState == null)   //pro uživatele zatím nebyl vygenerován záznam v j72 -> vygenerovat první uživatelovo grid pro daný prefix a masterprefix a rez
            {
                string strJ72Columns = _f.j72TheGridTemplateBL.getDefaultPalletePreSaved(ret.GridInput.entity, ret.GridInput.master_entity, ret.GridInput.query);
                if (strJ72Columns == null)
                {
                    var cols = _colsProvider.getDefaultPallete(false, ret.GridInput.query, _f);    //výchozí paleta sloupců
                    strJ72Columns = String.Join(",", cols.Select(p => p.UniqueName));
                }

                var recJ72 = new BO.j72TheGridTemplate() { j72SystemFlag = BO.j72SystemFlagEnum.Grid, j72Entity = ret.GridInput.entity, j02ID = _f.CurrentUser.pid, j72Columns = strJ72Columns, j72MasterEntity = ret.GridInput.master_entity, j72Rez = ret.GridInput.rez };

                var intJ72ID = _f.j72TheGridTemplateBL.Save(recJ72, null, null, null);
                ret.GridState = _f.j72TheGridTemplateBL.LoadState(intJ72ID, _f.CurrentUser.pid);
            }
            if (!string.IsNullOrEmpty(ret.GridInput.fixedcolumns))
            {
                ret.GridState.j72Columns = ret.GridInput.fixedcolumns;
            }
            ret.GridState.j75CurrentRecordPid = ret.GridInput.go2pid;
            ret.GridState.j72MasterEntity = ret.GridInput.master_entity;
            ret.GridState.j75CurrentPagerIndex = 0; //na úvodní zobrazení vždy začínat grid na první stránce!

            

            var cSup = new myGridSupport(ret.GridInput, _f, _colsProvider);


            ret.firstdata = cSup.GetFirstData(ret.GridState);


            ret.Columns = _colsProvider.ParseTheGridColumns(ret.GridInput.entity.Substring(0, 3), ret.GridState.j72Columns, _f);


            //if (!_f.CurrentUser.TestPermission(BO.PermValEnum.GR_AllowRates) && ret.Columns.Any(p => p.IHRC == true))   //kontrola přístupu k  billing sloupcům  
            //{
            //    var recJ72 = _f.j72TheGridTemplateBL.Load(ret.GridState.j72ID);
            //    recJ72.j72Columns = String.Join(",", ret.Columns.Where(p => p.IHRC == false).Select(p => p.UniqueName)); //vyhodit billing sloupce        
            //    _f.j72TheGridTemplateBL.Save(recJ72, null, null, null);
            //    ret.GridState = _f.j72TheGridTemplateBL.LoadState(recJ72.pid, _f.CurrentUser.pid);

            //    ret.firstdata = cSup.GetFirstData(ret.GridState);
            //    ret.Columns = _colsProvider.ParseTheGridColumns(ret.GridInput.entity.Substring(0, 3), ret.GridState.j72Columns, _f);
            //}

            

            if (!ret.GridInput.isrecpagegrid)    //pokud není vypnutý sloupcový filtr
            {
                ret.AdhocFilter = _colsProvider.ParseAdhocFilterFromString(ret.GridState.j75Filter, ret.Columns);
            }

            if (_f.p07LevelsCount > 0)
            {
                foreach (var c in ret.Columns)
                {
                    if (c.Header == "L1") c.Header = _f.getP07Level(1, true);
                    if (c.Header == "L2") c.Header = _f.getP07Level(2, true);
                    if (c.Header == "L3") c.Header = _f.getP07Level(3, true);
                    if (c.Header == "L4") c.Header = _f.getP07Level(4, true);
                    if (c.Header == "L5") c.Header = _f.getP07Level(5, true);
                }
            }
            else
            {
                foreach (var c in ret.Columns)
                {
                    if (c.Header == "L5") c.Header = _f.getP07Level(5, true);
                }
            }


            return View("Default", ret);


        }

        

    }
}
