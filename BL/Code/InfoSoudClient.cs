// ============================================================================
// InfoSoudClient — klient REST API InfoSoudu pro MARKTIME
//
// Požadavky: .NET 6+ (System.Text.Json, HttpClient). Žádné další NuGet balíčky.
// Nezávislá třída — nic se neregistruje ve Startup/Program, stačí ji vytvořit.
//
// Použití:
//   var infoSoud = new InfoSoudClient();   // výchozí adresa https://infosoud.marktime.net
//   // nebo: new InfoSoudClient("http://localhost:5001")
//
//   var vysledek = await infoSoud.UlozSpisAsync(tokenUzivatele, new InfoSoudSpisPozadavek
//   {
//       KodSoudu      = "OSPHA03",
//       SpisovaZnacka = "12 C 345/2024",
//       Nazev         = "Novák vs. ABC s.r.o.",
//       UrlLink       = "https://marktime.example/spis/4711",
//   });
//
//   if (vysledek.Uspech)
//       Console.WriteLine($"{vysledek.Akce}: spis {vysledek.Spis!.Id}");
//   else
//       Console.WriteLine($"Chyba {vysledek.StatusCode}: {vysledek.Chyba}");
//
// Token (isk_…, 47 znaků) si každý uživatel vygeneruje v InfoSoudu
// v Nastavení → Napojení aplikací a uloží do svého profilu v MARKTIME.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace BL.Code
{


    /// <summary>
    /// Klient REST API InfoSoudu. Instanci lze klidně vytvářet při každém použití (new InfoSoudClient()) —
    /// interně se sdílí jeden statický HttpClient, takže nevznikají nová spojení ani nedochází
    /// k vyčerpání socketů. Token uživatele se předává u každého volání.
    /// </summary>
    public sealed class InfoSoudClient
    {
        public const string VychoziAdresa = "https://infosoud.marktime.net";
        private const string HlavickaTokenu = "X-Api-Key";

        // Jeden HttpClient pro celou aplikaci. PooledConnectionLifetime zajistí,
        // že se po čase znovu načte DNS (náhrada za IHttpClientFactory).
        private static readonly HttpClient SdilenyHttp = new(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(10),
        })
        {
            Timeout = TimeSpan.FromSeconds(30),
        };

        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
        {
            // Důležité: vynechané (null) pole = "beze změny" při aktualizaci existujícího spisu
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private readonly HttpClient _http;
        private readonly Uri _baseUri;

        /// <param name="baseUrl">Adresa InfoSoudu; výchozí https://infosoud.marktime.net.</param>
        public InfoSoudClient(string baseUrl = VychoziAdresa)
        {
            if (string.IsNullOrWhiteSpace(baseUrl)) throw new ArgumentException("Chybí adresa InfoSoudu.", nameof(baseUrl));
            _http = SdilenyHttp;
            _baseUri = new Uri(baseUrl.TrimEnd('/') + "/");
        }

        /// <summary>
        /// Založí spis, nebo aktualizuje existující (podle soudu + spisové značky).
        /// Vynechaná (null) pole se u existujícího spisu nemění, prázdný řetězec hodnotu vymaže.
        /// </summary>
        /// <param name="apiToken">Osobní API token uživatele InfoSoudu (isk_…).</param>
        /// <exception cref="HttpRequestException">InfoSoud není dostupný (síť, DNS, TLS).</exception>
        /// <exception cref="TaskCanceledException">Vypršel timeout nebo bylo volání zrušeno.</exception>
        public async Task<InfoSoudVysledek> UlozSpisAsync(
            string apiToken, InfoSoudSpisPozadavek pozadavek, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(pozadavek);

            using var request = VytvorPozadavek(HttpMethod.Post, "api/v1/spisy", apiToken);
            request.Content = JsonContent.Create(pozadavek, options: Json);

            using var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            return await ZpracujOdpovedAsync(response, ct).ConfigureAwait(false);
        }

        /// <summary>Načte spis včetně posledního stavu řízení. Cizí nebo neexistující spis vrací 404.</summary>
        /// <param name="apiToken">Osobní API token uživatele InfoSoudu (isk_…).</param>
        public async Task<InfoSoudVysledek> NactiSpisAsync(
            string apiToken, int id, CancellationToken ct = default)
        {
            using var request = VytvorPozadavek(HttpMethod.Get, $"api/v1/spisy/{id}", apiToken);
            using var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            return await ZpracujOdpovedAsync(response, ct).ConfigureAwait(false);
        }

        // ------------------------------------------------------------------------

        private HttpRequestMessage VytvorPozadavek(HttpMethod metoda, string cesta, string apiToken)
        {
            if (string.IsNullOrWhiteSpace(apiToken))
                throw new ArgumentException("Chybí API token uživatele.", nameof(apiToken));

            var request = new HttpRequestMessage(metoda, new Uri(_baseUri, cesta));
            request.Headers.Add(HlavickaTokenu, apiToken.Trim());
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return request;
        }

        private static async Task<InfoSoudVysledek> ZpracujOdpovedAsync(HttpResponseMessage response, CancellationToken ct)
        {
            var status = (int)response.StatusCode;
            var obsah = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var spis = string.IsNullOrWhiteSpace(obsah)
                    ? null
                    : JsonSerializer.Deserialize<InfoSoudSpis>(obsah, Json);

                return new InfoSoudVysledek
                {
                    Uspech = spis is not null,
                    StatusCode = status,
                    Spis = spis,
                    Chyba = spis is null ? "InfoSoud vrátil prázdnou odpověď." : null,
                };
            }

            // Chyby vrací API jako ProblemDetails (application/problem+json)
            InfoSoudProblem? problem = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(obsah))
                    problem = JsonSerializer.Deserialize<InfoSoudProblem>(obsah, Json);
            }
            catch (JsonException) { /* např. HTML chybová stránka z IIS */ }

            var chyba = problem?.Detail ?? problem?.Title ?? response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Neplatný nebo zrušený API token. Vygenerujte nový v InfoSoudu (Nastavení → Napojení aplikací).",
                HttpStatusCode.TooManyRequests => "Příliš mnoho požadavků na InfoSoud, zkuste to za minutu.",
                HttpStatusCode.ServiceUnavailable => "InfoSoud je dočasně nedostupný.",
                _ => $"InfoSoud vrátil chybu {status} {response.ReasonPhrase}.",
            };

            // Validační chyby polí (400) připojíme do textu, ať je hláška pro uživatele úplná
            if (problem?.Errors is { Count: > 0 } errors)
                chyba = string.Join(" ", errors.SelectMany(e => e.Value));

            return new InfoSoudVysledek
            {
                Uspech = false,
                StatusCode = status,
                Chyba = chyba,
                ChybyPoli = problem?.Errors,
            };
        }
    }

    // ============================================================================
    // Požadavek
    // ============================================================================

    /// <summary>
    /// Data spisu pro uložení. Povinné je KodSoudu a buď SpisovaZnacka,
    /// nebo DruhVeci + BcVec + Rocnik (+ volitelně CisloSenatu).
    /// U existujícího spisu: null = beze změny, "" = vymazat.
    /// </summary>
    public sealed class InfoSoudSpisPozadavek
    {
        /// <summary>Kód soudu z číselníku InfoSoudu, např. "OSPHA03". Povinné.</summary>
        public string KodSoudu { get; set; } = "";

        /// <summary>Spisová značka textem, např. "12 C 345/2024" nebo "INS 1234/2023".</summary>
        public string? SpisovaZnacka { get; set; }

        /// <summary>Alternativa ke SpisovaZnacka — číslo senátu.</summary>
        public string? CisloSenatu { get; set; }

        /// <summary>Alternativa ke SpisovaZnacka — druh věci (C, INS, EPR…).</summary>
        public string? DruhVeci { get; set; }

        /// <summary>Alternativa ke SpisovaZnacka — běžné číslo věci.</summary>
        public int? BcVec { get; set; }

        /// <summary>Alternativa ke SpisovaZnacka — ročník.</summary>
        public int? Rocnik { get; set; }

        /// <summary>Název spisu (max. 200 znaků).</summary>
        public string? Nazev { get; set; }

        /// <summary>Volná poznámka.</summary>
        public string? Memo { get; set; }

        /// <summary>Příjemci notifikací místo uživatele, e-maily oddělené čárkou (max. 500 znaků).</summary>
        public string? Prijemce { get; set; }

        /// <summary>Skryté kopie notifikací, e-maily oddělené čárkou (max. 500 znaků).</summary>
        public string? VKopii { get; set; }

        /// <summary>Odkaz zobrazený v detailu spisu, např. zpět do MARKTIME (http/https, max. 255 znaků).</summary>
        public string? UrlLink { get; set; }

        /// <summary>Pořadí v seznamu spisů.</summary>
        public int? Poradi { get; set; }

        /// <summary>false = ukončit sledování. Vynecháno = sledovat (ukončené sledování se obnoví).</summary>
        public bool? Aktivni { get; set; }

        /// <summary>Názvy existujících štítků uživatele; nahradí současné ([] = odebrat všechny, null = beze změny).</summary>
        public List<string>? Stitky { get; set; }
    }

    // ============================================================================
    // Odpověď
    // ============================================================================

    /// <summary>Výsledek volání API.</summary>
    public sealed class InfoSoudVysledek
    {
        /// <summary>True = spis uložen / načten (HTTP 200 nebo 201).</summary>
        public bool Uspech { get; init; }

        /// <summary>HTTP status: 200 aktualizováno, 201 založeno, 400 chybný vstup, 401 token,
        /// 404 nenalezeno, 422 limit spisů, 429 příliš mnoho požadavků.</summary>
        public int StatusCode { get; init; }

        /// <summary>Uložený / načtený spis (při úspěchu).</summary>
        public InfoSoudSpis? Spis { get; init; }

        /// <summary>Text chyby pro uživatele (při neúspěchu).</summary>
        public string? Chyba { get; init; }

        /// <summary>Validační chyby podle polí (při 400), klíč = název pole v camelCase.</summary>
        public Dictionary<string, string[]>? ChybyPoli { get; init; }

        /// <summary>"zalozeno" | "aktualizovano" | "bezeZmeny" (jen u UlozSpisAsync).</summary>
        public string? Akce => Spis?.Akce;

        public bool Zalozeno => Akce == "zalozeno";

        /// <summary>Nefatální upozornění, např. neznámé štítky.</summary>
        public IReadOnlyList<string> Varovani => Spis?.Varovani ?? (IReadOnlyList<string>)Array.Empty<string>();

        /// <summary>Token je neplatný nebo zrušený — uživatel si musí vygenerovat nový.</summary>
        public bool NeplatnyToken => StatusCode == 401;
    }

    /// <summary>Spis v InfoSoudu.</summary>
    public sealed class InfoSoudSpis
    {
        public string? Akce { get; set; }
        public int Id { get; set; }
        public string UzivatelEmail { get; set; } = "";
        public string SpisovaZnacka { get; set; } = "";
        public string KodSoudu { get; set; } = "";
        public string? NazevSoudu { get; set; }
        public string? Nazev { get; set; }
        public string? Memo { get; set; }
        public string? UrlLink { get; set; }
        public string? Prijemce { get; set; }
        public string? VKopii { get; set; }
        public bool Aktivni { get; set; }
        public List<string> Stitky { get; set; } = new();

        /// <summary>Poslední stav řízení z InfoSoudu (null = ještě nenačteno).</summary>
        public string? StavRizeni { get; set; }
        public string? StavDatum { get; set; }
        public DateTime? PosledniMonitoring { get; set; }

        /// <summary>Datum poslední chyby monitoringu (null = bez chyby).</summary>
        public DateTime? ChybaMonitoringu { get; set; }
        public DateTime DateInsert { get; set; }

        /// <summary>Odkaz na detail spisu ve webu InfoSoudu.</summary>
        public string DetailUrl { get; set; } = "";

        public List<string>? Varovani { get; set; }
    }

    /// <summary>ProblemDetails vracené API při chybě.</summary>
    internal sealed class InfoSoudProblem
    {
        public string? Title { get; set; }
        public string? Detail { get; set; }
        public int? Status { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }

}