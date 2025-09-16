using System.ComponentModel.DataAnnotations;

public class ResetPasswordViewModel
{
    public string? Email { get; set; }
    public string? Token { get; set; }

    [Required(ErrorMessage = "Yeni parola gereklidir.")]
    [DataType(DataType.Password)]
    [StringLength(20, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter olmalıdır.", MinimumLength = 6)]
    public string? NewPassword { get; set; }

    [Required(ErrorMessage = "Parola doğrulaması gereklidir.")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Parolalar eşleşmiyor.")]
    public string? ConfirmPassword { get; set; }
}
