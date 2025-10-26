using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuildManagement.Entities
{
    [Table("members")]
    public class Member
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column("name", TypeName = "varchar(50)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("level")]
        [Range(1, 60)]
        public int Level { get; set; }

        [Required]
        [Column("member_class")]
        public MemberClass MemberClass { get; set; }

        //связи
        public List<Event> Events { get; set; } = new();
        public List<Achievement> Achievements { get; set; } = new();
        public List<Resource> Resources { get; set; } = new();

        public Member() { }

        public Member(string name, int level, MemberClass memberClass)
        {
            Name = name;
            Level = level;
            MemberClass = memberClass;
        }
    }

    public enum MemberClass
    {
        Warrior,
        Rogue,
        Mage,
    }
}