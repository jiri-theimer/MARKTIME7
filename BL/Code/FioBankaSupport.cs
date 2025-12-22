

namespace BL.Code
{
    public class FioBankaSupport
    {
        const string _BaseUrl = "https://fioapi.fio.cz/v1/rest/periods";

        public async Task<BO.Banka.Fio.Rootobject> LoadVypis(string apikey,HttpClient hc = null)
        {
            var d1 = DateTime.Today.AddDays(-89);
            var d2 = DateTime.Today;

            
            if (hc == null)
            {
                hc = new HttpClient();
            }
            
            string url = $"{_BaseUrl}/{apikey}/{d1.ToString("yyyy-MM-dd")}/{d2.ToString("yyyy-MM-dd")}/transactions.json";

                

            using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
            {
                var response = await hc.SendAsync(request);
                var strJson = await response.Content.ReadAsStringAsync();
                try
                {
                    return BO.Code.basJson.DeserializeData<BO.Banka.Fio.Rootobject>(strJson);                
                }
                catch
                {

                    return null;
                }


            }


        }
    }
}
