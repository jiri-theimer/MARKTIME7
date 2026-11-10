namespace BO
{
    public enum u01TypeFlagENUM
    {
        Direct = 1,     //přímá konverzace 1:1
        Group = 2,      //skupinová konverzace
        Record = 3      //konverzace k záznamu (projekt, úkol, klient...)
    }

    /// <summary>Konverzace interního chatu.</summary>
    public class u01Channel : BaseBO
    {
        public int x01ID { get; set; }
        public u01TypeFlagENUM u01TypeFlag { get; set; } = u01TypeFlagENUM.Direct;
        public string u01Name { get; set; }
        public string u01DirectKey { get; set; }        //pouze 1:1: "menší j02ID-větší j02ID"
        public string u01RecordEntity { get; set; }
        public int u01RecordPid { get; set; }
        public int j02ID_Owner { get; set; }
        public int u02ID_Last { get; set; }
        public DateTime? u01DateLastMessage { get; set; }
    }
}
