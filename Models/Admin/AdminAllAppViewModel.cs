namespace babiltr.Models.Admin
{
    public class AdminAllAppViewModel
    {
        public int AppId { get; set; }
        public int JobId { get; set; }
        public string? EmployerPhotoUrl { get; set; }
        public string? TranslatorPhotoUrl { get; set; }
        public string EmployerName { get; set; }
        public string EmployerLastName { get; set; }
        public string EmployerEmail { get; set; }  
        public string TranslatorName { get; set; }
        public string TranslatorLastName { get; set; }
        public string TranslatorEmail { get; set; } 
        public string Title { get; set; }
        public string ApplicationStatus { get; set; } 
        public DateTime ApplicationDate { get; set; }
    }
}
