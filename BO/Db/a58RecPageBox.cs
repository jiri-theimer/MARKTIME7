using System;
using System.ComponentModel.DataAnnotations;

namespace BO
{    
    public enum a58ContentFlagEnum
    {
        _OnlyHtmlText=0,
        ButtonWorkflowDialog = 1,
        ButtonReportDialog = 2,
        RecNotepad=3,
        ButtonOneReport = 4,
        ButtonOneWorkflowStep = 5,
        RecTags=6,
        ButtonUploadO27 = 8,       
        o27List=10,
        RecHeaderBox=11,
        o23Vazby=12,
        o32List=13,
        p30List = 14,
        p28Address1=15,
        p28ActivityStat=16,
        p41List=17,
        p91CenovyRozpis=18,
        p91Text=19,
        p91Uhrady = 20,
        p91Zalohy=21,
        p91Stat=22,
        p41ActivityStat=23,
        j02ActivityStat=24,
        p41RecBillingBox=25,
        p28RecBillingBox=26,
        p91Datumy=27,
        p91Castky=28,
        p91Klient=29,
        RolesAssigned=30

    }
    public class a58RecPageBox: BaseBO
    {       
        public int a59ID { get; set; }
        public string a58Name { get; set; }
        public string a58Code { get; set; }
        public string a58DefaultName { get; set; }
        public string a58Guid { get; set; }
        public string a58HtmlText { get; set; }
        public int x04ID { get; set; }
        public a58ContentFlagEnum? a58ContentFlag { get; set; }
        
        public string a58ButtonText { get; set; }        
        public int x31ID_Button { get; set; }
        public int b06ID_Button { get; set; }

        public string a58CssClassName { get; set; }
        public string a58ControlFlag { get; set; }  //doplňující příznak prvku, hodnoty: FormNameHtml
        public bool a58IsHtmlByPlaintext { get; set; }        
        public string b06Name { get; }
        public string x31Name { get; }

        

        public string Name { get
            {
                if (this.a58Name != null)
                {
                    return this.a58Name;
                }
                else
                {
                    if (this.a58ButtonText != null)
                    {
                        return this.a58ButtonText;
                    }
                    else
                    {
                        return this.a58DefaultName;
                    }
                    
                }
            } 
        }

        public bool IsButton()
        {
            switch (this.a58ContentFlag)
            {
                case BO.a58ContentFlagEnum.ButtonReportDialog:
                case BO.a58ContentFlagEnum.ButtonOneReport:                
                case BO.a58ContentFlagEnum.ButtonUploadO27:                
                case BO.a58ContentFlagEnum.ButtonWorkflowDialog:
                case BO.a58ContentFlagEnum.ButtonOneWorkflowStep:                                
                    return true;                    
                default:
                    return false;
            }
        }

        
        
    }
}
