namespace UI.Code
{
    public sealed class basTelerikReporting
    {
        public static string ShowAsHHMM(double? dblHours)
        {            
            if (dblHours==null)
            {
                return null;
            }
            return BO.Code.Time.GetTimeFromSeconds((double)dblHours * 60 * 60);
        }

        public static string GetDateFormat(DateTime d, string strFormat)
        {
            if (string.IsNullOrEmpty(strFormat) || strFormat.ToLower() == "mm/dd/yyyy")
                return d.ToString("mm/dd/yyyy");
            
            else
                return d.ToString(strFormat);
        }
    }
}
