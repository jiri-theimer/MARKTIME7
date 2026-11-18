using Microsoft.AspNetCore.Mvc;
using UI.Code.Integrace;
using UI.Models.Integrace;

namespace UI.Controllers
{
    //Odeslání vybraných záznamů přehledu do cizího systému (menu Vybrané záznamy -> Integrace).
    //Připojení se zakládají v administraci (x07Integration); tady se jen vybere připojení a spustí odeslání.
    public class IntegraceController : BaseController
    {
        public const int MaxRecords = 2000;     //nejvyšší počet záznamů na jedno odeslání

        public IActionResult Index(string prefix, string pids, string guid_pids, int x07id)
        {
            if (!string.IsNullOrEmpty(guid_pids))
            {
                pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message;   //delší výběr je předán přes p85Tempbox
            }
            var v = new IntegraceViewModel() { prefix = prefix, pids = pids, SelectedX07ID = x07id, IncludeContactPersons = true };
            RefreshState(v);
            return View(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(IntegraceViewModel v)
        {
            RefreshState(v);
            if (v.IsPostback && v.PostbackOper == "run" && v.RecX07 != null && v.SettingError == null)
            {
                Run(v);
            }
            return View(v);
        }

        private List<int> Pids(IntegraceViewModel v)
        {
            return BO.Code.Bas.ConvertString2ListInt(v.pids ?? "").Where(p => p > 0).Distinct().ToList();
        }

        private void RefreshState(IntegraceViewModel v)
        {
            var pids = Pids(v);
            v.CountSelected = pids.Count;
            v.lisX07 = Factory.x07IntegrationBL.GetList(new BO.myQuery("x07")).Where(p => p.RecordPrefix == v.prefix).OrderBy(p => p.x07Ordinary).ToList();
            if (v.SelectedX07ID == 0 && v.lisX07.Any()) v.SelectedX07ID = v.lisX07.First().pid;
            v.RecX07 = v.lisX07.FirstOrDefault(p => p.pid == v.SelectedX07ID);
            if (v.RecX07 == null) return;

            if (v.CountSelected == 0)
            {
                v.SettingError = "Na vstupu nejsou žádné vybrané záznamy.";
                return;
            }
            if (v.CountSelected > MaxRecords)
            {
                v.SettingError = $"Najednou lze odeslat nejvýše {MaxRecords} záznamů, vybráno je {v.CountSelected}.";
                return;
            }
            var cfg = Factory.x07IntegrationBL.LoadConfig(v.RecX07);

            if (v.RecX07.x07Flag == BO.x07FlagEnum.Ecomail)
            {
                v.lisEcomailLists = new EcomailService(v.RecX07).GetLists(out string strErr);
                if (v.lisEcomailLists == null)
                {
                    v.SettingError = strErr;
                    return;
                }
                if (v.lisEcomailLists.Count == 0)
                {
                    v.SettingError = "V Ecomailu není žádný seznam kontaktů.";
                    return;
                }
                if (string.IsNullOrEmpty(v.SelectedListID) || !v.lisEcomailLists.Any(p => p.Key == v.SelectedListID))
                {
                    v.SelectedListID = v.lisEcomailLists.Any(p => p.Key == cfg.LastListID) ? cfg.LastListID : v.lisEcomailLists.First().Key;
                }
            }

            if (v.RecX07.x07Flag == BO.x07FlagEnum.AbraFlexi)
            {
                v.SettingError = new AbraFlexiService(v.RecX07, cfg).ValidateSetting();
                if (v.SettingError != null) return;

                var lisO23 = Factory.o23DocBL.GetList(new BO.myQueryO23() { pids = pids, IsRecordValid = null });
                var lisO18 = Factory.o18DocTypeBL.GetList(new BO.myQueryO18());
                var sent = Factory.x07IntegrationBL.GetList_x08(v.RecX07.pid, "o23", pids).Select(p => p.x08RecordPid).ToHashSet();
                foreach (var grp in lisO23.GroupBy(p => p.o18ID))
                {
                    var map = FindMap(cfg, grp.Key);
                    string strTyp = lisO18.FirstOrDefault(p => p.pid == grp.Key)?.o18Name ?? "?";
                    int intAgain = grp.Count(p => sent.Contains(p.pid));
                    if (map == null)
                    {
                        v.Preview.Add($"{strTyp}: {grp.Count()}× – neodešle se, typ dokumentu není v nastavení integrace přiřazen k dokladu.");
                    }
                    else
                    {
                        v.Preview.Add($"{strTyp}: {grp.Count()}× → {(map.Evidence == AbraFlexiService.EV_FAKTURA ? "přijatá faktura" : "pokladní výdej")}" + (intAgain > 0 ? $" (z toho {intAgain}× už odesláno dříve, doklad se aktualizuje)" : ""));
                    }
                }
                if (lisO23.Count() < pids.Count) v.Preview.Add($"K {pids.Count - lisO23.Count()} vybraným dokumentům nemáte přístup, neodešlou se.");
            }
        }

        private static BO.x07ConfigDocMap FindMap(BO.x07Config cfg, int o18id)
        {
            return cfg.DocMaps?.FirstOrDefault(p => p.o18ID == o18id && (p.Evidence == AbraFlexiService.EV_FAKTURA || p.Evidence == AbraFlexiService.EV_POKLADNA));
        }

        private void Run(IntegraceViewModel v)
        {
            var cfg = Factory.x07IntegrationBL.LoadConfig(v.RecX07);
            IntegraceResult ret;
            switch (v.RecX07.x07Flag)
            {
                case BO.x07FlagEnum.Ecomail:
                    ret = RunEcomail(v, cfg); break;
                case BO.x07FlagEnum.AbraFlexi:
                    ret = RunAbraFlexi(v, cfg); break;
                default:
                    return;
            }
            v.Result = ret;

            var errs = ret.Rows.Where(p => !p.IsOk && !p.IsSkipped).Select(p => p.Message).Where(p => !string.IsNullOrEmpty(p)).Distinct().Take(5).ToList();
            if (ret.Error != null) errs.Insert(0, ret.Error);
            int intAll = ret.Rows.Count(p => !p.IsSkipped);
            if (ret.Error != null && intAll == ret.CountOk) intAll++;      //chyba celého běhu se musí projevit i ve stavu integrace
            Factory.x07IntegrationBL.AppendLog(v.RecX07.pid, v.prefix, intAll, ret.CountOk, (ret.Summary + (errs.Count > 0 ? " | " + string.Join(" | ", errs) : "")).Trim());
        }

        private IntegraceResult RunEcomail(IntegraceViewModel v, BO.x07Config cfg)
        {
            var lisP28 = Factory.p28ContactBL.GetList(new BO.myQueryP28() { pids = Pids(v) }).ToList();
            var subs = new List<EcomailSubscriber>();
            var skipped = new List<IntegraceRow>();

            foreach (var recP28 in lisP28)
            {
                int intBefore = subs.Count;
                foreach (string strMail in Emails(recP28.pid))
                {
                    var c = new EcomailSubscriber() { p28ID = recP28.pid, email = strMail, street = recP28.p28Street1, city = recP28.p28City1, zip = recP28.p28PostCode1 };
                    if (recP28.p28IsCompany)
                    {
                        c.company = recP28.p28CompanyName;
                    }
                    else
                    {
                        c.name = recP28.p28FirstName; c.surname = recP28.p28LastName; c.pretitle = recP28.p28TitleBeforeName;
                    }
                    subs.Add(c);
                }
                if (v.IncludeContactPersons)
                {
                    foreach (var recP30 in Factory.p28ContactBL.GetList_p30(recP28.pid).Where(p => !p.isclosed))
                    {
                        var recPerson = Factory.p28ContactBL.Load(recP30.p28ID_Person);
                        if (recPerson == null) continue;
                        foreach (string strMail in Emails(recPerson.pid))
                        {
                            subs.Add(new EcomailSubscriber() { p28ID = recP28.pid, email = strMail, name = recPerson.p28FirstName, surname = recPerson.p28LastName, pretitle = recPerson.p28TitleBeforeName, company = recP28.p28CompanyName });
                        }
                    }
                }
                if (subs.Count == intBefore)
                {
                    skipped.Add(new IntegraceRow() { pid = recP28.pid, Name = recP28.p28Name, IsSkipped = true, Message = "Kontakt nemá e-mailovou adresu." });
                }
            }
            //stejná adresa u více kontaktů se pošle jednou
            subs = subs.GroupBy(p => p.email.ToLower()).Select(p => p.First()).ToList();

            var ret = subs.Count > 0 ? new EcomailService(v.RecX07).Subscribe(v.SelectedListID, subs) : new IntegraceResult();
            ret.Rows.AddRange(skipped);
            string strList = v.lisEcomailLists?.FirstOrDefault(p => p.Key == v.SelectedListID)?.Value;
            ret.Summary = $"Ecomail, seznam [{strList}]: kontaktů {lisP28.Count}, adres odesláno {ret.CountOk} z {subs.Count}";

            if (ret.Error == null && cfg.LastListID != v.SelectedListID)
            {
                cfg.LastListID = v.SelectedListID;      //příště nabídnout stejný seznam
                var rec = Factory.x07IntegrationBL.Load(v.RecX07.pid);
                rec.x07Config = System.Text.Json.JsonSerializer.Serialize(cfg);
                Factory.x07IntegrationBL.Save(rec);
            }
            return ret;
        }

        private List<string> Emails(int p28id)
        {
            return Factory.p28ContactBL.GetList_o32(p28id, 0)
                .Where(p => !string.IsNullOrWhiteSpace(p.o32Value) && p.o32Value.Contains("@") && (p.o33ID == BO.o33FlagEnum.Email || p.o33ID == BO.o33FlagEnum.EmailCC || p.o33ID == BO.o33FlagEnum.EmailBCC))
                .Select(p => p.o32Value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private IntegraceResult RunAbraFlexi(IntegraceViewModel v, BO.x07Config cfg)
        {
            var ret = new IntegraceResult();
            var service = new AbraFlexiService(v.RecX07, cfg);
            string strErr = service.TestConnection();
            if (strErr != null)
            {
                ret.Error = strErr;
                ret.Summary = "ABRA Flexi: nic se neodeslalo";
                return ret;
            }

            var lisO23 = Factory.o23DocBL.GetList(new BO.myQueryO23() { pids = Pids(v), IsRecordValid = null }).ToList();
            foreach (var rec in lisO23)
            {
                var map = FindMap(cfg, rec.o18ID);
                if (map == null)
                {
                    ret.Rows.Add(new IntegraceRow() { pid = rec.pid, Name = rec.NameWithCode, IsSkipped = true, Message = $"Typ dokumentu [{rec.o18Name}] není v nastavení integrace přiřazen k dokladu." });
                    continue;
                }
                if (rec.o23FreeDate01 == null || rec.o23FreeNumber01 == 0)
                {
                    ret.Rows.Add(new IntegraceRow() { pid = rec.pid, Name = rec.NameWithCode, Message = "V dokumentu chybí datum nebo částka." });
                    continue;
                }
                var d = new AbraFlexiDoc()
                {
                    pid = rec.pid, Text = rec.o23Name, Code = rec.o23Code, Supplier = rec.o23FreeText01, Date = rec.o23FreeDate01.Value,
                    AmountWithVat = rec.o23FreeNumber01, VatRate = rec.o23FreeNumber02, AmountWithoutVat = rec.o23FreeNumber03, AmountVat = rec.o23FreeNumber04,
                    Currency = (rec.j27ID_Expense > 0 ? Factory.FBL.LoadCurrencyByID(rec.j27ID_Expense)?.j27Code : null) ?? cfg.HomeCurrency ?? "CZK"
                };
                if (d.AmountWithoutVat == 0 && d.AmountVat == 0) d.AmountWithoutVat = d.AmountWithVat;     //vyplněna jen částka včetně DPH
                if (cfg.SendFiles)
                {
                    foreach (var f in Factory.o27AttachmentBL.GetList(new BO.myQueryO27() { o23id = rec.pid }))
                    {
                        d.Files.Add(new AbraFlexiFile() { FileName = f.o27OriginalFileName, ContentType = f.o27ContentType, FullPath = System.IO.Path.Combine(Factory.UploadFolder, f.o27ArchiveFolder ?? "", f.o27ArchiveFileName ?? "") });
                    }
                }
                var row = service.Send(d, map);
                row.Name = rec.NameWithCode;
                if (row.ExternalPid != null) Factory.x07IntegrationBL.SaveBinding(v.RecX07.pid, "o23", rec.pid, row.ExternalPid);
                ret.Rows.Add(row);
            }
            ret.Summary = $"ABRA Flexi: dokladů odesláno {ret.CountOk}, chyb {ret.CountErr}, přeskočeno {ret.CountSkipped}";
            return ret;
        }
    }
}
