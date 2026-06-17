using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace UI.Models.Tab1
{
    public enum Tab1BoxEnum
    {
        recheader = 2,
        tags = 4,
        o27list = 8,
        notepad = 16,
        quickstat = 32,
        p30list = 64,
        p26list = 128,
        p91clientbox = 256,
        p82list = 512,
        navigor = 1024,
        o32list = 2048,
        p41billingbox = 4096,
        p28billingbox = 8192

    }
    public class BaseTab1ViewModel:BaseViewModel
    {
        public BL.Factory Factory;
        public string prefix { get; set; }
        public string p31guid { get; set; }
        public string caller { get; set; }
        public string TagHtml { get; set; }
        public string rez { get; set; }
        public int pid { get; set; }

        public int Tab1BitStream { get; set; }
        public List<int> SelectedBoxes { get; set; }
        public List<BO.StringPair> lisAllBoxes { get; set; }
        

        public string BgCssClass
        {
            get
            {
                switch (this.prefix)
                {
                    case "o23":
                        return "bgo23";
                    case "j02":
                        return "bgj02";
                    case "p28":
                        return "bgp28";
                    default:
                        return "";
                }
                    
                    
            }
        }

        public FreeFieldsViewModel ff1 { get; set; }

        
        public void SetTagging()
        {
            if (this.pid == 0) return;
            this.Tab1BitStream = Factory.j02UserBL.LoadBitstreamFromUserCache($"t{this.prefix.Substring(1,2)}", 0); //seznam zvolených boxů v tab1
            if (this.caller != null)
            {
                Factory.CBL.SaveLastCallingRecPid(this.prefix, this.pid, this.caller, false, false, this.rez);    //zapsat informaci o naposledy navštíveném záznamu

            }
            
            var tg = Factory.o51TagBL.GetTagging(this.prefix, this.pid);

            this.TagHtml = tg.TagHtml;
        }

        public void SetFreeFields(int intRecTypePid)
        {
            if (this.ff1 == null)
            {
                this.ff1 = new FreeFieldsViewModel();
                this.ff1.InhaleFreeFieldsView(Factory, this.pid, this.prefix);

                if (this.ff1.VisibleInputsCount > 0)
                {
                    this.ff1.RefreshInputsVisibility(Factory, this.pid, this.prefix, intRecTypePid);
                }
            }
        }

        public void ab(Tab1BoxEnum pos,string strName)
        {
            if (this.lisAllBoxes == null) this.lisAllBoxes = new List<BO.StringPair>();
            this.lisAllBoxes.Add(new BO.StringPair() { Key = ((int) pos).ToString(), Value = strName });
        }
        public bool IsRenderBox(Tab1BoxEnum pos)  //true: box s hodnotou pos se má vykreslit
        {
            if (this.Tab1BitStream == 0) return true;
            return BO.Code.Bas.bit_compare_or(this.Tab1BitStream, (int)pos);
        }
    }
}
