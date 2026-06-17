using System.ComponentModel.DataAnnotations;

namespace UI.Models.Guru
{
    /// <summary>
    /// ViewModel pro registraci přes Google účet.
    /// E-mail pochází z ověřeného Google tokenu (nastavuje controller,
    /// ne uživatel). Heslo je automaticky vygenerováno. Kód země = "CZ".
    /// </summary>
    public class CreateLicenseFreeGoogleViewModel : BaseViewModel
    {
        public string Message { get; set; }

        [Required(ErrorMessage = "Zadejte jméno")]
        [Display(Name = "Jméno")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Zadejte příjmení")]
        [Display(Name = "Příjmení")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Zadejte název firmy")]
        [Display(Name = "Název firmy")]
        public string p93Company { get; set; }

        public string Email { get; set; }

        public string CountryCode { get; set; } = "CZ";  // výchozí CZ, uživatel může vybrat SK
    }
}