namespace babiltr.EntityLayer
{
    public class Jobs
    {
        public int JobID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string JobType { get; set; }
        public string JobCategory { get; set; }
        public int Price { get; set; }
        // YORUM SATIRI ALTINDA: EKLENEN KOD BUDUR
        public decimal CustomerSurchargePct { get; set; }  // örn. 5.00
        public decimal PlatformCommissionPct { get; set; } // örn. 5.00
        public string OriginalLanguage { get; set; }
        public string Experience { get; set; }
        public string Skills { get; set; }
        public string TargetLanguage { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public bool JobCompleted { get; set; }
        public bool PaymentCompleted { get; set; }
        public int RevisionCount { get; set; }
        public string? Status { get; set; } //Publish, Active, Payment,PaymentApproved, Revision, Completed
        public int CompletionPercentage { get; set; }
        public string? PdfFileName { get; set; }
        public string? TranslatedFileName { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? TimerStartTime { get; set; }
        public string? MmerchantOid { get; set; }
        public int? UserID { get; set; }
        public User? User { get; set; }
        public virtual ICollection<Translation> Translations { get; set; }
        public virtual ICollection<Application> Applications { get; set; }
    }
}
