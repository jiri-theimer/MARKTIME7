namespace UI.Models
{
    public class p31TabQueryViewModel
    {
        public string Value { get; set; }
        public string UserParamKey { get; set; }
        public string javascript_onchange { get; set; } = "handle_p31tabquery_change";
        public string getStateAlias()
        {
            switch (Value)
            {
                case "time":
                    return "Hodiny";
                case "kusovnik":
                    return "Kusovník";
                case "expense":
                    return "Peněžní výdaje";
                case "fee":
                    return "Pevné odměny";


                default:
                    return "Formát úkonů";

            }
        }
    }
}
