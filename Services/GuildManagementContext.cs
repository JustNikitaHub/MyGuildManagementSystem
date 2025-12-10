using Microsoft.EntityFrameworkCore;
using GuildManagement.Entities;

namespace GuildManagement.Data
{
    public class GuildManagementContext : DbContext
    {
        public GuildManagementContext(DbContextOptions<GuildManagementContext> options) : base(options)
        {
        }

        public DbSet<Member> Members { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<Resource> Resources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Name).IsRequired().HasMaxLength(50);
                entity.Property(m => m.Level).IsRequired();
                entity.Property(m => m.MemberClass).IsRequired().HasConversion<string>();
            });

            
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Type).IsRequired().HasConversion<string>();
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();

                
                entity.HasOne(e => e.Member)
                      .WithMany(m => m.Events)
                      .HasForeignKey(e => e.MemberId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            
            modelBuilder.Entity<Achievement>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Title).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Description).IsRequired().HasMaxLength(500);
                entity.Property(a => a.EarnedDate).IsRequired();

                
                entity.HasOne(a => a.Member)
                      .WithMany(m => m.Achievements)
                      .HasForeignKey(a => a.MemberId)
                      .OnDelete(DeleteBehavior.Restrict);

                
                entity.HasOne(a => a.Event)
                      .WithOne(e => e.Achievement)
                      .HasForeignKey<Achievement>(a => a.EventId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            
            modelBuilder.Entity<Resource>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Type).IsRequired().HasConversion<string>();
                entity.Property(r => r.Rarity).HasConversion<string>();
                entity.Property(r => r.Quantity).IsRequired();

                
                entity.HasOne(r => r.Member)
                      .WithMany(m => m.Resources)
                      .HasForeignKey(r => r.MemberId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            
            modelBuilder.Entity<Event>()
                .HasIndex(e => e.StartDate);

            modelBuilder.Entity<Achievement>()
                .HasIndex(a => a.EarnedDate);
        }
    }
}