using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Microsoft.Extensions.Configuration;


namespace API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        public readonly BL.Singleton.RunningApp App;
        private string _login { get; set; }        
        private BL.Factory _f { get; set; }
        public BaseApiController()
        {
            this.App = new BL.Singleton.RunningApp();

            
        }

       

        protected BL.Factory GetFactory()
        {
            
            if (_f !=null && _f.CurrentUser == null && _login != null)
            {
                _f.InhaleUserByLogin(_login);
            }            

            if (_f != null)
            {
               
                return _f;
            }

            

            InhaleLogin();
            
            var ru = new BO.RunningUser() { j02Login = _login };
            _f = new BL.Factory(ru, this.App, null, null);

          
            return _f;            
        }


    
        private void InhaleLogin()
        {
            if (this.User != null && this.User.Claims != null && this.User.Claims.Any(p => p.Type == "login"))
            {
                _login = this.User.Claims.First(p => p.Type == "login").Value;
                if (string.IsNullOrEmpty(_login.Trim()))
                {
                    _login = null;
                }
            }
            if (_login == null)
            {
                throw new Exception("V hlavičce dotazu chybí login přihlášený do služby.");
            }


        }

       


        protected BO.Result ResultBad(string message)
        {
            
            string strHeader = null;
            try
            {
                strHeader = GetFactory().CurrentUser.FullnameDesc;
            }
            catch
            {
                strHeader = "Anonym";
            }
            message = strHeader + ": " + message;

            handle_write_error2file(message);
            return new BO.Result(true, message);
        }
        protected BO.Result ResultGood(string message = null)
        {
            return new BO.Result(false, message);
        }

        protected string GetUserErrorMessages(string preambule = null,bool savefile2wcf=true)
        {            
            if (GetFactory().CurrentUser == null || GetFactory().CurrentUser.Messages4Notify == null)
            {
                if (savefile2wcf)
                {
                    handle_write_error2file(preambule);
                }
                return preambule;
            }
            string s = preambule == null ? "" : preambule + ": ";

            foreach (var c in GetFactory().CurrentUser.Messages4Notify)
            {
                s += " ** " + c.Key + ": " + c.Value;
            }

            if (savefile2wcf)
            {
                handle_write_error2file(s);
            }

            return s;
        }

        protected void handle_write_error2file(string s)
        {
           
            BO.Code.File.LogError(DateTime.Now.ToString() + System.Environment.NewLine+System.Environment.NewLine+s);
            
        }
        


        

    }

}

