

namespace UI.Models
{
    public class MyToolbarViewModel
    {        
        
        public int RecordPID { get; set; }
        public bool RecordIsClosed { get; set; }
        public string RecordEntity { get; set; }
        public string ControllorName { get; set; } = "Record";
        public string TimeStamp { get; set; }
      
        public bool AllowArchive { get; set; }
        public bool AllowClone { get; set; } = true;
        public bool AllowDelete { get; set; } = true;
        public string ExplicitValidFrom { get; set; }
        public String ExplicitValidUntil { get; set; }
        public bool IsCurrentClone { get; set; }
        public bool IsSave { get; set; }
        public bool IsRefresh { get; set; }
        public bool IsClose { get; set; } = true;
        public bool IsDelete { get; set; }
        public bool IsApply { get; set; } = false;
        public bool IsNew { get; set; }
        public bool IsClone { get; set; }
        public bool IsToArchive { get; set; }
        public bool IsFromArchive { get; set; }
        
        public string Message { get; set; }
        //public string ArchiveFlag { get; set; }
        public string BG;

        //private BO.BaseBO _rec { get; set; }
        public BO.BaseBO BaseRecord { get; set; }

        public MyToolbarViewModel(BO.BaseBO rec,bool bolAllowArchive=true)
        {
            this.BaseRecord = rec;
            //_rec = rec;
            this.AllowArchive = bolAllowArchive;
            this.RecordEntity = rec.entity;
            this.RecordPID = rec.pid;
            this.RecordIsClosed = rec.isclosed;
            if (this.AllowArchive && rec.pid>0)
            {
                if (rec.ValidFrom !=null) this.ExplicitValidFrom = BO.Code.Bas.ObjectDate2String(rec.ValidFrom);
                if (rec.ValidUntil !=null) this.ExplicitValidUntil = BO.Code.Bas.ObjectDate2String(rec.ValidUntil);
            }

            
            
            RefreshState();

        }
        public MyToolbarViewModel()
        {
            RefreshState();
        }

        public string getTimeStampHtml(BL.Factory f)
        {
            if (this.BaseRecord == null)
            {
                return null;
            }
            if (this.BaseRecord.pid > 0)
            {
                var sb = new System.Text.StringBuilder();
                //sb.AppendLine("<div style='border-top:solid 1px #F5F5F5;margin-top:20px;'>");
                if (this.BaseRecord.DateInsert != null)
                {
                    sb.Append("<small style='color:silver'>"+f.tra("Založení záznamu")+ ":&nbsp;</small>");
                                      
                    sb.Append("<small>");
                    sb.Append(this.BaseRecord.UserInsert + " / " + BO.Code.Time.GetTimestamp(this.BaseRecord.DateInsert));
                    sb.Append("</small>");
                }
                if (this.BaseRecord.DateUpdate > this.BaseRecord.DateInsert)
                {
                    sb.Append("<small style='color:silver;margin-left:20px;'>"+f.tra("Poslední aktualizace")+ ":&nbsp;</small>");

                    sb.Append("<small style='color:brown'>");
                    if (this.BaseRecord.UserInsert == this.BaseRecord.UserUpdate)
                    {
                        sb.Append(BO.Code.Time.GetTimestamp(this.BaseRecord.DateUpdate));
                    }
                    else
                    {
                        sb.Append(this.BaseRecord.UserUpdate + " / " + BO.Code.Time.GetTimestamp(this.BaseRecord.DateUpdate));
                    }
                    
                    sb.Append("</small>");
                }
                if (this.AllowArchive && this.BaseRecord.ValidFrom != null && this.BaseRecord.ValidUntil != null)
                {
                    //this.ExplicitValidFrom = BO.Code.Bas.ObjectDateTime2String(this.BaseRecord.ValidFrom);
                    this.ExplicitValidFrom = BO.Code.Time.GetTimestamp(this.BaseRecord.ValidFrom);
                    //this.ExplicitValidUntil = BO.Code.Bas.ObjectDateTime2String(this.BaseRecord.ValidUntil);
                    this.ExplicitValidUntil = BO.Code.Time.GetTimestamp(this.BaseRecord.ValidUntil);

                    sb.Append("<small style='margin-left:20px;color:silver;'>"+f.tra("Platnost záznamu")+ ":&nbsp;</small>");
                    sb.Append("<small class='text-info'>");
                    if (this.BaseRecord.ValidUntil > DateTime.Now && this.BaseRecord.ValidFrom <= DateTime.Now)
                    {
                        sb.Append(" "+f.tra("Záznam je časově platný."));
                    }
                    else
                    {
                        sb.Append(this.BaseRecord.ValidFrom.ToString() + " - " + this.BaseRecord.ValidUntil.ToString());
                        sb.Append("<kbd style='font-size:120%;'>"+f.tra("Záznam je v archivu.")+"</kbd>");
                    }
                    sb.Append("</small>");
                }

                //sb.AppendLine("</div>");
                return sb.ToString();
            }
            else
            {
                return null;
            }
        }

        public void MakeClone()
        {
            Message = "Kopírovaný záznam";
            this.RecordPID = 0;
            this.ExplicitValidFrom = BO.Code.Bas.ObjectDateTime2String(DateTime.Now);
            this.ExplicitValidUntil = BO.Code.Bas.ObjectDateTime2String(new DateTime(3000, 1, 1));
            RefreshState();
        }

        public void RefreshState()
        {            
            
            IsSave = true;
            BG = "bg-light";
            if (RecordPID == 0)
            {
                IsDelete = false;
                IsNew = false;
                IsClone = false;
                IsRefresh = false;
                IsToArchive = false;
                IsFromArchive = false;
            }
            else
            {
                IsDelete = this.AllowDelete;
                IsNew = true;
                IsClone = this.AllowClone;
                IsRefresh = true;
                if (this.RecordIsClosed)
                {
                    IsFromArchive = this.AllowArchive;
                    BG = "bg-dark";
                }
                else
                {
                    IsToArchive = this.AllowArchive;
                }
                
            }
        }

        public DateTime? GetValidUntil(BO.BaseBO rec)
        {
            if (string.IsNullOrEmpty(this.ExplicitValidUntil) == true)
            {
                return new DateTime(3000, 1, 1);                
            }
            else
            {
                return BO.Code.Bas.String2Date(this.ExplicitValidUntil);
            }
            //switch (this.ArchiveFlag)
            //{
            //    case "1":
            //        return DateTime.Now;                    
            //    case "2":
            //        return new DateTime(3000, 1, 1);                    
            //    default:
            //        if (rec.pid == 0)
            //        {
            //            return new DateTime(3000, 1, 1);
            //        }
            //        else
            //        {
            //            return rec.ValidUntil;
            //        }

            //}            
        }
        public DateTime? GetValidFrom(BO.BaseBO rec)
        {
            if (string.IsNullOrEmpty(this.ExplicitValidFrom) == true)
            {
                return DateTime.Now.AddSeconds(-10);
            }
            else
            {
                return BO.Code.Bas.String2Date(this.ExplicitValidFrom);
            }
            //if (rec.pid == 0)
            //{
            //    return DateTime.Now.AddSeconds(-10);
            //}
            //else
            //{
            //    return rec.ValidFrom;
            //}
        }
    }
}
