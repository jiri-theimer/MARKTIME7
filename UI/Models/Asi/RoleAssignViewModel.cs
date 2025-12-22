using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models
{
    public class RoleAssignViewModel
    {
        public string elementidprefix { get; set; } = "roles.";
        public string RolePrefix { get; set; }
        public string RolePrefixSecond { get; set; }
        public string RecPrefix { get; set; }
        public int RecPid { get; set; }
        public string Header { get; set; }

        public int Default_j02ID { get; set; }
        public string Default_Person { get; set; }
        public int Default_j11ID { get; set; }
        public string Default_Team { get; set; }
        public int Default_x67ID { get; set; }
        public int OnlyOne_x67ID { get; set; }

        public List<RoleAssignRepeator> lisRepeator { get; set; }


        public List<BO.x69EntityRole_Assign> getList4Save(BL.Factory f)
        {
            var lis = new List<BO.x69EntityRole_Assign>();
            if (this.lisRepeator == null)
            {
                return lis;
            }
            foreach (var c in lisRepeator)   //.Where(p=>!p.IsNone)
            {
                if (c.IsNone || (c.j02IDs==null && c.j11IDs==null && !c.x69IsAllUsers) )
                {
                    //role neobsazena
                    var cc = new BO.x69EntityRole_Assign() { x67ID = c.x67ID, x69RecordEntity = this.RecPrefix, x67IsRequired = c.x67IsRequired, x67Name = c.x67Name };                    
                    lis.Add(cc);
                    continue;
                }
                if (c.x69IsAllUsers)
                {
                    var cc = new BO.x69EntityRole_Assign() { x67ID = c.x67ID, x69RecordEntity = this.RecPrefix, x67IsRequired = c.x67IsRequired, x67Name = c.x67Name };
                    cc.x69IsAllUsers = true;
                    lis.Add(cc);
                }
                else
                {
                    if (c.j02IDs != null)
                    {
                        foreach (int intJ02ID in BO.Code.Bas.ConvertString2ListInt(c.j02IDs))
                        {
                            var cc = new BO.x69EntityRole_Assign() { x67ID = c.x67ID, j02ID = intJ02ID,x67IsRequired=c.x67IsRequired, x67Name = c.x67Name };
                            lis.Add(cc);
                        }
                    }
                    if (c.j11IDs != null)
                    {
                        foreach (int intJ11ID in BO.Code.Bas.ConvertString2ListInt(c.j11IDs))
                        {
                            var cc = new BO.x69EntityRole_Assign() { x67ID = c.x67ID, j11ID = intJ11ID,x67IsRequired=c.x67IsRequired,x67Name=c.x67Name };
                            lis.Add(cc);
                        }
                    }
                }


            }

            return lis;
        }
    }

    public class RoleAssignRepeator
    {
        public int x67ID { get; set; }
        public string x67Name { get; set; }
        public string x67Entity { get; set; }
        public string j02IDs { get; set; }
        public string Persons { get; set; }
        public bool x67IsRequired { get; set; }

        public string j11IDs { get; set; }
        public string Teams { get; set; }

        public bool x69IsAllUsers { get; set; }
        public bool IsNone { get; set; } = true;

        public string CssVisibility_j11 { get; set; } = "visible";
        public string CssVisibility_j02 { get; set; } = "visible";
        public string CssVisibility_all { get; set; } = "visible";

    }
}
