using SQLitePCL;

namespace UI.Models
{
    public class Field1
    {
        public string Field { get; set; }
        public string Label { get; set; }
    }
    public class changelogViewModel:BaseViewModel
    {
        public string prefix { get; set; }
        public int pid { get; set; }

        public IEnumerable<Object> lisData { get; set; }

        public List<Field1> fields { get; set; }

        public bool IsHideNullRows { get; set; }

        public void AddField(string field,string label)
        {
            if (this.fields == null)
            {
                this.fields = new List<Field1>();
            }
            this.fields.Add(new Field1() { Field = field, Label = label });
        }

        public string ClassName(object c)
        {
            if (c == null || c == System.DBNull.Value)
            {
                return null;
            }
            switch (c.GetType().Name)
            {

                
                case "Double":
                case "Int32":
                    return "cislo";
                default:
                    return null;
            }
        }

        
        public string SV(object c,string strField,BL.Factory f)
        {
            if (c==null || c == System.DBNull.Value)
            {
                return null;
            }
            switch (c.GetType().Name)
            {
                
                case "DateTime":
                    
                    return BO.Code.Bas.ObjectDateTime2String(c);
                case "Double":
                    return BO.Code.Bas.Num2StringNull((double) c);
                case "Int32":
                    if ((int) c == 0)
                    {
                        return null;
                    }
                    else
                    {
                        switch (strField)
                        {
                            case "j02ID_ApprovedBy":
                                var recJ02 = f.j02UserBL.Load((int)c);
                                if (recJ02 != null) return recJ02.FullnameDesc;
                                break;
                            case "j27ID_Billing_Invoiced":
                                var recJ27 = f.FBL.LoadCurrencyByID((int)c);
                                if (recJ27 != null) return recJ27.j27Code;
                                break;
                            case "p51ID_BillingRate":
                            case "p51ID_CostRate":
                                var recP51 = f.p51PriceListBL.Load((int)c);
                                if (recP51 != null) return recP51.p51Name;
                                break;
                                
                        }
                        return c.ToString();
                    }
                case "String":
                    return c.ToString().Replace(System.Environment.NewLine, "<br><br>");
                    //return c.ToString();
                default:
                    return c.ToString();
            }

            
        }
    }

    
}
