namespace babiltr.Models.Message
{
    public class MessageViewModel
    {
        public int UserId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; }
        public DateTime? SentAt { get; set; }
        public string PhotoUrl { get; set; }
        public string UserName { get; set; }
        public string TimeElapsed { get; set; }
    }
}
