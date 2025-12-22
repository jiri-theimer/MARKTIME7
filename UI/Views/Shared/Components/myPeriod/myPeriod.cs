using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using UI.Views.Shared.Components.myGrid;

namespace UI.Views.Shared.Components.myPeriod
{    
    public class myPeriod: ViewComponent
    {
        private readonly BL.Factory _f;
        private readonly BL.Singleton.ThePeriodProvider _pp;
        public myPeriod(BL.Factory f, BL.Singleton.ThePeriodProvider pp)
        {
            _pp = pp;
            _f = f;
        }

        public IViewComponentResult Invoke(myPeriodViewModel v)
        {
                        
            if (!v.IsLoaded())
            {
                v.LoadUserSetting(_pp, _f); //načtení naposledy nastaveného období uživatelem, protože na vstupu očividně načtené není
            }
            
           

            return View("Default", v);
        }


       
    }
}
