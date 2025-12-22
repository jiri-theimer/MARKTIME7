using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public enum p33IdENUM
    {
        Cas = 1,
        PenizeBezDPH = 2,
        Kusovnik = 3,
        PenizeVcDPHRozpisu = 5
    }
    public enum p34IncomeStatementFlagENUM
    {
        Vydaj = 1,
        Prijem = 2
    }

    public enum p34ActivityEntryFlagENUM
    {
        AktivitaSeNezadava = 1,
        AktivitaJeNepovinna = 2,
        AktivitaJePovinna = 3
    }


    public class p34ActivityGroup : BaseBO
    {
        public int x01ID { get; set; }
        public p33IdENUM p33ID { get; set; }
        
        public string p34Name { get; set; }
        public p34IncomeStatementFlagENUM p34IncomeStatementFlag { get; set; }
        public p34ActivityEntryFlagENUM p34ActivityEntryFlag { get; set; }
        
        public string p34Code { get; set; }
        
        public int p34Ordinary { get; set; }

        public byte p34TextInternalFlag { get; set; }
        public byte p34FilesFlag { get; set; }
        public byte p34TagsFlag { get; set; }
        public byte p34InboxFlag { get; set; }
        public byte p34TrimmingFlag { get; set; }

        public string p33Name { get; }
        
        



    }

}
