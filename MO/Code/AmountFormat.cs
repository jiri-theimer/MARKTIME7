namespace MO.Code
{
    /// <summary>Formátování peněžních částek s oddělovačem tisíců (dle aktuální kultury požadavku).</summary>
    public static class AmountFormat
    {
        public static string Thousands(double value)
        {
            return value.ToString("#,##0.##");
        }

        /// <summary>Naformátuje částku uloženou jako invariantní string (např. "1234.5") s oddělovačem tisíců.</summary>
        public static string Thousands(string invariantValue)
        {
            if (string.IsNullOrWhiteSpace(invariantValue)) return invariantValue;
            if (double.TryParse(invariantValue, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var num))
            {
                return Thousands(num);
            }
            return invariantValue;
        }
    }
}
