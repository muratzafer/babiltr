using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace babiltr.Models.Applications
{
    public class JobApplyUserViewModel
    {
        public int JobID { get; set; }
        public string? JobTitle { get; set; }

        [Required(ErrorMessage = "Lütfen adınızı giriniz.")]
        [StringLength(30, ErrorMessage = "Ad en fazla 30 karakter olabilir")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Lütfen soyadınızı giriniz.")]
        [StringLength(20, ErrorMessage = "Soyad en fazla 20 karakter olabilir")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Lütfen mesleki ünvanınızı giriniz.")]
        [StringLength(50, ErrorMessage = "İş Unvanı en fazla 50 karakter olabilir")]
        public string? Occupation { get; set; }

        [Required(ErrorMessage = "Lütfen e-posta adresinizi giriniz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Lütfen bir telefon numarası giriniz.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [StringLength(11, ErrorMessage = "Telefon numarası en fazla 11 karakter olabilir")]
        public string? PhoneNumber { get; set; }
        public string? CvOrResumeUrl { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? CvOrResumeFile { get; set; }

        [StringLength(1000, ErrorMessage = "Tanıtım metni en fazla 1000 karakter olabilir")]
        public string? Introduction { get; set; }
    }
}
