using System.Collections.Concurrent;

namespace BL.Singleton
{
    //Aplikační překlad (en/de/sk): slovník z tabulky x97Translate držený v paměti, klíčem je český text.
    //  - hledání je jedno vyhledání ve slovníku, bez výjimek (čeština se nepřekládá vůbec: text se vrací hned)
    //  - text, který ve slovníku není, se vrací jako ?text?; text bez překladu do daného jazyka jako !text! (aby byl na stránce hned vidět)
    //  - režim Collect (appsettings: App.TranslatorMode): neznámý text se založí do x97Translate a přidá do slovníku
    //  - slovník se z databáze načítá při startu a potom jen na vyžádání přes Recovery() (administrace: Aplikační překlad -> Obnovit slovník)
    public class TheTranslator
    {
        private sealed class Entry
        {
            public string Eng;
            public string De;
            public string Sk;
        }

        private volatile ConcurrentDictionary<string, Entry> _dict = new ConcurrentDictionary<string, Entry>(StringComparer.Ordinal);
        private readonly BL.Singleton.RunningApp _app;

        public TheTranslator(BL.Singleton.RunningApp app)
        {
            _app = app;
            SetupPallete();
        }

        private DL.DbHandler Db()
        {
            return new DL.DbHandler((_app.HostingMode == HostingModeEnum.SharedApp ? _app.ConnectStringCloudHeader : _app.ConnectString), new BO.RunningUser(), _app.LogFolder);
        }

        //SharedApp: slovník je společný pro všechny zákazníky v centrální databázi a7_cloudheader
        private string TableName()
        {
            return _app.HostingMode == HostingModeEnum.SharedApp ? "a7_cloudheader.dbo.x97Translate" : "x97Translate";
        }

        private static Entry NewEntry(string strCode, string eng, string de, string sk)
        {
            string strMissing = "!" + strCode + "!";    //výraz je ve slovníku, ale překlad do jazyka chybí
            return new Entry()
            {
                Eng = string.IsNullOrEmpty(eng) ? strMissing : eng,
                De = string.IsNullOrEmpty(de) ? strMissing : de,
                Sk = string.IsNullOrEmpty(sk) ? strMissing : sk
            };
        }

        private void SetupPallete()
        {
            var lis = Db().GetList<BO.x97Translate>("select x97Code,x97Lang1,x97Lang2,x97Lang4 from " + TableName());
            var dict = new ConcurrentDictionary<string, Entry>(StringComparer.Ordinal);
            if (lis != null)    //null = v db ještě neexistuje tabulka x97Translate
            {
                foreach (var rec in lis)
                {
                    if (string.IsNullOrEmpty(rec.x97Code)) continue;
                    dict[rec.x97Code] = NewEntry(rec.x97Code, rec.x97Lang1, rec.x97Lang2, rec.x97Lang4);   //případný duplicitní kód: platí poslední
                }
            }
            _dict = dict;   //výměna celého slovníku naráz: běžící požadavky dočtou ze starého
        }

        //znovu načte slovník z databáze; vrací počet výrazů
        public int Recovery()
        {
            SetupPallete();
            return _dict.Count;
        }

        public int Count
        {
            get { return _dict.Count; }
        }

        public string DoTranslate(string strExpression, string strCulture, string strCodeSource = null)
        {
            int intLang;
            switch (strCulture)
            {
                case "en-US": intLang = 1; break;
                case "de-DE": intLang = 2; break;
                case "sk-SK": intLang = 4; break;
                default:
                    return strExpression;   //čeština a neznámé jazyky: bez překladu
            }
            if (string.IsNullOrEmpty(strExpression)) return strExpression;

            if (_dict.TryGetValue(strExpression, out Entry c))
            {
                return intLang == 1 ? c.Eng : (intLang == 2 ? c.De : c.Sk);
            }

            if (_app.TranslatorMode == BL.Singleton.TranslateModeEnum.Collect)
            {
                //založit výraz do slovníku (porovnání v db přesně včetně velikosti písmen, stejně jako slovník v paměti); TryAdd pustí k zápisu do db jen první ze souběžných požadavků
                if (_dict.TryAdd(strExpression, NewEntry(strExpression, null, null, null)))
                {
                    try
                    {
                        Db().RunSql("IF NOT EXISTS(SELECT 1 FROM " + TableName() + " WHERE x97Code COLLATE Latin1_General_100_BIN2=@code) INSERT INTO " + TableName() + "(x97Code,x97Orig,x97UserInsert,x97UserUpdate,x97DateInsert,x97DateUpdate,x97OrigSource) VALUES(@code,@orig,'collect','collect',GETDATE(),GETDATE(),@origsource)", new { code = strExpression, orig = strExpression, origsource = strCodeSource });
                    }
                    catch
                    {
                        _dict.TryRemove(strExpression, out _);     //zápis se nepovedl: zkusit příště znovu
                    }
                }
            }
            return "?" + strExpression + "?";    //výraz ve slovníku není
        }
    }
}
