

namespace BO
{
    public enum j05Disposition_p31ENUM
    {
        _NotSpecified = 0,
        Cist = 1,
        CistAEdit = 2,
        CistASchvalovat = 3,
        CistAEditASchvalovat = 4
    }
    public enum j05Disposition_j02ENUM
    {
        _NotSpecified = 0,
        Cist = 1,
        CistAEdit = 2
    }
    public class j05MasterSlave:BaseBO
    {
        public int j02ID_Master { get; set; }
        public int j02ID_Slave { get; set; }
        public int j11ID_Slave { get; set; }
        public j05Disposition_p31ENUM j05Disposition_p31 { get; set; }
        public bool j05IsCreate_p31 { get; set; }
        public string PersonMaster { get; }      
        public string PersonSlave { get; }       
        public string TeamSlave { get; }
        public j05Disposition_j02ENUM j05Disposition_j02 { get; set; }
    }
}
