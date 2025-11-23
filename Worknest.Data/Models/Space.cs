using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Worknest.Data.Enums;

namespace Worknest.Data.Models
{
    public class Space
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }

        [Required]
        public string Key { get; set; } // e.g., "PROJ"

        public SpaceType Type { get; set; }

        // --- Relationships ---

        [Required]
        public Guid OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        // A space has many members (through the SpaceMember linking table)
        public ICollection<SpaceMember> Members { get; set; } = new List<SpaceMember>();

        // A space has many sprints (if Scrum)
        public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
        public ICollection<BoardColumn> BoardColumns { get; set; } = new List<BoardColumn>();
    }
}