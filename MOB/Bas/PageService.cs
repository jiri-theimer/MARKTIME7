
namespace Bas
{
    
    public class PageService
    {
        //private readonly Singleton.TheTranslator _tra;

        //public PageService(Singleton.TheTranslator tra)
        //{
        //    _tra = tra;



        //}



        public string tra(string expr)
        {
            return expr;
            //return _tra.DoTranslate(expr, System.Globalization.CultureInfo.CurrentUICulture.Name);

        }

        public bool IsCZrSK()
        {
            if (System.Globalization.CultureInfo.CurrentUICulture.Name == "cs-CZ" || System.Globalization.CultureInfo.CurrentUICulture.Name == "sk-SK")
            {
                return true;
            }

            return false;
        }
            
    }
}
