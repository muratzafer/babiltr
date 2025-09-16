namespace babiltr.Models.Admin
{
    public class AdminAllJobsViewModel
    {
        public int JobId { get; set; }
        public string Title { get; set; }
        public int Price { get; set; }
        public string Status { get; set; }
        public int CompletionPercentage { get; set; }
        public int? UserID { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
