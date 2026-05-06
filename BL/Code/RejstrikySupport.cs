



namespace BL.Code
{
    public class RejstrikySupport
    {
       
        const string _BaseUrlZvarik = "https://mas.marktime.net/NajitSubjekt";
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


        public async Task<BO.Rejstrik.DefaultZaznam> LoadDefaultZaznam(string pole,string hodnota,string countrycode="CZ",HttpClient hc=null, IViesClient viesClient=null)
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
            string url = _BaseUrlZvarik + $"?{pole}={hodnota}&country={countrycode}";
            
           
            
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
