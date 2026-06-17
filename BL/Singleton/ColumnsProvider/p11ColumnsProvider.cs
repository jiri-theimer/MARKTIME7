
namespace BL
{
    public class p11ColumnsProvider: ColumnsProviderBase
    {
        public p11ColumnsProvider()
        {
            this.EntityName = "p11Attendance";

            AFDATE("p11Date", "Datum").DefaultColumnFlag = gdc1;
            oc = AF("Den", "Den", "dbo.get_datename(a.p11Date,0)"); oc.FixedWidth = 60;oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;
            oc = AF("Prichod", "Příchod", "CONVERT(varchar(5),a.p11TodayStart,108)"); oc.FixedWidth = 70; oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;
            oc = AF("p11TodayEnd", "Odchod", "CONVERT(varchar(5),a.p11TodayEnd,108)", "string"); oc.FixedWidth = 70; oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;
            oc = AF("VPraciVcPrestavek", "V práci vč. přestávek", "convert(float,DATEDIFF(MINUTE,p11TodayStart,p11TodayEnd))/60.00", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Fond", "Fond hodin", "c22.c22Hours_Work", "num", true); oc.NotShowRelInHeader = true;
            oc.RelSqlInCol = "LEFT OUTER JOIN c22FondCalendar_Date c22 ON a.p11Date=c22.c22Date AND j02x.c21ID=c22.c21ID AND isnull(j02x.j02CountryCode,'CZ')=isnull(c22.c22CountryCode,'CZ')";
            oc = AF("p11Prescas", "Přesčas z docházky2", "convert(float,DATEDIFF(MINUTE,a.p11TodayStart,a.p11TodayEnd))/60.00-isnull(c22.c22Hours_Work,0)", "num", true); oc.NotShowRelInHeader = true;
            

            this.EntityName = "com_dochazka";
            oc = AF("Hodiny_bez_Prestavky", "Hodiny bez přestávky", "a.Hodiny_bez_Prestavky", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_Prestavka", "Přestávka", "a.Hodiny_Prestavka", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_V_Praci", "V práci bez přestávek", "a.Hodiny_V_Praci", "num", true); oc.NotShowRelInHeader = true;

            oc = AF("Hodiny_PrescasKladne", "Přesčas z hodin", "case when a.Hodiny_Prescas>0 then a.Hodiny_Prescas end", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_Prescas", "+- Přesčas z hodin", "a.Hodiny_Prescas", "num", true); oc.NotShowRelInHeader = true;

            oc = AF("Dochazka_PrescasKladne", "Přesčas z docházky1", "case when a.Dochazka_Prescas>0 then a.Dochazka_Prescas end", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Dochazka_Prescas", "+- Přesčas z docházky1", "a.Dochazka_Prescas", "num", true); oc.NotShowRelInHeader = true;

            oc = AF("Hodiny", "Hodiny", "a.Hodiny", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("HodinyFa", "Hodiny Fa", "a.Hodiny_Fa", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("HodinyNefa", "Hodiny Nefa", "a.Hodiny_Nefa", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_Nepritomnost", "Nepřítomnost", "a.Hodiny_Nepritomnost", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_Nemoc", "Nemoc", "a.Hodiny_Nemoc", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_Dovolena", "Dovolená", "a.Hodiny_Dovolena", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_Ocr", "OČR", "a.Hodiny_Ocr", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_Paragraf", "Lékař", "a.Hodiny_Paragraf", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_SickDay", "Sickday", "a.Hodiny_SickDay", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_Obed", "Oběd", "a.Hodiny_Obed", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_RodicovskaDovolena", "Rodičovská dovolená", "a.Hodiny_RodicovskaDovolena", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_NahradniVolnoZaPrescas", "Náhradní volno za přesčas", "a.Hodiny_NahradniVolnoZaPrescas", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_NeplaceneVolno", "Neplacené volno", "a.Hodiny_NeplaceneVolno", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_VojenskeCviceni", "Vojenské cvičení", "a.Hodiny_VojenskeCviceni", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_MaterskaDovolena", "Mateřská dovolená", "a.Hodiny_MaterskaDovolena", "num", true); oc.NotShowRelInHeader = true;
            
            oc = AF("Hodiny_DarovaniKrve", "Darování krve", "a.Hodiny_DarovaniKrve", "num", true); oc.NotShowRelInHeader = true;            

            oc = AF("Hodiny_Other", "Nepřítomnost - Ostatní", "a.Hodiny_Other", "num", true); oc.NotShowRelInHeader = true;

            AppendTimestamp();
        }
    }
}
