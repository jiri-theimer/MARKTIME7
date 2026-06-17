using Microsoft.AspNetCore.Mvc;
using UI.Models.capacity;

namespace UI.Controllers.capacity
{
    public class captmlController : BaseController
    {
        public IActionResult Index(string prefix)
        {

            var v = new captmlViewModel() { prefix = prefix };

            if (v.prefix == null)
            {
                v.prefix = Factory.CBL.LoadUserParam("captml-prefix", "j02");
            }
            


            if (v.prefix == "j02")
            {
                v.timeline_j02 = new CapacityTimelineJ02ViewModel() { UserKeyBase = "captml", ExternalQuery = new BO.myQueryJ02() { explicit_orderby = "a.j02LastName" } };
                
                v.j02IDs = Factory.CBL.LoadUserParam("captml-j02ids");
                if (v.j02IDs != null)
                {
                    var j02ids = BO.Code.Bas.ConvertString2ListInt(v.j02IDs);
                    var lis = Factory.j02UserBL.GetList(new BO.myQueryJ02() { pids = j02ids });
                    v.SelectedPersons = string.Join(",", lis.Select(p => p.FullnameDesc));
                    v.timeline_j02.ExternalQuery.pids = BO.Code.Bas.ConvertString2ListInt(v.j02IDs);
                }
                v.j07IDs = Factory.CBL.LoadUserParam("captml-j07ids");
                if (v.j07IDs != null)
                {
                    var j07ids = BO.Code.Bas.ConvertString2ListInt(v.j07IDs);
                    var lis = Factory.j07PersonPositionBL.GetList(new BO.myQuery("j07") { pids = j07ids });
                    v.SelectedPositions = string.Join(",", lis.Select(p => p.j07Name));
                    v.timeline_j02.ExternalQuery.j07ids = j07ids;
                }
                v.j11IDs = Factory.CBL.LoadUserParam("captml-j11ids");
                if (v.j11IDs != null)
                {
                    var j11ids = BO.Code.Bas.ConvertString2ListInt(v.j11IDs);
                    var lis = Factory.j11TeamBL.GetList(new BO.myQueryJ11() { pids = j11ids });
                    v.SelectedTeams = string.Join(",", lis.Select(p => p.j11Name));
                    v.timeline_j02.ExternalQuery.j11ids = j11ids;
                }
            }
            
            if (v.prefix == "p41")
            {
                v.timeline_p41 = new CapacityTimelineP41ViewModel() { UserKeyBase = "captml" };
               
                
            }
            

            

            return View(v);
        }

        
    }
}
