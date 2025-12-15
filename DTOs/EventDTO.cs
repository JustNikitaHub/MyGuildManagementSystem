namespace GuildManagement.DTOs
{
    public class EventDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MemberId { get; set; }
        public string? MemberName { get; set; }
        public int? AchievementId { get; set; }
        public string? AchievementTitle { get; set; }
        public bool IsUpcoming => StartDate > DateTime.Now;
        public bool IsCompleted => EndDate < DateTime.Now;
        public bool IsActive => DateTime.Now >= StartDate && DateTime.Now <= EndDate;
    }
}