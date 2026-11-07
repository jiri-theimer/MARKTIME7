using UI.Views.Shared.Components.myGrid;

namespace UI.Models.p31view
{
    /// <summary>Okno se zdrojovými úkony buňky MARKTIME pivotu (p31totals/Drill).</summary>
    public class p31TotalsDrillViewModel : BaseViewModel
    {
        public myGridInput gridinput { get; set; }
        public string Label { get; set; }
        public int Count { get; set; }
    }
}
