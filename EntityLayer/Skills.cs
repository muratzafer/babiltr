namespace babiltr.EntityLayer
{
    public class Skills
    {
        public int SkillsID { get; set; }
        public int UserID { get; set; }
        public string? SkillName { get; set; }
        public int? SkillPercentage { get; set; }
        public User User { get; set; }
    }
}
