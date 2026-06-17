

namespace BO
{
    public enum b06JobFlagEnum
    {
        _None=0,
        PdfFaktura=1
    }
    public enum b06AutoRunFlagEnum
    {
        UzivatelskyKrok=0,
        NeUzivatelskyKrok=1,        
        AutomatickySpustenySeStavem=2

    }
    public enum b06NomineeFlagENum
    {
        _None=0,
        RucniNepovinna = 1,
        RucniPovinna=2,
        Pevna=3
    }
    public enum b06NotepadFlagEnum
    {
        _Default=0,
        WithoutNotepad=1,
        NotepadIsRequired=2
    }
    public enum b06GeoFlagEnum
    {
        _None=0,
        LoadFromP15=1,
        LoadFromCurrentUser=2
    }
    public enum b06AutoTaskFlagEnum
    {
        _None=0,
        Manual=1,
        Auto=2
    }
    public class b06WorkflowStep : BaseBO
    {
        public int b02ID { get; set; }
        public int b02ID_Target { get; set; }
        public b06AutoRunFlagEnum b06AutoRunFlag { get; set; }
        public b06GeoFlagEnum b06GeoFlag { get; set; }
        public string b06Name { get; set; }
        public b06NotepadFlagEnum b06NotepadFlag { get; set; }
        public string b06RunSQL { get; set; }                   //spuštěním kroku spustit tento SQL
        public string b06ValidateBeforeRunSQL { get; set; }     //pokud SQL vrací 1, pak lze krok spustit
        public string b06ValidateAutoMoveSQL { get; set; }      //pokud SQL vrací 1, pak dojde automaticky ke spuštění kroku (poháněno nepřetržitě robotem)
        public string b06ValidateBeforeErrorMessage { get; set; }
        public int b06Ordinary { get; set; }
        public int b06NextRunAfterHours { get; set; }
        public string b06FrameworkSql { get; set; }
        public string b06CacheValueSql { get; set; }

        public b06NomineeFlagENum b06NomineeFlag { get; set; }
        public int x67ID_Nominee { get; set; }  //ruční nominace role x67ID_Nominee
        public int x67ID_Direct { get; set; }   //automatická změna obsazení role:x67ID_Direct
        public int j11ID_Direct { get; set; }   //nové obsazení role x67ID_Direct
        public int x67ID_Direct_Source { get; set; }    //nové obsazení role x67ID_Direct nositelem role manažera projektu apod.
        public int b02ID_LastReceiver_ReturnTo { get; set; }

        public string b01Name { get; }
        public int b01ID { get; }
        public string b01Entity { get; }
        public string b02Name { get; }
        public string b02Color { get; }
        
        public int p60ID { get; set; }  //šablona úkolu
        public b06AutoTaskFlagEnum b06AutoTaskFlag { get; set; }
        public int p83ID { get; set; }  //typ upomínky
        public string NameWithTargetStatus { get; }

        public b06JobFlagEnum b06JobFlag { get; set; }
       
        public bool b06IsNotifyMerge { get; set; }
        public string b06NotifyMergeTime { get; set; }
    }
}
