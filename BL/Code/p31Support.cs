

using BO.Code;
using DocumentFormat.OpenXml.Drawing;
using System.Text;

namespace BL.Code
{
    public static class p31Support
    {
        public static void UpdateExternalCode(BL.Factory _f, DL.DbHandler _db, int pid,string strCodeValue)
        {
            _db.RunSql("UPDATE p31Worksheet set p31ExternalCode=@ep WHERE p31ID=@pid", new { ep = strCodeValue, pid = pid });            
        }

       
        public static int SaveOrigRecord(BL.Factory _f, DL.DbHandler _db, BO.p31WorksheetEntryInput rec, BO.p33IdENUM p33ID, List<BO.FreeFieldInput> lisFF)
        {
            int intSavedP31ID = 0;bool bolSavedRec = false;
            
            using (var sc = new System.Transactions.TransactionScope())     // ukládání podléhá transakci
            {
                foreach(DateTime d in rec.p31Date)
                {
                    var p = new DL.Params4Dapper();

                    p.Add("pid", rec.pid);
                    if (rec.pid == 0)
                    {
                        p.AddInt("j02ID_Owner", _f.CurrentUser.pid, true);
                        p.AddDouble("p31ExchangeRate_Fixed", 1);
                    }
                    else
                    {
                        p.AddInt("j02ID_Owner", rec.j02ID, true);
                    }

                    p.AddInt("p41ID", rec.p41ID, true);
                    p.AddInt("j02ID", rec.j02ID, true);
                    p.AddInt("o23ID", rec.o23ID, true);
                    p.AddInt("p56ID", rec.p56ID, true);
                    p.AddInt("p32ID", rec.p32ID, true);
                    p.AddInt("p49ID", rec.p49ID, true);
                    p.AddInt("p40ID_Source", rec.p40ID_Source, true);

                    p.AddInt("p72ID_AfterTrimming", (int)rec.p72ID_AfterTrimming, true);
                    p.AddInt("p28ID_Supplier", rec.p28ID_Supplier, true);
                    p.AddInt("p28ID_ContactPerson", rec.p28ID_ContactPerson, true);

                    p.AddString("p31Text", rec.p31Text);
                    p.AddString("p31TextInternal", rec.p31TextInternal);

                    p.AddEnumInt("p31HoursEntryflag", rec.p31HoursEntryflag);
                    //p.AddDateTime("p31Date", rec.p31Date);
                    p.AddDateTime("p31Date", d);
                    p.AddDateTime("p31DateUntil", rec.p31DateUntil);

                    p.AddDouble("p31Value_Orig", rec.p31Value_Orig);
                    p.AddDouble("p31Value_Trimmed", rec.p31Value_Trimmed);
                    p.AddInt("p31Minutes_Orig", rec.p31Minutes_Orig);
                    p.AddInt("p31Minutes_Trimmed", rec.p31Minutes_Trimmed);
                    p.AddString("p31HHMM_Orig", rec.p31HHMM_Orig);
                    p.AddDouble("p31Hours_Orig", rec.p31Hours_Orig);
                    p.AddDouble("p31Hours_Trimmed", rec.p31Hours_Trimmed);
                    p.AddDateTime("p31DateTimeFrom_Orig", rec.p31DateTimeFrom_Orig);
                    p.AddDateTime("p31DateTimeUntil_Orig", rec.p31DateTimeUntil_Orig);

                    p.AddString("p31Value_Orig_Entried", BO.Code.Bas.LeftString(rec.Value_Orig_Entried, 20));

                    p.AddInt("p31BitStream", rec.p31BitStream);
                    if (rec.pid == 0)
                    {
                        p.AddInt("p31MasterID", rec.p31MasterID, true); //vazba na master úkon se ukládá pouze při zapisování úkonu
                    }
                    

                    p.AddString("p31PostRecipient", rec.p31PostRecipient);
                    p.AddString("p31PostCode", rec.p31PostCode);
                    p.AddInt("p31PostFlag", rec.p31PostFlag, true);

                    p.AddString("p31ExternalCode", rec.p31ExternalCode);
                    p.AddString("p31Code", rec.p31Code);

                    if ((p33ID == BO.p33IdENUM.Cas || p33ID == BO.p33IdENUM.Kusovnik) && rec.ManualFee != 0)
                        p.AddDouble("p31Amount_WithoutVat_Orig", rec.ManualFee);    // pevný honorář

                    if (p33ID == BO.p33IdENUM.PenizeBezDPH || p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu)
                    {
                        p.AddDouble("p31Amount_WithoutVat_Orig", rec.p31Amount_WithoutVat_Orig);
                        p.AddDouble("p31VatRate_Orig", rec.VatRate_Orig);
                        p.AddDouble("p31Amount_WithVat_Orig", rec.p31Amount_WithVat_Orig);
                        p.AddDouble("p31Amount_Vat_Orig", rec.p31Amount_Vat_Orig);
                        p.AddInt("j27ID_Billing_Orig", rec.j27ID_Billing_Orig, true);
                        p.AddDouble("p31Calc_Pieces", rec.p31Calc_Pieces);
                        p.AddDouble("p31Calc_PieceAmount", rec.p31Calc_PieceAmount);
                        p.AddInt("p35ID", rec.p35ID, true);
                        p.AddInt("j19ID", rec.j19ID, true);
                        p.AddDouble("p31MarginHidden", rec.p31MarginHidden);
                        p.AddDouble("p31MarginTransparent", rec.p31MarginTransparent);
                    }
                    else
                    {
                        p.AddInt("p54ID", rec.p54ID, true);
                        p.AddInt("p40ID_FixPrice", rec.p40ID_FixPrice, true);
                    }

                    if ((p33ID == BO.p33IdENUM.Cas || p33ID == BO.p33IdENUM.Kusovnik) && rec.ManualFee != 0)
                        p.AddDouble("p31Amount_WithoutVat_Orig", rec.ManualFee);    // pevný honorář pro aktivitu p32ManualFeeFlag=1

                    if (p33ID == BO.p33IdENUM.Cas)
                        p.AddDateTime("p31TimerTimestamp", rec.p31TimerTimestamp);

                    if (rec.p31ValidUntil != null)
                        p.AddDateTime("p31ValidUntil", rec.p31ValidUntil);





                    intSavedP31ID = _db.SaveRecord("p31Worksheet", p, rec, true, true);

                    if (intSavedP31ID > 0)
                    {

                        if (!DL.BAS.SaveFreeFields(_db, intSavedP31ID, lisFF))
                        {
                            return 0;
                        }


                        var pars = new Dapper.DynamicParameters();
                        {

                            pars.Add("p31id", intSavedP31ID, System.Data.DbType.Int32);
                            pars.Add("j02id_sys", _f.CurrentUser.pid, System.Data.DbType.Int32);

                        }

                        if (_db.RunSp("p31_aftersave", ref pars, false) == "1")
                        {
                            //sc.Complete();
                            bolSavedRec = true;
                        }
                        else
                        {
                            return 0;
                        }

                    }
                    else
                    {
                        return 0;
                    }
                }
                //uloženy všechny datumy
                if (bolSavedRec)
                {
                    sc.Complete();
                }
                
            }

            return intSavedP31ID;
        }



