
namespace UI.Models
{
    public class NavTab
    {
        public string Name { get; set; }
        public string Entity { get; set; }
        public string Url { get; set; }

        public string CssClass { get; set; } = "nav-link text-dark";


        public string Badge { get; set; }

        public string ClientID { get; set; }

        public string Tooltip { get; set; }
    }
}
