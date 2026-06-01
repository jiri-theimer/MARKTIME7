



using BO.Rejstrik;
using Newtonsoft.Json;

namespace BL.Code
{
    public class RejstrikySupport
    {
               
        const string _baseurl = "https://zvarik.cz/raw/firmy.php?apikey=561c5ad2c1a";
        const string _BaseUrlMasService = "https://mas.marktime.net/NajitSubjekt";

        private readonly BL.Code.IViesClient _viesClient;
        public RejstrikySupport(IViesClient viesClient)
        {
            _viesClient = viesClient;
        }
        public async Task<BO.Rejstrik.DefaultZaznam> LoadViesZaznam(string dic)
        {
            //vyhledat EU subjekt přes VIES

            if (string.IsNullOrEmpty(dic))
            {
                return null;
            }
            string strError = null;
            
           
            try
            {
                var result = await _viesClient.CheckAsync(dic);
                var ret = new BO.Rejstrik.DefaultZaznam();
                ret.dic = result.VatNumber;
                ret.name = result.Name;
                ret.street = result.Address;

                return ret;
                

            }
            catch (ArgumentException ex)
            {

                strError = ex.Message;
                return null;

            }
            catch (ViesException ex)
            {
                strError = "Chyba při komunikaci se službou VIES: " + ex.Message;
                return null;
            }
        }


        public async Task<BO.Rejstrik.DefaultZaznam> LoadDefaultZaznam(string pole, string hodnota, string countrycode = "CZ", HttpClient hc = null, IViesClient viesClient = null)
        {

            var rec = new DefaultZaznam();

            if (string.IsNullOrEmpty(pole) && string.IsNullOrEmpty(hodnota))
            {
                rec.errormessage = "Chybí [ico] nebo [dic].";
                return rec;
            }

            if (pole == "dic" && _viesClient != null && !string.IsNullOrEmpty(hodnota) && hodnota.Substring(0, 2) != "CZ")
            {
                return await LoadViesZaznam(hodnota);   //hledání subjektu přes VIES službu
            }

            if (hc == null)
            {
                hc = new HttpClient();
            }

            using (hc)
            {
                string url = _baseurl + $"&country={countrycode}";
                if (pole=="ico")
                {
                    url += "&search_ico=" + hodnota;
                }
                if (pole=="dic")
                {
                    url += "&search_dic=" + hodnota;
                }
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {

                    HttpResponseMessage response = hc.SendAsync(request).Result;
                    string strResult = response.Content.ReadAsStringAsync().Result;

                    if (string.IsNullOrEmpty(strResult) || strResult.ToLower() == "false")
                    {
                        rec.errormessage = "false";
                        return rec;
                    }
                    if (strResult.Length < 10)
                    {
                        rec.errormessage = strResult;
                        return rec;
                    }
                    
                    rec = JsonConvert.DeserializeObject<DefaultZaznam>(strResult);
                    

                }

            }


            return rec;
        }

        public async Task<BO.Rejstrik.DefaultZaznam> LoadDefaultZaznamByService(string pole,string hodnota,string countrycode="CZ",HttpClient hc=null, IViesClient viesClient=null)
        {
            if (string.IsNullOrEmpty(countrycode)) countrycode = "CZ";
            if (hc == null)
            {
                hc= new HttpClient();
            }


            if (pole=="dic" && _viesClient != null && !string.IsNullOrEmpty(hodnota) && hodnota.Substring(0,2) !="CZ")
            {
                return await LoadViesZaznam(hodnota);   //hledání subjektu přes VIES službu
            }

            //hledat přes ZVAŘÍKa
            string url = _BaseUrlMasService + $"?{pole}={hodnota}&country={countrycode}";
            
           
            
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
            {
                var response = await hc.SendAsync(request);
                var strJson = await response.Content.ReadAsStringAsync();
                try
                {
                    return BO.Code.basJson.DeserializeData<BO.Rejstrik.DefaultZaznam>(strJson);
                    
                }
                catch
                {
                    
                    return null;
                }
                

            }


        }

    }
}
