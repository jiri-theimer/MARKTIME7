

namespace BO.Integrace.Ecomail.CreateSubscriber
{
    public class RootObject
    {
        public Subscriber_Data subscriber_data { get; set; }
        public Groups groups { get; set; }
        public bool trigger_autoresponders { get; set; }
        public bool update_existing { get; set; }
        public bool resubscribe { get; set; }
    }

    public class Subscriber_Data
    {
        public string name { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string vokativ { get; set; }
        public string vokativ_s { get; set; }
        public string gender { get; set; }
        public string company { get; set; }
        public string city { get; set; }
        public string street { get; set; }
        public string zip { get; set; }
        public string country { get; set; }
        public string phone { get; set; }
        public string pretitle { get; set; }
        public string surtitle { get; set; }
        public string birthday { get; set; }
        public string nameday { get; set; }
        public string source { get; set; }
        public Custom_Fields custom_fields { get; set; }
        public string[] tags { get; set; }
    }

    public class Custom_Fields
    {
        public string SHOP { get; set; }
        public NAME NAME { get; set; }
        public BIRTHDAY BIRTHDAY { get; set; }
        public FEED FEED { get; set; }
        public NUMBER NUMBER { get; set; }
        public WEBSITE WEBSITE { get; set; }
    }

    public class NAME
    {
        public string value { get; set; }
        public string type { get; set; }
    }

    public class BIRTHDAY
    {
        public string value { get; set; }
        public string type { get; set; }
    }

    public class FEED
    {
        public string value { get; set; }
        public string type { get; set; }
    }

    public class NUMBER
    {
        public int value { get; set; }
        public string type { get; set; }
    }

    public class WEBSITE
    {
        public string value { get; set; }
        public string type { get; set; }
    }

    public class Groups
    {
        public string[] grp_5a145ee75780f { get; set; }
    }
}
