using System.ComponentModel.DataAnnotations;

namespace babiltr.Models.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
