

namespace BO
{
    public class FreeFieldInput : x28EntityField
    {
        public string StringInput { get; set; }
        public double NumInput { get; set; }
        public int IntInput { get; set; }
        public DateTime? DateInput { get; set; }
        public bool CheckInput { get; set; }

        public string ComboTextInput { get; set; }

        public bool IsExternalDataSource { get; set; }
        
        public int TextboxHeight { get; set; }

        public bool IsVisible { get; set; } = true;  

        public string GroupName { get; set; }     //skupina polí (x27Name): nadpis ve formuláři
        public int GroupOrdinary { get; set; }    //pořadí skupiny (x27Ordinary)
        public string CssDisplay
        {
            get
            {
                if (this.IsVisible)
                {
                    return "inline-flex;";                    
                }
                else
                {
                    return "display:none;";
                }
            }
        }
    }
}
