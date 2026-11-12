namespace BL.Mcp
{
    /// <summary>Úložiště v databázi MARKTIME (skript SQL: mcp_oauth.sql).</summary>
    public class McpSqlStore : IMcpStore
    {
        private readonly DL.DbHandler _db;
        private const string SqlGrant = "SELECT a.*,j02.j02Login FROM j96McpGrant a INNER JOIN j02User j02 ON a.j02ID=j02.j02ID";

        public McpSqlStore(DL.DbHandler db) { _db = db; }

        public int InsertGrant(McpGrant g)
        {
            return _db.Load<int>(@"INSERT INTO j96McpGrant(j02ID,j96ClientName,j96ClientKey,j96RedirectUri,j96Scope,j96CodeHash,j96CodeChallenge,j96CodeExpires,j96DateInsert,j96DateExpires)
VALUES(@j02ID,@j96ClientName,@j96ClientKey,@j96RedirectUri,@j96Scope,@j96CodeHash,@j96CodeChallenge,@j96CodeExpires,@j96DateInsert,@j96DateExpires);
SELECT CAST(SCOPE_IDENTITY() AS int)", new { g.j02ID, g.j96ClientName, g.j96ClientKey, g.j96RedirectUri, g.j96Scope, g.j96CodeHash, g.j96CodeChallenge, g.j96CodeExpires, g.j96DateInsert, g.j96DateExpires });
        }

        public McpGrant LoadGrant(int j96id) => _db.Load<McpGrant>(SqlGrant + " WHERE a.j96ID=@id", new { id = j96id });

        public McpGrant LoadGrantByCode(string codeHash) => _db.Load<McpGrant>(SqlGrant + " WHERE a.j96CodeHash=@hash", new { hash = codeHash });

        public bool MarkCodeUsed(int j96id, DateTime now) =>
            _db.RunSql("UPDATE j96McpGrant SET j96CodeUsed=@now WHERE j96ID=@id AND j96CodeUsed IS NULL", new { id = j96id, now });

        public bool InsertToken(McpToken t) =>
            _db.RunSql("INSERT INTO j99McpToken(j96ID,j99Kind,j99Hash,j99DateInsert,j99DateExpires) VALUES(@j96ID,@j99Kind,@j99Hash,@j99DateInsert,@j99DateExpires)",
                new { t.j96ID, t.j99Kind, t.j99Hash, t.j99DateInsert, t.j99DateExpires });

        public McpToken LoadToken(string hash) => _db.Load<McpToken>("SELECT * FROM j99McpToken WHERE j99Hash=@hash", new { hash });

        public bool MarkTokenUsed(int j99id, DateTime now) =>
            _db.RunSql("UPDATE j99McpToken SET j99DateUsed=@now WHERE j99ID=@id AND j99DateUsed IS NULL", new { id = j99id, now });

        public void RevokeGrant(int j96id, DateTime now, string reason)
        {
            _db.RunSql("UPDATE j96McpGrant SET j96DateRevoked=@now,j96RevokeReason=@reason WHERE j96ID=@id AND j96DateRevoked IS NULL", new { id = j96id, now, reason });
            _db.RunSql("DELETE FROM j99McpToken WHERE j96ID=@id", new { id = j96id });
        }

        public void TouchGrant(int j96id, DateTime now) =>
            _db.RunSql("UPDATE j96McpGrant SET j96DateLastUsed=@now WHERE j96ID=@id", new { id = j96id, now });

        public void Cleanup(int j96id, DateTime now) =>
            _db.RunSql("DELETE FROM j99McpToken WHERE j96ID=@id AND (j99DateExpires<@now OR j99DateUsed<@old)", new { id = j96id, now, old = now.AddDays(-1) });

        public List<McpGrant> ListByUser(int j02id) =>
            _db.GetList<McpGrant>(SqlGrant + " WHERE a.j02ID=@j02id AND a.j96CodeUsed IS NOT NULL ORDER BY a.j96ID DESC", new { j02id })?.ToList() ?? new List<McpGrant>();
    }
}
