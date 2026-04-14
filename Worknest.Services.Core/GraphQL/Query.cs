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
        [UseProjection]   
        public IQueryable<User> GetMe(
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal) 
        {
            var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Enumerable.Empty<User>().AsQueryable();
            }

            var userId = Guid.Parse(userIdString);
            return context.Users.Where(u => u.Id == userId);
        }

        [Authorize]       
        [UseProjection]   
        public IQueryable<Space> GetSpaceDetails(
            string spaceKey, 
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdString!);

            return context.Spaces
                .Where(s => s.Key == spaceKey)
                .Where(s => s.Members.Any(m => m.UserId == userId));
        }

        [Authorize]
        [UseProjection]
        public IQueryable<Space> GetSpace(
            string spaceKey,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdString!);

            return context.Spaces
                .Where(s => s.Key == spaceKey)
                .Where(s => s.Members.Any(m => m.UserId == userId));
        }

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
            var userId = Guid.Parse(userIdString!);

            var spaceId = context.Spaces
                .Where(s => s.Key == spaceKey && s.Members.Any(m => m.UserId == userId))
                .Select(s => (Guid?)s.Id)
                .FirstOrDefault();

            if (spaceId == null)
            {
                return Enumerable.Empty<WorkItem>().AsQueryable();
            }

            // Explicitly including Reporter and Assignee to ensure they are available in board/list views
            return context.WorkItems
                .Include(wi => wi.Reporter)
                .Include(wi => wi.Assignee)
                .Include(wi => wi.Comments)
                    .ThenInclude(c => c.Author)
                .Where(wi => wi.SpaceId == spaceId.Value);
        }

        [Authorize]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<WorkItemComment> GetWorkItemComments(
    Guid workItemId,
    [Service] AppDbContext context)
        {
            return context.WorkItemComments
                .Where(c => c.WorkItemId == workItemId);
        }


        [Authorize]
        [UseProjection] 
        public IQueryable<WorkItem> GetWorkItem(
            Guid id,
            [Service] AppDbContext context)
        {
            // Fetching a single work item with all related data for the Detail Modal
            return context.WorkItems
                .Include(wi => wi.Reporter)    // Fixes the "Unknown" reporter issue
                .Include(wi => wi.Assignee)
                .Include(wi => wi.BoardColumn)
                .Include(wi => wi.Comments)
                    .ThenInclude(c => c.Author)
                    
                .Include(wi => wi.Activities)  // Populates the History tab
                    .ThenInclude(a => a.Author)
                .Where(wi => wi.Id == id);
        }
    }
}
