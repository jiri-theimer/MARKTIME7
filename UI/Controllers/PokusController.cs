using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;


using Microsoft.AspNetCore.Mvc;
using UI.Models;

namespace UI.Controllers
{
    public class PokusController : BaseController
    {
        static readonly string openAiApiKey = "";
        static readonly string openAiEndpoint = "https://api.openai.com/v1/chat/completions";

        public IActionResult Canvas(int x31id, string code, string caller_prefix, string caller_pids, string guid_pids)
        {
            if (x31id == 0 && !string.IsNullOrEmpty(code))
            {
                x31id = Factory.x31ReportBL.LoadByCode(code, 0).pid;
            }
            var v = new ReportNoContextFrameworkViewModel() { caller_prefix = caller_prefix, caller_pids = caller_pids };
            if (!string.IsNullOrEmpty(guid_pids))
            {
                v.caller_pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }

            if (!Factory.CurrentUser.j04IsModule_x31)
            {
                return this.StopPage(false, "Nemáte oprávnění pro tuto stránku.");
            }
            if (x31id == 0)
            {
                x31id = Factory.CBL.LoadUserParamInt("x31/last-reportnocontext-x31id");
            }
            if (x31id > 0)
            {
                v.SelectedReport = Factory.x31ReportBL.Load(x31id);
            }
            v.lisX31 = Factory.x31ReportBL.GetList(new BO.myQueryX31()).Where(p => p.x31Entity == null);
            switch (v.caller_prefix)
            {
                case "p41":
                case "le5":
                    v.lisX31 = v.lisX31.Where(p => p.x31QueryFlag == BO.x31QueryFlagENUM.p41 || p.x31QueryFlag == BO.x31QueryFlagENUM.p31 || p.x31QueryFlag == BO.x31QueryFlagENUM.p91); break;
                case "p28":
                    v.lisX31 = v.lisX31.Where(p => p.x31QueryFlag == BO.x31QueryFlagENUM.p28 || p.x31QueryFlag == BO.x31QueryFlagENUM.p41 || p.x31QueryFlag == BO.x31QueryFlagENUM.p31 || p.x31QueryFlag == BO.x31QueryFlagENUM.p91); break;
                case "p56":
                    v.lisX31 = v.lisX31.Where(p => p.x31QueryFlag == BO.x31QueryFlagENUM.p56 || p.x31QueryFlag == BO.x31QueryFlagENUM.p41 || p.x31QueryFlag == BO.x31QueryFlagENUM.p31 || p.x31QueryFlag == BO.x31QueryFlagENUM.p91); break;
                case "p91":
                    v.lisX31 = v.lisX31.Where(p => p.x31QueryFlag == BO.x31QueryFlagENUM.p91 || p.x31QueryFlag == BO.x31QueryFlagENUM.p31); break;
                case "j02":
                    v.lisX31 = v.lisX31.Where(p => p.x31QueryFlag == BO.x31QueryFlagENUM.j02 || p.x31QueryFlag == BO.x31QueryFlagENUM.p31 || p.x31QueryFlag == BO.x31QueryFlagENUM.p91); break;
                case "p31":
                    v.lisX31 = v.lisX31.Where(p => p.x31QueryFlag == BO.x31QueryFlagENUM.p31); break;
            }

            var qry = v.lisX31.Select(p => new { p.j25ID, p.j25Name, p.j25Ordinary }).Distinct();
            v.lisJ25 = new List<BO.j25ReportCategory>();
            foreach (var c in qry)
            {
                var cc = new BO.j25ReportCategory() { pid = c.j25ID, j25Name = c.j25Name, j25Ordinary = c.j25Ordinary, j25Code = "accordion-button collapsed py-2" };
                if (cc.j25Name == null)
                {
                    cc.j25Ordinary = -999999;
                    cc.j25Name = Factory.tra("Bez kategorie");
                }
                if (v.SelectedReport != null && c.j25ID == v.SelectedReport.j25ID)
                {
                    cc.j25Code = "accordion-button py-2";
                }
                cc.j25Name += " (" + v.lisX31.Where(p => p.j25ID == cc.pid).Count().ToString() + ")";
                v.lisJ25.Add(cc);
            }
            return View(v);

        }


        public async Task<IActionResult> Index()
        {

            var v = new BaseViewModel();



            //var xxx = await Pokus("Jak resetovat heslo nebo založit fakturu?");


            //BO.Code.File.WriteText2File("c:\\temp\\hovado_vystup_markdown.txt", xxx);
            //BO.Code.File.WriteText2File("c:\\temp\\hovado_vystup_html.txt", Markdig.Markdown.ToHtml(xxx));

            //v.PageSymbol = Markdig.Markdown.ToHtml(xxx);
            
            return View(v);

        }

        public async Task<string> Pokus(string strDotaz)
        {
            
            StringBuilder contextBuilder = new();
            

            contextBuilder.AppendLine(System.IO.File.ReadAllText("c:\\temp\\hovado.txt"));

            string kontext = contextBuilder.ToString();

            string odpoved = await ZavolejOpenAI(strDotaz, kontext);

            return odpoved;

        }


        public async Task<string> ZavolejOpenAI(string dotaz, string kontext)
        {
            using HttpClient client = new();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);

            //var dotazDoAI = new
            //{
            //    model = "gpt-4",  // nebo "gpt-3.5-turbo"
            //    messages = new[]
            //    {
            //    new { role = "system", content = "Odpovídej jako chytrý chatbot na základě poskytnutého textu. Do odpovědi uveď ID textu, ze kterého čerpáš." },
            //    new { role = "user", content = $"Zde je obsah několika dokumentů:\n\n{kontext}\n\nDotaz: {dotaz}" }
            //},
            //    max_tokens = 500,
            //    temperature = 0.7
            //};

            var dotazDoAI = new
            {
                model = "gpt-4",  // nebo "gpt-3.5-turbo"
                messages = new[]
                {
                new { role = "system", content = "Jsi chytrý chatbot, který odpovídá pouze na základě poskytnutých textů." +
                " U každé odpovědi uveď ID textu (např. [zdroj: HELP_MT_001]) ze kterého informace pochází." +
                " Nepřidávej vlastní znalosti. Pokud odpověď není ve zdrojích, napiš 'Odpověď nelze určit ze zadaných dokumentů.'" },
                new { role = "user", content = $"Zde je obsah několika dokumentů:\n\n{kontext}\n\nDotaz: {dotaz}" }
            },
                max_tokens = 500,
                temperature = 0.7
            };

            var response = await client.PostAsJsonAsync(openAiEndpoint, dotazDoAI);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Chyba API: {response.StatusCode}");
                return "Došlo k chybě při komunikaci s OpenAI.";
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            
            var message = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return message.Trim();
        }

    }
}
