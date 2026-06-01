namespace BO.ISDOC
{
    public class ReplaceXmlItem
    {
        public string FindString { get; set; }
        public string ReplaceWith { get; set; }

        public ReplaceXmlItem(string strFind, string strReplace = null)
        {
            this.FindString = strFind;
            this.ReplaceWith = strReplace;
        }
    }
}
