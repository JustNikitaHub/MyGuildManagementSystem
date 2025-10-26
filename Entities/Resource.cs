using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuildManagement.Entities
{
    [Table("resources")]
    public class Resource
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name", TypeName = "varchar(100)")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("resource_type")]
        public ResourceType Type { get; set; }

        [Required]
        [Column("quantity")]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Column("rarity")]
        public Rarity Rarity { get; set; }

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Required]
        [Column("member_id")]
        public int MemberId { get; set; }

        [ForeignKey("MemberId")]
        public Member Member { get; set; } = null!;

        public Resource() { }
        public Resource(string name, ResourceType type, int quantity, int memberId)
        {
            Name = name;
            Type = type;
            Quantity = quantity;
            MemberId = memberId;
        }
    }

    public enum ResourceType
    {
        Wood,
        Stone,
        Iron,
        Gold,
        Gem
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare
    }
}