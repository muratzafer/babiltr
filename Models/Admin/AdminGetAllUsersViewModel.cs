using babiltr.EntityLayer;

namespace babiltr.Models.Admin
{
    public class AdminGetAllUsersViewModel
    {
        public int UserID { get; set; }
        public string? PhotoUrl { get; set; } 
        public string FirstName { get; set; }  
        public string LastName { get; set; }  
        public string Email { get; set; } 
        public string? Occupation { get; set; } 
        public bool Translator { get; set; } 
        public DateTime RegisterDate { get; set; }
    }
}
