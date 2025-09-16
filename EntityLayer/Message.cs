namespace babiltr.EntityLayer
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string? Content { get; set; }
        public DateTime? SentAt { get; set; }
        public int? ChatId { get; set; }
        public Chat Chat { get; set; }
        public Message()
        {
            TimeZoneInfo turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            SentAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
        }
    }
}
