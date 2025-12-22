using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Code
{
    public static class p90Support
    {
        public static string GeneratePohodaXml(List<BO.Integrace.InputZaloha> recs, HttpClient hp, string strDestFolder)
        {
            var url = "https://mas.marktime.net/Zaloha/PackZaloha";
            var recjson = BO.Code.basJson.SerializeObject(recs);
            var requestContent = new StringContent(recjson, Encoding.UTF8, "application/json");
            var response1 = hp.PostAsync(url, requestContent).Result;
            string strXML = response1.Content.ReadAsStringAsync().Result;
            strXML = strXML.Replace("encoding=\"utf-16\"", "encoding=\"Windows-1250\"");
            BO.Code.File.WriteText2File($"{strDestFolder}\\POHODA_ZALOHA.xml", strXML, 1250);   //pohoda vyžaduje win1250

            return $"{strDestFolder}\\POHODA_ZALOHA.xml";
        }
    }
}
