using Worknest.Data.Enums;

namespace Worknest.Services.Core.Models
{
    public class UpdateWorkItemInput
    {
        // All fields are optional (nullable)
        public string? Summary { get; set; }
        public Guid? SprintId { get; set; }
        public Guid? AssigneeId { get; set; }
        public string? Description { get; set; }
        public WorkItemPriority? Priority { get; set; }
        public Guid? BoardColumnId { get; set; }
        public int? StoryPoints { get; set; }
        public bool? Flagged { get; set; }
        public DateTime? DueDate { get; set; }
    }
}