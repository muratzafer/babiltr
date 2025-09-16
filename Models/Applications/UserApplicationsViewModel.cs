namespace babiltr.Models.Applications
{
    public class UserApplicationsViewModel
    {
        public int AppId { get; set; }
        public int JobId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string JobType { get; set; }
        public int Price { get; set; }
        public string Country { get; set; }
        public int? UserID { get; set; }
        public int CompletionPercentage { get; set; }
        public bool JobCompleted { get; set; }
        public string? JobStatus { get; set; }
        public DateTime ApplicationDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string ApplicationStatus { get; set; }
    }
}
