


namespace BL.Code
{
    public class RejstrikySupport
    {
       
        const string _BaseUrlZvarik = "https://mas.marktime.net/NajitSubjekt";
       
        public async Task<BO.Rejstrik.DefaultZaznam> LoadDefaultZaznam(string pole,string hodnota,string countrycode="CZ",HttpClient hc=null)
        {
            if (string.IsNullOrEmpty(countrycode)) countrycode = "CZ";
            if (hc == null)
            {
                hc= new HttpClient();
            }
            string url = _BaseUrlZvarik + $"?{pole}={hodnota}&country={countrycode}";
            
           

            using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
            {
                var response = await hc.SendAsync(request);
                var strJson = await response.Content.ReadAsStringAsync();
                try
                {
                    return BO.Code.basJson.DeserializeData<BO.Rejstrik.DefaultZaznam>(strJson);
                    //return JsonConvert.DeserializeObject<BO.Rejstrik.DefaultZaznam>(strJson);
                }
                catch
                {
                    
                    return null;
                }
                

            }


        }

    }
}
