using System.Collections.Concurrent;

namespace UI.Code.DbCreate
{
    /// <summary>Zadání z formuláře /CreateLicense/Index.</summary>
    public class DbCreateRequest
    {
        //firma = vystavovatel faktur + interní kontakt
        public string CompanyName { get; set; }
        public string RegID { get; set; }
        public string VatID { get; set; }
        public string IcDphSk { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Registration { get; set; }
        public string CountryCode { get; set; }

        //první uživatel (administrátor)
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string LoginName { get; set; }   //část loginu před zavináčem
        public string Password { get; set; }

        //licence
        public string LoginDomain { get; set; } //nasefirma.cz
        public string LicenseName { get; set; }
        public string AppName { get; set; }
        public string AppHost { get; set; }
        public int CurrencyID { get; set; }     //j27ID
        public int LangIndex { get; set; }      //0: česky, 1: english, 4: slovenčina
        public int Round2Minutes { get; set; }
        public int LimitUsers { get; set; }
        public string DbName { get; set; }      //pouze mimo režim SharedApp, jinak se odvozuje z domény

        //první klient a jeho projekty
        public bool CreateClient { get; set; }
        public string ClientCompanyName { get; set; }
        public string ClientRegID { get; set; }
        public string ClientVatID { get; set; }
        public string ClientIcDphSk { get; set; }
        public string ClientStreet { get; set; }
        public string ClientCity { get; set; }
        public string ClientZip { get; set; }
        public string Project1 { get; set; }
        public string Project2 { get; set; }

        public bool SendWelcomeMail { get; set; }

        public string UserLogin
        {
            get
            {
                return $"{this.LoginName}@{this.LoginDomain}";
            }
        }
        public string DomainPrefix
        {
            get
            {
                return (this.LoginDomain ?? "").Split('.')[0];
            }
        }
    }

    public class DbCreateFieldError
    {
        public string field { get; set; }
        public string message { get; set; }
    }

    public static class DbCreateStepStatus
    {
        public const string Pending = "pending";
        public const string Ok = "ok";
        public const string Warning = "warn";
        public const string Error = "error";
        public const string Skipped = "skipped";
    }

    public class DbCreateStep
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Status { get; set; } = DbCreateStepStatus.Pending;
        public string Message { get; set; }
        public List<string> Details { get; set; } = new List<string>();
        public long DurationMs { get; set; }

        public bool IsDone
        {
            get
            {
                return this.Status == DbCreateStepStatus.Ok || this.Status == DbCreateStepStatus.Warning || this.Status == DbCreateStepStatus.Skipped;
            }
        }
    }

    /// <summary>Rozpracované založení jedné databáze. Žije pouze v paměti aplikace.</summary>
    public class DbCreateJob
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime Created { get; set; } = DateTime.Now;
        public DbCreateRequest Request { get; set; }
        public bool IsGuru { get; set; }
        public string RemoteIp { get; set; }

        public string DbName { get; set; }
        public string MasterConnectString { get; set; }
        public string TargetConnectString { get; set; }

        public List<DbCreateStep> Steps { get; set; } = new List<DbCreateStep>();

        //průběžný stav, aby se dal neúspěšný krok bezpečně zopakovat:
        public bool DbCreatedByJob { get; set; }
        public bool IsResume { get; set; }      //databáze už obsahuje licenci téže domény z nedokončeného pokusu
        public bool SqlObjectsDone { get; set; }
        public bool LicenseSaved { get; set; }
        public int InternalP28ID { get; set; }
        public int InternalP41ID { get; set; }
        public int ClientP28ID { get; set; }
        public bool ClientProject1Done { get; set; }
        public bool ClientProject2Done { get; set; }
        public bool Finished { get; set; }

        /// <summary>Převezme stav z předchozího nedokončeného pokusu o založení téže databáze.</summary>
        public void TakeStateFrom(DbCreateJob prev)
        {
            this.DbCreatedByJob = prev.DbCreatedByJob;
            this.IsResume = prev.IsResume;
            this.SqlObjectsDone = prev.SqlObjectsDone;
            this.LicenseSaved = prev.LicenseSaved;
            this.InternalP28ID = prev.InternalP28ID;
            this.InternalP41ID = prev.InternalP41ID;
            this.ClientP28ID = prev.ClientP28ID;
            this.ClientProject1Done = prev.ClientProject1Done;
            this.ClientProject2Done = prev.ClientProject2Done;
            this.SqlLog.Append(prev.SqlLog.ToString());
        }

        public System.Text.StringBuilder SqlLog { get; } = new System.Text.StringBuilder(4000);
        public object Locker { get; } = new object();
    }

    /// <summary>Paměťové úložiště rozpracovaných založení + jednoduchý limit pro anonymní samoobsluhu.</summary>
    public static class DbCreateJobStore
    {
        private static readonly ConcurrentDictionary<string, DbCreateJob> _jobs = new ConcurrentDictionary<string, DbCreateJob>();

        public static void Add(DbCreateJob job)
        {
            Cleanup();
            _jobs[job.Id] = job;
        }

        public static DbCreateJob Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            _jobs.TryGetValue(id, out var job);
            return job;
        }

        /// <summary>Nedokončené založení téže databáze z poslední doby (např. po návratu z chyby zpět do formuláře).</summary>
        public static DbCreateJob FindUnfinished(string dbname)
        {
            return _jobs.Values.Where(p => !p.Finished && p.Created > DateTime.Now.AddHours(-2) && string.Equals(p.DbName, dbname, StringComparison.OrdinalIgnoreCase)).OrderByDescending(p => p.Created).FirstOrDefault();
        }

        public static void Remove(string id)
        {
            _jobs.TryRemove(id, out _);
        }

        public static int CountAnonymousByIp(string ip, int hours)
        {
            return _jobs.Values.Count(p => !p.IsGuru && p.RemoteIp == ip && p.Created > DateTime.Now.AddHours(-hours));
        }

        public static int CountAnonymous(int hours)
        {
            return _jobs.Values.Count(p => !p.IsGuru && p.Created > DateTime.Now.AddHours(-hours));
        }

        private static void Cleanup()
        {
            foreach (var c in _jobs.Values.Where(p => p.Created < DateTime.Now.AddHours(-24)).ToList())
            {
                _jobs.TryRemove(c.Id, out _);
            }
        }
    }
}
