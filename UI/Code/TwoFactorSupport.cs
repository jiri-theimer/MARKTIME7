namespace UI.Code
{
    /// <summary>
    /// Dvoufázové ověření při přihlášení: kód z ověřovací aplikace (TOTP, viz BL.Code.Totp), záložně kód e-mailem.
    /// Stav „uživatel se přihlásil heslem, ale ještě nezadal kód“ drží sloupec j02User.j02SmsVerifyCode
    /// (název sloupce zůstal z doby SMS ověření): "TOTP" = čeká se na kód z aplikace, "E:123456" = čeká se na kód z e-mailu.
    /// Dokud je sloupec vyplněný, BaseController pouští uživatele jen na stránku Home/TwoFactor.
    /// </summary>
    public static class TwoFactorSupport
    {
        public const string PendingApp = "TOTP";
        public const string PendingEmailPrefix = "E:";
        public const int EmailCodeMinutes = 15;         //platnost kódu z e-mailu
        public const int MaxFails = 5;                  //počet chybných pokusů, po kterém je uživatel odhlášen
        public const string ParamFails = "2fa-fails";
        public const string ParamSetupSecret = "2fa-setup-secret";

        public static bool IsEmailPending(string strPending)
        {
            return strPending != null && strPending.StartsWith(PendingEmailPrefix);
        }

        /// <summary>Po úspěšném přihlášení heslem: zapne čekání na kód. Bez spárované aplikace pošle kód e-mailem.</summary>
        public static BO.Result StartPending(BL.Factory f, BO.j02User rec)
        {
            if (!string.IsNullOrEmpty(rec.j02TotpSecret))
            {
                f.j02UserBL.UpdateSmsVerifyCode(rec.pid, PendingApp);
                return new BO.Result(false);
            }
            return SendEmailCode(f, rec);
        }

        /// <summary>Vygeneruje šestimístný kód, uloží ho jako čekající a pošle na e-mail uživatele.</summary>
        public static BO.Result SendEmailCode(BL.Factory f, BO.j02User rec)
        {
            if (string.IsNullOrEmpty(rec.j02Email))
            {
                return new BO.Result(true, $"V osobním profilu uživatele {rec.FullNameAsc} chybí e-mailová adresa, na kterou má přijít ověřovací kód. Kontaktujte administrátora.");
            }
            string strCode = BL.Code.Totp.GenerateNumericCode(6);
            string strSubject = "MARKTIME: ověřovací kód pro přihlášení";
            string strHtml = $"<p>Ověřovací kód pro přihlášení do systému MARKTIME ({System.Net.WebUtility.HtmlEncode(rec.j02Login)}):</p>"
                + $"<p style='font-size:28px;font-weight:bold;letter-spacing:4px;'>{strCode}</p>"
                + $"<p>Kód platí {EmailCodeMinutes} minut. Pokud se právě nepřihlašujete, někdo zná vaše heslo – změňte si ho.</p>";

            //odesílatel: pevná systémová adresa (UI.Code.SystemMail), ne kontaktní e-mail z licence
            var ret = f.MailBL.SendMessageWithoutFactory(strHtml, strSubject, rec.j02Email, null, SystemMail.Sender(f), "MARKTIME");
            if (ret.Flag == BO.ResultEnum.Failed)
            {
                return ret;
            }
            f.j02UserBL.UpdateSmsVerifyCode(rec.pid, PendingEmailPrefix + strCode);
            return new BO.Result(false);
        }

        /// <summary>Kód z e-mailu: musí souhlasit a nesmí být starší než EmailCodeMinutes.</summary>
        public static bool VerifyEmailCode(string strPending, DateTime? datCreated, string strCode)
        {
            if (!IsEmailPending(strPending) || datCreated == null || string.IsNullOrEmpty(strCode)) return false;
            string strExpected = strPending.Substring(PendingEmailPrefix.Length);
            if (strExpected.Length < 6) return false;      //kód zneplatněný po chybných pokusech
            if (Convert.ToDateTime(datCreated).AddMinutes(EmailCodeMinutes) < DateTime.Now) return false;
            strCode = new string(strCode.Where(char.IsDigit).ToArray());
            return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(System.Text.Encoding.ASCII.GetBytes(strExpected), System.Text.Encoding.ASCII.GetBytes(strCode));
        }

        /// <summary>ji***@firma.cz – na stránce ověření se celá adresa neukazuje.</summary>
        public static string MaskEmail(string strEmail)
        {
            if (string.IsNullOrEmpty(strEmail)) return "";
            int x = strEmail.IndexOf('@');
            if (x <= 0) return "***";
            return strEmail.Substring(0, Math.Min(2, x)) + "***" + strEmail.Substring(x);
        }
    }
}
