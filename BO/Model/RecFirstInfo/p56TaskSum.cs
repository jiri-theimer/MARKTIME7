

namespace BO
{
    public class p56TaskSum
    {
        public int p91_Count { get; set; }
        public int p31_Wip_Time_Count { get; set; }
        public int p31_Wip_Expense_Count { get; set; }
        public int p31_Wip_Fee_Count { get; set; }
        public int p31_Wip_Kusovnik_Count { get; set; }
        public int p31_Approved_Time_Count { get; set; }
        public int p31_Approved_Expense_Count { get; set; }
        public int p31_Approved_Fee_Count { get; set; }
        public int p31_Approved_Kusovnik_Count { get; set; }
       
        public string Last_Invoice { get; set; }
        public int Last_p91ID { get; set; }
        public string Last_Wip_Worksheet { get; set; }

        public double Hours_Orig { get; set; }
        public double Hours_Orig_Billable { get; set; }
        public double Expenses_Orig { get; set; }
        public double Incomes_Orig { get; set; }
        public int p49_Count { get; set; }
        public int o43_Count { get; set; }
        public int b05_Count { get; set; }

    }
}
