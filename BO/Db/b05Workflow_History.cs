
namespace BO
{
    public class b05Workflow_History:BaseBO
    {        
        
        public int b06ID { get; set; }
        public string b05RecordEntity { get; set; }
        public int b05RecordPid { get; set; }
        public int j02ID_Sys { get; set; }
        public int j11ID_Nominee { get; set; }
        public int j02ID_Nominee { get; set; }
        public int x67ID_Nominee { get; set; }
        public bool b05IsNominee { get; set; }
        public DateTime? b05Date { get; set; }
        public string b05Name { get; set; }
        public string b05NotepadText200 { get; set; }
        public int b02ID_From { get; set; }
        public int b02ID_To { get; set; }
        public bool b05IsManualStep { get; set; }
        public bool b05IsCommentOnly { get; set; }
        public string b05Notepad { get; set; }
        public int x04ID { get; set; }
        public string b05SQL { get; set; }
        public string b05ErrorMessage { get; set; }
        public int p56ID { get; set; }
        public int o22ID { get; set; }
        public int p58ID { get; set; }
        public int b05PortalFlag { get; set; }
        public int b05Tab1Flag { get; set; }    //1: zobrazovat ihned v tab1, 2: fakturační poznámka
        

        //readonly
        public string b06Name { get; }
       
        public string b02Name_From { get; }
        public string b02Name_To { get; }
       public string b02Color { get; }


        public string Person { get; }
        public string p56Name { get; }
        public string o22Name { get; }
        public string o21Name { get; }
        public string StatusMoveHtml
        {
            get
            {
                if (this.b02ID_From == this.b02ID_To)
                {
                    return null;
                }
                else
                {
                    return string.Format("{0} ➝ {1}", this.b02Name_From, this.b02Name_To);
                }
            }
        }
        public string StatusMoveHtmlRed
        {
            get
            {
                if (this.b02ID_From == this.b02ID_To)
                {
                    return null;
                }
                else
                {
                    return string.Format("<span>{0} ➝ </span><span style='color:red;'>{1}</span>", this.b02Name_From,this.b02Name_To);
                }
            }
        }
        public int j95ID { get; set; }
        public double j95Longitude { get; }
        public double j95Latitude { get; }
        public double j95Temp { get; }
        public double j95TempFeelsLike { get; }
        public int j95Humidity { get; }
        public double j95WindSpeed { get; }
        public string j95Location { get; }
        public string j95Description { get; }
        public string j95Icon { get; }
        public string WeatherIconUrl
        {
            get
            {
                return $"http://openweathermap.org/img/wn/{this.j95Icon}.png";
            }
        }

        public string getMapUrlClassic()
        {
            if (this.j95Longitude == 0 || this.j95Latitude == 0) return null;
            string x= this.j95Longitude.ToString().Replace(",", ".");
            string y = this.j95Latitude.ToString().Replace(",", ".");
            return $"https://mapy.cz/zakladni?source=coor&id={x}%2C{y}&ds=1&x={x}&y={y}&z=17";
        }


    }
}
