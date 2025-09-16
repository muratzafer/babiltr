namespace babiltr.EntityLayer
{
    public class Notification
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }  
        public bool IsRead { get; set; }       
        public int UserId { get; set; }
        public int ChatId { get; set; }
        public string Type { get; set; }
        public virtual User User { get; set; }   
    }
}
