
namespace BO
{
    public class myQuery:baseQuery
    {
        public int x01id { get; set; }
        public string x38entity { get; set; }
        public string x28entity { get; set; }
        public string b01entity { get; set; }
        public string x31entity { get; set; }
        public string x67entity { get; set; }
        public string b20entity { get; set; }
        public string j61entity { get; set; }
        public string p85guid { get; set; }

        public bool? o21isusercombo { get; set; }
        public bool? p57ishelpdesk { get; set; }
        public int x54id { get; set; }
        public int j02id_a_query { get; set; }
       
        public myQuery(string prefix)
        {
            this.Prefix = prefix.Substring(0,3);
            
        }

       

        public override List<QRow> GetRows()
        {
            if (this.p85guid != null)
            {
                AQ("a.p85Guid=@p85guid", "p85guid", this.p85guid);
            }
            if (this.x01id > 0)
            {
                AQ("a.x01ID=@x01id", "x01id", this.x01id);
            }
           
            

            if (this.Prefix == "x67")
            {
                AQ("a.x67ParentID IS NULL", null, null);
            }
            if (this.x54id > 0)
            {
                AQ("a.x55ID IN (select x55ID FROM x57WidgetToGroup WHERE x54ID=@x54id)", "x54id", this.x54id);
            }
            
            handle_entity_query("x28",this.x28entity);
            handle_entity_query("x67", this.x67entity);
            handle_entity_query("x38", this.x38entity);
            handle_entity_query("b01", this.b01entity);
            handle_entity_query("x31", this.x31entity);
            handle_entity_query("j61", this.j61entity);
            handle_entity_query("b20", this.b20entity);

            if (this.o21isusercombo != null)
            {
                if (this.o21isusercombo == true)
                {
                    AQ("a.o21TypeFlag<>@o21typeflag", "o21typeflag", "4");
                }
                else
                {
                    AQ("a.o21TypeFlag=@o21typeflag", "o21typeflag", "4");
                }
                
            }
         
            if (this.p57ishelpdesk == true)
            {
                AQ("a.p57HelpdeskFlag=1", null, null);
            }
            

            return this.InhaleRows();

        }

        private void handle_entity_query(string entity_name,string entity_value)
        {
            if (entity_value == null) return;
            AQ($"a.{entity_name}Entity=@entity_value", "entity_value", entity_value);
        }
        
    }
}
