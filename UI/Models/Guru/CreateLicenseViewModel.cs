namespace UI.Models.Guru
{

    public class CreateLicenseViewModel : BaseViewModel
    {
        public string guru { get; set; }
        public string DestDbName { get; set; }
        public int DestX01ID { get; set; }
        public bool AllowPrefill { get; set; }
        public int DestP28ID_Interni { get; set; }
        public int DestP28ID_Klient { get; set; }
        public int DestP41ID_Interni { get; set; }
        public int DestP41ID_Klient1 { get; set; }
        public int DestP41ID_Klient2 { get; set; }
        public BO.x01License Rec { get; set; }

        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string VerifyPassword { get; set; }
        public string Message { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string LoginName { get; set; }
        public string LoginDomainPrefix { get; set; }
        

        public int UserLangIndex { get; set; }


        public string p93Company { get; set; }
        public string p93City { get; set; }
        public string p93Street { get; set; }
        public string p93Zip { get; set; }
        public string p93RegID { get; set; }
        public string p93VatID { get; set; }

        public string p93Registration { get; set; }
        public string p93ICDPH_SK { get; set; }

        public bool IsZakladatKlientaPlusProjekty { get; set; }
        public string p28CompanyName { get; set; }
        public string p28City1 { get; set; }
        public string p28Street1 { get; set; }
        public string p28PostCode1 { get; set; }
        public string p28RegID { get; set; }
        public string p28VatID { get; set; }
        public string p28ICDPH_SK { get; set; }

        public string Project1 { get; set; }
        public string Project2 { get; set; }
        public List<CreateLicenseKey> lisKeys { get; set; }

        


}

    public class CreateLicenseKey
    {
        public string Table { get; set; }
        public string Field { get; set; }
        public int OrigValue { get; set; }
        public int NewValue { get; set; }

    }

}

