namespace UI.Models.p31approve
{
    public class p31approveMother : BaseViewModel
    {
        public string p31guid { get; set; }
        public string prefix { get; set; }

        public IEnumerable<BO.p31Worksheet> lisP31 { get; set; }
        public IEnumerable<BO.p41Project> lisP41 { get; set; }

        public int p31RecordsCount { get; set; }    //celkkový počet úkonů v lisP31
        public int p91id { get; set; }
        public BO.p91Invoice RecP91_Append2Invoice { get; set; }

        public List<NavTab> OverGridTabs { get; set; }

       
        

        public GridRecord LoadGridRecord(BL.Factory f, int p31id, string p31guid,BO.p31Worksheet rec=null)
        {
            if (p31id == 0) return new GridRecord();
            if (rec==null) rec = f.p31WorksheetBL.LoadTempRecord(p31id, p31guid);
            if (rec == null)
            {
                return new GridRecord() { errormessage = "Záznam nelze načíst: rec is null" };
            }
            //var recP41 = f.p41ProjectBL.Load(rec.p41ID);
            
            var c = new GridRecord() {p31guid=p31guid,pid=rec.pid, Datum = BO.Code.Bas.ObjectDate2String(rec.p31Date), Popis = rec.p31Text,PopisInterni=rec.p31TextInternal, Jmeno = rec.Person, Projekt = rec.Project, Aktivita = rec.p32Name,Sesit=rec.p34Name, p33id = (int)rec.p33ID };
            c.b05id_last = rec.b05ID_Last;
            c.p71id = (int)rec.p71ID;
            c.p72id = (int)rec.p72ID_AfterApprove;
            c.p40id_fixprice = rec.p40ID_FixPrice;
            c.p40name = rec.p40Name;
            c.fakturovatelne = rec.p32IsBillable;
            c.tabquery = rec.tabquery();
            //c.pl = f.getP07Level(recP41.p07Level, true);
            c.p28name = rec.ClientName;
            c.p41name = rec.p41NameShort == null ? rec.p41Name : rec.p41NameShort;
            c.p56name = rec.p56Name;
            c.uroven = rec.p31ApprovingLevel;
            c.sazba = rec.p31Rate_Billing_Approved;
            c.bezdph = rec.p31Amount_WithoutVat_Approved;
            c.j27code = rec.j27Code_Billing_Orig;
            c.dphsazba = rec.p31VatRate_Approved;
            c.timestamp_insert = rec.UserInsert + "/" + BO.Code.Time.GetTimestamp(rec.DateInsert);
            if (Convert.ToDateTime(rec.DateUpdate).AddMinutes(-10) > rec.DateInsert)
            {
                c.timestamp_update = rec.UserUpdate + "/" + BO.Code.Time.GetTimestamp(rec.DateUpdate);
            }


            switch (rec.p33ID)
            {
                case BO.p33IdENUM.Cas:
                    c.vykazano = format_hodiny(f, rec.p31Hours_Orig);
                    
                    c.vykazano_sazba = BO.Code.Bas.Number2String(rec.p31Rate_Billing_Orig) + " " + rec.j27Code_Billing_Orig;
                    c.honorar_orig= BO.Code.Bas.Number2String(rec.p31Amount_WithoutVat_Orig) + " " + rec.j27Code_Billing_Orig;
                    break;
                case BO.p33IdENUM.Kusovnik:
                    c.vykazano = BO.Code.Bas.Number2String(rec.p31Value_Orig);
                    c.vykazano_sazba = BO.Code.Bas.Number2String(rec.p31Rate_Billing_Orig) + " " + rec.j27Code_Billing_Orig;
                    c.honorar_orig = BO.Code.Bas.Number2String(rec.p31Amount_WithoutVat_Orig) + " " + rec.j27Code_Billing_Orig;
                    break;
                default:
                    c.vykazano = BO.Code.Bas.Number2String(rec.p31Amount_WithoutVat_Orig) + " " + rec.j27Code_Billing_Orig;
                    break;

            }
            if (rec.p71ID == BO.p71IdENUM.Schvaleno && (rec.p33ID == BO.p33IdENUM.Cas || rec.p33ID == BO.p33IdENUM.Kusovnik))
            {
                
                if (rec.p31Hours_Approved_Billing != rec.p31Hours_Orig)
                {
                    c.rozdil_vykazano_schvaleno_hodnota = format_hodiny(f, rec.p31Hours_Approved_Billing - rec.p31Hours_Orig);
                    if (rec.p31Hours_Approved_Billing > rec.p31Hours_Orig)
                    {
                        c.rozdil_vykazano_schvaleno_hodnota = $"+{c.rozdil_vykazano_schvaleno_hodnota}";
                    }
                }

                if (rec.p31Rate_Billing_Approved != rec.p31Rate_Billing_Orig)
                {
                    c.rozdil_vykazano_schvaleno_sazba = $"{BO.Code.Bas.Number2String(rec.p31Rate_Billing_Approved-rec.p31Rate_Billing_Orig)} {c.j27code}";
                    if (rec.p31Rate_Billing_Approved > rec.p31Rate_Billing_Orig)
                    {
                        c.rozdil_vykazano_schvaleno_sazba = $"+{c.rozdil_vykazano_schvaleno_sazba}";
                    }
                }
                if (c.rozdil_vykazano_schvaleno_hodnota !=null || c.rozdil_vykazano_schvaleno_sazba != null)
                {
                    c.rozdil_vykazano_schvaleno_bezdph = $"{BO.Code.Bas.Number2String(rec.p31Rate_Billing_Approved*rec.p31Hours_Approved_Billing - rec.p31Rate_Billing_Orig*rec.p31Hours_Orig)} {c.j27code}";
                    if (rec.p31Rate_Billing_Approved * rec.p31Hours_Approved_Billing - rec.p31Rate_Billing_Orig * rec.p31Hours_Orig > 0)
                    {
                        c.rozdil_vykazano_schvaleno_bezdph = $"+{c.rozdil_vykazano_schvaleno_bezdph}";
                    }

                }
               
                c.honorar_schvaleno = BO.Code.Bas.Number2String(rec.p31Value_Approved_Billing*rec.p31Rate_Billing_Approved) + " " + rec.j27Code_Billing_Orig;
                
            }
            
            if (rec.p71ID == BO.p71IdENUM.Schvaleno && (rec.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu || rec.p33ID == BO.p33IdENUM.PenizeBezDPH))
            {
                if (rec.p31Amount_WithoutVat_Approved != rec.p31Amount_WithoutVat_Orig)
                {
                    c.rozdil_vykazano_schvaleno_hodnota = $"{BO.Code.Bas.Number2String(rec.p31Amount_WithoutVat_Approved - rec.p31Amount_WithoutVat_Orig)} {c.j27code}";
                    if (rec.p31Amount_WithoutVat_Approved > rec.p31Amount_WithoutVat_Orig)
                    {
                        c.rozdil_vykazano_schvaleno_hodnota = $"+{c.rozdil_vykazano_schvaleno_hodnota}";
                    }
                    c.rozdil_vykazano_schvaleno_bezdph = c.rozdil_vykazano_schvaleno_hodnota;
                }
                

            }
            if (c.p33id == 1)
            {
                c.hodiny = format_hodiny(f, rec.p31Hours_Approved_Billing);
            }
            if (c.p33id == 3)
            {
                c.hodiny = rec.p31Value_Approved_Billing.ToString();
            }
            c.hodinypausal = format_hodiny(f, rec.p31Value_FixPrice);
            c.hodinyinterni = format_hodiny(f, rec.p31Hours_Approved_Internal);

            c.emotion = get_emotion(rec);
            c.cssclass = UI.Code.TheGridRowSymbol.p31_tr_cssclass(rec);

            return c;
        }


