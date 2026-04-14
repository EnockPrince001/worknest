using HotChocolate;
using Worknest.Data.Enums;

namespace Worknest.Services.Core.Models
{
    public class UpdateSpaceInput
    {
        public Optional<string?> Name { get; set; }
        public Optional<string?> Key { get; set; }
        public Optional<SpaceType?> Type { get; set; }
    }
}
