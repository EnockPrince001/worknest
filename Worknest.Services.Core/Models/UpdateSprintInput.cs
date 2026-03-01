using Worknest.Data.Enums;
using HotChocolate;

namespace Worknest.Services.Core.Models
{
    public class UpdateSprintInput
    {
        public Optional<string?> Name { get; set; }
        public Optional<string?> Goal { get; set; }
        public Optional<DateTime?> StartDate { get; set; }
        public Optional<DateTime?> EndDate { get; set; }
        public Optional<string?> Duration { get; set; }
        public Optional<SprintStatus?> Status { get; set; }
    }
}
