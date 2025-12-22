
namespace BL
{
    class BaseBL
    {
        protected BL.Factory _mother;

        private readonly System.Text.StringBuilder _sb;

        public BaseBL(BL.Factory mother)
        {
            _mother = mother;

            _sb = new System.Text.StringBuilder();
        }

        protected DL.DbHandler _db
        {
            get
            {
                return _mother.db;
            }
        }

        public void sb(string s = null)
        {
            if (s != null)
            {
                _sb.Append(s);
            }

        }
        public void sbinit()
        {
            _sb.Clear();
        }
        public string sbret()
        {
            string s = _sb.ToString();
            sbinit();
            return s;
        }

        public void AddMessage(string strMessage, string template = "error")
        {
            _mother.CurrentUser.AddMessage(_mother.tra(strMessage), template);  //automaticky podléhá překladu do ostatních jazyků

        }
        public void AddMessageWithPars(string strMessage, string strPar1, string strPar2 = null, string template = "error")
        {
            string s = _mother.tra(strMessage);

            if (!string.IsNullOrEmpty(strPar2))
            {
                s = string.Format(s, strPar1, strPar2);
            }
            else
            {
                s = string.Format(s, strPar1);
            }
            _mother.CurrentUser.AddMessage(s, template);  //automaticky podléhá překladu do ostatních jazyků

        }
        public bool FalsehMessage(string strMessage)
        {
            AddMessage(strMessage);
            return false;
        }
        public int ZeroMessage(string strMessage)
        {
            AddMessage(strMessage);
            return 0;
        }
        public void AddMessageTranslated(string strMessage, string template = "error")
        {
            _mother.CurrentUser.AddMessage(strMessage, template);  //nepodléhá překladu do ostatních jazyků
        }

        public string AppendCloudQuery(string strCurrentSQL, string strX01Field = "a.x01ID")
        {
            //režim jedné databáze pro N firem rozlišených přes X01ID

            if (string.IsNullOrEmpty(strCurrentSQL))
            {
                return $" WHERE {strX01Field}={_mother.CurrentUser.x01ID}";
            }
            else
            {
                return $"{strCurrentSQL} AND {strX01Field}={_mother.CurrentUser.x01ID}";
            }


        }

    }
}
