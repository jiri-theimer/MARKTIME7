namespace UI.Views.Shared.Components.myGrid
{
    public class myGridUIContext
    {
        public string entity { get; set; }
        public string prefix
        {
            get
            {
                return entity.Substring(0, 3);
            }
        }
        public int j72id { get; set; }
        public int j72id_query { get; set; }
        public int go2pid { get; set; }
        public string oper { get; set; }    //pagerindex/pagesize/sortfield
        public string key { get; set; }
        public string value { get; set; }

        public string master_entity { get; set; }
        public int master_pid { get; set; }

        public string ondblclick { get; set; }
        public string oncmclick { get; set; }


        public string fixedcolumns { get; set; }

        public string pathname { get; set; }   //volající url v prohlížeči
        public List<string> viewstate { get; set; }   //data ze serveru, aby se přenášela s gridem z klienta na server: oddělovač pipe
        public string myqueryinline { get; set; }
        public string rez { get; set; }
        public int currentpid { get; set; }

        public bool isperiodovergrid { get; set; }
        public bool isnotuse_filtering { get; set; }
        //public int selectableflag { get; set; }
        public bool is_enable_selecting { get; set; }
        public int reczoomflag { get; set; }

        public string record_bin_query { get; set; }
        public string p31_tab_query { get; set; }
        public string period_value { get; set; }
        public string period_field { get; set; }
        public string d1_iso { get; set; }
        public string d2_iso { get; set; }
        public int p31_state_query { get; set; }

    }
}
