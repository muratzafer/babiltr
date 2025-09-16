using babiltr.EntityLayer;

namespace babiltr.Models.USER
{
    public class GetAllTranslatorViewModel
    {
        public int UserID { get; set; }
        public int JobID { get; set; }
        public int ChatId { get; set; }
        public string? PhotoUrl { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Occupation { get; set; }
        public string? Salary { get; set; }
        public string? Experience { get; set; }
        public List<Skills>? Skills { get; set; } = new List<Skills>();
    }
}


