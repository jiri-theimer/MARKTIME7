

namespace UI.Models.p31view
{
    public class daylineViewModel : BaseViewModel
    {
        public int ScaleIndex { get; set; }  //0:měsíc, 1:týden, 2:2 týdny
        public bool ShowHHMM { get; set; }
        public DateTime d0 { get; set; }
        public DateTime d1 { get; set; }
        public DateTime d2 { get; set; }

        public TheGridQueryViewModel TheGridQueryButton { get; set; }
        public p31StateQueryViewModel p31statequery { get; set; }   //filtrování podle stavu aktivit v horním pruhu
        public bool ShowP56 { get; set; }
        public bool ShowO22 { get; set; }
        public int x67ID_o22 { get; set; }
        public int x67ID_p56 { get; set; }
        
        public IEnumerable<BO.p61ActivityCluster> lisP61 { get; set; }
        public IEnumerable<BO.x67EntityRole> lisX67_o22 { get; set; }
        public IEnumerable<BO.x67EntityRole> lisX67_p56 { get; set; }
        public daylineGroupBy GroupBy { get; set; }
        public IEnumerable<BO.j02User> lisJ02 { get; set; }

        public IEnumerable<BO.p12ApproveUserDay> lisP12 { get; set; }
        public IEnumerable<BO.p31Worksheet> lisP31 { get; set; }
        public IEnumerable<BO.c26Holiday> lisC26 { get; set; }
        public List<BO.p31WorksheetTimelineDay> lisSums { get; set; }
        public IEnumerable<BO.c24DayColor> lisC24 { get; set; }
        public IEnumerable<BO.c23PersonalDayColor> lisC23 { get; set; }
        public IEnumerable<BO.p32Activity> lisP32FUT { get; set; }
        public IEnumerable<BO.p56TaskDayline> lisP56 { get; set; }
        //public IEnumerable<BO.o22MilestoneDayline> lisO22 { get; set; }
        public IEnumerable<BO.o22MilestoneDayline> lisO22_Udalosti { get; set; }
        public IEnumerable<BO.o22MilestoneDayline> lisO22_MimoUdalosti { get; set; }
        public string j07IDs { get; set; }
        public List<int> pids_j07 { get; set; }
        public string SelectedPositions { get; set; }
        public string j02IDs { get; set; }
        public string SelectedPersons { get; set; }
        public string j11IDs { get; set; }
        public string SelectedTeams { get; set; }

        public bool IsJustXlsExporting { get; set; }

    }

    public enum daylineGroupBy
    {
        None=1,
        NoneRecs=2,
        p41=3,
        p41Recs=4,
        p28=5,
        p28Recs=6
    }
}
