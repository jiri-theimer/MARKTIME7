using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ListsController : BaseApiController
    {
        [SwaggerOperation(Summary = "Projekty le5/le4/le3/le2/le1")]
        [HttpGet]
        [Route("p41")]
        public IEnumerable<BO.p41Project> p41(string prefix)
        {            

            var lis=this.GetFactory().p41ProjectBL.GetList(new BO.myQueryP41(prefix) { MyRecordsDisponible=true});

            return lis;

        }

        [SwaggerOperation(Summary = "Otevřené projekty pro vykazování")]
        [HttpGet]
        [Route("p41_p31entry")]
        public IEnumerable<BO.p41Project> p41_p31entry(string prefix)
        {

            var lis = this.GetFactory().p41ProjectBL.GetList(new BO.myQueryP41(prefix) { MyRecordsDisponible = true,j02id_query=this.GetFactory().CurrentUser.pid });

            return lis;

        }

        [SwaggerOperation(Summary = "Kontakty")]
        [HttpGet]
        [Route("p28")]
        public IEnumerable<BO.p28Contact> p28()
        {
            
            var lis = this.GetFactory().p28ContactBL.GetList(new BO.myQueryP28());

            return lis;

        }

        [SwaggerOperation(Summary = "Aktivity")]
        [HttpGet]
        [Route("p32")]
        public IEnumerable<BO.p32Activity> p32()
        {

            
            var lis = this.GetFactory().p32ActivityBL.GetList(new BO.myQueryP32());

            return lis;

        }

        [SwaggerOperation(Summary = "Sešity")]
        [HttpGet]
        [Route("p34")]
        public IEnumerable<BO.p34ActivityGroup> p34()
        {
            return this.GetFactory().p34ActivityGroupBL.GetList(new BO.myQueryP34());            
        }

        [SwaggerOperation(Summary = "Sešity pro vykazování úkonů")]
        [HttpGet]
        [Route("p34_p31entry")]
        public IEnumerable<BO.p34ActivityGroup> p34_p31entry()
        {
            return this.GetFactory().p34ActivityGroupBL.GetList(new BO.myQueryP34() { j02id_query=this.GetFactory().CurrentUser.pid});
        }

        [SwaggerOperation(Summary = "Seznam založených záznamů ve stopkách")]
        [HttpGet]
        [Route("p68")]
        public IEnumerable<BO.p68StopWatch> p68()
        {
            return this.GetFactory().p68StopWatchBL.GetList(this.GetFactory().CurrentUser.pid);
        }

        
    }
}
