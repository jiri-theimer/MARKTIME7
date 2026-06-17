

namespace BL.Code
{
    public class GpcFileHandle
    {
        public List<BO.GpcRecord> ParseFile(string fullpath)
        {
            var ret = new List<BO.GpcRecord>();
            string[] rows = null;
            try
            {
                rows = File.ReadAllLines(fullpath, System.Text.Encoding.GetEncoding(1250));
            }
            catch
            {
                rows = File.ReadAllLines(fullpath);
            }
            

            foreach (string s in rows)
            {
                if (s.Substring(0,3) != "075")
                {
                    continue;
                }

                var c = new BO.GpcRecord() {
                    TypZaznamu = IZ(s,1,3),MujUcet=IZ(s,4,16)
                    ,CisloUctuProtistrany = IZ(s,20,16)
                    , IdentifikatorTransakce=IZ(s,36,13)
                    ,CastkaTransakce = IZ(s, 49, 12)
                    ,KodUctovani=IZ(s,61,1)
                    ,VariabilniSymbol=IZ(s,62,10)
                    ,Oddelovac=IZ(s,72,2)
                    ,KodBankyProtistrany = IZ(s, 74, 4)
                    ,KonstantniSymbol = IZ(s, 78, 4)
                    ,SpecifickySymbol = IZ(s, 82, 10)
                    ,DatumValuty=IZ(s,92,6)                   
                    ,KodMeny=IZ(s,118,5)                    
                    
                };

                
                c.Popis = BO.Code.File.RemoveDiacritism(IZ(s, 98, 20));
                
                
                

                ret.Add(c);

            }


            return ret;
        }

        private string IZ(string s,int pozice,int delka)
        {
            return s.Substring(pozice - 1, delka);
        }
    }
}
