namespace UI.Models
{
    public class p31StateQueryViewModel
    {
        public int Value { get; set; }
        public string UserParamKey { get; set; }
        public string javascript_onchange { get; set; } = "handle_p31statequery_change";
        public string getStateAlias()
        {
            switch (Value)
            {
                case 1:
                    return "Rozpracované (Čeká na schvalování)";
                case 2:
                    return "Rozpracované s korekcí (Čeká na schvalování)";
                case 16:
                    return "Rozpracované [Fa aktivita] (Čeká na schvalování)";
                case 17:
                    return "Rozpracované [NeFa aktivita] (Čeká na schvalování)";
                case 18:
                    return "Vyúčtované s nulovou cenou";
                case 21:
                    return "Úkony v 100% uhrazeném vyúčtování";
                case 3:
                    return "Nevyúčtované (Rozpracované nebo Schválené)";
                case 4:
                    return "Schválené (Čeká na vyúčtování)";
                case 5:
                    return "Schválené jako [Fakturovat] (Čeká na vyúčtování)";
                case 6:
                    return "Schválené jako [Zahrnout do paušálu] (Čeká na vyúčtování)";
                case 7:
                    return "Schválené jako [Skrytý/Viditelný odpis] (Čeká na vyúčtování)";
                case 8:
                    return "Schválené jako [Fakturovat později] (Čeká na pře-schválení)";
                case 9:
                    return "Neschválené (Čeká na pře-schválení)";
                case 10:
                    return "Vyúčtované";
                case 11:
                    return "DRAFT vyúčtování";
                case 12:
                    return "Vyúčtované jako [Fakturovat]";
                case 13:
                    return "Vyúčtované s nulovou cenou jako [Zahrnout do paušálu]";
                case 14:
                    return "Vyúčtované s nulovou cenou jako [Skrytý/Viditelný odpis]";

                case 15:
                    return "Úkony vyloučené z vyúčtování";
                case 19:
                    return "Úkony bez vyloučených z vyúčtování";

                default:
                    return "Stav úkonů";

            }
        }
    }
}
