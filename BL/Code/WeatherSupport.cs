using Newtonsoft.Json;


namespace BL.Code
{
    public class WeatherSupport //https://openweathermap.org/current
    {
        const string _BaseUrl = "https://api.openweathermap.org/data/2.5/weather";
        const string _ApiKey = "0cf085b8ce024e8e9b507da722795db4";      //https://home.openweathermap.org/api_keys
        public WeatherSupport()
        {

        }

        public async Task<BO.Geo.WeatherPlace> LoadWeatherPlace(double lon, double lat)
        {
            string url = _BaseUrl + "?lat=" + lat + "&lon=" + lon + "&appid=" + _ApiKey + "&lang=cz&units=metric";


            var ret = new BO.Geo.WeatherPlace() { LongitudeInput =lon,LatitudeInput=lat };
            if (lon==0.00 || lat==0.00)
            {
                ret.ErrorMessage = "Na vstupu chybí souřadnice.";
                return ret;
            }
            var httpClient = new HttpClient();
            
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
            {
                var response = await httpClient.SendAsync(request);
                var strJson = await response.Content.ReadAsStringAsync();
                try
                {
                    var result = JsonConvert.DeserializeObject<BO.Geo.OpenWeather.Rootobject>(strJson);
                    ret.Temp = result.main.temp;
                    ret.TempFeelsLike = result.main.feels_like;
                    ret.Pressure = result.main.pressure;
                    ret.Humidity = result.main.humidity;
                    ret.TempMax = result.main.temp_max;
                    ret.TempMin = result.main.temp_min;
                    if (result.weather.Count() > 0)
                    {
                        ret.Description = result.weather.First().description;
                        ret.Icon = result.weather.First().icon;
                    }
                    ret.WindSpeed = result.wind.speed;
                    ret.WindDeg = result.wind.deg;
                    ret.WindGust = result.wind.gust;
                    ret.Visibility = result.visibility;
                    ret.Country = result.sys.country;
                    ret.Sunrise = result.sys.sunrise;   //východ slunce kdy v UTC
                    ret.Sunset = result.sys.sunset; //západ slunce kdy v UTC
                    ret.Location = result.name;
                }
                catch (Exception ex)
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
