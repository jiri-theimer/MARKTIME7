

using System.Net.Http;
using System;
using System.Text;

namespace BL.Code
{
    public static class p91Support
    {
        public static string GenerateIsdoc(BO.Integrace.InputInvoice rec,HttpClient hp,string strDestFolder,string strExplicitFileName=null)
        {
            //vrátí plnou cestu na vygenerovaný ISDOC soubor

            var recjson = BO.Code.basJson.SerializeObject(rec);
            var requestContent = new StringContent(recjson, Encoding.UTF8, "application/json");
            
            var url = "https://mas.marktime.net/Isdoc";
            var response1 = hp.PostAsync(url, requestContent).Result;
            string strXML = response1.Content.ReadAsStringAsync().Result;
            if (strExplicitFileName == null)
            {
                strExplicitFileName = $"{BO.Code.File.PrepareFileName(rec.p91Code,true)}.ISDOC";
            }
            BO.Code.File.WriteText2File($"{strDestFolder}\\{strExplicitFileName}", strXML);

            return $"{strDestFolder}\\{strExplicitFileName}";

        }

        public static string GeneratePohodaXml(List<BO.Integrace.InputInvoice> recs,HttpClient hp,string strDestFolder)
        {
            var url = "https://mas.marktime.net/Pohoda/Pack";
            var recjson = BO.Code.basJson.SerializeObject(recs);
            var requestContent = new StringContent(recjson, Encoding.UTF8, "application/json");
            var response1 = hp.PostAsync(url, requestContent).Result;
            string strXML = response1.Content.ReadAsStringAsync().Result;
            strXML = strXML.Replace("encoding=\"utf-16\"", "encoding=\"Windows-1250\"");
            BO.Code.File.WriteText2File($"{strDestFolder}\\POHODA.xml", strXML, 1250);   //pohoda vyžaduje win1250

            return $"{strDestFolder}\\POHODA.xml";
        }
    }
}
