using System;

namespace BO
{
    public enum p28BillingFlagENUM
    {
        _NotSpecified = 0,
        CenikDedit = 1,
        CenikPrirazeny = 2,
        CenikIndividualni = 3       
    }
    public class p28Contact : BaseBO
    {
        public int p29ID { get; set; }    
        public int p92ID { get; set; }
        public int p63ID { get; set; }
        
        public int b02ID { get; set; }
        public int j02ID_Owner { get; set; }
        public p28BillingFlagENUM p28BillingFlag { get; set; }
        public int p51ID_Billing { get; set; }
        public int p51ID_Internal { get; set; }
        public int j61ID_Invoice { get; set; }
        public string p28ShortName { get; set; }
        public bool p28IsCompany { get; set; }
        public string p28CompanyName { get; set; }
        public string p28FirstName { get; set; }
        public string p28LastName { get; set; }
        public string p28TitleBeforeName { get; set; }
        public string p28TitleAfterName { get; set; }
        public string p28Code { get; set; }
        public string p28ExternalCode { get; set; }
      
        public int p28BillingLangIndex { get; set; }
        
      public int p28BitStream { get; set; }
        public string p28JobTitle { get; set; }
       public string p28CountryCode { get; set; }
        public string p28RegID { get; set; }
        public string p28VatID { get; set; }
        public string p28ICDPH_SK { get; set; }
        public string p28BankAccount { get; set; }
        public string p28BankCode { get; set; }
        public Guid p28Guid { get; set; }
        public string p28Street1 { get; set; }
        public string p28City1 { get; set; }
        public string p28PostCode1 { get; set; }
        public string p28Country1 { get; set; }
        public string p28BeforeAddress1 { get; set; }
        public string p28Street2 { get; set; }
        public string p28City2 { get; set; }
        public string p28PostCode2 { get; set; }
        public string p28Country2 { get; set; }

        public int p28Round2Minutes { get; set; }
     
        public int p28InvoiceMaturityDays { get; set; }
        public string p28InvoiceDefaultText2 { get; set; }
        public string p28InvoiceDefaultText1 { get; set; }
        public string p28Salutation { get; set; }
        public string TagsInlineHtml { get; set; }
        public string p28VatCodePohoda { get; set; }


        public int p28ParentID { get; set; }  // strom
        public string p28TreePath { get; set; }   // strom
        public int p28TreeLevel { get; set; } // strom
        public int p28TreeIndex { get; set; } // strom
        public int p28TreePrev { get; set; }  // strom
        public int p28TreeNext { get; set; }  // strom
        public string p28TreeRelName { get; set; }    // strom

        public string p28BillingMemo200 { get; set; }

        //readonly:
        public string p28Name { get; }
        public string p29Name { get; }
        public BO.p29ScopeFlagENUM p29ScopeFlag { get; }
        public string p92Name { get; }
        public string p51Name_Billing { get; }
        public string Owner { get; }
        public int x38ID { get; }
        public int b01ID { get; }
        public int p28Cache_p31Count { get; }
        public string FullNameAsc
        {
            get
            {
                if (this.p28IsCompany)
                {
                    return this.p28CompanyName;
                }
                else
                {
                    return (p28TitleBeforeName + " " + p28FirstName + " " + p28LastName + " " + p28TitleAfterName).Trim();
                }
                
            }
        }
        public string FullNameDesc
        {
            get
            {
                if (this.p28IsCompany)
                {
                    return this.p28CompanyName;
                }
                else
                {
                    return (p28LastName + " " + p28FirstName + " " + p28TitleBeforeName).Trim();
                }
                
            }
        }

        public string GetFullAddress(int intIndex)
        {
            string s = "";
            if (intIndex == 1)
            {
                if (this.p28Street1 != null)
                    s = this.p28Street1 + ", " + this.p28City1;
                else
                    s = this.p28City1;
                if (this.p28PostCode1 != null)
                    s += " " + this.p28PostCode1;
                if (this.p28Country1 != null)
                    s += System.Environment.NewLine + this.p28Country1;
            }

            if (intIndex == 2)
            {
                if (this.p28Street2 != null)
                    s = this.p28Street2 + ", " + this.p28City2;
                else
                    s = this.p28City2;
                if (this.p28PostCode2 != null)
                    s += " " + this.p28PostCode2;
                if (this.p28Country2 != null)
                    s += System.Environment.NewLine + this.p28Country2;
            }


            return s;
        }

        public string GetFullAddressWithBreaks(int intIndex)
        {
            string s = "";
            if (intIndex == 1)
            {
                if (this.p28Street1 != null)
                    s = this.p28Street1 + System.Environment.NewLine + this.p28City1;
                else
                    s = this.p28City1;
                if (this.p28PostCode1 != null)
                    s += System.Environment.NewLine + this.p28PostCode1;
                if (this.p28Country1 != null)
                    s += System.Environment.NewLine + this.p28Country1;
            }

            if (intIndex == 2)
            {
                if (this.p28Street2 != null)
                    s = this.p28Street2 + System.Environment.NewLine + this.p28City2;
                else
                    s = this.p28City2;
                if (this.p28PostCode2 != null)
                    s += System.Environment.NewLine + this.p28PostCode2;
                if (this.p28Country2 != null)
                    s += System.Environment.NewLine + this.p28Country2;
            }


            return s;
        }






    }
}
