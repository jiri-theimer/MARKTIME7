namespace BL.Mcp
{
    /// <summary>Připojení AI klienta k účtu uživatele (tabulka j96McpGrant). Vzniká souhlasem uživatele, končí vypršením nebo odpojením.</summary>
    public class McpGrant
    {
        public int j96ID { get; set; }
        public int j02ID { get; set; }
        public string j96ClientName { get; set; }
        /// <summary>Otisk client_id – token smí vyzvednout a obnovit jen klient, kterému uživatel dal souhlas.</summary>
        public string j96ClientKey { get; set; }
        public string j96RedirectUri { get; set; }
        public string j96Scope { get; set; }
        public string j96CodeHash { get; set; }
        public string j96CodeChallenge { get; set; }
        public DateTime j96CodeExpires { get; set; }
        public DateTime? j96CodeUsed { get; set; }
        public DateTime j96DateInsert { get; set; }
        public DateTime j96DateExpires { get; set; }
        public DateTime? j96DateRevoked { get; set; }
        public DateTime? j96DateLastUsed { get; set; }
        public string j96RevokeReason { get; set; }

        /// <summary>Přihlašovací jméno uživatele (j02User) – jen při čtení.</summary>
        public string j02Login { get; set; }

        public bool IsActive(DateTime now) => j96DateRevoked == null && j96DateExpires > now;
    }

    /// <summary>Vydaný token (tabulka j99McpToken). V databázi je jen otisk SHA-256, token samotný zná pouze klient.</summary>
    public class McpToken
    {
        public int j99ID { get; set; }
        public int j96ID { get; set; }
        /// <summary>A = přístupový token, R = obnovovací token.</summary>
        public string j99Kind { get; set; }
        public string j99Hash { get; set; }
        public DateTime j99DateInsert { get; set; }
        public DateTime j99DateExpires { get; set; }
        /// <summary>Obnovovací token: kdy byl vyměněn za nový.</summary>
        public DateTime? j99DateUsed { get; set; }
    }

    /// <summary>Úložiště připojení a tokenů. Logika OAuth je v <see cref="McpOAuthService"/>, tady jen čtení a zápis.</summary>
    public interface IMcpStore
    {
        /// <returns>j96ID, nebo 0 při chybě</returns>
        int InsertGrant(McpGrant g);
        McpGrant LoadGrant(int j96id);
        McpGrant LoadGrantByCode(string codeHash);
        /// <summary>Označí autorizační kód za použitý. true jen pro první použití.</summary>
        bool MarkCodeUsed(int j96id, DateTime now);
        bool InsertToken(McpToken t);
        McpToken LoadToken(string hash);
        /// <summary>Označí obnovovací token za vyměněný. true jen pro první výměnu.</summary>
        bool MarkTokenUsed(int j99id, DateTime now);
        void RevokeGrant(int j96id, DateTime now, string reason);
        void TouchGrant(int j96id, DateTime now);
        /// <summary>Smaže propadlé tokeny připojení.</summary>
        void Cleanup(int j96id, DateTime now);
        List<McpGrant> ListByUser(int j02id);
    }
}
