using babiltr.EntityLayer;

namespace babiltr.Models.USER
{
    public class EditUserViewModel
    {
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly DOB { get; set; }
        public string? Occupation { get; set; }
        public string? Email { get; set; }
        public bool? Translator { get; set; }
        public string PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? CvOrResumeUrl { get; set; }
        public string? Introduction { get; set; }
        public string? Salary { get; set; }
        public List<Skills>? Skills { get; set; } = new List<Skills>();
        public List<Education>? Educations { get; set; } = new List<Education>();
        public string? Experience { get; set; }
        public string? Linkedin { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? Twitter { get; set; }
    }
}
