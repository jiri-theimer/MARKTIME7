

namespace UI.Models.p31oper
{
    public class hesViewModel:BaseViewModel
    {
        public int ActiveTabIndex { get; set; } = 1;
        public int HesBitStream { get; set; }

        public int HoursInterval { get; set; }
        public string HoursFormat { get; set; }

        public bool TimesheetEntryByMinutes { get; set; }
        
        public int TimeInputFrom { get; set; }
        public int TimeInputTo { get; set; }
        public int TimeInputInterval { get; set; }
        public bool OfferContactPerson { get; set; }

        public int ActivityFlag { get; set; }

        public bool Approve_InterniHodiny { get; set; }
        public bool Approve_HodinyVPausalu { get; set; }
        public bool Approve_UrovenSchvalovani { get; set; }
        public bool Approve_DoDefaultApproveState { get; set; }
        public bool Approve_ShowRecZoom { get; set; }
        public int Approve_GridBox_Position { get; set; }   //0:nezobrazovat, 1:vlevo, 2:vpravo, neukládá se do bitstream, ale do userkeys

        public bool Approve_IsSkipGateway { get; set; }
        public string Approve_Default_UI { get; set; }     //Inline/Grid
        public void InhaleSetting() //odvodit parametry podle hodnoty HesBitStream
        {
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 2)) this.HoursInterval = 30;
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 4)) this.HoursInterval = 60;
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 8)) this.HoursInterval = 5;
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 16)) this.HoursInterval = 10;
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 32)) this.HoursInterval = 6;
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 64)) this.HoursInterval = 15;
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 128)) this.TimesheetEntryByMinutes = true;
            
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 512)) this.OfferContactPerson = true;
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 1024)) this.ActivityFlag = 99;   //nepředvyplňovat sešit
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 2048)) this.ActivityFlag = 0;    //sešit první v pořadí
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 4096)) this.ActivityFlag = 1;    //sešit podle posledního úkonu
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 8192)) this.ActivityFlag = 2;    //sešit + aktivita podle posledního úkonu

            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 16384)) this.Approve_InterniHodiny = true;
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 32768)) this.Approve_HodinyVPausalu = true;
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 65536)) this.Approve_UrovenSchvalovani = true;
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 131072)) this.Approve_DoDefaultApproveState = true;
            if (BO.Code.Bas.bit_compare_or(this.HesBitStream, 262144)) this.Approve_ShowRecZoom = true;

        }
    }
}
