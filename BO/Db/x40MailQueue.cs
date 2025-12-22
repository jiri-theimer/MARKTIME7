using System;

namespace BO
{
    public enum x40StateENUM
    {
        _NotSpecified = 0,
        InQueque = 1,
        IsError = 2,
        IsProceeded = 3,
        IsStopped = 4,
        WaitOnConfirm = 5
    }
    public class x40MailQueue:BaseBO
    {
        public string x40RecordEntity { get; set; }
        
        public x40StateENUM x40Status { get; set; } = x40StateENUM.InQueque;
        public bool x40IsProcessed { get; set; }
        public DateTime? x40DatetimeProcessed { get; set; }
        public int x40RecordPid { get; set; }
        public int j02ID_Creator { get; set; }
        public int j40ID { get; set; }
        public int o24ID { get; set; }
        public int x40MailID { get; set; }
        public string x40Subject { get; set; }
        public string x40Body { get; set; }
        public int x04ID { get; set; }
        public bool x40IsRecordLink { get; set; }
        public bool x40IsUserSignature { get; set; }
        public bool x40IsHtmlBody { get; set; }
        public string x40SenderName { get; set; }
        public string x40SenderAddress { get; set; }
        public string x40Recipient { get; set; }
        public string x40CC { get; set; }
        public string x40BCC { get; set; }

        public int j61ID { get; set; }
        public string x40CC_j61 { get; set; }
        public string x40BCC_j61 { get; set; }
        public string x40TO_j61 { get; set; }
        public string x40Attachments { get; set; }
        
        public string x40ErrorMessage { get; set; }

        public bool x40IsAutoNotification { get; set; }
        


        public string x40BatchGuid { get; set; }
        public string x40MessageGuid { get; set; }
        public string x40SkeletonFile { get; set; }
        public int x40EmlFileSize { get; set; }
        public string x40EmlFolder { get; set; }
        public string x40GridColumns { get; set; }
        public string StatusAlias
        {
            get
            {
                switch (this.x40Status)
                {
                    case x40StateENUM.InQueque:
                        {
                            return "Odesílá se";
                        }

                    case x40StateENUM.IsError:
                        {
                            return "Chyba";
                        }

                    case x40StateENUM.IsProceeded:
                        {
                            return "Odesláno";
                        }

                    case x40StateENUM.IsStopped:
                        {
                            return "Zastaveno";
                        }

                    case x40StateENUM.WaitOnConfirm:
                        {
                            return "Čeká na odeslání";
                        }

                    default:
                        {
                            return "?";
                        }
                }
            }
        }
        public string StatusColor
        {
            get
            {
                switch (this.x40Status)
                {
                    case x40StateENUM.InQueque:
                        {
                            return "#996633";
                        }

                    case x40StateENUM.IsError:
                        {
                            return "#ff0000";
                        }

                    case x40StateENUM.IsProceeded:
                        {
                            return "#008000";
                        }

                    case x40StateENUM.IsStopped:
                        {
                            return "#ff66ff";
                        }

                    case x40StateENUM.WaitOnConfirm:
                        {
                            return "#0000ff";
                        }

                    default:
                        {
                            return "?";
                        }
                }
            }
        }
        
    }
}
