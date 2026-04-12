using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore;
using Worknest.Data.Enums;
using Worknest.Data.Models;

namespace Worknest.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Space> Spaces { get; set; }
        public DbSet<SpaceMember> SpaceMembers { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<WorkItem> WorkItems { get; set; }
        public DbSet<WorkItemComment> WorkItemComments { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<BoardColumn> BoardColumns { get; set; }
        // --- ADD THIS LINE ---
        public DbSet<Activity> Activities { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            // SpaceMember configuration
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

            // WorkItem (Reporter/Assignee)
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

            // WorkItem Subtasks
            modelBuilder.Entity<WorkItem>()
                .HasOne(wi => wi.ParentWorkItem)
                .WithMany(wi => wi.Subtasks)
                .HasForeignKey(wi => wi.ParentWorkItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comments
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkItemComment>()
                .HasOne(c => c.WorkItem)
                .WithMany(wi => wi.Comments)
                .HasForeignKey(c => c.WorkItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkItemComment>()
                .HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // --- ADD ACTIVITY CONFIGURATION ---
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.WorkItem)
                .WithMany(wi => wi.Activities) 
                .HasForeignKey(a => a.WorkItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Activity>()
                .HasOne(a => a.Author)
                .WithMany()
                .HasForeignKey(a => a.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Space & Columns
            modelBuilder.Entity<Space>()
                .HasOne(s => s.Owner)
                .WithMany()
                .HasForeignKey(s => s.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Space>()
                .HasMany(s => s.BoardColumns)
                .WithOne(c => c.Space)
                .HasForeignKey(c => c.SpaceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkItem>()
                .HasOne(wi => wi.BoardColumn)
                .WithMany()
                .HasForeignKey(wi => wi.BoardColumnId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Space>()
                .HasIndex(s => s.Key)
                .IsUnique();

            modelBuilder.Entity<WorkItem>()
                .HasIndex(w => w.Key)
                .IsUnique();

            // Enum Conversions
            modelBuilder.Entity<Space>().Property(s => s.Type).HasConversion<string>();
            modelBuilder.Entity<SpaceMember>().Property(sm => sm.Role).HasConversion<string>();
            modelBuilder.Entity<Sprint>().Property(s => s.Status).HasConversion<string>();
            modelBuilder.Entity<WorkItem>().Property(wi => wi.Priority).HasConversion<string>();
        }
    }
}