        public static List<BO.p31ValidateBeforeSave> ValidateBeforeSaveOrigRecord(BL.Factory _f, DL.DbHandler _db,BO.p31WorksheetEntryInput rec)
        {
            var rets = new List<BO.p31ValidateBeforeSave>();
            
            foreach(DateTime d in rec.p31Date)
            {
                var p = new DL.Params4Dapper();
                p.AddInt("p31id", rec.pid, true);
                p.AddInt("j02id_sys", _f.CurrentUser.pid, true);
                p.AddInt("j02id_rec", rec.j02ID, true);
                p.AddInt("p41id", rec.p41ID, true);
                p.AddInt("o23id", rec.o23ID, true);
                p.AddInt("p56id", rec.p56ID, true);
                p.AddDateTime("p31date", d);
                p.AddInt("p32id", rec.p32ID, true);
                p.AddInt("x01id", _f.CurrentUser.x01ID, true);
                p.AddDouble("p31vatrate_orig", rec.VatRate_Orig);
                p.AddInt("j27id_explicit", rec.j27ID_Billing_Orig, true);
                p.AddString("p31text", rec.p31Text);
                
                if (rec.p31HoursEntryflag == BO.p31HoursEntryFlagENUM.Hodiny)
                {
                    p.AddDouble("value_orig", BO.Code.Time.ShowAsDec(rec.Value_Orig));
                }
                else
                {
                    p.AddDouble("value_orig", BO.Code.Bas.InDouble(rec.Value_Orig));
                }
                
                //p.AddDouble("value_orig", rec.p31Value_Orig);
                p.AddDouble("manualfee", rec.ManualFee);
                p.AddInt("p40id_fixprice", rec.p40ID_FixPrice);
                p.AddString("approve_guid", rec.approve_guid);

                var pars = p.getDynamicDapperPars();

                pars.Add("err", null, System.Data.DbType.String, System.Data.ParameterDirection.Output, 500);
                pars.Add("round2minutes", null, System.Data.DbType.Int32, System.Data.ParameterDirection.Output);
                pars.Add("j27id_domestic", null, System.Data.DbType.Int32, System.Data.ParameterDirection.Output);
                pars.Add("p33id", null, System.Data.DbType.Int32, System.Data.ParameterDirection.Output);
                pars.Add("vatrate", null, System.Data.DbType.Double, System.Data.ParameterDirection.Output);


                var ret = new BO.p31ValidateBeforeSave() { p31Date = d };
                
                string s = _db.RunSp("p31_test_beforesave", ref pars, false);
                if (s == "1")
                {
                    ret.ErrorMessage = pars.Get<string>("err");
                }
                else
                {
                    ret.ErrorMessage = s;
                }
                if (string.IsNullOrEmpty(ret.ErrorMessage))
                {
                    ret.Round2Minutes = pars.Get<int>("round2minutes");
                    ret.j27ID_Domestic = pars.Get<int>("j27id_domestic");
                    ret.p33ID = pars.Get<BO.p33IdENUM>("p33id");
                    if (rec.x15ID == BO.x15IdEnum.Nic)
                    {
                        ret.VatRate = pars.Get<double>("vatrate");
                    }
                    else
                    {
                        ret.VatRate = rec.VatRate_Orig;
                    }
                }

                rets.Add(ret);
            }
            


            return rets;
        }



