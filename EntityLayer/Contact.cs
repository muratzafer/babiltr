namespace babiltr.EntityLayer
{
    public class Contact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? Subject { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now.ToLocalTime();
        public Contact()
        {
            TimeZoneInfo turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
        }
    }
}
