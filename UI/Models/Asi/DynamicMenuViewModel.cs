namespace UI.Models
{
    public class DynamicMenuViewModel
    {
        public string Prefix { get; set; }
       public string MasterPrefix { get; set; }
        public int MasterPid { get; set; }
        public string Rez { get; set; }
        public int p07Level { get; set; }   //předává se pro menu main_p41
        public int j72id { get; set; }  //předává se pro grid_menu
        
    }
}
