using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuildManagement.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();
        public int? MemberId { get; set; }
        
        [ForeignKey("MemberId")]
        public virtual Member? Member { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
    }
}