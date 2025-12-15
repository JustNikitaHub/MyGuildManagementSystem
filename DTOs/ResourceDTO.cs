namespace GuildManagement.DTOs
{
    public class ResourceDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Rarity { get; set; }
        public string? Description { get; set; }
        public int MemberId { get; set; }
        public string? MemberName { get; set; }
        public bool IsRare => Rarity == "Rare";
        public bool IsLimited => Quantity < 100;
    }
}