

namespace BL.Code
{
    public class TimeApiSupport
    {
        
        public async Task<BO.TimeApi.Record> LoadCurrentTime(HttpClient hc = null)
        {
            
            if (hc == null)
            {
                hc = new HttpClient();
            }
           
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://timeapi.io/api/Time/current/zone?timeZone=Europe/Amsterdam"))
            {
                var response = await hc.SendAsync(request);
                var strJson = await response.Content.ReadAsStringAsync();
                try
                {
                    return BO.Code.basJson.DeserializeData<BO.TimeApi.Record>(strJson);
                    
                }
                catch
                {

                    return null;
                }


            }
        }
    }
}
