using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email je povinný")]
        [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", ErrorMessage = "Neplatný formát emailu")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Heslo je povinné")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
