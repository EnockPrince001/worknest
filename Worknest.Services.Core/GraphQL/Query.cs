using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; 
using Worknest.Data;
using Worknest.Data.Models;

namespace Worknest.Services.Core.GraphQL
{
    public class Query
    {
        [Authorize]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<User> GetUsers(AppDbContext context)
             => context.Users;

        

        [Authorize]       
        [UseProjection]   // Allows the frontend to select related data (like spaces)
        public IQueryable<User> GetMe(
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal) // Injects the user's "identity" from the token
        {
            // Get the user's ID (which we stored in the token)
            var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                // This should be impossible if [Authorize] is working, but it's good practice
                return Enumerable.Empty<User>().AsQueryable();
            }

            var userId = Guid.Parse(userIdString);

            // Return a query for ONLY this one user
            return context.Users.Where(u => u.Id == userId);
        }

        [Authorize]       // User must be logged in
        [UseProjection]   // Allows frontend to get related data
        public IQueryable<Space> GetSpaceDetails(
            string spaceKey, // The key of the space to load (e.g., "MFP")
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            // 1. Get User ID from token
            var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdString);

            // 2. Build a query that finds the space *only if* the user is a member.
            // This is your core security check.
            var query = context.Spaces
                .Where(s => s.Key == spaceKey) // Find the space by its key
                .Where(s => s.Members.Any(m => m.UserId == userId)); // Check if user is a member

            // 3. We also need to get all work items for this space.
            // We'll create a special property for this.
            // This part is complex, we'll do it in the next step
            // For now, let's just return the space.

            return query;
        }

        // --- QUERY 1: Get Space Info ---
        [Authorize]
        [UseProjection]
        public IQueryable<Space> GetSpace(
            string spaceKey,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdString);

            // Return the space (e.g., "MFP") IF the user is a member
            return context.Spaces
                .Where(s => s.Key == spaceKey)
                .Where(s => s.Members.Any(m => m.UserId == userId));
        }

        // --- QUERY 2: Get Work Items for that Space ---
        [Authorize]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<WorkItem> GetWorkItemsForSpace(
            string spaceKey,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdString);

            // 1. First, check if the user is a member of this space
            var isMember = context.SpaceMembers
                .Any(m => m.User.Id == userId && m.Space.Key == spaceKey);

            if (!isMember)
            {
                // Not a member? Return an empty list.
                return Enumerable.Empty<WorkItem>().AsQueryable();
            }

            // 2. User is a member, so return all work items for that space
            // We find them by checking the work item's "Key" (e.g., "MFP-1")
            return context.WorkItems
                .Where(wi => wi.Key.StartsWith(spaceKey + "-"));
        }
    }
}