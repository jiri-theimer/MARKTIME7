using System;
using System.Collections.Generic;
using System.Text;

namespace BO
{
    public class myQueryJ02:baseQuery
    {

        public bool? isvirtualperson { get; set; }
        public bool? allowed_for_p31_entry { get; set; }    //osoby, za které přihlášený uživatel může vykazovat úkony
        public bool? allowed_for_p31_read { get; set; }    //osoby, za které přihlášený uživatel může číst  úkony
        public int j04id { get; set; }
        public int j11id { get; set; }
        public List<int> j11ids { get; set; }
        public int j07id { get; set; }
        public List<int> j07ids { get; set; }
        public int j18id { get; set; }
        public int o51id { get; set; }

        public int p41id { get; set; }
        public int p91id { get; set; }
        public string tabquery { get; set; }
        public int j27id_query { get; set; }

        public string x67Entity { get; set; }
        public int x69RecordPid { get; set; }


        public myQueryJ02()
        {
            this.Prefix = "j02";            
        }

        public override List<QRow> GetRows()
        {
            if (this.p31statequery > 0) this.Handle_p31StateQuery();
            if (!string.IsNullOrEmpty(this.p31tabquery)) this.Handle_p31TabQuery();
            Handle_Wip();

            if (this.IsActivePeriodQuery())
            {
                switch (this.period_field)
                {
                    case "j02DateInsert":
                        AQ("a.j02DateInsert BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "p91Date":

                        AQ("EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p91Invoice xb ON xa.p91ID=xb.p91ID WHERE xa.j02ID=a.j02ID AND xb.p91Date BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        break;
                    case "p91DateSupply":

                        AQ("EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p91Invoice xb ON xa.p91ID=xb.p91ID WHERE xa.j02ID=a.j02ID AND xb.p91DateSupply BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        break;
                    case "p31Date":
                    default:
                        AQ("EXISTS (select 1 FROM p31Worksheet WHERE j02ID=a.j02ID AND p31Date BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        break;
                }
            }



            if (!this.CurrentUser.IsAdmin)   //neřešit pokud je admin
            {
                if (this.allowed_for_p31_entry==true)   //zda oprávnění zapisovat za uživatele úkony
                {
                    if (this.CurrentUser.IsMasterPerson)
                    {
                        string s = "(a.j02ID IN (SELECT j02ID_Slave FROM j05MasterSlave WHERE j02ID_Master=@j02id_me AND j05IsCreate_p31=1)";
                        s += " OR a.j02ID IN (SELECT j12.j02ID FROM j12Team_Person j12 INNER JOIN j05MasterSlave xj05 ON j12.j11ID=xj05.j11ID_Slave WHERE xj05.j02ID_Master=@j02id_me AND xj05.j05IsCreate_p31=1)";
                        s += " OR a.j02ID=@j02id_me)";
                        AQ(s, "j02id_me", this.CurrentUser.pid);
                    }
                    else
                    {
                        AQ("a.j02ID=@j02id_me", "j02id_me", this.CurrentUser.pid);
                    }
                }
                if (this.allowed_for_p31_read == true && !CurrentUser.TestPermission(PermValEnum.GR_P31_Reader))    //zda oprávnění číst všechny úkon v db nebo pouze část
                {
                    if (this.CurrentUser.IsMasterPerson)
                    {
                        string s = "(a.j02ID IN (SELECT j02ID_Slave FROM j05MasterSlave WHERE j02ID_Master=@j02id_me AND j05IsCreate_p31=1)";
                        s += " OR a.j02ID IN (SELECT j12.j02ID FROM j12Team_Person j12 INNER JOIN j05MasterSlave xj05 ON j12.j11ID=xj05.j11ID_Slave WHERE xj05.j02ID_Master=@j02id_me AND xj05.j05Disposition_p31>0)";
                        s += " OR a.j02ID=@j02id_me)";
                        AQ(s, "j02id_me", this.CurrentUser.pid);
                    }
                    else
                    {
                        AQ("a.j02ID=@j02id_me", "j02id_me", this.CurrentUser.pid);
                    }
                }
                if (this.MyRecordsDisponible)
                {
                    Handle_MyDisponible();
                }
            }

            if (this.isvirtualperson != null)
            {
                if (this.isvirtualperson == true)
                {
                    AQ("a.j02VirtualParentID IS NOT NULL", null, null);
                }
                else
                {
                    AQ("a.j02VirtualParentID IS NULL", null, null);
                }
            }

            if (this.j04id > 0)
            {
                AQ("a.j04ID=@j04id", "j04id", this.j04id);
            }            
            if (this.j11id > 0)
            {
                AQ("a.j02ID IN (select j02ID FROM j12Team_Person WHERE j11ID=@j11id)", "j11id", this.j11id);
            }
            if (this.j11ids != null && this.j11ids.Count > 0)
            {
                AQ("a.j02ID IN (select j02ID FROM j12Team_Person WHERE j11ID IN ("+ string.Join(",", this.j11ids) + "))", null, null);
            }
            if (this.j07id > 0)
            {
                AQ("a.j07ID=@j07id", "j07id", this.j07id);
            }
            if (this.j07ids !=null && this.j07ids.Count > 0)
            {
                AQ("a.j07ID IN (" + string.Join(",", this.j07ids)+")", null, null);
            }
            if (this.j18id > 0)
            {
                AQ("a.j18ID=@j18id", "j18id", this.j18id);
            }
           
            if (_searchstring != null && _searchstring.Length > 2)
            {                
                AQ("(a.j02FirstName like @expr+'%' COLLATE Latin1_General_CI_AI OR a.j02LastName LIKE '%'+@expr+'%' COLLATE Latin1_General_CI_AI OR a.j02Email LIKE '%'+@expr+'%' OR a.j02Login LIKE '%'+@expr+'%')", "expr", _searchstring);

            }

            if (this.p41id > 0)
            {
                AQ("a.j02ID IN (select j02ID FROM p31Worksheet WHERE p41ID=@p41id", "p41id", this.p41id);
            }

            if (this.p91id > 0)
            {
                
                AQ("a.j02ID IN (select j02ID FROM p31Worksheet WHERE p91ID=@p91id)", "p91id", this.p91id);
            }
            if (this.o51id > 0)
            {
                AQ("a.j02ID IN (select o52RecordPid FROM o52TagBinding where o52RecordEntity='j02' AND o51ID=@o51id)", "o51id", this.o51id);
            }

            if (this.j27id_query > 0)
            {
                AQ("EXISTS (select 1 FROM p31Worksheet WHERE j02ID=a.j02ID AND j27ID_Billing_Orig=@j27id_query)", "j27id_query", this.j27id_query);
            }


            if (this.iswip != null)
            {
                if (this.iswip == true)
                {
                    AQ("EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xa.j02ID=a.j02ID AND xa.p71ID IS NULL AND xa.p91ID IS NULL AND xa.p31ExcludeBillingFlag IS NULL AND xa.p31Date between @p31date1 AND @p31date2)", null, null);
                }
                else
                {
                    AQ("NOT EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xa.j02ID=a.j02ID AND xa.p71ID IS NULL AND xa.p91ID IS NULL AND xa.p31Date between @p31date1 AND @p31date2)", null, null);
                }
            }
            if (this.isapproved_and_wait4invoice != null)
            {
                if (this.isapproved_and_wait4invoice == true)
                {
                    AQ("EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xa.j02ID=a.j02ID AND xa.p71ID=1 AND xa.p91ID IS NULL AND xa.p31Date between @p31date1 AND @p31date2)", null, null);
                }
                //else
                //{
                //    AQ("NOT EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xa.j02ID=a.j02ID AND xa.p71ID=1 AND xa.p72ID_AfterApprove=4 AND xa.p91ID IS NULL AND xa.p31Date between @p31date1 AND @p31date2)", null, null);
                //}
            }

            if (this.x67Entity !=null && this.x69RecordPid > 0) //lidí podle x69EntityRole_Assign
            {
                AQ("EXISTS (select 1 FROM x67EntityRole xa INNER JOIN x69EntityRole_Assign xb ON xa.x67ID=xb.x67ID LEFT OUTER JOIN j12Team_Person xc ON xb.j11ID=xc.j11ID WHERE (xb.j02ID IS NOT NULL AND xb.j02ID=a.j02ID) OR (xb.j11ID IS NOT NULL AND xc.j02ID=a.j02ID))",null,null);
            }


            return this.InhaleRows();

        }

        private void Handle_MyDisponible()
        {
            if (this.CurrentUser.IsAdmin)
            {
                return;
            }
            if (this.CurrentUser.IsMasterPerson)
            {
                string s = "(a.j02ID IN (SELECT j02ID_Slave FROM j05MasterSlave WHERE j02ID_Master=@j02id_me)";
                s += " OR a.j02ID IN (SELECT j12.j02ID FROM j12Team_Person j12 INNER JOIN j05MasterSlave xj05 ON j12.j11ID=xj05.j11ID_Slave WHERE xj05.j02ID_Master=@j02id_me)";
                s += " OR a.j02ID=@j02id_me)";
                AQ(s, "j02id_me", this.CurrentUser.pid);
            }
            else
            {
                AQ("a.j02ID=@j02id_me", "j02id_me", this.CurrentUser.pid);
            }
        }

        private void Handle_Wip()
        {
            if (this.iswip != null)
            {
                if (this.iswip == true)
                {
                    AQ("a.j02ID IN (select j02ID FROM p31Worksheet WHERE p71ID IS NULL AND p91ID IS NULL AND p31ExcludeBillingFlag IS NULL AND p31Date between @p31date1 AND @p31date2)", null, null);
                }
                else
                {
                    AQ("a.j02ID NOT IN (select j02ID FROM p31Worksheet WHERE p71ID IS NULL AND p91ID IS NULL AND p31Date between @p31date1 AND @p31date2)", null, null);
                }
            }

            if (this.isapproved_and_wait4invoice != null)
            {
                if (this.isapproved_and_wait4invoice == true)
                {
                    AQ("a.j02ID IN (select za.j02ID FROM p31Worksheet za INNER JOIN p41Project zb ON za.p41ID=zb.p41ID WHERE za.p71ID=1 AND za.p91ID IS NULL AND za.p31Date between @p31date1 AND @p31date2)", null, null);
                }
                else
                {
                    AQ("a.j02ID IN (select j02ID FROM p31Worksheet WHERE p91ID IS NULL AND p31Date between @p31date1 AND @p31date2)", null, null);
                }
            }
        }



    }
}