        public static void SetupTempApproving(BL.Factory _f, IEnumerable<BO.p31Worksheet> lisP31, string strGUID, int intApprovingLevel,bool bolDoDefaultApproveState, BO.p72IdENUM p72id)
        {
            int intLastP41ID = 0; int intP41ID = 0; int intSpatnaKorekceRows = 0;
            

            foreach (var rec in lisP31)
            {
                intP41ID = rec.p41ID;
                var c = new BO.p31WorksheetApproveInput() { p31ID = rec.pid, p33ID = rec.p33ID, Guid = strGUID, p31Date = rec.p31Date, p31ApprovingLevel = rec.p31ApprovingLevel };
                if (rec.p71ID == BO.p71IdENUM.Nic)
                {
                    //dosud neprošlo schvalováním
                    c.Rate_Internal_Approved = rec.p31Rate_Internal_Orig;
                    c.p31ApprovingLevel = intApprovingLevel;
                    if (bolDoDefaultApproveState)
                    {
                        c.p71id = BO.p71IdENUM.Schvaleno;
                        if (rec.p32IsBillable)
                        {
                            c.p72id = BO.p72IdENUM.Fakturovat;
                            if (p72id == BO.p72IdENUM.FakturovatPozdeji) c.p72id = p72id;
                            c.Value_Approved_Billing = rec.p31Value_Orig;
                            c.Value_Approved_Internal = rec.p31Value_Orig;
                            switch (c.p33ID)
                            {
                                case BO.p33IdENUM.Cas:
                                case BO.p33IdENUM.Kusovnik:
                                    c.Rate_Billing_Approved = rec.p31Rate_Billing_Orig;
                                    c.VatRate_Approved = rec.p31VatRate_Orig;
                                    if ((rec.p31Rate_Billing_Orig == 0 && rec.p32ManualFeeFlag == 0) || (rec.p31Amount_WithoutVat_Orig == 0 && rec.p32ManualFeeFlag == 0))
                                    {
                                        c.p72id = BO.p72IdENUM.ZahrnoutDoPausalu;
                                    }
                                    if (rec.p32ManualFeeFlag == 1)
                                    {
                                        c.p32ManualFeeFlag = 1;
                                        c.ManualFee_Approved = rec.p31Amount_WithoutVat_Orig;
                                    }
                                    if (rec.p72ID_AfterTrimming != BO.p72IdENUM._NotSpecified)
                                    {
                                        //uživatel zadal v úkonu výchozí korekci pro schvalování nebo projekt má výchozí koreci úkonu!
                                        c.p72id = rec.p72ID_AfterTrimming;
                                        if (c.p72id == BO.p72IdENUM.Fakturovat)
                                        {
                                            c.Value_Approved_Billing = rec.p31Value_Trimmed;
                                            if (c.Rate_Billing_Approved == 0)
                                            {
                                                //korekce zní na status [fakturovat], ale hodinová sazba je nulová -> je třeba nahodit paušál
                                                c.p72id = BO.p72IdENUM.ZahrnoutDoPausalu;
                                                c.Value_Approved_Billing = 0;
                                                if (intSpatnaKorekceRows == 0)
                                                {
                                                    var cTmpErr = new BO.p85Tempbox() { p85GUID = "err-" + strGUID, p85DataPID = rec.pid, p85FreeText01 = "Minimálně u jednoho úkonu zapisovač navrhl korekcí status [Fakturovat], ale úkon má nulovou fakturační sazbu.<hr>V takovém případě systém nahazuje status [Zahrnout do paušálu]." };
                                                    _f.p85TempboxBL.Save(cTmpErr);

                                                }
                                                intSpatnaKorekceRows += 1;
                                            }
                                        }
                                        else
                                        {
                                            c.Rate_Billing_Approved = 0;
                                            c.Value_Approved_Billing = 0;
                                        }
                                    }
                                    
                                    break;
                                case BO.p33IdENUM.PenizeBezDPH:
                                    if (rec.p31Value_Orig == 0)
                                    {
                                        c.p72id = BO.p72IdENUM.ZahrnoutDoPausalu;
                                    }
                                    break;
                                case BO.p33IdENUM.PenizeVcDPHRozpisu:
                                    c.VatRate_Approved = rec.p31VatRate_Orig;
                                    if (rec.p31Value_Orig == 0)
                                    {
                                        c.p72id = BO.p72IdENUM.ZahrnoutDoPausalu;
                                    }
                                    switch (rec.p72ID_AfterTrimming)
                                    {
                                        case BO.p72IdENUM._NotSpecified:
                                            break;
                                        case BO.p72IdENUM.Fakturovat:
                                        case BO.p72IdENUM.FakturovatPozdeji:
                                            c.p72id = rec.p72ID_AfterTrimming;
                                            c.Value_Approved_Billing = rec.p31Value_Trimmed;
                                            break;
                                        default:
                                            c.p72id = rec.p72ID_AfterTrimming;
                                            c.Value_Approved_Billing = 0;
                                            break;
                                    }
                                    break;
                            }
                            if (rec.p31MarginHidden != 0 || rec.p31MarginTransparent != 0)
                            {
                                //navýšit částku o expense marži
                                c.Value_Approved_Billing = rec.p31Value_Orig + (rec.p31Value_Orig * rec.p31MarginHidden / 100);
                                c.Value_Approved_Billing = c.Value_Approved_Billing + (c.Value_Approved_Billing * rec.p31MarginTransparent / 100);
                            }
                        }
                        else
                        {
                            c.Value_Approved_Internal = rec.p31Value_Orig;
                            if (rec.p72ID_AfterTrimming == BO.p72IdENUM._NotSpecified || rec.p72ID_AfterTrimming == BO.p72IdENUM.FakturovatPozdeji || rec.p72ID_AfterTrimming == BO.p72IdENUM.Fakturovat)
                            {
                                c.p72id = _f.p31WorksheetBL.Get_p72ID_NonBillableWork(c.p31ID);
                            }
                            else
                            {
                                c.p72id = rec.p72ID_AfterTrimming;
                            }
                        }
                    }
                }
                else
                {
                    //již dříve prošlo schvalováním
                    c.p71id = rec.p71ID;
                    c.p72id = rec.p72ID_AfterApprove;
                    c.Value_Approved_Billing = rec.p31Value_Approved_Billing;
                    c.Value_Approved_Internal = rec.p31Value_Approved_Internal;
                    c.VatRate_Approved = rec.p31VatRate_Approved;
                    c.Rate_Billing_Approved = rec.p31Rate_Billing_Approved;
                    c.Rate_Internal_Approved = rec.p31Rate_Internal_Approved;
                    c.p32ManualFeeFlag = rec.p32ManualFeeFlag;
                    if (rec.p32ManualFeeFlag == 1)
                    {
                        c.ManualFee_Approved = rec.p31Amount_WithoutVat_Approved;
                    }
                    if (c.p72id == BO.p72IdENUM.ZahrnoutDoPausalu)
                    {
                        c.p31Value_FixPrice = rec.p31Value_FixPrice;
                    }
                }
                if (p72id != BO.p72IdENUM._NotSpecified && p72id != BO.p72IdENUM.Fakturovat && p72id != BO.p72IdENUM.FakturovatPozdeji && (rec.p33ID == BO.p33IdENUM.Cas || rec.p33ID == BO.p33IdENUM.Kusovnik))
                {
                    c.p72id = p72id;
                    
                    c.Value_Approved_Billing = 0;
                }

                if (!_f.p31WorksheetBL.Save_Approving(c, true, false))
                {

                }

                intLastP41ID = intP41ID;

            }

        }

