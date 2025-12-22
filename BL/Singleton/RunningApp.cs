
using Microsoft.Extensions.Configuration;

namespace BL.Singleton
{
    public enum HostingModeEnum
    {
        None=0,      //jedna aplikace a jedna databáze AppConnection s jediným x01 záznamem
        SharedApp =1,    //jedna aplikace, která se připojuje do N databází
        TotalCloud=2    //jedna aplikace a jediná sendvičová databáze s více x01 záznamy

    }
    public enum TranslateModeEnum
    {
        None=0,
        Collect=1
    }
    public class RunningApp
    {
        public RunningApp()
        {
            _AppRootFolder = System.IO.Directory.GetCurrentDirectory();
            var config = new ConfigurationBuilder().AddJsonFile(_AppRootFolder + "\\appsettings.json", true).Build();

            this.Configuration = config;

            _ConnectString = config.GetSection("ConnectionStrings")["AppConnection"];
            
            _HostingMode = Configuration.GetSection("App").GetValue<HostingModeEnum>("HostingMode");
            
            _AllowGoogleLogin = Configuration.GetSection("App").GetValue<Boolean>("AllowGoogleLogin");
            _ConnectStringCloudHeader = Configuration.GetSection("ConnectionStrings")["CloudHeader"];
            _AppName = Configuration.GetSection("App")["Name"];
            _TranslatorMode = Configuration.GetSection("App").GetValue<TranslateModeEnum>("TranslatorMode");
            _LogFolder = Configuration.GetSection("Folders")["Log"];
            if (string.IsNullOrEmpty(_LogFolder))
            {
                _LogFolder = System.IO.Directory.GetCurrentDirectory() + "\\Logs";
            }
            RefreshX01List();
            
            
            RefreshP07List();
            if (_AppName != "API")
            {
                
                RefreshO17List();

                var db = GetDbHandle();
                if (this.HostingMode == BL.Singleton.HostingModeEnum.SharedApp)
                {
                    var cDL = new DL.HostingTasks(db);
                    cDL.UpdateCloudHeader_X01();    //narovnat tabulku a7_cloudheader.dbo.x01License

                }
            }
            
        }

        public string ParseConnectStringFromLogin(string login)
        {
            return this.ConnectString.Replace("app_dbname", $"a7{login.Split("@")[1].Split(".")[0]}");
        }
        public string ParseConnectStringFromDbname(string dbname)
        {
            var arr = BO.Code.Bas.ConvertString2List(this.ConnectString, ";");
            for(int i = 0; i < arr.Count(); i++)
            {
                if (arr[i].ToLower().Replace(" ", "").Contains("database="))
                {
                    arr[i] = $"database={dbname}";
                }
            }

            return string.Join(";", arr);
        }
        public string ParseDbNameFromConnectString()
        {
            var arr = BO.Code.Bas.ConvertString2List(this.ConnectString, ";");
            for (int i = 0; i < arr.Count(); i++)
            {
                string s = arr[i].ToLower().Replace(" ", "");
                if (s.Contains("database="))
                {
                    return s.Replace("database=", "");
                }
            }
            return null;
        }

        private DL.DbHandler GetDbHandle()
        {
            return new DL.DbHandler((_HostingMode == HostingModeEnum.SharedApp ? _ConnectStringCloudHeader : _ConnectString), new BO.RunningUser(), _LogFolder);
        }

        public void RefreshX01List()
        {            
            var db = GetDbHandle();            
            this.lisX01 = db.GetList<BO.x01License>($"select a.*,{db.GetSQL1_Ocas("x01")} from x01License a");
            
        }
        public void RefreshP07List()
        {
            var db = GetDbHandle();
            this.lisP07 = db.GetList<BO.p07ProjectLevel>($"select a.*,{db.GetSQL1_Ocas("p07")} from p07ProjectLevel a ORDER BY a.p07ID DESC");
        }
        public void RefreshO17List()
        {
            var db = GetDbHandle();
            this.lisO17 = db.GetList<BO.o17DocMenu>("SELECT o17ID as pid, * FROM o17DocMenu WHERE GETDATE() BETWEEN o17ValidFrom AND o17ValidUntil");
        }

        
        public IEnumerable<BO.x01License> lisX01 { get; set; }
        public IEnumerable<BO.p07ProjectLevel> lisP07 { get; set; }
        
        public IEnumerable<BO.o17DocMenu> lisO17 { get; set; }
        public IConfiguration Configuration { get; private set; }

        private string _ConnectString { get; set; }
        private string _ConnectStringCloudHeader { get; set; }
        private string _AppName { get; set; }
        private string _AppRootFolder { get; set; }
        private TranslateModeEnum _TranslatorMode { get; set; }
        private string _LogFolder { get; set; }
        private HostingModeEnum _HostingMode { get; set; }
        private bool _AllowGoogleLogin { get; set; }

        public HostingModeEnum HostingMode
        {
            get
            {
                return _HostingMode;
            }
        }


        public string ConnectString {
            get
            {
                return _ConnectString;
            }
        }
        public string LogFolder { get {
                return _LogFolder;
            }
        }
        

        public bool IsRobot { get {
                return Configuration.GetSection("App").GetValue<Boolean>("IsRobot");
            } 
        }
        public string RobotUrl
        {
            get
            {
                return Configuration.GetSection("App").GetValue<string>("RobotUrl");
            }
        }
       
        public string AppRootFolder { get
            {
                return _AppRootFolder;
            }
        }
        public string WwwRootFolder { get; set; }       //předává z venku Startup přes IWebHostEnvironment
        public string AppName { get
            {
                return _AppName;
            }
        }
        public string AppVersion {
            get
            {
                return Configuration.GetSection("App")["Version"];
            }
        }
        public string AppBuild
        {
            get
            {
                var execAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                var versionTime = new System.IO.FileInfo(execAssembly.Location).LastWriteTime;

                return "build: " + BO.Code.Bas.ObjectDateTime2String(versionTime);
            }
        }
        public TranslateModeEnum TranslatorMode {
            get
            {
                return _TranslatorMode;
            }
        }

        public string RootUploadFolder
        {
            get
            {
                return Configuration.GetSection("Folders")["RootUpload"];
            }
        }
        

        public int DefaultLangIndex { get {
                return BO.Code.Bas.InInt(Configuration.GetSection("App")["DefaultLangIndex"]);
            }
        }
        public string Implementation { get {
                return Configuration.GetSection("App")["Implementation"];
            }
        }

        
        public bool AllowGoogleLogin
        {
            get
            {
                return _AllowGoogleLogin;
            }
        }
        public string GuruLogin { get
            {
                return Configuration.GetSection("Guru")["Login"];
            }
        }
        public string GuruPassword { get
            {
                return Configuration.GetSection("Guru")["Password"];
            }
        }
       
        public string ConnectStringCloudHeader
        {
            get
            {
                return _ConnectStringCloudHeader;
            }
        }
        

    }
}
