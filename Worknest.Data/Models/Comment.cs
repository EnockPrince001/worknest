using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Worknest.Data.Models
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Content { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Relationship to WorkItem
        [Required]
        public Guid WorkItemId { get; set; }
        [ForeignKey("WorkItemId")]
        public WorkItem WorkItem { get; set; }

        // Relationship to Author (User)
        [Required]
        public Guid AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public User Author { get; set; }
    }
}