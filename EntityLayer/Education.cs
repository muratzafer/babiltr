namespace babiltr.EntityLayer
{
    public class Education
    {
        public int EducationID { get; set; }
        public int UserID { get; set; }
        public string? Degree { get; set; }
        public string? SchoolName { get; set; }
        public string? Years { get; set; }
        public User User { get; set; }
    }
}
