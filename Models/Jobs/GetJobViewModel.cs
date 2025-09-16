namespace babiltr.Models.Jobs
{
    public class GetJobViewModel
    {
        public int? JobId { get; set; }
        public int? ChatId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? JobType { get; set; }
        public string JobCategory { get; set; }
        public int? Price { get; set; }
        public string? Skills { get; set; }
        public string? Experience { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public int? UserID { get; set; }
        public string? Status { get; set; }
        public int? HiredUserID { get; set; }
        public int? CompletionPercentage { get; set; }
        public bool JobCompleted { get; set; }
        public bool PaymentCompleted { get; set; }
        public int RevisionCount { get; set; }
        public string? TranslatedFileName { get; set; }
        public bool IsApplied { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public bool Translator { get; set; }
    }
}
