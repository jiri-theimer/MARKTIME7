

namespace BO
{
    public enum p44FixingFlagEnum
    {
        _None=0,
        FixAll=1,
        FixProject=2,
        FixSetting=3

    }
    public class p44ProjectTemplate:BaseBO
    {
        public BO.p44FixingFlagEnum p44FixingFlag { get; set; }
        public int x01ID { get; set; }
        public string p44Name { get; set; }
        public int p41ID_Pattern { get; set; }
        public int p44Ordinary { get; set; }
        public bool p44IsRoles { get; set; }
        public bool p44IsP56 { get; set; }
        public bool p44IsP40 { get; set; }
        public bool p44IsO22 { get; set; }
        public bool p44IsClient { get; set; }
        public bool p44IsBilling { get; set; }
        public bool p44IsO24 { get; set; }

        public bool p44IsB20 { get; set; }
        public bool p44IsTags { get; set; }
        public bool p44IsJ18ID { get; set; }
    }
}
