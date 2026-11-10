using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Dapper;

namespace DL
{
    /// <summary>
    /// Nízkoúrovňové SQL operace pro průvodce založením nové zákaznické databáze (/CreateLicense/Index).
    /// Na rozdíl od DbHandler chyby nepolyká: metody buď vyhazují výjimku, nebo vrací text chyby,
    /// aby průvodce mohl u každého kroku ukázat, co přesně se nepovedlo.
    /// </summary>
    public class DbCreateTasks
    {
        private readonly string _conString;
        private readonly int _timeoutSeconds;

        public DbCreateTasks(string connectstring, int timeoutSeconds = 300)
        {
            _conString = connectstring;
            _timeoutSeconds = timeoutSeconds;
        }

        /// <summary>Název databáze smí obsahovat pouze písmena bez diakritiky, číslice a podtržítko (max. 60 znaků).</summary>
        public static bool IsValidDbName(string dbname)
        {
            if (string.IsNullOrEmpty(dbname)) return false;
            return Regex.IsMatch(dbname, "^[A-Za-z][A-Za-z0-9_]{1,59}$");
        }

        /// <summary>Vrátí connect-string se zaměněným názvem databáze, ostatní parametry (server, login) zůstávají.</summary>
        public static string BuildConnectString(string baseConnectString, string dbname)
        {
            var b = new SqlConnectionStringBuilder(baseConnectString);
            b.InitialCatalog = dbname;
            return b.ConnectionString;
        }

        /// <summary>Název SQL serveru z connect-stringu - pouze pro zobrazení v průvodci.</summary>
        public static string GetServerName(string connectString)
        {
            try
            {
                return new SqlConnectionStringBuilder(connectString).DataSource;
            }
            catch
            {
                return null;
            }
        }

        public T Scalar<T>(string strSQL, object param = null)
        {
            using (var con = new SqlConnection(_conString))
            {
                return con.ExecuteScalar<T>(strSQL, param, null, _timeoutSeconds);
            }
        }

        /// <summary>Provede SQL, při chybě vyhazuje výjimku.</summary>
        public int Exec(string strSQL, object param = null)
        {
            using (var con = new SqlConnection(_conString))
            {
                return con.Execute(strSQL, param, null, _timeoutSeconds);
            }
        }

        /// <summary>Provede SQL, vrací NULL při úspěchu, jinak text chyby.</summary>
        public string TryExec(string strSQL, object param = null)
        {
            try
            {
                Exec(strSQL, param);
                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public System.Data.DataTable GetDataTable(string strSQL)
        {
            var dt = new System.Data.DataTable();
            using (var con = new SqlConnection(_conString))
            using (var cmd = new SqlCommand(strSQL, con))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandTimeout = _timeoutSeconds;
                adapter.Fill(dt);
            }
            return dt;
        }

        /// <summary>Volat nad connect-stringem do databáze master.</summary>
        public bool DatabaseExists(string dbname)
        {
            return Scalar<int>("SELECT COUNT(*) FROM sys.databases WHERE name=@name", new { name = dbname }) > 0;
        }

        /// <summary>Volat nad connect-stringem do databáze master.</summary>
        public void CreateDatabase(string dbname)
        {
            if (!IsValidDbName(dbname))
            {
                throw new ArgumentException($"Neplatný název databáze: {dbname}");
            }
            Exec($"CREATE DATABASE [{dbname}]");
        }

        /// <summary>Existuje v databázi objekt (tabulka, procedura, funkce, view)? Název včetně schématu, např. dbo.x01License.</summary>
        public bool ObjectExists(string objectname)
        {
            return Scalar<int>("SELECT CASE WHEN OBJECT_ID(@name) IS NULL THEN 0 ELSE 1 END", new { name = objectname }) == 1;
        }

        public bool TableHasIdentity(string tablename)
        {
            return Scalar<int>("SELECT ISNULL(OBJECTPROPERTY(OBJECT_ID(@name),'TableHasIdentity'),0)", new { name = "dbo." + tablename }) == 1;
        }

        public int CountRows(string tablename)
        {
            if (!Regex.IsMatch(tablename, "^[A-Za-z0-9_]+$"))
            {
                throw new ArgumentException($"Neplatný název tabulky: {tablename}");
            }
            return Scalar<int>($"SELECT COUNT(*) FROM dbo.[{tablename}]");
        }

        /// <summary>Rozdělí skript na dávky podle řádků GO.</summary>
        public static List<string> SplitBatches(string script)
        {
            var lis = new List<string>();
            if (string.IsNullOrEmpty(script)) return lis;

            foreach (var s in Regex.Split(script, @"^[ \t]*GO[ \t]*;?[ \t]*\r?$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(s))
                {
                    lis.Add(s);
                }
            }
            return lis;
        }

        /// <summary>
        /// Spustí SQL skript po dávkách (oddělovač GO) na jednom otevřeném spojení.
        /// Chyba v dávce běh nezastaví - vrací se seznam chyb (prázdný seznam = vše proběhlo).
        /// </summary>
        public List<string> RunScript(string script, out int batchCount)
        {
            var errors = new List<string>();
            var batches = SplitBatches(script);
            batchCount = batches.Count;

            using (var con = new SqlConnection(_conString))
            {
                con.Open();
                int x = 0;
                foreach (var strSQL in batches)
                {
                    x++;
                    try
                    {
                        using (var cmd = new SqlCommand(strSQL, con))
                        {
                            cmd.CommandTimeout = _timeoutSeconds;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception e)
                    {
                        errors.Add($"Dávka #{x}: {e.Message} | {Zkratit(strSQL)}");
                        if (con.State != System.Data.ConnectionState.Open)
                        {
                            con.Open(); //fatální chyba mohla spojení zavřít
                        }
                    }
                }
            }

            return errors;
        }

        private static string Zkratit(string strSQL)
        {
            var s = Regex.Replace(strSQL.Trim(), @"\s+", " ");
            return s.Length > 120 ? s.Substring(0, 120) + "…" : s;
        }
    }
}
