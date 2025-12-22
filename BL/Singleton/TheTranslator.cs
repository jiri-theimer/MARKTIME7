

namespace BL.Singleton
{
    public class TheTranslator
    {
        private class TranslateScope
        {
            public string x97Code { get; set; }
            public string Orig { get; set; }
            public string Eng { get; set; }
            public string De { get; set; }
            public string Sk { get; set; }
        }
        
        private HashSet<TranslateScope> _hash;
        private readonly BL.Singleton.RunningApp _app;

        public TheTranslator(BL.Singleton.RunningApp app)
        {
            _app = app;            
            SetupPallete();
        }
        private void SetupPallete()
        {
            var db = new DL.DbHandler((_app.HostingMode == HostingModeEnum.SharedApp ? _app.ConnectStringCloudHeader : _app.ConnectString), new BO.RunningUser(), _app.LogFolder);


            var lis = db.GetList<BO.x97Translate>("select * from x97Translate");
            if (lis==null || lis.Count() == 0)
            {
                _hash = new HashSet<TranslateScope>(0);
                return; //buď v db ještě neexistuje tabulka x97Translate nebo je prázdná
            }
            _hash = new HashSet<TranslateScope>(lis.Count());
            foreach(var rec in lis)
            {
                var c = new TranslateScope() { x97Code = rec.x97Code,Orig=rec.x97Orig, Eng = rec.x97Lang1, De = rec.x97Lang2, Sk = rec.x97Lang4 };
                if (string.IsNullOrEmpty(c.Eng)) c.Eng = "!" + rec.x97Code + "!";
                if (string.IsNullOrEmpty(c.Sk)) c.Sk = "!" + rec.x97Code + "!";

                _hash.Add(c);
            }
        }
        public void Recovery()
        {
            SetupPallete();
        }

        public string DoTranslate(string strCode,int langindex,string strCodeSource=null)
        {
            try
            {
                switch (langindex)
                {
                    case 1:
                        return _hash.First(p => p.x97Code == strCode).Eng;
                       
                    case 2:
                        return _hash.First(p => p.x97Code == strCode).De;
                    case 4:
                        return _hash.First(p => p.x97Code == strCode).Sk;

                    default:
                        return _hash.First(p => p.x97Code == strCode).Orig;
                        
                }                
                
            }
            catch
            {
                if (_app.TranslatorMode == BL.Singleton.TranslateModeEnum.Collect)
                {
                    var db = new DL.DbHandler((_app.HostingMode == HostingModeEnum.SharedApp ? _app.ConnectStringCloudHeader : _app.ConnectString), new BO.RunningUser(), _app.LogFolder);

                    db.RunSql("INSERT INTO x97Translate(x97Code,x97Orig,x97UserInsert,x97UserUpdate,x97DateInsert,x97DateUpdate,x97OrigSource) VALUES(@code,@orig,'collect','collect',GETDATE(),GETDATE(),@origsource)", new { code = strCode,orig=strCode,origsource= strCodeSource });
                    SetupPallete();
                }
                return $"?{strCode}?";
                
            }            
            
        }
    }
}
