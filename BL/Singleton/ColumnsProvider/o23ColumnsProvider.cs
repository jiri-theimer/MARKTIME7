
namespace BL
{
    public class o23ColumnsProvider: ColumnsProviderBase
    {
        public o23ColumnsProvider()
        {
            this.EntityName = "o23Doc";
            
            oc = AF("o23Name", "Název"); oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;
            
            oc = AF("DocType", "Typ dokumentu", "o18x.o18Name"); oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;oc.FixedWidth = 140;oc.SqlExplicitGroupBy = "a.o18ID";

            oc = AF("TagsHtml", "Štítky", "o23_o54x.o54InlineHtml"); oc.RelSqlInCol = "LEFT OUTER JOIN o54TagBindingInline o23_o54x ON a.o23ID=o23_o54x.o54RecordPid AND o23_o54x.o54RecordEntity='o23'";

            oc = AF("RowColor", "Barva", "convert(char(1),a.o23RowColorFlag)");
            oc.FixedWidth = 50;

            AF("o23Code", "Kód záznamu");
            oc=AF("o23NotepadText200", "Notepad"); oc.FixedWidth = 340;
            AFBOOL("o23IsEncrypted", "Notepad zakryptován");

            oc = AF("AktualniStav", "Workflow stav", "b02x.b02Name"); oc.DefaultColumnFlag = gdc1; oc.SqlExplicitGroupBy = "a.b02ID";
            //oc = AF("WorkflowStav", "Workflow stav", "p41_b02y.b02Name"); oc.RelSqlInCol = "LEFT OUTER JOIN b02WorkflowStatus p41_b02y ON a.b02ID=p41_b02y.b02ID";oc.SqlExplicitGroupBy = "a.b02ID";

            AppendTimestamp();
        }
    }
}
