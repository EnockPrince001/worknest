using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Worknest.Data.Models
{
    public class WorkItemComment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkItemId { get; set; }
        public WorkItem WorkItem { get; set; } = default!;

        [Required]
        public string CommentText { get; set; } = default!;

        public Guid? CreatedBy { get; set; }
        public User? Author { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // GraphQL compatibility for detail modal query shape.
        [NotMapped]
        public string Content => CommentText;

        [NotMapped]
        public DateTime CreatedDate => CreatedAt;
    }
}