        public static BO.p31WorksheetApproveInput InhaleApprovingInput(BO.p31Worksheet rec)
        {
            var c = new BO.p31WorksheetApproveInput() { p31ID = rec.pid, p33ID = rec.p33ID, p31Date = rec.p31Date, p31Text = rec.p31Text, p32ID = rec.p32ID, p32ManualFeeFlag = rec.p32ManualFeeFlag };
            c.p71id = rec.p71ID;
            c.p31ApprovingLevel = rec.p31ApprovingLevel;
            c.p72id = rec.p72ID_AfterApprove;

            c.Value_Approved_Billing = rec.p31Value_Approved_Billing;
            c.Value_Approved_Internal = rec.p31Value_Approved_Internal;


            c.Rate_Internal_Approved = rec.p31Rate_Internal_Approved;

            if (c.p72id == BO.p72IdENUM.ZahrnoutDoPausalu)
            {
                c.p31Value_FixPrice = rec.p31Value_FixPrice;
            }
            else
            {
                c.p31Value_FixPrice = 0;
            }

            if (rec.p32ManualFeeFlag == 1)
            {
                c.p32ManualFeeFlag = 1;
                c.ManualFee_Approved = rec.p31Amount_WithoutVat_Approved;
            }
            else
            {
                c.Rate_Billing_Approved = rec.p31Rate_Billing_Approved;
            }

            if (c.p33ID == BO.p33IdENUM.PenizeBezDPH || c.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu)
            {
                c.VatRate_Approved = rec.p31VatRate_Approved;
            }

            return c;
        }


        public static BO.Integrace.IsdocImport ImportIsDoc(string xml, HttpClient hc = null)
        {
            
            if (hc == null)
            {
                hc = new HttpClient();
            }
            string url = "https://mas.marktime.net/IsdocImport";
            var vstup = new BO.Integrace.IsdocImport() { xml = xml };

            var recjson = BO.Code.basJson.SerializeObject(vstup);
            var requestContent = new StringContent(recjson, Encoding.UTF8, "application/json");
           

            var response1 = hc.PostAsync(url, requestContent).Result;
            var ret = response1.Content.ReadAsStringAsync().Result;

            return basJson.DeserializeData<BO.Integrace.IsdocImport>(ret);

            


        }


    }
}
