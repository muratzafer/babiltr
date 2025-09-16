using System.ComponentModel.DataAnnotations;

namespace babiltr.Models.Jobs
{
    public class CreateJobViewModel
    {
        [Required(ErrorMessage = "Lütfen bir başlık giriniz")]
        [StringLength(30, ErrorMessage = "Başlık en fazla 30 karakter olabilir")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Lütfen bir açıklama giriniz")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Lütfen bir İş türü seçiniz")]
        public string JobType { get; set; }

        [Required(ErrorMessage = "Lütfen bir İş türü seçiniz")]
        public string JobCategory { get; set; }

        [Required(ErrorMessage = "Lütfen bir ücret bilgisi giriniz")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Geçerli bir maaş giriniz")]
        public int Price { get; set; }

        [Required(ErrorMessage = "Lütfen en az bir beceri giriniz")]
        [StringLength(100, ErrorMessage = "Beceriler en fazla 100 karakter olabilir")]
        public string Skills { get; set; }

        [Required(ErrorMessage = "Lütfen deneyim bilgisini giriniz")]
        public string Experience { get; set; }

        [Required(ErrorMessage = "Lütfen Orjinal dil bilgisini giriniz")]
        public string OriginalLanguage { get; set; }

        [Required(ErrorMessage = "Lütfen çevrilecek dil bilgisini giriniz")]
        public string TargetLanguage { get; set; }

        [Required(ErrorMessage = "Lütfen son teslim tarihini giriniz")]
        public DateTime? DeliveryDate { get; set; }
        public string? Status { get; set; }

        [Required(ErrorMessage = "Lütfen bir ülke seçiniz")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Lütfen şehir veya eyalet bilgisini giriniz")]
        public string City { get; set; }

        public int? UserID { get; set; }
    }

}
