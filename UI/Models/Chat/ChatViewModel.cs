namespace UI.Models
{
    public class ChatViewModel : BaseViewModel
    {
        public int InitialU01ID { get; set; }   //konverzace, která se má po načtení otevřít
        public bool IsAvailable { get; set; }   //v databázi licence jsou tabulky chatu
    }
}
