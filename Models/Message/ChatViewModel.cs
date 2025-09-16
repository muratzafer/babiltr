using babiltr.EntityLayer;

namespace babiltr.Models.Message
{
    public class ChatViewModel
    {
        public int UserId { get; set; }
        public int ChatId { get; set; }
        public string PhotoUrl { get; set; }
        public string UserName { get; set; }
        public bool IsBlocked { get; set; }
        public List<MessageViewModel> Messages { get; set; }
        public User OtherUser { get; set; }
    }
}
