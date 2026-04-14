using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Worknest.Data.Models
{
    public class BoardColumn
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; } = default!; // e.g., "To Do", "Failed"

        public int Order { get; set; } // To keep them in the right order (0, 1, 2...)
        public bool IsSystem { get; set; } = false; // If true, cannot be deleted

        // Relationships
        [Required]
        public Guid SpaceId { get; set; }
        [ForeignKey("SpaceId")]
        public Space Space { get; set; } = default!;
    }
}
