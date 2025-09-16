namespace EntityLayer.Concrete
{
    public class Payment
    {
        public babiltr.EntityLayer.Jobs Job { get; set; }
        public int Id { get; set; }
        public int JobId { get; set; }
        public string MerchantOid { get; set; } = default!;
        public decimal BaseAmount { get; set; }          // B
        public decimal CustomerSurchargePct { get; set; } // f
        public decimal PlatformCommissionPct { get; set; } // c
        public decimal ChargedAmount { get; set; }       // B*(1+f)
        public decimal TranslatorPayout { get; set; }    // B*(1-c)
        public decimal PlatformProfit { get; set; }      // B*(f+c)
        public string Status { get; set; } = "PENDING";  // PENDING/OK/FAIL
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}