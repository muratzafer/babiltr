using babiltr.EntityLayer;
using System.ComponentModel.DataAnnotations;

namespace babiltr.Models.USER
{
    public class GetUserViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Lütfen adınızı giriniz.")]
        [StringLength(30, ErrorMessage = "Ad en fazla 30 karakter olabilir")]
        public string FirstName { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Lütfen soyadınızı giriniz.")]
        [StringLength(20, ErrorMessage = "Soyad en fazla 20 karakter olabilir")]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateOnly DOB { get; set; }

        [RequiredIf("Translator", true, ErrorMessage = "Ünvan bilgisi zorunludur.")]
        [StringLength(50, ErrorMessage = "İş Unvanı en fazla 50 karakter olabilir")]
        public string? Occupation { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Lütfen e-posta adresinizi giriniz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        public bool Translator { get; set; }

        [RequiredIf("Translator", true, ErrorMessage = "İban bilgisi zorunludur.")]
        [StringLength(26, ErrorMessage = "IBAN numarası sayıdan oluşmalıdır.")]
        public string? Iban { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Lütfen bir telefon numarası giriniz.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [StringLength(11, ErrorMessage = "Telefon numarası en fazla 11 karakter olabilir")]
        public string? PhoneNumber { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Lütfen şehir bilginizi giriniz.")]
        public string? City { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Lütfen ülke bilginizi giriniz.")]
        public string? Country { get; set; }

        public string? PhotoUrl { get; set; }
        public string? CvOrResumeUrl { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? CvOrResumeFile { get; set; }

        [StringLength(1000, ErrorMessage = "Tanıtım metni en fazla 1000 karakter olabilir")]
        public string? Introduction { get; set; }

        [RequiredIf("Translator", true, ErrorMessage = "Ücret bilgisi zorunludur.")]
        public string? Salary { get; set; }

        public List<Skills>? Skills { get; set; } = new List<Skills>();
        public List<Education>? Educations { get; set; } = new List<Education>();

        [RequiredIf("Translator", true, ErrorMessage = "İş tecrübesi zorunludur.")]
        public string? Experience { get; set; }

        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        public string? Linkedin { get; set; }

        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        public string? Facebook { get; set; }

        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        public string? Instagram { get; set; }

        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        public string? Twitter { get; set; }
    }

    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _conditionPropertyName;
        private readonly object _desiredValue;

        public RequiredIfAttribute(string conditionPropertyName, object desiredValue)
        {
            _conditionPropertyName = conditionPropertyName;
            _desiredValue = desiredValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var conditionProperty = validationContext.ObjectType.GetProperty(_conditionPropertyName);
            if (conditionProperty == null)
                throw new ArgumentException($"Property {_conditionPropertyName} not found.");

            var conditionValue = conditionProperty.GetValue(validationContext.ObjectInstance);

            if (conditionValue?.Equals(_desiredValue) == true && value == null)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
