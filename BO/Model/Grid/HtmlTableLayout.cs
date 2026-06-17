
using BO.Code;

namespace BO.Model.Grid
{
    //Společná třída pro TheGrid a HtmlTable
    //css pro grid 2: resize, 4: fixed, 8: auto, 16: stripped, 32: mřížka, 64: hover,  128: výška 1, 256: výška 1.5, 512: výška 2
    //css pro htmltable 1024: resize, 2048: fixed, 4096: auto, 8192: stripped, 16384: mřížka, 32768: hover,  65536: výška 1, 131072: výška 1.5, 262144: výška 2
    //j02FontSizeFlag: 1 - malé písmo, 2 - střední (výchozí), 3 - veliké písmo
    public class HtmlTableLayout
    {
        public HtmlTableLayout(int j02GridCssBitStream,int j02FontSizeFlag, bool bolTheGrid)
        {
            _LineHeight = null;   //1rem je výchozí výška tr
            if (j02FontSizeFlag == 3) _LineHeight = "lh12"; //velké písmo má výchozí výšku 1.2rem

            if (bolTheGrid)
            {
                _Resize = Bas.bit_compare_or(j02GridCssBitStream, 2);
                _Fixed = Bas.bit_compare_or(j02GridCssBitStream, 4);                
                //_Autosize = Bas.bit_compare_or(j02GridCssBitStream, 8);
                _Stripped = Bas.bit_compare_or(j02GridCssBitStream, 16);
                _Mrizka = true; // _Mrizka = Bas.bit_compare_or(j02GridCssBitStream, 32);
                _Hover = true;  // _Hover = Bas.bit_compare_or(j02GridCssBitStream, 64);
                if (Bas.bit_compare_or(j02GridCssBitStream, 256))
                {
                    _LineHeight = "lh15";
                }
                if (Bas.bit_compare_or(j02GridCssBitStream, 512))
                {
                    _LineHeight = "lh20";
                }
            }
            else
            {
                _Resize = Bas.bit_compare_or(j02GridCssBitStream, 1024);
                _Fixed = Bas.bit_compare_or(j02GridCssBitStream, 2048);
                //_Autosize = Bas.bit_compare_or(j02GridCssBitStream, 4096);
                _Stripped = Bas.bit_compare_or(j02GridCssBitStream, 8192);
                _Mrizka = Bas.bit_compare_or(j02GridCssBitStream, 16384);
                _Hover = true;  // _Hover = Bas.bit_compare_or(j02GridCssBitStream, 32768);
                if (Bas.bit_compare_or(j02GridCssBitStream, 131072))
                {
                    _LineHeight = "lh15";
                }
                if (Bas.bit_compare_or(j02GridCssBitStream, 262144))
                {
                    _LineHeight = "lh20";
                }
            }
            //autosize se už nepoužívá!!!!

            if (!_Resize && !_Fixed)
            {
                _Resize = true;
            }


        }

        private bool _Autosize { get; set; }
        private bool _Fixed { get; set; }
        private bool _Resize { get; set; }
        private bool _Mrizka { get; set; }
        private bool _Hover { get; set; }
        private bool _Stripped { get; set; }
        private string _LineHeight { get; set; }

        public bool Autosize
        {
            get
            {
                return _Autosize;
            }
        }
        public bool Fixed
        {
            get
            {
                return _Fixed;
            }
        }
        public bool Resize
        {
            get
            {
                return _Resize;
            }
        }
        public bool Mrizka
        {
            get
            {
                return _Mrizka;
            }
        }
        public bool Hover
        {
            get
            {
                return _Hover;
            }
        }
        public bool Stripped
        {
            get
            {
                return _Stripped;
            }
        }
        public string Lineheight
        {
            get
            {
                return _LineHeight;
            }
        }
        public string LayoutName
        {
            get
            {
                if (_Fixed) return "fixed";
                if (_Autosize) return "auto";
                return "resize";
            }
        }


        public string getGridColumnDefaultWidth(TheGridColumn col)
        {
            if (col.FixedWidth == 0)
            {
                //není explicitně definována šířka sloupce
                if (_Autosize)
                {
                    return "auto";

                }
                else
                {
                    return "200px";
                }

            }
            else
            {
                return $"{col.FixedWidth}px";
            }
        }

        public int getGridColumnRealWidth(TheGridColumn col, List<string> arrcolwidths)
        {
            if (_Resize && arrcolwidths.Count() > 0)
            {
                var qry = arrcolwidths.Where(p => p.Contains(col.UniqueName));
                if (qry.Count() > 0)
                {
                    return Convert.ToInt32(qry.First().Substring(qry.First().Length - 3, 3));
                }
            }
            if (col.FixedWidth > 0) return col.FixedWidth;

            if (_Autosize)
            {
                return 0;
            }
            else
            {
                return 200;
            }
            
        }

    }
}
