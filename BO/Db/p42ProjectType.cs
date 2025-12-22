
namespace BO
{
    public enum p42ArchiveFlagENUM      //archivace projektů
    {
        NoLimit = 0,
        NoArchive_Waiting_Invoice = 1,
        NoArchive_Waiting_Approve = 2
    }
    public enum p42ArchiveFlagP31ENUM       //archivace úkonů
    {
        EditingOnly = 1,   //pouze rozpracované úkony
        EditingOrApproved = 2, //rozpracované nebo schválené
        NoRecords = 3
    }
    public class p42ProjectType : BaseBO
    {
        public int x01ID { get; set; }
        public int b01ID { get; set; }
        public int x38ID { get; set; }
        public int p61ID { get; set; }
        public string b01Name { get; }
        public string x38Name { get; }
        

        public p42ArchiveFlagENUM p42ArchiveFlag  { get; set; }
        public p42ArchiveFlagP31ENUM p42ArchiveFlagP31 { get; set; }
        public string p42Name { get; set; }
        public string p42Code { get; set; }
        public int p42Ordinary { get; set; }
       
        public byte p42FilesTab { get; set; }
        public byte p42RolesTab { get; set; }
        public byte p42BillingTab { get; set; }
        public byte p42ClientTab { get; set; }
        public byte p42BudgetTab { get; set; }
        public byte p42ContactsTab { get; set; }
        public bool p42IsP54 { get; set; }
        public byte p42BillingFlag { get; set; }
        
    }
}
