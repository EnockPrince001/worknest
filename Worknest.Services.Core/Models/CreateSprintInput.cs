using System.ComponentModel.DataAnnotations;

namespace Worknest.Services.Core.Models
{
    public class CreateSprintInput
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public Guid SpaceId { get; set; } // The space this sprint belongs to

        public string? Goal { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Duration { get; set; } // e.g., "2 weeks"
    }
}