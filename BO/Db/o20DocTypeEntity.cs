using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public enum o20EntryModeENUM
    {
        Combo = 1,
        InsertUpdateWithoutCombo = 2,
        ExternalByWorkflow = 3
    }

    public enum o20GridColumnENUM
    {
        EntityColumn = 1,
        CategoryColumn = 2,
        Both = 3,
        _None = 4
    }

    public enum o20EntityPageENUM
    {
        Label = 1,
        Hyperlink = 2,
        HyperlinkPlusNew = 3,
        NotUsed = 9
    }
    public class o20DocTypeEntity:BaseBO
    {
        public int o20ID { get; set; }
        public int o18ID { get; set; }
        public string o20Entity { get; set; }
        public string o20Name { get; set; }
        public bool o20IsMultiSelect { get; set; }
        public bool o20IsEntryRequired { get; set; }
        public string o20RecTypeEntity { get; set; }
        public int o20RecTypePid { get; set; }
        public o20EntryModeENUM o20EntryModeFlag { get; set; } = o20EntryModeENUM.Combo;
        public o20GridColumnENUM o20GridColumnFlag { get; set; } = o20GridColumnENUM.EntityColumn;

        public string EntityTypeAlias { get; set; }   // pomocný atribut - není v SQL
        public bool o20IsClosed { get; set; }
        public int o20Ordinary { get; set; }
        public o20EntityPageENUM o20EntityPageFlag { get; set; } = o20EntityPageENUM.Label;

        public string BindName {
            get
            {
                if (this.o20Name == null)
                {
                    return BO.Code.Entity.GetAlias(this.o20Entity);
                }
                else
                {
                    return this.o20Name;
                }
            }
        }

        public string BindPrefix
        {
            get
            {
                return this.o20Entity;
            }
        }
    }
}
