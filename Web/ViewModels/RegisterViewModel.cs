using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email je povinný")]
        [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", ErrorMessage = "Neplatný formát emailu")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Heslo je povinné")]
        [MinLength(6, ErrorMessage = "Heslo musí mít alespoň 6 znaků")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Potvrzení hesla je povinné")]
        [Compare("Password", ErrorMessage = "Hesla se neshodují")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Jméno je povinné")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Příjmení je povinné")]
        public string LastName { get; set; }
        [RegularExpression(@"^(\+420|0)?\s?[0-9]{3}\s?[0-9]{3}\s?[0-9]{3}$", ErrorMessage = "Neplatný formát telefonu (např. +420 123 456 789)")]
        public string Phone { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Musíte souhlasit s podmínkami")]
        public bool AgreeToTerms { get; set; }
    }

}
