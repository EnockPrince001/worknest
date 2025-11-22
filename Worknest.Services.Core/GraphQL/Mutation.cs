using HotChocolate;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore; 
using System.Security.Claims;
using Worknest.Data;
using Worknest.Data.Enums;
using Worknest.Data.Models;
using Worknest.Services.Core.Models;

namespace Worknest.Services.Core.GraphQL
{
    public class Mutation
    {
        [Authorize]
        public async Task<Space> CreateSpace(
            CreateSpaceInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            // 2. Get the logged-in user's ID from the token
            var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdString);

            // We must fetch the user object to assign it to the 'Owner' property
            var owner = await context.Users.FindAsync(userId);
            if (owner == null)
            {
                // This should never happen if the user is authorized, but it's safe to check
                throw new Exception("Authenticated user could not be found.");
            }
           
            // 3. Create the new Space entity
            var space = new Space
            {
                Name = input.Name,
                Key = input.Key.ToUpper(),
                Type = input.Type,
                OwnerId = userId,
                Owner = owner // <-- ASSIGN THE OBJECT HERE
            };

            // 4. ALSO add the creator as a SpaceMember with an ADMIN role
            var member = new SpaceMember
            {
                Space = space,
                UserId = userId,
                Role = SpaceRole.ADMINISTRATOR
            };

            // 5. Add both to the database in a single transaction
            await context.Spaces.AddAsync(space);
            await context.SpaceMembers.AddAsync(member);
            await context.SaveChangesAsync();

            // 6. Return the newly created space
            return space;
        }

        [Authorize]
    public async Task<Sprint> CreateSprint(
    CreateSprintInput input,
    [Service] AppDbContext context,
    ClaimsPrincipal claimsPrincipal)
    {
        // 1. Get User ID from token
        var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.Parse(userIdString);

        // 2. SECURITY CHECK: Is this user an Admin of this space?
        var membership = await context.SpaceMembers
            .FirstOrDefaultAsync(m => m.SpaceId == input.SpaceId && m.UserId == userId);

        if (membership == null || membership.Role != SpaceRole.ADMINISTRATOR)
        {
            // Not an admin? Throw an error.
            throw new HotChocolate.GraphQLException(
                new Error("You are not authorized to create a sprint in this space.", "AUTH_NOT_ADMIN"));
        }

        // 3. Create the new Sprint entity
        var sprint = new Sprint
        {
            Name = input.Name,
            SpaceId = input.SpaceId,
            Goal = input.Goal,
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            Duration = input.Duration,
            Status = SprintStatus.PLANNED // Sprints are always 'PLANNED' on creation
        };

        // 4. Save to database
        await context.Sprints.AddAsync(sprint);
        await context.SaveChangesAsync();

        // 5. Return the new sprint
        return sprint;
    }

        // ... existing code ...

        [Authorize]
        public async Task<Sprint> StartSprint(
            Guid sprintId,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            // 1. Get User ID
            var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdString);

            // 2. Find the Sprint
            var sprint = await context.Sprints
                .FirstOrDefaultAsync(s => s.Id == sprintId);

            if (sprint == null)
            {
                throw new HotChocolate.GraphQLException("Sprint not found.");
            }

