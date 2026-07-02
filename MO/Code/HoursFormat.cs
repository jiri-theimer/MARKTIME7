namespace MO.Code
{
    /// <summary>
    /// Zobrazení hodin respektující uživatelské nastavení Factory.CurrentUser.j02DefaultHoursFormat:
    /// "N" (výchozí) = dekadické číslo (např. "1,5 h"), "T" = formát HH:MM (např. "01:30").
    /// Převod na HH:MM zajišťuje BO.Code.Time.ShowAssHHMM - MO samo žádnou vlastní logiku převodu nemá.
    /// </summary>
    public static class HoursFormat
    {
        public static bool IsHHMM(BL.Factory f)
        {
            return f?.CurrentUser?.j02DefaultHoursFormat == "T";
        }

        /// <summary>Naformátuje hodiny včetně jednotky - "1,5 h" nebo "01:30" (u HH:MM formátu jednotka nedává smysl).</summary>
        public static string Show(double hours, BL.Factory f)
        {
            if (IsHHMM(f))
            {
                return BO.Code.Time.ShowAssHHMM(hours);
            }
            return hours.ToString("0.##") + " h";
        }

        /// <summary>Naformátuje hodiny bez jednotky - pro předvyplnění editovatelného pole "Hodiny".</summary>
        public static string ShowForInput(double hours, BL.Factory f)
        {
            if (IsHHMM(f))
            {
                return BO.Code.Time.ShowAssHHMM(hours);
            }
            return hours.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
