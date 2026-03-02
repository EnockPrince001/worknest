using System.ComponentModel.DataAnnotations;
using Worknest.Data.Enums;

namespace Worknest.Services.Core.Models
{
    public class CreateWorkItemInput
    {
        [Required]
        public string Summary { get; set; }

        [Required]
        public Guid SpaceId { get; set; } // Which space this item belongs to

        public WorkItemType? Type { get; set; }

        // Optional fields
        public Guid? SprintId { get; set; }
        public Guid? AssigneeId { get; set; }
        public string? Description { get; set; }
        public WorkItemPriority? Priority { get; set; } = WorkItemPriority.MEDIUM;
        public int? StoryPoints { get; set; }
        public Guid? ParentWorkItemId { get; set; } // For subtasks
        public Guid? BoardColumnId { get; set; } // Initial column
    }
}