using System.ComponentModel.DataAnnotations;
using Worknest.Data.Enums;

namespace Worknest.Services.Core.Models
{
    public class InviteUserInput
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } // Email of the user to invite

        [Required]
        public Guid SpaceId { get; set; } // Space to invite them to

        public SpaceRole Role { get; set; } = SpaceRole.MEMBER; // Default role
    }
}