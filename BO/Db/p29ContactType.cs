
namespace BO
{
    public enum p29ScopeFlagENUM
    {
        Anyone = 0,        
        Client = 1,
        Supplier = 2,
        ClientOrSupplier = 3,
        ContactPerson=4

    }
    public class p29ContactType: BaseBO
    {
        public int x01ID { get; set; }
        public p29ScopeFlagENUM p29ScopeFlag { get; set; }
        
        public string p29Name { get; set; }
        public int p29Ordinary { get; set; }
       public int x38ID { get; set; }
        public int b01ID { get; set; }

        public string b01Name { get; }
        
        public byte p29FilesTab { get; set; }
        public byte p29RolesTab { get; set; }
        public byte p29BillingTab { get; set; }
        public byte p29ContactPersonsTab { get; set; }
        public byte p29ContactMediaTab { get; set; }

    }
}
