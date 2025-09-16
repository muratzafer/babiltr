

namespace BusinessLayer.Services
{
    public static class PaymentCalculationService
    {
        // B: baseAmount, f: müşteri ek ücreti (%), c: platform komisyonu (%)
        public static (decimal charged, decimal translator, decimal platform)
            Compute(decimal baseAmount, decimal surchargePct, decimal commissionPct)
        {
            var f = surchargePct / 100m;
            var c = commissionPct / 100m;

            var charged    = decimal.Round(baseAmount * (1 + f), 2);  // müşteriden çekilecek tutar
            var translator = decimal.Round(baseAmount * (1 - c), 2);  // çevirene gidecek net
            var platform   = decimal.Round(baseAmount * (f + c), 2);  // platform kazancı

            return (charged, translator, platform);
        }
    }
}