using System.ComponentModel.DataAnnotations;
using Worknest.Data.Enums;

namespace Worknest.Data.Models
{
    // This is the many-to-many linking entity between User and Space
    public class SpaceMember
    {
        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; }

        [Required]
        public Guid SpaceId { get; set; }
        public Space Space { get; set; }

        [Required]
        public SpaceRole Role { get; set; }
    }
}