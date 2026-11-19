namespace UI.Code
{
    /// <summary>
    /// Uvítací okno „Co je nového“ po prvním přihlášení do nové verze. Obsah je jen zde.
    /// Okno se uživateli ukáže jednou; aby se ukázalo znovu (další vydání), zvyšte <see cref="Version"/> a upravte body.
    /// Obrázek bodu je nepovinný: soubor wwwroot/images/welcome/{Image} – když chybí, bod se zobrazí bez obrázku.
    /// Po změně textů doplňte jejich překlad do x97Translate.
    /// </summary>
    public static class WelcomeContent
    {
        /// <summary>Číslo obsahu. Uživatel, který má uloženo nižší číslo, uvidí okno znovu.</summary>
        public const int Version = 1;
        /// <summary>Klíč uživatelského parametru (tabulka x36UserParam) s číslem naposledy viděného obsahu.</summary>
        public const string UserParamKey = "welcome-seen";
        public const string Title = "Vítejte v MARKTIME 8";
        public const string Subtitle = "Krátce, co je ve verzi 8 nového.";
        /// <summary>Odkaz „Zjistit víc“.</summary>
        public const string MoreUrl = "https://marktime.cz/#v8";

        public class Item
        {
            public string Icon { get; set; }    //material ikona
            public string Title { get; set; }
            public string Text { get; set; }
            public string Image { get; set; }   //název souboru ve wwwroot/images/welcome
            /// <summary>Nepovinná podmínka: bod se ukáže jen uživateli, který funkci opravdu má (modul, oprávnění).</summary>
            public Func<BO.RunningUser, bool> Visible { get; set; }
        }

        public static readonly List<Item> Items = new List<Item>
        {
            new Item { Icon = "auto_awesome", Image = "ai.png", Title = "Zeptej se MARKTIME", Text = "Zeptejte se na svá data běžnou větou. Odpověď dostanete jako tabulku, kterou si uložíte na dashboard nebo otevřete v Pivot-Exploreru.", Visible = u => u.TestPermission(BO.PermValEnum.GR_AI) },
            new Item { Icon = "history_toggle_off", Image = "stopky.png", Title = "Předělané stopky", Text = "Stopky máte stále po ruce v horní liště, kde vidíte i právě běžící čas.", Visible = u => u.j04IsModule_p31 },
            new Item { Icon = "mail", Image = "outlook.png", Title = "Vykazování z Outlooku", Text = "Přímo z otevřeného e-mailu v Outlooku vykážete úkon, založíte úkol nebo e-mail uložíte do FILEBOXu." },
            new Item { Icon = "forum", Image = "chat.png", Title = "Interní chat", Text = "Pište si s kolegy přímo v MARKTIME: ve dvou, ve skupině nebo u konkrétního projektu, úkolu či faktury.", Visible = u => u.TestPermission(BO.PermValEnum.GR_CHAT) },
            new Item { Icon = "view_timeline", Image = "gantt.png", Title = "Gantt a kapacitní plán", Text = "Plánujte projekty a úkoly v čase a sledujte vytížení lidí." },
            new Item { Icon = "checklist", Image = "prehledy.png", Title = "Rychlejší práce s přehledy", Text = "Řádky označíte myší i s klávesami Ctrl a Shift a hned vidíte součet za označené řádky." },
            new Item { Icon = "verified_user", Image = "2fa.png", Title = "Bezpečnější přihlášení", Text = "Dvoufázové ověření ochrání váš účet, i kdyby se někdo dostal k heslu." }
        };

        /// <summary>Body, které se přihlášenému uživateli zobrazí.</summary>
        public static List<Item> GetItems(BL.Factory f)
        {
            return Items.Where(p => p.Visible == null || p.Visible(f.CurrentUser)).ToList();
        }

        /// <summary>Uživatel ještě neviděl aktuální obsah.</summary>
        public static bool IsUnseen(BL.Factory f)
        {
            return f.CBL.LoadUserParamInt(UserParamKey, 0) < Version;
        }
    }
}
