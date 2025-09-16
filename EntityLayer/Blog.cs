using System.ComponentModel.DataAnnotations;

namespace babiltr.EntityLayer
{
    public class Blog
    {
        public int Id { get; set; }
        public string? Author { get; set; }

        [Required(ErrorMessage = "Başlık boş olamaz.")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "İçerik boş olamaz.")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        [NotZero(ErrorMessage = "Okuma süresi 0 olamaz.")]
        public int ReadTime { get; set; }

        public string? ImageName { get; set; }

        [Required(ErrorMessage = "En az 1 tane etiket giriniz")]
        public string Tags { get; set; }

        public ICollection<BlogContent>? BlogContents { get; set; }
    }
    public class NotZeroAttribute : ValidationAttribute
    {
        public NotZeroAttribute() : base("Okuma süresi 0 olamaz.") { }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is int readTime && readTime == 0)
            {
                return new ValidationResult(ErrorMessage ?? "Okuma süresi 0 olamaz.");
            }
            return ValidationResult.Success;
        }
    }
}
