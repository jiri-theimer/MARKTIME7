namespace UI.Models
{
    public class ChatViewModel : BaseViewModel
    {
        public int InitialU01ID { get; set; }   //konverzace, která se má po načtení otevřít
        public bool IsAvailable { get; set; }   //uživatel má oprávnění k chatu a v databázi licence jsou tabulky chatu
        public bool IsDenied { get; set; }      //aplikační role uživatele nemá oprávnění GR_CHAT
    }
}
