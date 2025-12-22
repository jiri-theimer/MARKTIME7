using System.ComponentModel.DataAnnotations;

namespace BO
{
    public enum j72SystemFlagEnum
    {
        User=0,
        Grid=1,
        QueryOnly=2
    }

    public class j72TheGridTemplate : BaseBO
    {
        [Key]
        public int j72ID { get; set; }
        public int j02ID { get; set; }
        public string j72Name { get; set; }
        
        public j72SystemFlagEnum j72SystemFlag { get; set; }        

        public string j72Entity { get; set; }
        public string j72MasterEntity { get; set; }
        public string j72Rez { get; set; }
        public string j72Columns { get; set; }

        public bool j72IsNoWrap { get; set; }


        public bool j72IsPublic{get;set;}
        public int j72SelectableFlag { get; set; } = 1;

        public bool j72IsQueryNegation { get; set; }

        public bool j72HashJ73Query;

        public string GetName()
        {

            switch (j72SystemFlag)
            {
                case j72SystemFlagEnum.Grid:
                    if (this.j72MasterEntity == "recpage")
                    {
                        return "Výchozí Tabulka (úzká)";
                    }
                    else
                    {
                        return "Výchozí Tabulka";
                    }
                    
                default:
                    if (this.j72Name != null)
                    {
                        return this.j72Name;
                    }
                    else
                    {
                        return "?????";
                    }
            }
        }

        

    }
}
