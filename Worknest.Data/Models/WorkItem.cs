using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Worknest.Data.Enums;

namespace Worknest.Data.Models
{
    public class WorkItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Key { get; set; } // e.g., "PROJ-123"

        [Required]
        public string Summary { get; set; }

        public string? Description { get; set; }

       public WorkItemPriority Priority { get; set; }

        public int? StoryPoints { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        public bool Flagged { get; set; }

        public int Order { get; set; }

        // --- Relationships ---

        [Required]
        public Guid ReporterId { get; set; }
        [ForeignKey("ReporterId")]
        public User Reporter { get; set; }

        public Guid? BoardColumnId { get; set; }
        [ForeignKey("BoardColumnId")]
        public BoardColumn? BoardColumn { get; set; }

        public Guid? AssigneeId { get; set; }
        [ForeignKey("AssigneeId")]
        public User? Assignee { get; set; }

        public Guid? SprintId { get; set; }
        [ForeignKey("SprintId")]
        public Sprint? Sprint { get; set; }

        // For Subtasks: A work item can have a parent
        public Guid? ParentWorkItemId { get; set; }
        [ForeignKey("ParentWorkItemId")]
        public WorkItem? ParentWorkItem { get; set; }

        // A work item can have many subtasks
        public ICollection<WorkItem> Subtasks { get; set; } = new List<WorkItem>();

        // A work item can have many comments
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}