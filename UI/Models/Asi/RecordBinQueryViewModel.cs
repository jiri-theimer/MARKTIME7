namespace UI.Models
{
    public class RecordBinQueryViewModel
    {
        public int Value { get; set; }
        public string UserParamKey { get; set; }
        public string Prefix { get; set; }

        public string getStateAlias()
        {
            switch (Value)
            {
                case 1:
                    return "Pouze otevřené záznamy";
                case 2:
                    return "Pouze záznamy v archivu";
                
                default:
                    return "Otevřené i v archivu";

            }
        }
    }
}