            // 3. SECURITY CHECK: Is user an ADMIN of the space this sprint belongs to?
            var membership = await context.SpaceMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.SpaceId == sprint.SpaceId && m.UserId == userId);

            if (membership == null || membership.Role != SpaceRole.ADMINISTRATOR)
            {
                throw new HotChocolate.GraphQLException(
                    new Error("Only administrators can start a sprint.", "AUTH_NOT_ADMIN"));
            }

            // 4. Update Status
            if (sprint.Status == SprintStatus.ACTIVE)
            {
                throw new HotChocolate.GraphQLException("This sprint is already active.");
            }

            sprint.Status = SprintStatus.ACTIVE;

            // Optional: Set StartDate to now if it wasn't set, or keep the planned date?
            // Let's ensure StartDate is recorded as the moment they clicked "Start".
            sprint.StartDate = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return sprint;
        }

        [Authorize]
        public async Task<Sprint> CompleteSprint(
            Guid sprintId,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            // 1. Get User ID
            var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdString);

            // 2. Find the Sprint
            var sprint = await context.Sprints.FindAsync(sprintId);

            if (sprint == null)
            {
                throw new HotChocolate.GraphQLException("Sprint not found.");
            }

            // 3. SECURITY CHECK
            var membership = await context.SpaceMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.SpaceId == sprint.SpaceId && m.UserId == userId);

            if (membership == null || membership.Role != SpaceRole.ADMINISTRATOR)
            {
                throw new HotChocolate.GraphQLException("Only administrators can complete a sprint.");
            }

            // 4. Update Status
            sprint.Status = SprintStatus.COMPLETED;
            sprint.EndDate = DateTime.UtcNow; // Record the actual finish time

            // 5. Logic: Move unfinished items?
            // For now, we just close the sprint. In a real Jira clone, 
            // you might move incomplete items to the backlog here.

            await context.SaveChangesAsync();

            return sprint;
        }

        [Authorize]
    public async Task<WorkItem> CreateWorkItem(
    CreateWorkItemInput input,
    [Service] AppDbContext context,
    ClaimsPrincipal claimsPrincipal)
    {
        // 1. Get User ID from token
        var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.Parse(userIdString);

        // 2. SECURITY CHECK: Is this user a member of this space?
        var membership = await context.SpaceMembers
            .AsNoTracking() // This is a read-only check
            .FirstOrDefaultAsync(m => m.SpaceId == input.SpaceId && m.UserId == userId);

        if (membership == null)
        {
            // Not a member? Throw an error.
            throw new HotChocolate.GraphQLException(
                new Error("You are not a member of this space.", "AUTH_NOT_MEMBER"));
        }

        // 3. Get the Space Key (e.g., "MFP") for the new WorkItem Key
        var space = await context.Spaces.FindAsync(input.SpaceId);
        if (space == null)
        {
            throw new HotChocolate.GraphQLException("Space not found.");
        }

        // 4. Generate the new WorkItem Key (e.g., "MFP-1")
        var itemKey = await GetNextWorkItemKey(context, space.Id);

        // 5. Create the new WorkItem entity
        var workItem = new WorkItem
        {
            Summary = input.Summary,
            Key = $"{space.Key}-{itemKey}",
            ReporterId = userId, // The creator is the reporter
            SprintId = input.SprintId,
            AssigneeId = input.AssigneeId,
            Description = input.Description,
            Priority = input.Priority ?? WorkItemPriority.MEDIUM,
            Status = input.Status ?? WorkItemStatus.TO_DO,
            StoryPoints = input.StoryPoints,
            ParentWorkItemId = input.ParentWorkItemId,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

            // 6. Save to database
            await context.WorkItems.AddAsync(workItem);
            await context.SaveChangesAsync();

            // The 'workItem' object in memory has null navigation properties.
            // We must explicitly load them for the GraphQL response.

            await context.Entry(workItem)
                .Reference(wi => wi.Reporter)
                .LoadAsync();

            if (workItem.AssigneeId.HasValue)
            {
                await context.Entry(workItem)
                    .Reference(wi => wi.Assignee)
                    .LoadAsync();
            }

            // 8. Return the new work item (now with relationships loaded)
            return workItem;
        
    }

    // --- HELPER METHOD to generate the new item key ---
    private async Task<int> GetNextWorkItemKey(AppDbContext context, Guid spaceId)
    {
        // Find the highest number for any WorkItem in this Space
        var latestItem = await context.WorkItems
            .AsNoTracking()
            .Where(wi => wi.Key.StartsWith(
                context.Spaces.First(s => s.Id == spaceId).Key + "-"
            ))
            .OrderByDescending(wi => wi.CreatedDate) // Order by date first
            .FirstOrDefaultAsync();

        if (latestItem == null)
        {
            return 1; // This is the first item
        }

        // Parse the number from the key (e.g., "MFP-123" -> 123)
        var lastKeyNumber = int.Parse(latestItem.Key.Split('-').Last());
        return lastKeyNumber + 1;
    }

        [Authorize]
    public async Task<WorkItem> UpdateWorkItem(
    Guid workItemId, // The ID of the item to update
    UpdateWorkItemInput input,
    [Service] AppDbContext context,
    ClaimsPrincipal claimsPrincipal)
    {
        // 1. Get User ID from token
        var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.Parse(userIdString);

        // 2. Find the existing work item
        var workItem = await context.WorkItems.FindAsync(workItemId);
        if (workItem == null)
        {
            throw new HotChocolate.GraphQLException("Work item not found.");
        }

        // 3. SECURITY CHECK: Is this user a member of the space this item belongs to?
        // We need to find the SpaceId. We can't trust the input.
        // Let's get the spaceId from the item's key.
        var spaceKey = workItem.Key.Split('-').First();
        var isMember = await context.SpaceMembers
            .AsNoTracking()
            .AnyAsync(m => m.UserId == userId && m.Space.Key == spaceKey);

        if (!isMember)
        {
            throw new HotChocolate.GraphQLException(
                new Error("You are not a member of this space.", "AUTH_NOT_MEMBER"));
        }

        // 4. Apply Updates: Dynamically update only the fields that were provided
        if (input.Summary != null)
        {
            workItem.Summary = input.Summary;
        }
        if (input.SprintId.HasValue)
        {
            workItem.SprintId = input.SprintId.Value;
        }
        if (input.AssigneeId.HasValue)
        {
            workItem.AssigneeId = input.AssigneeId.Value;
        }
        if (input.Description != null)
        {
            workItem.Description = input.Description;
        }
        if (input.Priority.HasValue)
        {
            workItem.Priority = input.Priority.Value;
        }
        if (input.Status.HasValue)
        {
            workItem.Status = input.Status.Value;
        }
        if (input.StoryPoints.HasValue)
        {
            workItem.StoryPoints = input.StoryPoints.Value;
        }
        if (input.Flagged.HasValue)
        {
            workItem.Flagged = input.Flagged.Value;
        }

        // Always update the 'UpdatedDate'
        workItem.UpdatedDate = DateTime.UtcNow;

        // 5. Save to database
        await context.SaveChangesAsync();

        // 6. Load navigation properties for the response
        await context.Entry(workItem).Reference(wi => wi.Reporter).LoadAsync();
        if (workItem.AssigneeId.HasValue)
        {
            await context.Entry(workItem).Reference(wi => wi.Assignee).LoadAsync();
        }

        // 7. Return the updated work item
        return workItem;
    }

 [Authorize]
    public async Task<SpaceMember> InviteUserToSpace(
    InviteUserInput input,
    [Service] AppDbContext context,
    ClaimsPrincipal claimsPrincipal)
    {
        // 1. Get the "inviter's" ID from the token
        var inviterIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var inviterId = Guid.Parse(inviterIdString);

        // 2. SECURITY CHECK: Is the inviter an ADMIN of this space?
        var inviterMembership = await context.SpaceMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.SpaceId == input.SpaceId && m.UserId == inviterId);

        if (inviterMembership == null || inviterMembership.Role != SpaceRole.ADMINISTRATOR)
        {
            throw new HotChocolate.GraphQLException(
                new Error("You are not authorized to invite users to this space.", "AUTH_NOT_ADMIN"));
        }

        // 3. Find the user being invited (the "invitee")
        var invitee = await context.Users
            .FirstOrDefaultAsync(u => u.Email == input.Email);

        if (invitee == null)
        {
            throw new HotChocolate.GraphQLException("A user with this email was not found.");
        }

        // 4. Check if the user is ALREADY a member
        var existingMembership = await context.SpaceMembers
            .FirstOrDefaultAsync(m => m.SpaceId == input.SpaceId && m.UserId == invitee.Id);

        if (existingMembership != null)
        {
            throw new HotChocolate.GraphQLException("This user is already a member of this space.");
        }

        // 5. All checks passed! Create the new membership.
        var newMember = new SpaceMember
        {
            SpaceId = input.SpaceId,
            UserId = invitee.Id,
            Role = input.Role
        };

        // 6. Save to database
        await context.SpaceMembers.AddAsync(newMember);
        await context.SaveChangesAsync();

        // 7. Load navigation properties for the response
        await context.Entry(newMember).Reference(m => m.User).LoadAsync();
        await context.Entry(newMember).Reference(m => m.Space).LoadAsync();

        // 8. Return the new membership record
        return newMember;
    }
}
}