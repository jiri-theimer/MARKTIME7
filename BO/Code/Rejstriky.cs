using System;

namespace BO.Code
{
    public static class Rejstriky
    {
        public static string Insolvence(string ico=null,string rc=null)
        {
            string s = "https://isir.justice.cz/isir/ueu/vysledek_lustrace.do?ceuprob=x&mesto=&cislo_senatu=&bc_vec=&rocnik=&id_osoby_puvodce=&druh_stav_konkursu=&datum_stav_od=&datum_stav_do=&aktualnost=AKTUALNI_I_UKONCENA&druh_kod_udalost=&datum_akce_od=&datum_akce_do=&nazev_osoby_f=&nazev_osoby_spravce=&rowsAtOnce=50&spis_znacky_datum=&spis_znacky_obdobi=14DNI";
            if (ico != null)
            {
                s += "&ic=" + ico;
            }
            if (rc != null)
            {
                s += "&rc=" + rc;
            }

            return s;
        }
        public static string Insolvence(string jmeno,string prijmeni,DateTime datBirthDate)
        {
            string s = "https://isir.justice.cz/isir/ueu/vysledek_lustrace.do?ceuprob=x&mesto=&cislo_senatu=&bc_vec=&rocnik=&id_osoby_puvodce=&druh_stav_konkursu=&datum_stav_od=&datum_stav_do=&aktualnost=AKTUALNI_I_UKONCENA&druh_kod_udalost=&datum_akce_od=&datum_akce_do=&nazev_osoby_f=&nazev_osoby_spravce=&rowsAtOnce=50&spis_znacky_datum=&spis_znacky_obdobi=14DNI";
            s += "&jmeno_osoby=" + jmeno + "&nazev_osoby=" + prijmeni + "&datum_narozeni=" + Bas.ObjectDate2String(datBirthDate,"dd.MM.yyyy");

            return s;
        }
        public static string Justice(string ico)
        {
            return "https://or.justice.cz/ias/ui/rejstrik-$firma?ico=" + ico;
        }
        public static string Ares(string ico)
        {
            return "http://wwwinfo.mfcr.cz/cgi-bin/ares/darv_res.cgi?jazyk=cz&xml=1&ico=" + ico;
        }
        public static string ObchodnyRegister(string ico)
        {
            return "https://orsr.sk/hladaj_ico.asp?ICO=" + ico;
        }
    }
}
