

namespace UI.Models.p31invoice
{
    public class p91CreateItem:BO.p91Create
    {
        public int p41ID { get; set; }
        public string p41Name { get; set; }
        public string p28Name { get; set; }
        public string p92Name { get; set; }
        public string WithoutVat { get; set; }

        

        public int p28ID_Invoice { get; set; }
        public string p28Name_Invoice { get; set; }
        
    }
}
