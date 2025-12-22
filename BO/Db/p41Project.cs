using System;

namespace BO
{
   
    public enum p41BillingFlagEnum
    {
        _NotSpecified = 0,
        CenikDedit=1,
        CenikPrirazeny=2,
        CenikIndividualni=3,
        NuloveSazbyWip = 6,
        NuloveSazbyBezWip = 99
    }
    public enum p41WorksheetOperFlagEnum
    {
        _NotSpecified = 0,
        ZakazVykazovat=1,
        ZakazVykazovatHodiny=2,
        UkolPovinnyUkony = 3,
        UkolPovinnyHodiny = 4
    }
    public class p41Project : BaseBO
    {
        public int p42ID { get; set; }
        public int p28ID_Client { get; set; }
        public int p28ID_Billing { get; set; }
        public int p51ID_Billing { get; set; }
        public int p51ID_Internal { get; set; }
        public int b02ID { get; set; }
        public int p07ID { get; set; }

        public int p92ID { get; set; }
        public int j18ID { get; set; }
        public int p61ID { get; set; }
        public int p15ID { get; set; }
        public int j02ID_Owner { get; set; }
        public BO.p72IdENUM p72ID_BillableHours { get; set; }   //výchozí korekce pro fakturovatelné hodiny
        public BO.p72IdENUM p72ID_NonBillable { get; set; }     //výchozí korekce pro nefakturovatelné hodiny

        public string p41Name { get; set; }

        public string p41NameShort { get; set; }
        public bool p41IsStopNotify { get; set; }
        public int p41ParentID { get; set; }
        public int p41ID_P07Level1 { get; set; }
        public int p41ID_P07Level2 { get; set; }
        public int p41ID_P07Level3 { get; set; }
        public int p41ID_P07Level4 { get; set; }
        
        public int p41BitStream { get; set; }
        public int p41CapacityStream { get; set; }
        public string TagsInlineHtml { get; }
        public int p41TreeOrdinary { get; set; }

        public string p41Code { get; set; }
        public string p41ExternalCode { get; set; }

        public int p41BillingLangIndex { get; set; }

        public DateTime? p41PlanFrom { get; set; }
        public DateTime? p41PlanUntil { get; set; }

        public double p41Plan_Hours { get; set; }
        public double p41Plan_Hours_Billable { get; set; }
        public double p41Plan_Hours_Nonbillable { get; set; }
        public double p41Plan_Expenses { get; set; }
        public double p41Plan_Revenue { get; set; }
        public double p41Plan_Internal_Rate { get; set; }
        public double p41Plan_Internal_Fee { get; set; }

       public string p41BillingMemo200 { get; set; }
        public p41BillingFlagEnum p41BillingFlag { get; set; }
        public p41WorksheetOperFlagEnum p41WorksheetOperFlag { get; set; }
        public int p41InvoiceMaturityDays { get; set; }
        public string p41InvoiceDefaultText2 { get; set; }
        public string p41InvoiceDefaultText1 { get; set; }

        public Guid p41Guid { get; set; }

        public string Owner { get; }
        public string p42Name { get; }
        public bool p42IsP54 { get; }
        public string j18Name { get; }
        public string j18CountryCode { get; }
        public string b02Name { get; }
        public string b02Color { get; }
        public string p92Name { get; }
        public string p51Name_Billing { get; }
        public int b01ID { get; }
        public int x38ID { get; }
        public int p41Cache_p31Count { get; }  //celkový počet vykázaných úkonů

        public string Client { get; }
        public int p28BillingLangIndex { get; }
        public string p28BillingMemo200 { get; }

        public int p07Level { get; }
        public string p07Name { get; set; }
        public int p41TreeLevel { get; set; }
        

        public int p41TreeIndex { get; set; }

        public int p41TreePrev { get; set; }
        public int p41TreeNext { get; set; }
        public string p41TreePath { get; }

        public int p61ID_Byp42ID { get; set; }

        public int p41Round2Minutes { get; set; }
        public string p41AccountingIds { get; set; }

        public string FullName
        {
            get
            {
                string s = this.p41NameShort;
                if (s == null)
                    s = this.p41Name;
                if (this.p41TreePath != null)
                    s = this.p41TreePath;

                
                if (this.p28ID_Client > 0)
                {
                    if (Client.Length > 25)
                        return $"{BO.Code.Bas.LeftString(this.Client, 25)}... - {s} ({this.p41Code})";

                    else
                        return $"{this.Client} - {s} ({this.p41Code})";                        
                }
                return s;
            }
        }
        //public string ProjectWithMask(int maskindex)
        //{
        //    string s = this.p41NameShort;
        //    if (s == null)
        //        s = this.p41Name;

        //    switch (maskindex)
        //    {
        //        case 1:
        //            {
        //                return s;  // pouze název
        //            }

        //        case 2:
        //            {
        //                return s + " [" + this.p41Code + "]";  // název projektu + kód
        //            }

        //        case 3:
        //            {
        //                return s + " [" + this.Client + "]";    // název+klient
        //            }

        //        case 4:
        //            {
        //                return this.p41Code;                // pouze kód projektu
        //            }

        //        case 5:
        //            {
        //                if (this.p41TreePath != null)
        //                    return this.p41TreePath;
        //                else
        //                    return this.PrefferedName; // nadřízený+podřízený projekt


        //            }

        //        default:
        //            {
        //                return FullName + " [" + this.p41Code + "]";
        //            }
        //    }
        //}
        public string PrefferedName
        {
            get
            {
                if (this.p41NameShort != null)
                    return this.p41NameShort;


                return this.p41Name;
            }
        }

        public string TypePlusName
        {
            get
            {
                return $"{this.p42Name}: {this.p41Name}";                
            }
        }

        public bool IsPlan_FaZastropovano()
        {
            return BO.Code.Bas.bit_compare_or(this.p41CapacityStream, 2);
        }
        public bool IsPlan_NefaZastropovano()
        {
            return BO.Code.Bas.bit_compare_or(this.p41CapacityStream, 4);
        }
    }
}
