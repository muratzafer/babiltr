namespace babiltr.Models.Message
{
    public class ChatUserViewModel
    {
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string UserPhoto { get; set; }
        public string LastMessageContent { get; set; }
        public DateTime? LastMessageDate { get; set; }
        public int LastMessageSenderId { get; set; }
    }
}
