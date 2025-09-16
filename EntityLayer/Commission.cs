namespace babiltr.EntityLayer
{
    public class Commission
    {
        public int Id { get; set; }
        public int TranslatorPercentage { get; set; }
        public int EmployerPercentage { get; set; }
        public DateTime LastUpdated { get; set; }
        public Commission()
        {
            TimeZoneInfo turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            LastUpdated = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
        }
    }
}