        private string format_hodiny(BL.Factory f, double hodiny)
        {
            if (f.CurrentUser.j02DefaultHoursFormat == "T" || hodiny.ToString().Length > 5)
            {
                return BO.Code.Time.ShowAsHHMM(hodiny.ToString());
            }
            else
            {
                return BO.Code.Bas.Number2String(hodiny);
            }
        }
        private string get_emotion(BO.p31Worksheet rec)
        {
            if (rec.p71ID == BO.p71IdENUM.Nic)
            {
                return null;
            }
            if (rec.p71ID == BO.p71IdENUM.Neschvaleno)
            {
                return "<span style='color:red;'>😡</span>";
            }
            switch (rec.p72ID_AfterApprove)
            {
                case BO.p72IdENUM.FakturovatPozdeji:
                    return "<span style='color:#ffd700;'>😉</span>";
                case BO.p72IdENUM.ZahrnoutDoPausalu:
                    return "<span style='color:pink;'>😏</span>";
                case BO.p72IdENUM.SkrytyOdpis:
                    return "<span style='color:red;'>😖</span>";
                case BO.p72IdENUM.ViditelnyOdpis:
                    return "<span style='color:brown;'>😒</span>";
                case BO.p72IdENUM.Fakturovat:
                    return "<span style='color:brown;'>😀</span>";
            }
            if (rec.p72ID_AfterApprove == BO.p72IdENUM.ViditelnyOdpis)
            {
                return "<span style='color:red;'>😡</span>";
            }

            return null;
        }
    }
}
