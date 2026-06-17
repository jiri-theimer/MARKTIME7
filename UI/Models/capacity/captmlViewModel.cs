namespace UI.Models.capacity
{
    public class captmlViewModel:BaseViewModel
    {
        public string prefix { get; set; }
        public CapacityTimelineJ02ViewModel timeline_j02 { get; set; }
        public CapacityTimelineP41ViewModel timeline_p41 { get; set; }

        public string j07IDs { get; set; }        
        public string SelectedPositions { get; set; }
        public string j02IDs { get; set; }
        public string SelectedPersons { get; set; }
        public string j11IDs { get; set; }
        public string SelectedTeams { get; set; }
    }
}
