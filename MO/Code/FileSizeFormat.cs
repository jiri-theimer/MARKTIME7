namespace MO.Code
{
    /// <summary>Formátování velikosti souboru pro zobrazení (B / kB / MB).</summary>
    public static class FileSizeFormat
    {
        public static string Show(double bytes)
        {
            if (bytes < 1024) return $"{bytes:0} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024:0.#} kB";
            return $"{bytes / (1024 * 1024):0.#} MB";
        }
    }
}
