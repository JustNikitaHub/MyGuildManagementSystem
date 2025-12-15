namespace GuildManagement.DTOs
{
    public class AchievementDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EarnedDate { get; set; }
        public int MemberId { get; set; }
        public string? MemberName { get; set; }
        
        public int? EventId { get; set; }
        public string? EventTitle { get; set; }
        public string FormattedDate => EarnedDate.ToString("dd.MM.yyyy HH:mm");
    }
}