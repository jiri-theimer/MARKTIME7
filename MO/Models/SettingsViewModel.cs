namespace MO.Models
{
    public class SettingsViewModel : BaseViewModel
    {
        /// <summary>Formát zobrazení hodin: "N" = dekadické číslo, "T" = HH:MM</summary>
        public string HoursFormat { get; set; } = "N";
    }
}
