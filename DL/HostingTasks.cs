

namespace DL
{
    public class HostingTasks
    {
        private DL.DbHandler _db;
        public HostingTasks(DL.DbHandler db_caller)
        {
            _db = db_caller;
        }
        public void UpdateCloudHeader_X01()
        {
            string s = "DELETE FROM a7_cloudheader.dbo.x01License WHERE x01LoginDomain IN (select x01LoginDomain FROM x01License) AND x01Guid NOT IN (select x01Guid FROM x01License)";
            _db.RunSql(s);

            s = "insert into a7_cloudheader.dbo.x01License(x01ID,x01LoginDomain,x01Guid,x01Name,x01CountryCode,x01UserInsert,x01UserUpdate)";
            s += " select x01ID,x01LoginDomain,x01Guid,x01Name,x01CountryCode,x01UserInsert,x01UserUpdate FROM x01License WHERE x01LoginDomain NOT IN (select x01LoginDomain FROM a7_cloudheader.dbo.x01License)";
            _db.RunSql(s);

            s = "update a set x01Name=b.x01AppName,x01AppName=b.x01AppName,x01LoginDomain=b.x01LoginDomain,x01CountryCode=b.x01CountryCode,j27ID=b.j27ID,x01ApiKey=b.x01ApiKey,x01AppHost=b.x01AppHost";
            s += ",x01BillingLang1=b.x01BillingLang1,x01BillingLang2=b.x01BillingLang2,x01BillingLang3=b.x01BillingLang3,x01BillingLang4=b.x01BillingLang4,x01IsCapacityFaNefa=b.x01IsCapacityFaNefa,x01ValidUntil=b.x01ValidUntil";
            s += ",x04ID_Default=b.x04ID_Default,x01RobotLogin=b.x01RobotLogin,x01RecordsCountP31=b.x01RecordsCountP31,x01Round2Minutes=b.x01Round2Minutes,x01LogoFileName=b.x01LogoFileName,x01IsAllowPasswordRecovery=b.x01IsAllowPasswordRecovery,x01ContactEmail=b.x01ContactEmail,x01ImportCnb_j27Codes=b.x01ImportCnb_j27Codes,x01LockFlag=b.x01LockFlag";
            s += ",x01IsAllowDuplicity_VatID=b.x01IsAllowDuplicity_VatID,x01IsAllowDuplicity_RegID=b.x01IsAllowDuplicity_RegID,x01LangIndex=b.x01LangIndex,x01PasswordRecovery_Question=b.x01PasswordRecovery_Question,x01PasswordRecovery_Answer=b.x01PasswordRecovery_Answer,x01DateUpdate=b.x01DateUpdate,x01LimitUsers=b.x01LimitUsers";
            s += ",x01InvoiceMaturityDays=b.x01InvoiceMaturityDays,x15ID=b.x15ID";
            s += ",x01BillingFlag1=b.x01BillingFlag1,x01BillingFlag2=b.x01BillingFlag2,x01BillingFlag3=b.x01BillingFlag3,x01BillingFlag4=b.x01BillingFlag4";
            s += " FROM a7_cloudheader.dbo.x01License a INNER JOIN x01License b ON a.x01LoginDomain=b.x01LoginDomain";
            _db.RunSql(s);

        }
        public void UpdateCloudHeader_P07()
        {
            string s = "insert into a7_cloudheader.dbo.p07ProjectLevel(x01LoginDomain,p07ID,p07Level,p07Name,p07NamePlural,p07NameInflection,p07DateInsert,p07UserInsert,p07UserUpdate,p07ValidFrom,p07ValidUntil,x01ID)";
            s += " SELECT b.x01LoginDomain,a.p07ID,a.p07Level,a.p07Name,a.p07NamePlural,a.p07NameInflection,a.p07DateInsert,a.p07UserInsert,a.p07UserUpdate,a.p07ValidFrom,a.p07ValidUntil,a.x01ID";
            s += " from p07ProjectLevel a INNER JOIN x01License b ON a.x01ID=b.x01ID";
            s += " WHERE NOT EXISTS(select x01LoginDomain,p07ID FROM a7_cloudheader.dbo.p07ProjectLevel WHERE x01LoginDomain=b.x01LoginDomain AND p07ID=a.p07ID)";
            _db.RunSql(s);

            s = "update c set p07Name=a.p07Name,p07NamePlural=a.p07NamePlural,p07NameInflection=a.p07NameInflection,p07DateUpdate=a.p07DateUpdate,p07UserUpdate=a.p07UserUpdate,p07ValidUntil=a.p07ValidUntil";
            s += " FROM p07ProjectLevel a INNER JOIN x01License b ON a.x01ID=b.x01ID INNER JOIN a7_cloudheader.dbo.p07ProjectLevel c ON a.p07ID=c.p07ID AND b.x01LoginDomain=c.x01LoginDomain";
            _db.RunSql(s);
        }

        public void UpdateCloudHeader_O17(string strX01LoginDomain)
        {
            string s = "INSERT INTO a7_cloudheader.dbo.o17DocMenu(x01LoginDomain,o17ID,o17Name,o17Ordinary,o17DateInsert,o17UserInsert,o17UserUpdate,o17ValidFrom,o17ValidUntil,x01ID)";
            s += " SELECT b.x01LoginDomain,a.o17ID,a.o17Name,a.o17Ordinary,a.o17DateInsert,a.o17UserInsert,a.o17UserUpdate,a.o17ValidFrom,a.o17ValidUntil,a.x01ID";
            s += " FROM o17DocMenu a INNER JOIN x01License b ON a.x01ID=b.x01ID";
            s += " WHERE NOT EXISTS(select x01LoginDomain,o17ID FROM a7_cloudheader.dbo.o17DocMenu WHERE x01LoginDomain=b.x01LoginDomain AND o17ID=a.o17ID)";
            _db.RunSql(s);

            s = "UPDATE c SET o17Name=a.o17Name,o17Ordinary=a.o17Ordinary,o17DateUpdate=a.o17DateUpdate,o17UserUpdate=a.o17UserUpdate,o17ValidUntil=a.o17ValidUntil";
            s += " FROM o17DocMenu a INNER JOIN x01License b ON a.x01ID=b.x01ID INNER JOIN a7_cloudheader.dbo.o17DocMenu c ON a.o17ID=c.o17ID AND b.x01LoginDomain=c.x01LoginDomain";
            _db.RunSql(s);

            s = "DELETE FROM a7_cloudheader.dbo.o17DocMenu WHERE x01LoginDomain=@dom AND o17ID NOT IN (select o17ID FROM o17DocMenu)";
            _db.RunSql(s, new { dom = strX01LoginDomain });
        }
    }
}
