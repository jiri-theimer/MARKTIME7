

namespace BL.Code
{
    public class SmsManagerSupport
    {
        private string _ApiKey = "580863346dc6e521a42d7cb08d6e15621615d8a7";
        private string _BaseUrl = "https://http-api.smsmanager.cz/Send";

        private readonly BL.Factory _f;
        public SmsManagerSupport(BL.Factory f)
        {
            _f = f;
        }
        

        public void ChangeApiKey(string apikey)
        {
            _ApiKey = apikey;
        }

        public async Task<BO.Result> SendMessage(string strNumber,string strMessage, HttpClient hc = null,int o24ID=0)
        {            
            strNumber = strNumber.Trim().Replace(" ", "").Replace("+", "").Replace(",", ";");
            if (strNumber.Contains(";"))
            {
                var lis = BO.Code.Bas.ConvertString2List(strNumber, ";");
                if (lis.Count() > 20)
                {
                    return new BO.Result(true, "Jednu zprávu lze odesílat maximálně než 20 čísel.");
                }
            }

            string url = _BaseUrl + $"?apikey={this._ApiKey}&number={strNumber}&customid={_f.Lic.pid}&message={strMessage}";

            if (hc == null)
            {
                hc = new HttpClient();
            }

            using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
            {
                var response = await hc.SendAsync(request);

                var strResult = await response.Content.ReadAsStringAsync();
                var arr = BO.Code.Bas.ConvertString2List(strResult, "|");
                string strCode = arr[1];
                try
                {

                    _f.FBL.AppendSmsLog(strNumber, strMessage, strResult, null, null, _f.Lic.pid,o24ID);
                    return new BO.Result(false,strResult);

                }
                catch(Exception e)
                {
                    _f.FBL.AppendSmsLog(strNumber,strMessage, strResult, strCode, e.Message,_f.Lic.pid,o24ID);
                    return new BO.Result(true, strResult+", Error: "+ e.Message);
                }


            }

        }


        public BO.Result SendLoginVerifyMessage(HttpClient hc, BO.j02User recJ02)
        {
            var rnd = new Random();
            var strCode = BO.Code.Bas.RightString("0000" + rnd.Next(1, 9999).ToString(), 4);
            var strMessage = $"Autorizační kód {strCode} pro přihlášení do systému MARKTIME ({recJ02.j02Login})";


            var ret = SendMessage(recJ02.j02Mobile, strMessage, hc).Result;
            
            if (ret.Flag==BO.ResultEnum.Success)
            {
                _f.j02UserBL.UpdateSmsVerifyCode(recJ02.pid, strCode);
                return new BO.Result(false, strCode);
            }
            else
            {
                return ret;
            }
        }




    }
}
