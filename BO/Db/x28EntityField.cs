

namespace BO
{
    public enum x24IdENUM
    {
        tInteger = 1,
        tString = 2,
        tDecimal = 3,
        tDate = 4,
        tDateTime = 5,
        tTime = 6,
        tBoolean = 7,
        tNone = 0
    }

    public enum x28FlagENUM
    {
        UserField = 1,
        GridField = 2
    }

    public class x28EntityField : BaseBO
    {
        public int x01ID { get; set; }
        public x28FlagENUM x28Flag { get; set; }
        
        public string x28Entity { get; set; }
        public x24IdENUM x24ID { get; set; }
        public int x27ID { get; set; }
        
        public string x28Name { get; set; }

        public int x28Ordinary { get; set; }
        public bool x28IsAllEntityTypes { get; set; }

        public string x28DataSource { get; set; }
        public bool x28IsFixedDataSource { get; set; }
        public int x28TextboxHeight { get; set; }
        public int x28TextboxWidth { get; set; }
        public bool x28IsRequired { get; set; }

        public string x28Field { get; set; }

        public bool x28IsPublic { get; set; } = true;
        public string x28NotPublic_j04IDs { get; set; }
        public string x28NotPublic_j07IDs { get; set; }

        public string x28Grid_Field { get; set; }
        public string x28Grid_SqlSyntax { get; set; }
        public string x28Grid_SqlFrom { get; set; }
        public bool x28IsGridTotals { get; set; }
        public string x28Pivot_SelectSql { get; set; }
        public string x28Pivot_GroupBySql { get; set; }
        public string x28HelpText { get; set; }
        public string x28Query_Field { get; set; }
        public string x28Query_SqlSyntax { get; set; }
        public string x28Query_sqlComboSource { get; set; }
        public int x28ReminderNotifyBefore { get; set; }


        public string x27Name { get; }


        public string TypeName { get; set; }

        protected string x23Name { get; }

        protected string x23DataSource { get; }



        public string SourceTableName
        {
            get
            {
                return BO.Code.Entity.GetEntity(this.x28Entity) + "_FreeField";

            }
        }
    }
}
