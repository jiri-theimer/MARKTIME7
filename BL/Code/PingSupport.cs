


using System.Xml.Linq;

namespace BL.Code
{
    public class PingSupport
    {

        const string _BaseUrlZvarik = "https://mas.marktime.net/Ping";

        public async Task<string> SendPing(BL.Factory f, HttpClient hc = null)
        {
            
            if (hc == null)
            {
                hc = new HttpClient();
            }
            var c = f.x01LicenseBL.Load(f.CurrentUser.x01ID);

            var dt=f.FBL.GetDataTable("select count(a.p31ID) as Pocet,MIN(a.p31DateInsert) as Poprve,MAX(a.p31DateInsert) as Naposledy,SUM(case when convert(date,a.p31DateInsert)=convert(date,DATEADD(day,-1,GETDATE())) then 1 end) as VceraPocet,DATEADD(day,-1,GETDATE()) as VceraDatum,count(distinct case when a.p31dateinsert between dateadd(day,-14,getdate()) and getdate() then a.j02id end) as PocetZapisovacu  FROM p31Worksheet a");

            int intPocetZapisovacu= 0;int intPocetUkonu = 0;
            DateTime? datNaposledy = null;
            int intPocetUkonuVcera = 0;
            DateTime? datVcera = DateTime.Today.AddDays(-1);
            DateTime? datPoprve = null;

            foreach (System.Data.DataRow dbrow in dt.Rows)
            {
                
                if (dbrow["PocetZapisovacu"] != System.DBNull.Value)
                {
                    intPocetZapisovacu = Convert.ToInt32(dbrow["PocetZapisovacu"]);
                }                    
                intPocetUkonu= Convert.ToInt32(dbrow["Pocet"]);
                if (dbrow["Naposledy"] != System.DBNull.Value)
                {
                    datNaposledy = Convert.ToDateTime(dbrow["Naposledy"]);
                }
                if (dbrow["VceraDatum"] != System.DBNull.Value)
                {
                    datVcera = Convert.ToDateTime(dbrow["VceraDatum"]);
                }
                if (dbrow["VceraPocet"] != System.DBNull.Value)
                {
                    intPocetUkonuVcera = Convert.ToInt32(dbrow["VceraPocet"]);
                }
                if (dbrow["Poprve"] != System.DBNull.Value)
                {
                    datPoprve = Convert.ToDateTime(dbrow["Poprve"]);
                }

                
            }

            var lisJ02 = f.j02UserBL.GetList(new BO.myQueryJ02() { IsRecordValid = null });
            DateTime? datLastPing=null;int intPocetAktivnich = 0;

            if (lisJ02.Any(p=>p.j02Ping_TimeStamp != null))
            {
                datLastPing = lisJ02.Max(p => p.j02Ping_TimeStamp).Value;
                intPocetAktivnich = lisJ02.Count(p => p.j02Ping_TimeStamp>DateTime.Now.AddDays(-14));
            }
        
            
            string url = _BaseUrlZvarik + $"?guid={c.x01Guid}&x01name={c.x01Name}&countrycode={c.x01CountryCode}&pocetzapisovacu={intPocetZapisovacu}&poslednizapiskdy={BO.Code.Bas.ObjectDateTime2String(datNaposledy)}&pocetukonu={intPocetUkonu}&hosturl={c.x01AppHost}&appversion={f.App.AppBuild}";
            url += $"&pocetukonuvcera={intPocetUkonuVcera}&datumvcera={BO.Code.Bas.ObjectDate2String(datVcera)}&prvniukonkdy={BO.Code.Bas.ObjectDate2String(datPoprve)}";
            url += $"&pocetuzivatelu={lisJ02.Count()}&pocetaktivnich={intPocetAktivnich}&lastping={BO.Code.Bas.ObjectDateTime2String(datLastPing)}";
            url += $"&pocetotevrenych={lisJ02.Where(p => p.isclosed == false).Count()}";

            using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
            {
                var response = await hc.SendAsync(request);
                var strReturn = await response.Content.ReadAsStringAsync();
                try
                {
                    return strReturn;
                    
                }
                catch
                {

                    return null;
                }


            }


        }

    }
}
