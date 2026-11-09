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

        /// <summary>Položka nabídky stavů: krátký název (na tlačítko a do menu), podtitul (šedě pod názvem).</summary>
        public record StateItem(int Value, string Name, string Sub, string ButtonLabel);
        public record StateGroup(string Title, StateItem[] Items);

        /// <summary>
        /// Nabídka stavů úkonů seskupená podle průběhu (rozpracované → schválené → vyúčtované). Kódy stavů beze změny (uložené parametry platí).
        /// Stav 18 (Vyúčtované s nulovou cenou) záměrně v nabídce není.
        /// </summary>
        public static readonly StateGroup[] Groups = new[]
        {
            new StateGroup(null, new[] {
                new StateItem(3, "Nevyúčtované", "rozpracované nebo schválené", "Nevyúčtované") }),
            new StateGroup("Před schválením", new[] {
                new StateItem(1, "Rozpracované", null, "Rozpracované"),
                new StateItem(2, "Rozpracované s korekcí", null, "Rozpracované s korekcí"),
                new StateItem(16, "Rozpracované – Fa aktivita", null, "Rozpracované · Fa aktivita"),
                new StateItem(17, "Rozpracované – NeFa aktivita", null, "Rozpracované · NeFa aktivita"),
                new StateItem(9, "Neschválené", "čeká na pře-schválení", "Neschválené") }),
            new StateGroup("Schválené – čeká na vyúčtování", new[] {
                new StateItem(4, "Všechny schválené", null, "Schválené"),
                new StateItem(5, "Fakturovat", null, "Schválené · Fakturovat"),
                new StateItem(6, "Zahrnout do paušálu", null, "Schválené · Paušál"),
                new StateItem(7, "Skrytý / viditelný odpis", null, "Schválené · Odpis"),
                new StateItem(8, "Fakturovat později", "čeká na pře-schválení", "Schválené · Fakturovat později") }),
            new StateGroup("Vyúčtované", new[] {
                new StateItem(10, "Všechny vyúčtované", null, "Vyúčtované"),
                new StateItem(11, "DRAFT vyúčtování", null, "DRAFT vyúčtování"),
                new StateItem(12, "Fakturovat", null, "Vyúčtované · Fakturovat"),
                new StateItem(13, "Zahrnout do paušálu", "nulová cena", "Vyúčtované · Paušál"),
                new StateItem(14, "Skrytý / viditelný odpis", "nulová cena", "Vyúčtované · Odpis"),
                new StateItem(21, "100% uhrazené vyúčtování", null, "Vyúčtované · Uhrazené") }),
            new StateGroup("Vyloučení z vyúčtování", new[] {
                new StateItem(15, "Vyloučené z vyúčtování", null, "Vyloučené z vyúčtování"),
                new StateItem(19, "Bez vyloučených z vyúčtování", null, "Bez vyloučených") })
        };

        /// <summary>Krátký text na tlačítko (celý název je v bublině – getStateAlias).</summary>
        public string getButtonLabel()
        {
            foreach (var g in Groups)
            {
                foreach (var i in g.Items)
                {
                    if (i.Value == Value) return i.ButtonLabel;
                }
            }
            return Value > 0 ? getStateAlias() : "Stav úkonů";      //např. 18 – mimo nabídku, ale může být uložený
        }
    }
}
