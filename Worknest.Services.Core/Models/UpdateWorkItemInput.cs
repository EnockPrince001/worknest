using HotChocolate;
using Worknest.Data.Enums;

namespace Worknest.Services.Core.Models
{
    public class UpdateWorkItemInput
    {
        public Optional<string?> Summary { get; set; }
        public Optional<Guid?> SprintId { get; set; }
        public Optional<bool?> MoveToBacklog { get; set; }
        public Optional<Guid?> AssigneeId { get; set; }
        public Optional<string?> Description { get; set; }
        public Optional<WorkItemPriority?> Priority { get; set; }
        public Optional<Guid?> BoardColumnId { get; set; }
        public Optional<int?> StoryPoints { get; set; }
        public Optional<bool?> Flagged { get; set; }
        public Optional<DateTime?> DueDate { get; set; }
    }
}
