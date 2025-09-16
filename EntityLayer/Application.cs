namespace babiltr.EntityLayer
{
    public class Application
    {
        public int ApplicationID { get; set; }
        public int JobID { get; set; }
        public int UserID { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string ApplicationStatus { get; set; } //Approved, Pending, Rejected
        public virtual Jobs Job { get; set; }
        public virtual User User { get; set; }
    }
}
