using System;

namespace Worknest.Data.Models
{
    public class WorkItemComment
    {
        public Guid Id { get; set; }

        public Guid WorkItemId { get; set; }

        public string CommentText { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
