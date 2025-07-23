namespace BookStore.Core.DTOs
{
    public class UserStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int AdminUsers { get; set; }
        public int RegularUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int ActiveUsersToday { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
