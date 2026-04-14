using System.ComponentModel.DataAnnotations;
using Worknest.Data.Enums; 

namespace Worknest.Services.Core.Models
{
    public class CreateSpaceInput
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = default!;

        [Required]
        [StringLength(10)] // e.g., "PROJ"
        public string Key { get; set; } = default!;

        [Required]
        public SpaceType Type { get; set; } // KANBAN or SCRUM
    }
}