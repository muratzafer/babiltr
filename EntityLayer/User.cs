using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace babiltr.EntityLayer
{
    public class User : IdentityUser<int>
    {

        // Kişisel bilgiler
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? RememberMe { get; set; }
        public string? Iban { get; set; }
        public DateOnly DOB { get; set; }
        public string? Occupation { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public DateTime RegisterDate { get; set; }
        public string? PhotoUrl { get; set; }
        public bool? KVKKStatus { get; set; }

        // Şifre değiştirme bilgileri
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ReTypeNewPassword { get; set; }

        // İlişkili diğer varlıklar
        public List<Jobs> PostedJobs { get; set; } = new List<Jobs>();
        public List<Application> Applications { get; set; } = new List<Application>();
        public List<Skills>? Skills { get; set; } = new List<Skills>();
        public List<Education>? Educations { get; set; } = new List<Education>();

        // İş
        public string? Experience { get; set; }
        public string? CvOrResumeUrl { get; set; }
        [NotMapped]
        public IFormFile? CvOrResumeFile { get; set; }
        public string? Introduction { get; set; }
        public string? Salary { get; set; }
        public bool Translator { get; set; }

        // Sosyal Bağlantı
        public string? Linkedin { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? Twitter { get; set; }

    }
}
