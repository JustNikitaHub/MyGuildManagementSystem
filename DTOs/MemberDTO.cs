namespace GuildManagement.DTOs
{
    public class MemberDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public string MemberClass { get; set; } = string.Empty;
        
        public List<int> EventIds { get; set; } = new();
        public List<int> AchievementIds { get; set; } = new();
        public List<int> ResourceIds { get; set; } = new();
        
        public int EventsCount { get; set; }
        public int AchievementsCount { get; set; }
        public int ResourcesCount { get; set; }
    }
}