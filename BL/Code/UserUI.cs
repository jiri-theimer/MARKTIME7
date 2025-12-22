

namespace BL.Code
{
    public static class UserUI
    {        
        public static bool IsShowLeftPanel(string prefix,int j02UIBitStream)    //zda zobrazovat levý+pravý panel
        {
            return BO.Code.Bas.bit_compare_or(j02UIBitStream, GetLeftPanelValue(prefix));
           
        }
        public static bool IsShowFlatView(string prefix, int j02UIBitStream)    //zda zobrazovat spodní panel
        {
            int intOneValue = GetFlatViewValue(prefix);
            if (intOneValue == 0) return true;    //pokud není definována hodnota, pak se jedná o jednoduchý přehled, kterých je v aplikaci většina

            return BO.Code.Bas.bit_compare_or(j02UIBitStream, intOneValue);
            
        }

        public static int GetLeftPanelValue(string prefix)
        {
            switch (prefix)
            {
                case "o23": return 1;
                case "j02": return 2;
                case "p28": return 4;
                case "p91": return 8;
                case "p90": return 16;
                case "p56": return 32;
                case "p55":
                case "le3":
                    return 64;
                case "le5":
                case "p41": return 128;
                case "le4": return 256;
                
                case "o43":
                case "le2":
                    return 1024;

                case "b05": return 131072;
                case "p40": return 268435456;
                case "o22": return 536870912;
                case "p51": return 1073741824;
                case "p84": return 512;
                default: return 0;
            }

            
        }

        public static int GetFlatViewValue(string prefix)
        {
            switch (prefix)
            {                
                case "p28": return 2048;
                case "p91": return 4096;
                case "j02": return 8192;                
                case "p56": return 16384;                
                case "le5":
                case "p41": return 32768;
                case "le4": return 65536;                
                case "o23": return 262144;
                case "p90": return 524288;
                case "o43":
                case "le2":
                    return 1048576;
                case "p51": return 2097152;
                case "p40": return 4194304;
                case "p84": return 8388608;
                case "o22": return 16777216;
                case "p58": return 33554432;
                case "b05": return 67108864;
                case "p55":
                case "le3":
                case "p75":
                    return 134217728;
                

                default: return 0;
            }
        }

        public static void SaveLeftPanel(BL.Factory f,string prefix,bool bolShow)
        {
            int x = f.CurrentUser.j02UIBitStream;

            if (IsShowFlatView(prefix, x))   //zjt: vojebávka
            {
                x = x - GetFlatViewValue(prefix);
            }

            if (IsShowLeftPanel(prefix, x)) 
            {                
                x = x - GetLeftPanelValue(prefix);                
            }
            if (bolShow) x = x + GetLeftPanelValue(prefix);

            
            
           
            Handle_Save(f, x);
        }
        public static void SaveFlatView(BL.Factory f, string prefix, bool bolShow)
        {
            int x = f.CurrentUser.j02UIBitStream;

            if (IsShowLeftPanel(prefix, x))  //zjt: vojebávka
            {
                x = x - GetLeftPanelValue(prefix);
            }

            if (IsShowFlatView(prefix, x))
            {
                x = x - GetFlatViewValue(prefix);
            }
            if (bolShow) x = x + GetFlatViewValue(prefix);

            
            

            Handle_Save(f, x);
        }

        private static void Handle_Save(BL.Factory f,int x)
        {
            var rec = f.j02UserBL.Load(f.CurrentUser.pid);
            rec.j02UIBitStream = x;
            f.j02UserBL.Save(rec, null);
        }
    }
}
