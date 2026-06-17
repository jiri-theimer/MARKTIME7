
using Newtonsoft.Json;

namespace BL.Code
{
    public class GeoSupport
    {
        const string _BaseUrl = "https://dev.virtualearth.net/REST/v1/Locations";   
        const string _ApiKey = "AkoQ4t20-FoCyxMyjNAADZxbpSm7Z54V1Ge-TECv6TcvOi_7whxtDZWkdQ77md6q";      //https://www.bingmapsportal.com/Application
        public GeoSupport()
        {
            
        }

        public async Task<BO.Geo.GeoPlace> LoadGeoPlace(string city, string street)
        {
            string strAddressInput = System.Web.HttpUtility.UrlEncode(city);
            if (!string.IsNullOrEmpty(street))
            {
                strAddressInput += "," + System.Web.HttpUtility.UrlEncode(street);
                
            }
            var ret = new BO.Geo.GeoPlace() { AddressInput = strAddressInput };
            if (string.IsNullOrEmpty(ret.AddressInput))
            {
                ret.ErrorMessage = "Na vstupu chybí zadat adresu.";
                return ret;
            }
            var httpClient = new HttpClient();
            //var httpResult = await httpClient.GetAsync($"{_BaseUrl}?addressLine={strAddressInput}&key={_ApiKey}");

            

            using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"{_BaseUrl}?addressLine={strAddressInput}&key={_ApiKey}"))
            {
                var response = await httpClient.SendAsync(request);
                var strJson = await response.Content.ReadAsStringAsync();
                try
                {
                    var result = JsonConvert.DeserializeObject<BO.Geo.BingMap.Rootobject>(strJson);
                    if (result.resourceSets.Any() && result.resourceSets.First().resources.Any())
                    {
                        var qry = result.resourceSets.First().resources.First();

                        ret.Latitude = qry.point.coordinates[0];
                        ret.Longitude = qry.point.coordinates[1];
                        ret.Locality = qry.address.locality;
                        ret.FormattedAddress = qry.address.formattedAddress;
                        ret.CountryRegion = qry.address.countryRegion;
                        ret.PostalCode = qry.address.postalCode;
                    }
                    else
                    {
                        ret.ErrorMessage = "Adresa nebyla nalezena.";
                    }
                }
                catch(Exception ex)
                {
                    ret.ErrorMessage = ex.Message;
                    return ret;
                }
            }

            //var strJson = await httpResult.Content.ReadAsStringAsync();

            
            return ret;

        }
       
    }
}
