using Microsoft.AspNetCore.Identity; // <-- 1. ADD THIS
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // <-- 2. ADD THIS
using Microsoft.EntityFrameworkCore;
using Worknest.Data.Enums;
using Worknest.Data.Models;

namespace Worknest.Data
{
    // 3. CHANGE THIS LINE to inherit from IdentityDbContext
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // We can REMOVE the DbSet<User> because IdentityDbContext handles it.
        // public DbSet<User> Users { get; set; } 
        public DbSet<Space> Spaces { get; set; }
        public DbSet<SpaceMember> SpaceMembers { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<WorkItem> WorkItems { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 4. ADD THIS LINE FIRST
            base.OnModelCreating(modelBuilder); // This is ESSENTIAL for Identity tables

            // Configure the many-to-many relationship for Space <-> User
            modelBuilder.Entity<SpaceMember>()
                .HasKey(sm => new { sm.SpaceId, sm.UserId });

            modelBuilder.Entity<SpaceMember>()
                .HasOne(sm => sm.Space)
                .WithMany(s => s.Members)
                .HasForeignKey(sm => sm.SpaceId);

            modelBuilder.Entity<SpaceMember>()
                .HasOne(sm => sm.User)
                .WithMany(u => u.Spaces)
                .HasForeignKey(sm => sm.UserId);

            // Configure the User -> WorkItem relationships (Reporter/Assignee)
            modelBuilder.Entity<WorkItem>()
                .HasOne(wi => wi.Reporter)
                .WithMany(u => u.ReportedWorkItems)
                .HasForeignKey(wi => wi.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkItem>()
                .HasOne(wi => wi.Assignee)
                .WithMany(u => u.AssignedWorkItems)
                .HasForeignKey(wi => wi.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure the WorkItem -> Subtasks self-referencing relationship
            modelBuilder.Entity<WorkItem>()
                .HasOne(wi => wi.ParentWorkItem)
                .WithMany(wi => wi.Subtasks)
                .HasForeignKey(wi => wi.ParentWorkItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the User -> Comment relationship
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the Space -> Owner relationship
            modelBuilder.Entity<Space>()
                .HasOne(s => s.Owner)
                .WithMany()
                .HasForeignKey(s => s.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Convert all Enums to strings in the database
            modelBuilder.Entity<Space>().Property(s => s.Type).HasConversion<string>();
            modelBuilder.Entity<SpaceMember>().Property(sm => sm.Role).HasConversion<string>();
            modelBuilder.Entity<Sprint>().Property(s => s.Status).HasConversion<string>();
            modelBuilder.Entity<WorkItem>().Property(wi => wi.Priority).HasConversion<string>();
            modelBuilder.Entity<WorkItem>().Property(wi => wi.Status).HasConversion<string>();
        }
    }
}