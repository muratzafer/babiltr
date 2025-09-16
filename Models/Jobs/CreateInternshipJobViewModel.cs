using System.ComponentModel.DataAnnotations;

namespace babiltr.Models.Jobs
{
    public class CreateInternshipJobViewModel
    {
        [Required(ErrorMessage = "Lütfen bir başlık giriniz")]
        [StringLength(30, ErrorMessage = "Başlık en fazla 30 karakter olabilir")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Lütfen bir açıklama giriniz")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Lütfen bir İş türü seçiniz")]
        public string JobType { get; set; }

        public string? JobCategory { get; set; }

        [Required(ErrorMessage = "Lütfen bir ücret bilgisi giriniz")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Geçerli bir maaş giriniz")]
        public int Price { get; set; }

        [Required(ErrorMessage = "Lütfen Orjinal dil bilgisini giriniz")]
        public string OriginalLanguage { get; set; }

        [Required(ErrorMessage = "Lütfen çevrilecek dil bilgisini giriniz")]
        public string TargetLanguage { get; set; }

        public string? Status { get; set; }
        public int? UserID { get; set; }
    }
}
