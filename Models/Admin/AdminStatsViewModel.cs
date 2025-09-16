namespace babiltr.Models.Admin
{
    public class AdminStatsViewModel
    {
        public int TotalApplications { get; set; }
        public int TotalUsers { get; set; }
        public int TotalJobs { get; set; }
        public int TodayApplications { get; set; }
        public int TodayUsers { get; set; }
        public int TodayJobs { get; set; }
        public int ApplicationGrowth { get; set; }
        public int UserGrowth { get; set; }
        public int JobGrowth { get; set; }
        public List<int> WeeklyUsers { get; set; } 
        public List<int> WeeklyJobs { get; set; }
        public List<int> WeeklyApplications { get; set; }
    }
}
