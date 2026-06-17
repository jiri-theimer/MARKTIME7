

namespace BO.Integrace.Ecomail.Subscriber
{
    public class RootObject
    {
        public Subscriber subscriber { get; set; }
    }

    public class Subscriber
    {
        public string name { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string gender { get; set; }
        public object vokativ { get; set; }
        public int bounce_soft { get; set; }
        public int bounce_soft_count { get; set; }
        public int bounced_hard { get; set; }
        public object bounce_message { get; set; }
        public string inserted_at { get; set; }
        public object last_position { get; set; }
        public float rating { get; set; }
        public object nameday { get; set; }
        public object source { get; set; }
        public object company { get; set; }
        public object street { get; set; }
        public object city { get; set; }
        public string country { get; set; }
        public object zip { get; set; }
        public object phone { get; set; }
        public object pretitle { get; set; }
        public object surtitle { get; set; }
        public object birthday { get; set; }
        public string notes { get; set; }
        public string vokativ_s { get; set; }
        public string[] tags { get; set; }
        public object last_open { get; set; }
        public object last_click { get; set; }
        public object last_pageview { get; set; }
        public object last_transaction_id { get; set; }
        public object last_transaction { get; set; }
        public object automations { get; set; }
        public Lists lists { get; set; }
    }

    public class Lists
    {
        public _1 _1 { get; set; }
    }

    public class _1
    {
        public int id { get; set; }
        public int list_id { get; set; }
        public string email { get; set; }
        public int status { get; set; }
        public DateTime subscribed_at { get; set; }
        public object unsubscribed_at { get; set; }
        public string c_fields { get; set; }
        public object groups { get; set; }
        public string source { get; set; }
        public object unsub_reason { get; set; }
        public object[] status_history { get; set; }
        public int sms_status { get; set; }
        public object doi_date { get; set; }
        public string subscribed_at_utc { get; set; }
        public object unsubscribed_at_utc { get; set; }
        public Owner owner { get; set; }
    }

    public class Owner
    {
        public int id { get; set; }
        public string name { get; set; }
        public Groups groups { get; set; }
        public int active_subscribers { get; set; }
    }

    public class Groups
    {
        public Grp_63739Cda09773 grp_63739cda09773 { get; set; }
    }

    public class Grp_63739Cda09773
    {
        public string name { get; set; }
        public Category[] category { get; set; }
        public string id { get; set; }
        public string type { get; set; }
    }

    public class Category
    {
        public string name { get; set; }
        public string id { get; set; }
    }
}
