namespace babiltr.EntityLayer
{
    public class Chat
    {
        public int Id { get; set; }
        public int User1Id { get; set; }
        public int User2Id { get; set; }
        public int DeleteId { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User1 { get; set; }
        public User User2 { get; set; }
        public Chat()
        {
            TimeZoneInfo turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
        }
    }
}
