using Microsoft.AspNetCore.Mvc;
using UI.Models.Admin;

namespace UI.Controllers.Admin
{
    //Vlastní výrazy zákazníka pro překlad tiskových sestav.
    //Ukládají se do složky sestav klienta: {Factory.ReportFolder}\reports.lang.json (stejný formát jako společný slovník, soubory jsou zaměnitelné).
    //Při překladu mají přednost - viz UI/Code/ReportTranslator.cs a TheReportSupport.GetCustomerLangFile
    public class ReportLangController : BaseController
    {
        private static readonly System.Text.Json.JsonSerializerOptions _json = new System.Text.Json.JsonSerializerOptions()
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public IActionResult Index()
        {
            var v = new ReportLangViewModel();
            if (!TUP(BO.PermValEnum.GR_Admin)) return ViewTup(v, BO.PermValEnum.GR_Admin);

            RefreshState(v);
            v.RowsJson = LoadRowsJson();
            return View(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ReportLangViewModel v)
        {
            if (!TUP(BO.PermValEnum.GR_Admin)) return ViewTup(v, BO.PermValEnum.GR_Admin);
            RefreshState(v);

            List<ReportLangRow> rows;
            try
            {
                rows = System.Text.Json.JsonSerializer.Deserialize<List<ReportLangRow>>(string.IsNullOrEmpty(v.RowsJson) ? "[]" : v.RowsJson, _json);
            }
            catch
            {
                AddMessage("Výrazy se nepodařilo načíst z formuláře.");
                return View(v);
            }
            int intPosted = rows.Count;
            rows = rows.Where(p => p != null && !p.IsEmpty()).ToList();
            if (rows.Count == 0 && intPosted > 0)
            {
                //formulář poslal řádky, ale žádný nemá výraz v žádném jazyce = stránka a server si nerozumí (např. stará verze skriptu v mezipaměti prohlížeče); soubor se nemění
                AddMessage("Výrazy se nepodařilo načíst z formuláře.");
                return View(v);
            }

            //stejný český výraz na dvou řádcích je chyba zadání; řádky bez češtiny (např. jen slovensky a anglicky) se neporovnávají - u nich platí první výskyt
            var dup = rows.Where(p => ReportLang.NormalizeKey(p.Cs).Length > 0).GroupBy(p => ReportLang.NormalizeKey(p.Cs)).FirstOrDefault(p => p.Count() > 1);
            if (dup != null)
            {
                AddMessageTranslated($"{Factory.tra("Výraz je ve slovníku vícekrát")}: {Safe(dup.Key)}");
                return View(v);
            }

            string strPath = TheReportSupport.GetCustomerLangFile(Factory);
            try
            {
                if (rows.Count == 0)
                {
                    if (System.IO.File.Exists(strPath)) System.IO.File.Delete(strPath);
                }
                else
                {
                    string strFolder = System.IO.Path.GetDirectoryName(strPath);
                    if (!System.IO.Directory.Exists(strFolder)) System.IO.Directory.CreateDirectory(strFolder);
                    System.IO.File.WriteAllText(strPath, ReportLang.SerializeFile(rows), new System.Text.UTF8Encoding(true));
                    if (!System.IO.File.Exists(strPath)) throw new System.IO.IOException(strPath);
                }
            }
            catch (Exception ex)
            {
                BO.Code.File.LogError($"{strPath}: {ex.Message}", Factory.CurrentUser.j02Login, "ReportLang.Save");
                AddMessageTranslated($"{Factory.tra("Soubor vlastních výrazů se nepodařilo uložit")}: {Safe(ex.Message)}");
                return View(v);
            }
            BO.Code.File.LogInfo($"{strPath}: {rows.Count}", Factory.CurrentUser.j02Login, "ReportLang.Save");    //plná cesta jen do logu, uživateli se neukazuje

            v.RowsJson = LoadRowsJson();
            v.Javascript_CallOnLoad = "$(function () { _window_close(); });";     //po uložení se okno zavře (_window_close je funkce layoutu, proto až po načtení stránky)
            return View(v);
        }

        //hlášky vypisuje layout do javascriptového řetězce v uvozovkách - text s uvozovkou, zpětným lomítkem nebo koncem řádku by skript stránky rozbil
        private static string Safe(string s)
        {
            return (s ?? "").Replace("\\", "/").Replace("\"", "'").Replace("\r", " ").Replace("\n", " ");
        }

        private string LoadRowsJson()
        {
            List<ReportLangRow> rows;
            try
            {
                rows = ReportLang.ParseFile(BO.Code.File.GetFileContent(TheReportSupport.GetCustomerLangFile(Factory)));
            }
            catch (System.Text.Json.JsonException ex)
            {
                //ručně poškozený soubor: stránka se otevře prázdná, uložením se soubor přepíše
                AddMessageTranslated($"{Factory.tra("Soubor vlastních výrazů není platný JSON")}: {Safe(ex.Message)}");
                rows = new List<ReportLangRow>();
            }
            return System.Text.Json.JsonSerializer.Serialize(rows, _json);
        }

        private void RefreshState(ReportLangViewModel v)
        {
            var shared = TheReportSupport.LoadLangDictionary(Factory, TheReportSupport.GetSharedLangFile(Factory));
            v.SharedCount = shared?.Rows.Count ?? 0;
            v.SharedRowsJson = System.Text.Json.JsonSerializer.Serialize(shared?.Rows ?? new List<ReportLangRow>(), _json);
        }
    }
}
