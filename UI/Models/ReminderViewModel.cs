using BO;

namespace UI.Models
{
    public class ReminderViewModel
    {
        public bool is_static_date { get; set; }
        public string elementidprefix { get; set; } = "reminder.";
        public string record_prefix { get; set; }
        public int record_pid { get; set; }
        public string postback_par { get; set; }
        public string postback_guid { get; set; }

        public List<ReminderRepeater> lisReminder { get; set; }

        
        public int edit_o24count { get; set; }
        public string edit_o24unit { get; set; }
        public int edit_o24mediumflag { get; set; }
        public string edit_bindprefix { get; set; }
        public int edit_j02id { get; set; }
        public int edit_j11id { get; set; }
        public int edit_p28id { get; set; }
        public int edit_p24id { get; set; }
        public int edit_x67id { get; set; }
        public string edit_o24memo { get; set; }
        public string edit_o24staticdate { get; set; }


        public void SaveChanges(BL.Factory f,int record_pid,DateTime? record_date=null)
        {            
            if (this.lisReminder == null || this.lisReminder.Count()==0) return;
            this.record_pid = record_pid;
            var lisO24 = f.o24ReminderBL.GetList(this.record_prefix, this.record_pid);
            foreach(var c in this.lisReminder)
            {
                if (c.IsTempDeleted || (c.j02ID==0 && c.j11ID==0 && c.p28ID==0 && c.x67ID==0 && c.p24ID==0))
                {
                    if (c.o24ID > 0) f.CBL.DeleteRecord("o24Reminder", c.o24ID);
                }
                else
                {                    
                    var rec = new BO.o24Reminder() { o24RecordEntity = this.record_prefix, o24RecordPid = this.record_pid };
                    if (c.o24ID > 0)
                    {
                        if (lisO24.Any(p => p.pid == c.o24ID))
                        {
                            rec = lisO24.First(p => p.pid == c.o24ID);
                            rec.j02ID = 0;rec.p28ID = 0;rec.x67ID = 0;rec.p24ID = 0;rec.j11ID = 0;
                        }                        
                    }
                    rec.o24MediumFlag = (BO.o24MediumFlagEnum) c.o24MediumFlag;
                    switch (c.BindPrefix)
                    {
                        case "j02": rec.j02ID = c.j02ID;break;
                        case "j11": rec.j11ID = c.j11ID; break;
                        case "x67": rec.x67ID = c.x67ID; break;
                        case "p28": rec.p28ID = c.p28ID; break;
                        case "p24": rec.p24ID = c.p24ID; break;
                    }

                    rec.o24Memo = c.o24Memo;

                    if (rec.o24Flag != null && rec.o24Flag.Contains("FreeDate")) //reminder z uživatelského pole s vyplněným x28ReminderNotifyBefore>0
                    {
                        rec.o24StaticDate = rec.o24RecordDate.Value.AddHours((double)rec.o24Count);   //přepsat čas upozornění podle nastavení uživatelského pole
                        rec.o24RecordDate = rec.o24StaticDate;
                    }
                    else
                    {
                        rec.o24Unit = c.o24Unit; rec.o24Count = c.o24Count;

                        if (this.is_static_date)
                        {
                            rec.o24StaticDate = c.o24StaticDate;
                            rec.o24RecordDate = c.o24StaticDate;
                        }
                        else
                        {
                            rec.o24RecordDate = record_date;
                        }
                    }

                    rec.ValidUntil = f.CBL.GetCurrentRecordValidUntil(this.record_prefix, this.record_pid); //platnost záznamu je stejná jako vodící záznam

                    
                    
                    f.o24ReminderBL.Save(rec);
                }
                
            }
        }
    }

    public class ReminderRepeater
    {
        public int o24Count { get; set; }
        public string o24Unit { get; set; }
        public int o24MediumFlag { get; set; }
        public DateTime? o24StaticDate { get; set; }
        public string o24Memo { get; set; }
        public string BindPrefix { get; set; }
        public int j02ID { get; set; }
        public string BindAlias { get; set; }
        public int j11ID { get; set; }
        public int p28ID { get; set; }
        public int p24ID { get; set; }
        
        public int x67ID { get; set; }
        public string TempGuid { get; set; }
        public bool IsTempDeleted { get; set; }
        public int o24ID { get; set; }
        public bool IsProcessed { get; set; }

        public string CssTempDisplay
        {
            get
            {
                if (this.IsTempDeleted)
                {
                    return "display:none;";
                }
                else
                {
                    return "display:table-row;";
                }
            }
        }
    }
}
