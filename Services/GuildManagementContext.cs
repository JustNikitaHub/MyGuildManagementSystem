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
            modelBuilder.Entity<Member>()
                .HasMany(p => p.Events)
                .WithOne(e => e.Member)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Member>()
                .HasMany(p => p.Achievements)
                .WithOne(a => a.Member)
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Member>()
                .HasMany(p => p.Resources)
                .WithOne(r => r.Member)
                .HasForeignKey(r => r.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Achievement)
                .WithOne(a => a.Event)
                .HasForeignKey<Achievement>(a => a.EventId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Event>()
                .HasIndex(e => e.StartDate);
            modelBuilder.Entity<Achievement>()
                .HasIndex(a => a.EarnedDate);
            base.OnModelCreating(modelBuilder);
        }
    }
}