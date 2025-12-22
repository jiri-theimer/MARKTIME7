namespace UI.Models
{
    public class PellEditorViewModel
    {
        public string HtmlValue { get; set; }
        public bool IsBillingMemo { get; set; } = true;

        public string GetText200()
        {
            if (string.IsNullOrEmpty(this.HtmlValue))
            {
                return null;
            }
            string str200 = BO.Code.Bas.Html2Text(this.HtmlValue);
            if (str200 != null && str200.Length > 200)
            {
                str200 = $"{str200.Substring(0, 197)}...";
            }

            return str200;
        }
    }
}
