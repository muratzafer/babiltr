namespace babiltr.Models.Payment
{
    public class PaymentSettings
    {
        public string MerchantId { get; set; }
        public string MerchantKey { get; set; }
        public string MerchantSalt { get; set; }
        public double CustomerSurchargeRate { get; set; }
        public double PlatformCommissionRate { get; set; }
        public bool AutoPayout { get; set; }
    }
}
