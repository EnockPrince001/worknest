using Microsoft.AspNetCore.Identity; 
using System.ComponentModel.DataAnnotations;
using Worknest.Data.Models;

namespace Worknest.Data.Models
{
    // 2. CHANGE THIS LINE to inherit from IdentityUser
    public class User : IdentityUser<Guid>
    {
        // We can REMOVE Id, Username, and Email
        // because IdentityUser already has them.

        public string? FullName { get; set; }
        public string? JobTitle { get; set; }

        // --- Relationships ---
        public ICollection<SpaceMember> Spaces { get; set; } = new List<SpaceMember>();
        public ICollection<WorkItem> ReportedWorkItems { get; set; } = new List<WorkItem>();
        public ICollection<WorkItem> AssignedWorkItems { get; set; } = new List<WorkItem>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}