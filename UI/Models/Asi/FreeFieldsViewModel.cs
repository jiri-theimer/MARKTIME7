

namespace UI.Models
{
    public class FreeFieldsViewModel
    {
        public string elementidprefix { get; set; } = "ff1.";
        public string caller { get; set; }
        public List<BO.FreeFieldInput> inputs { get; set; }
        

        public int VisibleInputsCount {
            get
            {

                if (this.inputs == null) return 0;
                return this.inputs.Where(p => p.IsVisible == true).Count();
            }
        }
        
       
        public void InhaleFreeFieldsView(BL.Factory f, int rec_pid, string prefix)
        {

            var lisX28 = f.x28EntityFieldBL.GetList(new BO.myQuery("x28")).Where(p => p.x28Flag == BO.x28FlagENUM.UserField && p.x28Entity == prefix).OrderBy(p => p.x28Ordinary);
            if (lisX28.Count() > 0)
            {
                SetupInputs(lisX28, f.x28EntityFieldBL.GetFieldsValues(rec_pid, lisX28));
            }
            
        }
        public void RefreshInputsVisibility(BL.Factory f, int rec_pid, string prefix, int intEntityTypeID)
        {   //podle typu záznamu (intEntityTypeID) určit, jaké pole je viditelné
            var lisX28 = f.x28EntityFieldBL.GetList_ApplicableInForm(prefix, intEntityTypeID, false);
            if (this.inputs == null) return;

            foreach(var c in this.inputs)
            {
                c.IsVisible = false;
            }
            foreach(var c in lisX28)
            {
                this.inputs.Where(p => p.x28Field == c.x28Field).First().IsVisible = true;
            }
        }
        private void SetupInputs(IEnumerable<BO.x28EntityField> lisX28, System.Data.DataTable vals)
        {
            this.inputs = new List<BO.FreeFieldInput>();

            foreach(var recX28 in lisX28)
            {
                var c = new BO.FreeFieldInput() { x28Field = recX28.x28Field, x28Name = recX28.x28Name, TypeName = recX28.TypeName,
                    x28Entity = recX28.x28Entity, x24ID = recX28.x24ID, x28IsRequired = recX28.x28IsRequired,
                    x28DataSource = recX28.x28DataSource, x28IsFixedDataSource = recX28.x28IsFixedDataSource, x28ReminderNotifyBefore = recX28.x28ReminderNotifyBefore,
                    TextboxHeight = recX28.x28TextboxHeight
                };             
                
                this.inputs.Add(c);
            }

            if (vals != null && vals.Rows.Count>0)
            {
                System.Data.DataRow dbRow = vals.Rows[0];
                
                foreach (var c in this.inputs)
                {
                    if (dbRow[c.x28Field] != System.DBNull.Value)
                    {
                        switch (c.TypeName)
                        {
                            case "boolean":
                                c.CheckInput = Convert.ToBoolean(dbRow[c.x28Field]);
                                break;
                            case "date":
                            case "datetime":
                                c.DateInput = Convert.ToDateTime(dbRow[c.x28Field]);
                                
                                                           
                                break;
                            case "decimal":
                            case "integer":
                                c.NumInput = Convert.ToDouble(dbRow[c.x28Field]);
                                break;
                            default:
                                c.StringInput = Convert.ToString(dbRow[c.x28Field]);
                                break;
                        }
                    }
                    
                        
                }
            }
        }
    }
}
