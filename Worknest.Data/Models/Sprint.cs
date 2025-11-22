using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Worknest.Data.Enums;

namespace Worknest.Data.Models
{
    public class Sprint
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        public string? Goal { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Duration { get; set; } // e.g., "2 weeks"

        public SprintStatus Status { get; set; }

        // --- Relationships ---

        [Required]
        public Guid SpaceId { get; set; }
        [ForeignKey("SpaceId")]
        public Space Space { get; set; }

        // A sprint has many work items
        public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
    }
}