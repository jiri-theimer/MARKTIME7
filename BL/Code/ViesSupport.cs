using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;


namespace BL.Code
{
    public sealed record ViesSubjectResult(
    string CountryCode,
    string VatNumber,
    bool IsValid,
    DateTime RequestDate,
    string? Name,
    string? Address
);

    public interface IViesClient
    {
        Task<ViesSubjectResult> CheckAsync(string dic, CancellationToken ct = default);
    }

    public sealed class ViesClient(HttpClient httpClient) : IViesClient
    {
        private static readonly XNamespace SoapNs = "http://schemas.xmlsoap.org/soap/envelope/";
        private static readonly XNamespace ViesNs = "urn:ec.europa.eu:taxud:vies:services:checkVat:types";

        public async Task<ViesSubjectResult> CheckAsync(string dic, CancellationToken ct = default)
        {
            var (countryCode, vatNumber) = ParseDic(dic);

            var soap = $$"""
        <?xml version="1.0" encoding="UTF-8"?>
        <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                          xmlns:urn="urn:ec.europa.eu:taxud:vies:services:checkVat:types">
          <soapenv:Header/>
          <soapenv:Body>
            <urn:checkVat>
              <urn:countryCode>{{WebUtility.HtmlEncode(countryCode)}}</urn:countryCode>
              <urn:vatNumber>{{WebUtility.HtmlEncode(vatNumber)}}</urn:vatNumber>
            </urn:checkVat>
          </soapenv:Body>
        </soapenv:Envelope>
        """;

            using var request = new HttpRequestMessage(HttpMethod.Post, "checkVatService")
            {
                Content = new StringContent(soap, Encoding.UTF8, "text/xml")
            };

            request.Headers.Add("SOAPAction", "");

            using var response = await httpClient.SendAsync(request, ct);
            var xml = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                throw new ViesException($"VIES returned HTTP {(int)response.StatusCode}: {xml}");

            var doc = XDocument.Parse(xml);

            var fault = doc.Descendants(SoapNs + "Fault").FirstOrDefault();
            if (fault is not null)
            {
                var faultString = fault.Element("faultstring")?.Value ?? "Unknown VIES SOAP fault";
                throw new ViesException(faultString);
            }

            var body = doc.Descendants(SoapNs + "Body").First();
            var result = body.Descendants(ViesNs + "checkVatResponse").First();

            return new ViesSubjectResult(
                CountryCode: result.Element(ViesNs + "countryCode")?.Value ?? countryCode,
                VatNumber: result.Element(ViesNs + "vatNumber")?.Value ?? vatNumber,
                IsValid: bool.Parse(result.Element(ViesNs + "valid")?.Value ?? "false"),
                RequestDate: DateTime.Parse(result.Element(ViesNs + "requestDate")?.Value ?? DateTime.UtcNow.ToString("O")),
                Name: Normalize(result.Element(ViesNs + "name")?.Value),
                Address: Normalize(result.Element(ViesNs + "address")?.Value)
            );
        }

        private static (string CountryCode, string VatNumber) ParseDic(string dic)
        {
            var cleaned = Regex.Replace(dic.Trim().ToUpperInvariant(), @"[\s\.\-]", "");

            if (cleaned.Length < 3)
                throw new ArgumentException("DIČ musí obsahovat kód země a číslo, např. CZ12345678.", nameof(dic));

            var countryCode = cleaned[..2];
            var vatNumber = cleaned[2..];

            if (!Regex.IsMatch(countryCode, "^[A-Z]{2}$"))
                throw new ArgumentException("DIČ musí začínat dvoupísmenným kódem země, např. CZ.", nameof(dic));

            return (countryCode, vatNumber);
        }

        private static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim() == "---")
                return null;

            return value.Trim();
        }
    }

    public sealed class ViesException(string message) : Exception(message);
}
