

using System;

namespace EntityLayer.Concrete
{
    public class Payout
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }               // İlişki: hangi ödemeden doğdu
        public string SubMerchantId { get; set; } = default!; // PayTR Alt Üye İşyeri (çevirmen)
        public decimal Amount { get; set; }              // Çevirene aktarılacak net tutar
        public string Status { get; set; } = "PENDING";  // PENDING/SENT/OK/FAIL
        public string? ProviderRef { get; set; }         // PayTR transfer referansı/numarası
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Payment Payment { get; set; }
    }
}