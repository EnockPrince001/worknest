using System;

namespace Worknest.Data.Models
{
    public class Activity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkItemId { get; set; }
        public WorkItem WorkItem { get; set; } = null!; // Navigation property
        public string Field { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public Guid AuthorId { get; set; }
        public User Author { get; set; } = null!; // Navigation property
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}