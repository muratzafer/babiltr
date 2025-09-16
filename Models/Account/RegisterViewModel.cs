using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad bilgisi gereklidir")]
    [StringLength(40, ErrorMessage = "Ad en fazla 40 karakter olabilir")]
    public string Firstname { get; set; }

    [Required(ErrorMessage = "SoyAd bilgisi gereklidir")]
    [StringLength(15, ErrorMessage = "SoyAd en fazla 15 karakter olabilir")]
    public string lastname { get; set; }

    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Şifre gereklidir")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Şifreyi tekrar giriniz")]
    [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor")]
    public string ConfirmPassword { get; set; }

    [RequiredIfTrue(ErrorMessage = "KVKK onayı gereklidir")]
    public bool KvkkStatus { get; set; }
}
public class RequiredIfTrueAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var boolValue = (bool)value;

        if (!boolValue)
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}