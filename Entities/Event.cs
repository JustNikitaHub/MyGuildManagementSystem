using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuildManagement.Entities
{
    [Table("events")]
    public class Event
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("title", TypeName = "varchar(100)")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        [Column("description", TypeName = "varchar(500)")]
        public string? Description { get; set; }

        [Required]
        [Column("event_type")]
        public EventType Type { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("member_id")]
        public int MemberId { get; set; }

        //связь с участником
        [ForeignKey("MemberId")]
        public Member Member { get; set; } = null!;

        //связь с достижением
        public Achievement? Achievement { get; set; }

        public Event() { }

        public Event(string title, EventType type, DateTime startDate, int memberId)
        {
            Title = title;
            Type = type;
            StartDate = startDate;
            MemberId = memberId;
            EndDate = startDate.AddHours(2);
        }
    }

    public enum EventType
    {
        Raid,
        Dungeon,
        Social
    }
}