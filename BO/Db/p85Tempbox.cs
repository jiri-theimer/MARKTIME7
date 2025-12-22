using System;


namespace BO
{
    public class p85Tempbox : BaseBO
    {
        public int p85ID { get; set; }
        public string p85GUID { get; set; }
        public string p85Prefix { get; set; }
        public int p85DataPID { get; set; }
        public int p85OtherKey1 { get; set; }
        public int p85OtherKey2 { get; set; }
        public int p85OtherKey3 { get; set; }
        public int p85OtherKey4 { get; set; }
        public int p85OtherKey5 { get; set; }
        public int p85OtherKey6 { get; set; }
        public int p85OtherKey7 { get; set; }
        public int p85OtherKey8 { get; set; }
        public string p85Message { get; set; }
        public bool p85IsFinished { get; set; }
        public bool p85IsDeleted { get; set; }

        public string p85FreeText01 { get; set; }
        public string p85FreeText02 { get; set; }
        public string p85FreeText03 { get; set; }
        public string p85FreeText04 { get; set; }
        public string p85FreeText05 { get; set; }
        public string p85FreeText06 { get; set; }
        public string p85FreeText07 { get; set; }
        public string p85FreeText08 { get; set; }
        public string p85FreeText09 { get; set; }

        public DateTime? p85FreeDate01 { get; set; }
        public DateTime? p85FreeDate02 { get; set; }
        public DateTime? p85FreeDate03 { get; set; }
        public DateTime? p85FreeDate04 { get; set; }
        public DateTime? p85FreeDate05 { get; set; }

        public double p85FreeNumber01 { get; set; }
        public double p85FreeNumber02 { get; set; }
        public double p85FreeNumber03 { get; set; }
        public double p85FreeNumber04 { get; set; }
      
        public double p85FreeFloat01 { get; set; }
        public double p85FreeFloat02 { get; set; }
        public double p85FreeFloat03 { get; set; }

        public bool p85FreeBoolean01 { get; set; }
        public bool p85FreeBoolean02 { get; set; }
        public bool p85FreeBoolean03 { get; set; }
        public bool p85FreeBoolean04 { get; set; }

        public int p85ClonePid { get; set; }

        public string CssTempDisplay
        {
            get
            {
                if (this.p85IsDeleted == true)
                {
                    return "display:none;";
                }
                else
                {
                    return "display:flex";
                }
            }
        }
    }
}
