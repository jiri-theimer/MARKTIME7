using Microsoft.AspNetCore.Mvc;
using UI.Models.wrk;

namespace UI.Controllers.wrk
{
    public class workflow_designerController : BaseController
    {
        public IActionResult Index(int b01id)
        {
            var v = new WorkflowDesignerViewModel() { SelectedB01ID = b01id };
            if (v.SelectedB01ID == 0)
            {
                v.SelectedB01ID = Factory.CBL.LoadUserParamInt("workflow_designer-b01id", 0);
            }
            v.lisB01 = Factory.b01WorkflowTemplateBL.GetList(new BO.myQuery("b01") { IsRecordValid = null });
            if (v.SelectedB01ID>0 && !v.lisB01.Any(p => p.pid == v.SelectedB01ID))
            {
                v.SelectedB01ID = 0;
            }
            if (v.SelectedB01ID==0 && v.lisB01.Count() > 0)
            {
                v.SelectedB01ID = v.lisB01.First().pid;

            }

            if (v.SelectedB01ID > 0)
            {
                v.RecB01 = Factory.b01WorkflowTemplateBL.Load(v.SelectedB01ID);
                v.lisB02 = Factory.b02WorkflowStatusBL.GetList(new BO.myQuery("b02") { IsRecordValid=null}).Where(p => p.b01ID == v.SelectedB01ID);
                v.lisB06 = Factory.b06WorkflowStepBL.GetList(new BO.myQuery("b06") { IsRecordValid = null }).Where(p => p.b01ID == v.SelectedB01ID);
                v.lisAllB11 = Factory.b01WorkflowTemplateBL.GetListB11(v.SelectedB01ID);
            }

           
            return ViewTup(v,BO.PermValEnum.GR_Admin);
        }
    }
}
