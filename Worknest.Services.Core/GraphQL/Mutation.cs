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
        // --- SPACE MANAGEMENT ---

        [Authorize]
        public async Task<Space> CreateSpace(
            CreateSpaceInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            var owner = await context.Users.FindAsync(userId);
            if (owner == null) throw new Exception("User not found");

            var space = new Space
            {
                Name = input.Name,
                Key = input.Key.ToUpper(),
                Type = input.Type,
                OwnerId = userId,
                Owner = owner
            };

            // --- 1. AUTOMATICALLY CREATE DEFAULT COLUMNS ---
            var cols = new List<BoardColumn>
            {
                new BoardColumn { Name = "TO DO", Order = 0, IsSystem = true, Space = space },
                new BoardColumn { Name = "IN PROGRESS", Order = 1, IsSystem = true, Space = space },
                new BoardColumn { Name = "DONE", Order = 2, IsSystem = true, Space = space }
            };

            await context.Spaces.AddAsync(space);
            await context.BoardColumns.AddRangeAsync(cols); // Save the columns

            var member = new SpaceMember { Space = space, UserId = userId, Role = SpaceRole.ADMINISTRATOR };
            await context.SpaceMembers.AddAsync(member);
            await context.SaveChangesAsync();

            return space;
        }

        [Authorize]
        public async Task<SpaceMember> InviteUserToSpace(
            InviteUserInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var inviterId = Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));

            var inviterMembership = await context.SpaceMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.SpaceId == input.SpaceId && m.UserId == inviterId);

            if (inviterMembership == null || inviterMembership.Role != SpaceRole.ADMINISTRATOR)
            {
                throw new HotChocolate.GraphQLException(
                    new Error("You are not authorized to invite users to this space.", "AUTH_NOT_ADMIN"));
            }

            var invitee = await context.Users.FirstOrDefaultAsync(u => u.Email == input.Email);
            if (invitee == null) throw new HotChocolate.GraphQLException("A user with this email was not found.");

            var existingMembership = await context.SpaceMembers
                .FirstOrDefaultAsync(m => m.SpaceId == input.SpaceId && m.UserId == invitee.Id);

            if (existingMembership != null) throw new HotChocolate.GraphQLException("This user is already a member of this space.");

            var newMember = new SpaceMember
            {
                SpaceId = input.SpaceId,
                UserId = invitee.Id,
                Role = input.Role
            };

            await context.SpaceMembers.AddAsync(newMember);
            await context.SaveChangesAsync();

            await context.Entry(newMember).Reference(m => m.User).LoadAsync();
            await context.Entry(newMember).Reference(m => m.Space).LoadAsync();

            return newMember;
        }

        // --- SPRINT MANAGEMENT ---

        [Authorize]
        public async Task<Sprint> CreateSprint(
            CreateSprintInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));

            var membership = await context.SpaceMembers
                .FirstOrDefaultAsync(m => m.SpaceId == input.SpaceId && m.UserId == userId);

            if (membership == null || membership.Role != SpaceRole.ADMINISTRATOR)
            {
                throw new HotChocolate.GraphQLException(
                    new Error("You are not authorized to create a sprint in this space.", "AUTH_NOT_ADMIN"));
            }

            var sprint = new Sprint
            {
                Name = input.Name,
                SpaceId = input.SpaceId,
                Goal = input.Goal,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                Duration = input.Duration,
                Status = SprintStatus.PLANNED
            };

            await context.Sprints.AddAsync(sprint);
            await context.SaveChangesAsync();

            return sprint;
        }

        [Authorize]
        public async Task<Sprint> StartSprint(
            Guid sprintId,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            var sprint = await context.Sprints.FirstOrDefaultAsync(s => s.Id == sprintId);

            if (sprint == null) throw new HotChocolate.GraphQLException("Sprint not found.");

            var membership = await context.SpaceMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.SpaceId == sprint.SpaceId && m.UserId == userId);

            if (membership == null || membership.Role != SpaceRole.ADMINISTRATOR)
            {
                throw new HotChocolate.GraphQLException(new Error("Only administrators can start a sprint.", "AUTH_NOT_ADMIN"));
            }

            if (sprint.Status == SprintStatus.ACTIVE)
            {
                throw new HotChocolate.GraphQLException("This sprint is already active.");
            }

            sprint.Status = SprintStatus.ACTIVE;
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
            var userId = Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            var sprint = await context.Sprints.FindAsync(sprintId);

            if (sprint == null) throw new HotChocolate.GraphQLException("Sprint not found.");

            var membership = await context.SpaceMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.SpaceId == sprint.SpaceId && m.UserId == userId);

            if (membership == null || membership.Role != SpaceRole.ADMINISTRATOR)
            {
                throw new HotChocolate.GraphQLException("Only administrators can complete a sprint.");
            }

            sprint.Status = SprintStatus.COMPLETED;
            sprint.EndDate = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return sprint;
        }

        // --- WORK ITEM MANAGEMENT ---

        [Authorize]
        public async Task<WorkItem> CreateWorkItem(
            CreateWorkItemInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));

            var membership = await context.SpaceMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.SpaceId == input.SpaceId && m.UserId == userId);

            if (membership == null)
            {
                throw new HotChocolate.GraphQLException(new Error("You are not a member of this space.", "AUTH_NOT_MEMBER"));
            }

            var space = await context.Spaces
                .Include(s => s.BoardColumns) // Load columns to find the first one
                .FirstOrDefaultAsync(s => s.Id == input.SpaceId);

            if (space == null) throw new GraphQLException("Space not found.");

            // --- 2. FIND THE FIRST COLUMN (usually "TO DO") ---
            var firstColumn = space.BoardColumns.OrderBy(c => c.Order).FirstOrDefault();
            if (firstColumn == null) throw new GraphQLException("Space configuration error: No columns found.");

            var itemKey = await GetNextWorkItemKey(context, space.Id);

            var workItem = new WorkItem
            {
                Summary = input.Summary,
                Key = $"{space.Key}-{itemKey}",
                ReporterId = userId,
                SprintId = input.SprintId,
                AssigneeId = input.AssigneeId,
                Description = input.Description,
                Priority = input.Priority ?? WorkItemPriority.MEDIUM,

                // --- ASSIGN TO COLUMN ID ---
                BoardColumnId = firstColumn.Id,

                StoryPoints = input.StoryPoints,
                ParentWorkItemId = input.ParentWorkItemId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await context.WorkItems.AddAsync(workItem);
            await context.SaveChangesAsync();

            await context.Entry(workItem).Reference(wi => wi.Reporter).LoadAsync();
            await context.Entry(workItem).Reference(wi => wi.BoardColumn).LoadAsync(); // Load column info for return

            return workItem;
        }

        [Authorize]
        public async Task<WorkItem> UpdateWorkItem(
            Guid workItemId,
            UpdateWorkItemInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            var workItem = await context.WorkItems.FindAsync(workItemId);

            if (workItem == null) throw new HotChocolate.GraphQLException("Work item not found.");

            // Basic security check (is member?)
            var spaceKey = workItem.Key.Split('-').First();
            var isMember = await context.SpaceMembers.AsNoTracking()
                .AnyAsync(m => m.UserId == userId && m.Space.Key == spaceKey);

            if (!isMember) throw new HotChocolate.GraphQLException(new Error("Not authorized.", "AUTH_NOT_MEMBER"));

            // --- Apply Updates ---
            if (input.Summary != null) workItem.Summary = input.Summary;
            if (input.MoveToBacklog == true)
            {
                workItem.SprintId = null; // Move to backlog
            }
            else if (input.SprintId.HasValue)
            {
                workItem.SprintId = input.SprintId.Value;
            }
            if (input.AssigneeId.HasValue) workItem.AssigneeId = input.AssigneeId.Value;
            if (input.Description != null) workItem.Description = input.Description;
            if (input.Priority.HasValue) workItem.Priority = input.Priority.Value;
            if (input.StoryPoints.HasValue) workItem.StoryPoints = input.StoryPoints.Value;
            if (input.Flagged.HasValue) workItem.Flagged = input.Flagged.Value;
            if (input.DueDate.HasValue) workItem.DueDate = input.DueDate.Value;

            // --- HANDLE COLUMN MOVE ---
            if (input.BoardColumnId.HasValue)
            {
                workItem.BoardColumnId = input.BoardColumnId.Value;
            }

            workItem.UpdatedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();

            await context.Entry(workItem).Reference(wi => wi.Reporter).LoadAsync();
            if (workItem.AssigneeId.HasValue) await context.Entry(workItem).Reference(wi => wi.Assignee).LoadAsync();
            await context.Entry(workItem).Reference(wi => wi.BoardColumn).LoadAsync();

            return workItem;
        }

        [Authorize]
        public async Task<List<BoardColumn>> MoveBoardColumnLeft(
    Guid columnId,
    [Service] AppDbContext context)
        {
            var column = await context.BoardColumns.FindAsync(columnId);
            if (column == null) throw new Exception("Column not found");

            var columns = await context.BoardColumns
                .Where(c => c.SpaceId == column.SpaceId)
                .OrderBy(c => c.Order)
                .ToListAsync();

            var index = columns.FindIndex(c => c.Id == columnId);

            if (index <= 0) return columns;

            var leftColumn = columns[index - 1];

            (column.Order, leftColumn.Order) = (leftColumn.Order, column.Order);

            await context.SaveChangesAsync();

            return columns;
        }

        [Authorize]
        public async Task<List<BoardColumn>> MoveBoardColumnRight(
    Guid columnId,
    [Service] AppDbContext context)
        {
            var column = await context.BoardColumns.FindAsync(columnId);
            if (column == null) throw new Exception("Column not found");

            var columns = await context.BoardColumns
                .Where(c => c.SpaceId == column.SpaceId)
                .OrderBy(c => c.Order)
                .ToListAsync();

            var index = columns.FindIndex(c => c.Id == columnId);

            if (index >= columns.Count - 1) return columns;

            var rightColumn = columns[index + 1];

            (column.Order, rightColumn.Order) = (rightColumn.Order, column.Order);

            await context.SaveChangesAsync();

            return columns;
        }

        [Authorize]
        public async Task<WorkItem> CreateSubtask(
            Guid parentWorkItemId,
            CreateWorkItemInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));

            var parentItem = await context.WorkItems
                .Include(wi => wi.Sprint)
                .FirstOrDefaultAsync(wi => wi.Id == parentWorkItemId);

            if (parentItem == null) throw new HotChocolate.GraphQLException("Parent work item not found.");

            var spaceKeyString = parentItem.Key.Split('-')[0];
            var space = await context.Spaces
                .Include(s => s.BoardColumns)
                .FirstOrDefaultAsync(s => s.Key == spaceKeyString);

            if (space == null) throw new Exception("Space data corruption.");

            // Find first column for subtask
            var firstColumn = space.BoardColumns.OrderBy(c => c.Order).FirstOrDefault();
            if (firstColumn == null) throw new Exception("No columns found.");

            var itemKey = await GetNextWorkItemKey(context, space.Id);

            var subtask = new WorkItem
            {
                Summary = input.Summary,
                Key = $"{space.Key}-{itemKey}",
                ReporterId = userId,
                SprintId = parentItem.SprintId,
                AssigneeId = input.AssigneeId,
                Description = input.Description,
                Priority = input.Priority ?? WorkItemPriority.MEDIUM,

                // Assign to Column
                BoardColumnId = firstColumn.Id,

                StoryPoints = input.StoryPoints,
                ParentWorkItemId = parentWorkItemId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            context.WorkItems.Add(subtask);
            await context.SaveChangesAsync();
            await context.Entry(subtask).Reference(wi => wi.Reporter).LoadAsync();

            return subtask;
        }

        // --- COMMENTS & COLUMNS ---

        [Authorize]
        public async Task<Comment> AddComment(
            Guid workItemId,
            string content,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            var workItem = await context.WorkItems.FindAsync(workItemId);
            if (workItem == null) throw new HotChocolate.GraphQLException("Work item not found.");

            var comment = new Comment
            {
                Content = content,
                WorkItemId = workItemId,
                AuthorId = userId,
                CreatedDate = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();
            await context.Entry(comment).Reference(c => c.Author).LoadAsync();

            return comment;
        }

        [Authorize]
        public async Task<BoardColumn> AddBoardColumn(
            string name,
            Guid spaceId,
            [Service] AppDbContext context)
        {
            var maxOrder = await context.BoardColumns
                .Where(c => c.SpaceId == spaceId)
                .MaxAsync(c => (int?)c.Order) ?? -1;

            var newColumn = new BoardColumn
            {
                Name = name,
                SpaceId = spaceId,
                Order = maxOrder + 1,
                IsSystem = false
            };

            context.BoardColumns.Add(newColumn);
            await context.SaveChangesAsync();
            return newColumn;
        }

        [Authorize]
        public async Task<bool> DeleteBoardColumn(
            Guid columnId,
            Guid targetColumnId,
            [Service] AppDbContext context)
        {
            var columnToDelete = await context.BoardColumns.FindAsync(columnId);
            if (columnToDelete == null) throw new GraphQLException("Column not found.");
            if (columnToDelete.IsSystem) throw new GraphQLException("Cannot delete default system columns.");

            var targetColumn = await context.BoardColumns.FindAsync(targetColumnId);
            if (targetColumn == null) throw new GraphQLException("Target column not found.");

            var tasksToMove = await context.WorkItems
                .Where(wi => wi.BoardColumnId == columnId)
                .ToListAsync();

            foreach (var task in tasksToMove)
            {
                task.BoardColumnId = targetColumnId;
            }

            context.BoardColumns.Remove(columnToDelete);
            await context.SaveChangesAsync();

            return true;
        }

        // --- HELPERS ---
        private async Task<int> GetNextWorkItemKey(AppDbContext context, Guid spaceId)
        {
            var spaceKey = (await context.Spaces.FindAsync(spaceId))?.Key;
            if (spaceKey == null) return 1;

            var latestItem = await context.WorkItems
                .AsNoTracking()
                .Where(wi => wi.Key.StartsWith(spaceKey + "-"))
                .OrderByDescending(wi => wi.CreatedDate)
                .FirstOrDefaultAsync();

            if (latestItem == null) return 1;

            var lastKeyNumber = int.Parse(latestItem.Key.Split('-').Last());
            return lastKeyNumber + 1;
        }
    }
}