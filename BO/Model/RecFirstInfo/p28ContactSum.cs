using System;

namespace BO
{
    public class p28ContactSum
    {
       public string tabname { get; set; }
        public int p91_Count { get; set; }
        
        public int childs_Count { get; set; }
        public int p31_Wip_Time_Count { get; set; }
        public int p31_Wip_Expense_Count { get; set; }
        public int p31_Wip_Fee_Count { get; set; }
        public int p31_Wip_Kusovnik_Count { get; set; }
        public int p31_Approved_Time_Count { get; set; }
        public int p31_Approved_Expense_Count { get; set; }
        public int p31_Approved_Fee_Count { get; set; }
        public int p31_Approved_Kusovnik_Count { get; set; }
       
        public int o23_Count { get; set; }
        public int o43_Count { get; set; }
        public int last_o23ID { get; set; }
       public int p40_Count { get; set; }
        public int p41_Actual_Count { get; set; }
        public int p41_Closed_Count { get; set; }
      
        public string Last_Invoice { get; set; }
        public int Last_p91ID { get; set; }
        public string Last_Wip_Worksheet { get; set; }

        public int p90_Count { get; set; }
        public bool o52_Exist { get; set; }

        public int b05_Count { get; set; }


    }

}
