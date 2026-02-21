using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Worknest.Data.Models
{
    public class WorkItemLink
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public Guid SourceId { get; set; }
        [ForeignKey("SourceId")]
        public virtual WorkItem Source { get; set; } = null!;

        [Required]
        public Guid TargetId { get; set; }
        [ForeignKey("TargetId")]
        public virtual WorkItem Target { get; set; } = null!;

        /// <summary>
        /// Helper for GraphQL to find the related item.
        /// Usually, we treat the 'Target' as the linked item when viewing from the source.
        /// </summary>
        [NotMapped]
        public WorkItem LinkedItem => Target;
    }
}