using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace BL.Code.Isir
{
    public enum InsolvencyStatus { NotFound, Active, Closed }

    public class InsolvencyResult
    {
        public InsolvencyStatus Status { get; set; }
        public List<InsolvencyCase> Cases { get; set; } = new();
    }

    public class InsolvencyCase
    {
        private static readonly HashSet<string> _aktivniStavy = new(StringComparer.OrdinalIgnoreCase)
    {
        "NEVYRIZENA",
        "NEVYR-POST",
        "MORATORIUM",
        "ODDLUZENI",
        "REORGANIZ",
        "UPADEK",
        "KONKURS",
        "OBZIVLA"
    };

        public string SpisZnacka { get; set; }
        public string StavRizeni { get; set; }

        public bool IsActive => StavRizeni != null && _aktivniStavy.Contains(RemoveDiacritics(StavRizeni));

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            return new string(normalized
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray())
                .Normalize(NormalizationForm.FormC);
        }
    }

    public static class IsirCuzk
    {
        public enum SearchType { Ico, Rc, SpisZnacka }

        private const string Endpoint = "https://isir.justice.cz:8443/isir_cuzk_ws/IsirWsCuzkService";
        private const string Ns = "http://isirws.cca.cz/types/";
        private static readonly HttpClient _http = new();

        public static Task<InsolvencyResult> SearchAsync(string value, SearchType type) =>
            type switch
            {
                SearchType.Ico => CallAsync($"<ic>{value}</ic>"),
                SearchType.Rc => CallAsync($"<rc>{value}</rc>"),
                SearchType.SpisZnacka => CallAsync($"<spisovaZnacka>{value}</spisovaZnacka>"),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

        private static async Task<InsolvencyResult> CallAsync(string innerXml)
        {
            var soap = $"""
            <?xml version="1.0" encoding="utf-8"?>
            <soapenv:Envelope
                xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                xmlns:typ="http://isirws.cca.cz/types/">
              <soapenv:Header/>
              <soapenv:Body>
                <typ:getIsirWsCuzkDataRequest>
                  {innerXml}
                </typ:getIsirWsCuzkDataRequest>
              </soapenv:Body>
            </soapenv:Envelope>
            """;

            var content = new StringContent(soap, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "\"\"");

            var response = await _http.PostAsync(Endpoint, content);
            BO.Code.File.LogInfo(response.Content.ReadAsStringAsync().Result);

            response.EnsureSuccessStatusCode();

            return Parse(await response.Content.ReadAsStringAsync());
        }

        private static InsolvencyResult Parse(string xml)
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = Ns;

            var fault = doc.Descendants("faultstring").FirstOrDefault();
            if (fault != null)
                throw new InvalidOperationException($"ISIR SOAP fault: {fault.Value}");

            var data = doc.Descendants("data").ToList();

            if (!data.Any())
                return new InsolvencyResult { Status = InsolvencyStatus.NotFound };

            var cases = data.Select(r => new InsolvencyCase
            {
                SpisZnacka = $"{r.Element("druhVec")?.Value} {r.Element("bcVec")?.Value}/{r.Element("rocnik")?.Value}",
                StavRizeni = r.Element("druhStavKonkursu")?.Value,
            }).ToList();

            var status = cases.Any(c => c.IsActive)
                ? InsolvencyStatus.Active
                : InsolvencyStatus.Closed;

            return new InsolvencyResult { Status = status, Cases = cases };
        }
    }
}