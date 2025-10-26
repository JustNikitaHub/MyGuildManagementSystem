using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuildManagement.Entities
{
    [Table("achievements")]
    public class Achievement
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("title", TypeName = "varchar(100)")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Column("description", TypeName = "varchar(500)")]
        public string Description { get; set; } = string.Empty;

        public DateTime EarnedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("member_id")]
        public int MemberId { get; set; }

        //связь с участником
        [ForeignKey("MemberId")]
        public Member Member { get; set; } = null!;

        //связь с достижением
        [Column("event_id")]
        public int? EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        public Achievement() { }

        public Achievement(string title, string description, int memberId)
        {
            Title = title;
            Description = description;
            MemberId = memberId;
        }
    }

}