

namespace BO.Sys
{
    public class sp_spaceused_table
    {
        public int realsize { get; set; }
        public string tabname { get; set; }
        public int tabrows { get; set; }
        public string tabreserved { get; set; }
        public string tabdata { get; set; }
        public string tabindexsize { get; set; }
        public string tabunused { get; set; }

        
    }

    public class sp_spaceused_db
    {
        
        public string database_name { get; set; }
        
        public string database_size { get; set; }
        public string unallocated_space { get; set; }
        public string reserved { get; set; }
        public string data { get; set; }
        public string index_size { get; set; }
        public string unused { get; set; }


    }
}
