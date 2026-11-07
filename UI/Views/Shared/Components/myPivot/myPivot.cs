using Microsoft.AspNetCore.Mvc;

namespace UI.Views.Shared.Components.myPivot
{
    /// <summary>MARKTIME pivot – kontingenční tabulka nad agregovanými daty (náhrada WebDataRocks bez limitu 1 MB).</summary>
    public class myPivot : ViewComponent
    {
        public IViewComponentResult Invoke(myPivotViewModel v)
        {
            return View("Default", v);
        }
    }
}
