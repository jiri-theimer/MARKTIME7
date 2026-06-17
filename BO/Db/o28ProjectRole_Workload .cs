

namespace BO
{
    public enum o28EntryFlagENUM
    {
        NemaPravoZapisovatWorksheet = 0,
        ZapisovatDoProjektuIDoUloh = 1,        
        ZapisovatDoProjektuNadrizenym = 4
    }

    public enum o28PermFlagENUM
    {
        PouzeVlastniWorksheet = 0,
        CistVseVProjektu = 1,
        CistAEditVProjektu = 2,
        CistASchvalovatVProjektu = 3,
        CistAEditASchvalovatVProjektu = 4
    }

    public class o28ProjectRole_Workload
    {
        public int o28ID { get; }
        public int p34ID { get; set; }
        public int x67ID { get; set; }
        public o28EntryFlagENUM o28EntryFlag { get; set; }
        public o28PermFlagENUM o28PermFlag { get; set; }

       
        public string p34Name { get; set; }
        
    }
}
