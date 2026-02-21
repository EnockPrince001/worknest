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

            var cols = new List<BoardColumn>
            {
                new BoardColumn { Name = "TO DO", Order = 0, IsSystem = true, Space = space },
                new BoardColumn { Name = "IN PROGRESS", Order = 1, IsSystem = true, Space = space },
                new BoardColumn { Name = "DONE", Order = 2, IsSystem = true, Space = space }
            };

            await context.Spaces.AddAsync(space);
            await context.BoardColumns.AddRangeAsync(cols);

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
            var inviterMembership = await context.SpaceMembers.AsNoTracking()
                .FirstOrDefaultAsync(m => m.SpaceId == input.SpaceId && m.UserId == inviterId);

            if (inviterMembership == null || inviterMembership.Role != SpaceRole.ADMINISTRATOR)
                throw new GraphQLException("Unauthorized: Admin role required.");

            var invitee = await context.Users.FirstOrDefaultAsync(u => u.Email == input.Email);
            if (invitee == null) throw new GraphQLException("User not found.");

            var newMember = new SpaceMember { SpaceId = input.SpaceId, UserId = invitee.Id, Role = input.Role };
            await context.SpaceMembers.AddAsync(newMember);
            await context.SaveChangesAsync();

            return newMember;
        }

        // --- WORK ITEM MANAGEMENT (WITH AUDIT LOGGING) ---

        [Authorize]
        public async Task<WorkItem> CreateWorkItem(
            CreateWorkItemInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            var space = await context.Spaces.Include(s => s.BoardColumns).FirstOrDefaultAsync(s => s.Id == input.SpaceId);
            if (space == null) throw new GraphQLException("Space not found.");

            var firstColumn = space.BoardColumns.OrderBy(c => c.Order).FirstOrDefault();
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
                BoardColumnId = firstColumn.Id,
                StoryPoints = input.StoryPoints,
                ParentWorkItemId = input.ParentWorkItemId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await context.WorkItems.AddAsync(workItem);
            await context.SaveChangesAsync();
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
            if (workItem == null) throw new GraphQLException("Work item not found.");

            // Track changes for History/Activity Log
            void LogChange(string field, string? oldVal, string? newVal) {
                if (oldVal == newVal) return;
                context.Activities.Add(new Activity {
                    WorkItemId = workItemId,
                    Field = field,
                    OldValue = oldVal,
                    NewValue = newVal,
                    AuthorId = userId,
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (input.Summary != null) {
                LogChange("Summary", workItem.Summary, input.Summary);
                workItem.Summary = input.Summary;
            }

            if (input.Description != null) {
                LogChange("Description", workItem.Description, input.Description);
                workItem.Description = input.Description;
            }

            if (input.BoardColumnId.HasValue) {
                LogChange("Status", workItem.BoardColumnId.ToString(), input.BoardColumnId.Value.ToString());
                workItem.BoardColumnId = input.BoardColumnId.Value;
            }

            // Fix for Assignee (handling null/unassigned)
            if (input.AssigneeId != workItem.AssigneeId) {
                LogChange("Assignee", workItem.AssigneeId?.ToString(), input.AssigneeId?.ToString());
                workItem.AssigneeId = input.AssigneeId;
            }

            if (input.Priority.HasValue) {
                LogChange("Priority", workItem.Priority.ToString(), input.Priority.Value.ToString());
                workItem.Priority = input.Priority.Value;
            }

            workItem.UpdatedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();

            // Reload relationships for frontend
            await context.Entry(workItem).Reference(wi => wi.Assignee).LoadAsync();
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
                //.OrderBy(c => c.Order)
                .ToListAsync();
            columns = columns.OrderBy(c => c.Order).ThenBy(c => c.Id).ToList();

            for (int i = 0; i < columns.Count; i++)
            {
                columns[i].Order = i;
            }

            var index = columns.FindIndex(c => c.Id == columnId);

            //if (index <= 0) return columns;
            if (index > 0)
            {

                var leftColumn = columns[index - 1];

                (column.Order, leftColumn.Order) = (leftColumn.Order, column.Order);
            }
            await context.SaveChangesAsync();

            //return columns;
            return columns.OrderBy(c => c.Order).ToList();
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
                //.OrderBy(c => c.Order)

                .ToListAsync();
            columns = columns.OrderBy(c => c.Order).ThenBy(c => c.Id).ToList();

            for (int i = 0; i < columns.Count; i++)
            {
                columns[i].Order = i;
            }

            var index = columns.FindIndex(c => c.Id == columnId);

            //if (index >= columns.Count - 1) return columns;
            if (index < columns.Count - 1)
            {

                var rightColumn = columns[index + 1];

                (column.Order, rightColumn.Order) = (rightColumn.Order, column.Order);
            }
            await context.SaveChangesAsync();

            return columns.OrderBy( c => c.Order).ToList();
        }
        [Authorize]
        public async Task<List<WorkItem>> MoveWorkItemUp(
            Guid workItemId,
            [Service] AppDbContext context)
        {
            var item = await context.WorkItems.FindAsync(workItemId);
            if (item == null) throw new Exception("Work item not found");

            var items = await context.WorkItems
                .Where(w => w.BoardColumnId == item.BoardColumnId)
                .OrderBy(w => w.Order)
                .ThenBy(w => w.Id)
                .ToListAsync();

            var index = items.FindIndex(w => w.Id == workItemId);

            if (index > 0)
            {
                var above = items[index - 1];
                (item.Order, above.Order) = (above.Order, item.Order);
            }

            await context.SaveChangesAsync();

            return items.OrderBy(w => w.Order).ToList();
        }

        [Authorize]
        public async Task<List<WorkItem>> MoveWorkItemToTop(
     Guid workItemId,
     [Service] AppDbContext context)
        {
            var item = await context.WorkItems.FindAsync(workItemId);
            if (item == null) throw new Exception("Work item not found");

            var items = await context.WorkItems
                .Where(w => w.BoardColumnId == item.BoardColumnId)
                .OrderBy(w => w.Order)
                .ThenBy(w => w.Id)
                .ToListAsync();

            items.Remove(item);
            items.Insert(0, item);

            for (int i = 0; i < items.Count; i++)
                items[i].Order = i;

            await context.SaveChangesAsync();

            return items;
        }

        // --- SUBTASK & SPRINT HELPERS ---

        [Authorize]
        public async Task<WorkItem> CreateSubtask(
            Guid parentWorkItemId,
            CreateWorkItemInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = Guid.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
            var parentItem = await context.WorkItems.FindAsync(parentWorkItemId);
            if (parentItem == null) throw new GraphQLException("Parent not found.");

            var spaceKey = parentItem.Key.Split('-')[0];
            var space = await context.Spaces.Include(s => s.BoardColumns).FirstOrDefaultAsync(s => s.Key == spaceKey);

            var subtask = new WorkItem {
                Summary = input.Summary,
                Key = $"{space.Key}-{await GetNextWorkItemKey(context, space.Id)}",
                ReporterId = userId,
                ParentWorkItemId = parentWorkItemId,
                BoardColumnId = space.BoardColumns.OrderBy(c => c.Order).First().Id,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await context.WorkItems.AddAsync(subtask);
            await context.SaveChangesAsync();
            return subtask;
        }

        private async Task<int> GetNextWorkItemKey(AppDbContext context, Guid spaceId)
        {
            var count = await context.WorkItems.CountAsync(wi => wi.Key.Contains("-"));
            return count + 1;
        }
    }
}