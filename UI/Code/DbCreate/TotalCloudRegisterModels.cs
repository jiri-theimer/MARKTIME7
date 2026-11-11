namespace UI.Code.DbCreate
{
    /// <summary>Zadání pro založení nové licence v režimu TotalCloud (jedna databáze, licence rozlišené přes x01ID).</summary>
    public class TotalCloudRegisterRequest
    {
        public string Email { get; set; }           //ověřený e-mail = přihlašovací jméno i doména licence
        public bool IsGoogle { get; set; }          //e-mail ověřil Google, heslo uživatel nezadával
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string RegID { get; set; }
        public string VatID { get; set; }           //z rejstříku
        public string Street { get; set; }          //z rejstříku
        public string City { get; set; }            //z rejstříku
        public string Zip { get; set; }             //z rejstříku
        public string CountryCode { get; set; } = "CZ";
        public string ProfileKey { get; set; }
        public string AppHost { get; set; }
        public string RemoteIp { get; set; }

        public string Login
        {
            get { return this.Email; }
        }
    }

    public class TotalCloudRegisterResult
    {
        public bool Ok { get; set; }
        public string Error { get; set; }           //technický popis chyby - pouze pro správce, uživateli se neukazuje
        public int x01ID { get; set; }
        public int j02ID { get; set; }
        public string Login { get; set; }
        public int Tables { get; set; }
        public int Rows { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>Obsah ověřovacího odkazu. Putuje v adrese zašifrovaný a časově omezený (Data Protection).</summary>
    public class TotalCloudRegisterToken
    {
        public string Email { get; set; }
        public bool IsGoogle { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CountryCode { get; set; }
    }

    /// <summary>Ukázková data podle oboru, který si uživatel zvolí při registraci.</summary>
    public class TotalCloudProfile
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string ClientName { get; set; }
        public string[] Projects { get; set; }
        public string[] Activities { get; set; }    //doplní se k aktivitám z distribuce, pokud tam ještě nejsou

        /// <summary>
        /// Seznam oborů. Texty lze volně měnit; klíč (Key) se nikam neukládá, slouží jen formuláři.
        /// Poslední položka je výchozí pro neznámý klíč.
        /// </summary>
        public static List<TotalCloudProfile> GetList()
        {
            int y = DateTime.Now.Year;
            return new List<TotalCloudProfile>()
            {
                new TotalCloudProfile()
                {
                    Key = "pravo", Name = "Advokacie a právní služby", ClientName = "Ukázkový klient s.r.o.",
                    Projects = new[] { "Smluvní agenda", "Soudní spor (ukázka)" },
                    Activities = new[] { "Právní porada", "Sepis smlouvy", "Studium spisu", "Jednání u soudu" }
                },
                new TotalCloudProfile()
                {
                    Key = "ucto", Name = "Účetnictví, daně, audit", ClientName = "Ukázkový klient s.r.o.",
                    Projects = new[] { $"Účetnictví {y}", "Daňové poradenství" },
                    Activities = new[] { "Účtování dokladů", "Zpracování mezd", "Daňové přiznání", "Konzultace" }
                },
                new TotalCloudProfile()
                {
                    Key = "it", Name = "IT a vývoj softwaru", ClientName = "Ukázkový klient s.r.o.",
                    Projects = new[] { "Vývoj webové aplikace", "Podpora a údržba" },
                    Activities = new[] { "Programování", "Analýza a návrh", "Testování", "Konzultace" }
                },
                new TotalCloudProfile()
                {
                    Key = "projekce", Name = "Architekti a projektanti", ClientName = "Ukázkový klient s.r.o.",
                    Projects = new[] { "Rodinný dům – studie", "Rekonstrukce – prováděcí dokumentace" },
                    Activities = new[] { "Projektová dokumentace", "Jednání s klientem", "Autorský dozor", "Inženýrská činnost" }
                },
                new TotalCloudProfile()
                {
                    Key = "jine", Name = "Jiný obor", ClientName = "Ukázkový klient s.r.o.",
                    Projects = new[] { "Ukázkový projekt", "Průběžná spolupráce" },
                    Activities = new string[] { }
                }
            };
        }

        public static TotalCloudProfile Find(string key)
        {
            var lis = GetList();
            return lis.FirstOrDefault(p => p.Key == key) ?? lis.Last();
        }
    }

    /// <summary>
    /// Ochrana veřejné registrace: počítadla požadavků v paměti aplikace a zámek, aby se licence zakládaly po jedné.
    /// Po restartu aplikace se počítadla nulují.
    /// </summary>
    public static class TotalCloudRegisterGuard
    {
        public const string KindLink = "link";      //žádost o ověřovací odkaz
        public const string KindCreate = "create";  //založená licence

        private class Hit
        {
            public DateTime When;
            public string Kind;
            public string Ip;
            public string Email;
        }

        private static readonly object _lock = new object();
        private static readonly List<Hit> _hits = new List<Hit>();

        /// <summary>Licence se zakládají jedna po druhé.</summary>
        public static readonly SemaphoreSlim EngineLock = new SemaphoreSlim(1, 1);

        public static void Add(string kind, string ip, string email)
        {
            lock (_lock)
            {
                _hits.RemoveAll(p => p.When < DateTime.Now.AddHours(-25));
                _hits.Add(new Hit() { When = DateTime.Now, Kind = kind, Ip = ip, Email = email });
            }
        }

        /// <summary>Počet požadavků daného druhu za posledních N hodin; ip a email zužují výběr, jsou-li zadané.</summary>
        public static int Count(string kind, int hours, string ip = null, string email = null)
        {
            var d = DateTime.Now.AddHours(-hours);
            lock (_lock)
            {
                return _hits.Count(p => p.Kind == kind && p.When >= d && (ip == null || p.Ip == ip) && (email == null || p.Email == email));
            }
        }
    }
}
