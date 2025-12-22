using Microsoft.AspNetCore.Mvc;
using UI.Controllers;
using UI.Models;

namespace UI.Views.Shared.Components.myGrid
{
    public class myGridController : BaseController
    {
        private readonly BL.TheColumnsProvider _colsProvider;
        
        public myGridController(BL.TheColumnsProvider cp)
        {
            _colsProvider = cp;
           
        }

        //-----------Začátek GRID událostí-------------
        public myGridOutput HandleTheGridFilter(myGridUIContext tgi, List<BO.StringPair> pathpars, List<BO.TheGridColumnFilter> filter) //myGrid povinná metoda: sloupcový filtr
        {
           
            var gridinput = Handle_Load_GridInput_FromClient(tgi);
            var c = new myGridSupport(gridinput, Factory, _colsProvider);
           
            return c.Event_HandleTheGridFilter(tgi, filter);
        }

        public myGridOutput HandleTheGridOper(myGridUIContext tgi, List<BO.StringPair> pathpars)    //myGrid povinná metoda: změna třídění, pageindex, změna stránky
        {
            var gridinput = Handle_Load_GridInput_FromClient(tgi);
            
            var c = new myGridSupport(gridinput, Factory, _colsProvider);
            
            return c.Event_HandleTheGridOper(tgi);

        }
        public myGridExportedFile HandleTheGridExport(string format, string pids, myGridUIContext tgi, List<BO.StringPair> pathpars)  //myGrid povinná metoda pro export dat
        {
            var gridinput = Handle_Load_GridInput_FromClient(tgi);
            var c = new myGridSupport(gridinput, Factory, _colsProvider);

            return c.Event_HandleTheGridExport(format, tgi.j72id, pids);
        }
        //-----------Konec GRID událostí-------------

        private myGridInput Handle_Load_GridInput_FromClient(myGridUIContext tgi)
        {
            var gridinput = new myGridInput() { entity = tgi.entity, go2pid = tgi.go2pid, master_entity = tgi.master_entity, master_pid = tgi.master_pid, myqueryinline = tgi.myqueryinline, ondblclick = tgi.ondblclick, isperiodovergrid = tgi.isperiodovergrid,rez=tgi.rez,j72id_query=tgi.j72id_query };

            gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load(tgi.prefix, tgi.master_entity, tgi.master_pid, tgi.myqueryinline,tgi.rez);
            switch (tgi.record_bin_query)
            {
                case "1":
                    gridinput.query.IsRecordValid = true;break;
                case "2":
                    gridinput.query.IsRecordValid = false; break;
                default:
                    gridinput.query.IsRecordValid = null;break;
            }
            if (gridinput.query.p31statequery==0 & tgi.p31_state_query > 0)
            {
                gridinput.query.p31statequery = tgi.p31_state_query;    //p31statequery může přijít v query přes myqueryinline a tímto by se to přepsalo - není dobře vyřešeno
            }
            if (gridinput.query.p31tabquery==null && !string.IsNullOrEmpty(tgi.p31_tab_query))
            {
                gridinput.query.p31tabquery = tgi.p31_tab_query;    //není dobře vyřešeno - může dojít k nechtěnému přepisu, pokud se posílá přes myqueryinline
            }
            
            gridinput.query.period_field = tgi.period_field;
            if (tgi.d1_iso != null)
            {
                gridinput.query.global_d1 = DateTime.Parse(tgi.d1_iso);
            }
            if (tgi.d2_iso != null)
            {
                gridinput.query.global_d2 = DateTime.Parse(tgi.d2_iso);
            }
            


            gridinput.query.MyRecordsDisponible = !Factory.CurrentUser.IsAdmin;

            gridinput.oncmclick = tgi.oncmclick;
            gridinput.fixedcolumns = tgi.fixedcolumns;
            gridinput.reczoomflag = tgi.reczoomflag;

            return gridinput;
        }
    }
}
