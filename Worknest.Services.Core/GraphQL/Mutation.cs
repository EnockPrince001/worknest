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
        private Guid GetUserId(ClaimsPrincipal principal)
        {
            var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) throw new GraphQLException("User not authenticated.");
            return Guid.Parse(userIdStr);
        }

        // --- SPACE MANAGEMENT ---

        [Authorize]
        public async Task<Space> CreateSpace(
            CreateSpaceInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = GetUserId(claimsPrincipal);
            var owner = await context.Users.FindAsync(userId);
            if (owner == null) throw new Exception("User not found");

            var normalizedKey = input.Key.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(normalizedKey))
                throw new GraphQLException("Space key cannot be empty.");

            var keyExists = await context.Spaces.AnyAsync(s => s.Key == normalizedKey);
            if (keyExists)
                throw new GraphQLException("Space key already exists.");

            var space = new Space
            {
                Name = input.Name.Trim(),
                Key = normalizedKey,
                Type = input.Type,
                OwnerId = userId,
                Owner = owner
            };

            var cols = new List<BoardColumn>
            {
                new() { Name = "TO DO", Order = 0, IsSystem = true, SpaceId = space.Id },
                new() { Name = "IN PROGRESS", Order = 1, IsSystem = true, SpaceId = space.Id },
                new() { Name = "DONE", Order = 2, IsSystem = true, SpaceId = space.Id }
            };

            await context.Spaces.AddAsync(space);
            await context.BoardColumns.AddRangeAsync(cols);

            var member = new SpaceMember { Space = space, UserId = userId, Role = SpaceRole.ADMINISTRATOR };
            await context.SpaceMembers.AddAsync(member);
            await context.SaveChangesAsync();

            return space;
        }

        [Authorize]
        public async Task<Space> UpdateSpace(
            Guid spaceId,
            UpdateSpaceInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = GetUserId(claimsPrincipal);
            var membership = await context.SpaceMembers.AsNoTracking()
                .FirstOrDefaultAsync(m => m.SpaceId == spaceId && m.UserId == userId);

            if (membership == null || membership.Role != SpaceRole.ADMINISTRATOR)
                throw new GraphQLException("Unauthorized: Admin role required.");

            var space = await context.Spaces.FindAsync(spaceId);
            if (space == null)
                throw new GraphQLException("Space not found.");

            if (input.Name.HasValue)
            {
                var newName = input.Name.Value?.Trim();
                if (string.IsNullOrWhiteSpace(newName))
                    throw new GraphQLException("Space name cannot be empty.");
                space.Name = newName;
            }

            if (input.Key.HasValue)
            {
                var newKey = input.Key.Value?.Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(newKey))
                    throw new GraphQLException("Space key cannot be empty.");

                var keyExists = await context.Spaces.AnyAsync(s => s.Id != spaceId && s.Key == newKey);
                if (keyExists)
                    throw new GraphQLException("Space key already exists.");

                space.Key = newKey;
            }

            if (input.Type.HasValue && input.Type.Value.HasValue)
            {
                space.Type = input.Type.Value.Value;
            }

            await context.SaveChangesAsync();
            return space;
        }

        [Authorize]
        public async Task<SpaceMember> InviteUserToSpace(
            InviteUserInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var inviterId = GetUserId(claimsPrincipal);
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
            var userId = GetUserId(claimsPrincipal);

            var space = await context.Spaces
                .Include(s => s.BoardColumns)
                .FirstOrDefaultAsync(s => s.Id == input.SpaceId);

            if (space == null)
                throw new GraphQLException("Space not found.");

            var firstColumn = space.BoardColumns
                .OrderBy(c => c.Order)
                .FirstOrDefault();

            if (firstColumn == null)
                throw new GraphQLException("Space has no columns.");

            var itemKey = await GetNextWorkItemKey(context, space.Id);

            var targetColumnId = input.BoardColumnId ?? firstColumn.Id;

            var maxOrder = await context.WorkItems
                .Where(w => w.BoardColumnId == targetColumnId)
                .Select(w => (int?)w.Order)
                .MaxAsync() ?? -1;

            var workItem = new WorkItem
            {
                Summary = input.Summary,
                Key = $"{space.Key}-{itemKey}",
                ReporterId = userId,
                SprintId = input.SprintId,
                AssigneeId = input.AssigneeId,
                Description = input.Description,
                Type = input.Type ?? WorkItemType.TASK,
                Priority = input.Priority ?? WorkItemPriority.MEDIUM,
                BoardColumnId = targetColumnId,
                Order = maxOrder + 1,
                StoryPoints = input.StoryPoints,
                ParentWorkItemId = input.ParentWorkItemId,
                SpaceId = space.Id,
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
            var userId = GetUserId(claimsPrincipal);
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

            if (input.Summary.HasValue) {
                var newVal = input.Summary.Value;
                LogChange("Summary", workItem.Summary, newVal);
                workItem.Summary = newVal ?? string.Empty; // Summary is required in DB
            }

            if (input.Description.HasValue) {
                var newVal = input.Description.Value;
                LogChange("Description", workItem.Description, newVal);
                workItem.Description = newVal;
            }

            if (input.BoardColumnId.HasValue)
            {
                var newVal = input.BoardColumnId.Value;

                // Get the column being moved to
                var column = await context.BoardColumns.FindAsync(newVal);

                LogChange("Status", workItem.BoardColumnId?.ToString(), newVal?.ToString());

                workItem.BoardColumnId = newVal;

                // ✅ NEW LOGIC: Auto mark as completed if moved to DONE
                if (column != null && column.Name.ToUpper() == "DONE")
                {
                    workItem.IsCompleted = true;
                    workItem.CompletedAt = DateTime.UtcNow;
                }
                else
                {
                    workItem.IsCompleted = false;
                    workItem.CompletedAt = null;
                }
            }

            if (input.SprintId.HasValue) {
                var newVal = input.SprintId.Value;
                LogChange("Sprint", workItem.SprintId?.ToString(), newVal?.ToString());
                workItem.SprintId = newVal;
            }

            if (input.MoveToBacklog.HasValue && input.MoveToBacklog.Value == true) {
                LogChange("Sprint", workItem.SprintId?.ToString(), "Backlog");
                workItem.SprintId = null;
            }

            if (input.AssigneeId.HasValue) {
                var newVal = input.AssigneeId.Value;
                LogChange("Assignee", workItem.AssigneeId?.ToString(), newVal?.ToString());
                workItem.AssigneeId = newVal;
            }

            if (input.Priority.HasValue) {
                var newVal = input.Priority.Value;
                if (newVal.HasValue) {
                    LogChange("Priority", workItem.Priority.ToString(), newVal.Value.ToString());
                    workItem.Priority = newVal.Value;
                }
            }

            if (input.StoryPoints.HasValue) {
                var newVal = input.StoryPoints.Value;
                LogChange("Story Points", workItem.StoryPoints?.ToString(), newVal?.ToString());
                workItem.StoryPoints = newVal;
            }

            if (input.Flagged.HasValue) {
                var newVal = input.Flagged.Value;
                if (newVal.HasValue) {
                    LogChange("Flagged", workItem.Flagged.ToString(), newVal.Value.ToString());
                    workItem.Flagged = newVal.Value;
                }
            }

            if (input.DueDate.HasValue) {
                var newVal = input.DueDate.Value;
                LogChange("Due Date", workItem.DueDate?.ToString(), newVal?.ToString());
                workItem.DueDate = newVal;
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
        public async Task<BoardColumn> AddBoardColumn(
            string name,
            Guid spaceId,
            [Service] AppDbContext context)
        {
            var maxOrder = await context.BoardColumns
                .Where(c => c.SpaceId == spaceId)
                .Select(c => (int?)c.Order)
                .MaxAsync() ?? -1;

            var column = new BoardColumn
            {
                Name = name,
                SpaceId = spaceId,
                Order = maxOrder + 1,
                IsSystem = false
            };

            await context.BoardColumns.AddAsync(column);
            await context.SaveChangesAsync();
            return column;
        }

        [Authorize]
        public async Task<WorkItemComment> AddWorkItemComment(
            Guid workItemId,
            string commentText,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            return await AddComment(workItemId, commentText, context, claimsPrincipal);
        }

        [Authorize]
        public async Task<WorkItemComment> AddComment(
            Guid workItemId,
            string content,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = GetUserId(claimsPrincipal);
            if (string.IsNullOrWhiteSpace(content))
                throw new GraphQLException("Comment cannot be empty.");

            var workItemExists = await context.WorkItems.AnyAsync(w => w.Id == workItemId);
            if (!workItemExists)
                throw new GraphQLException("Work item not found.");

            var comment = new WorkItemComment
            {
                Id = Guid.NewGuid(),
                WorkItemId = workItemId,
                CommentText = content.Trim(),
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await context.WorkItemComments.AddAsync(comment);

            await context.SaveChangesAsync();
            await context.Entry(comment).Reference(c => c.Author).LoadAsync();

            return comment;
        }

        [Authorize]
        public async Task<WorkItemComment> UpdateWorkItemComment(
    Guid commentId,
    string commentText,
    [Service] AppDbContext context,
    ClaimsPrincipal claimsPrincipal)
        {
            var userId = GetUserId(claimsPrincipal);
            var comment = await context.WorkItemComments.FindAsync(commentId);

            if (comment == null)
                throw new GraphQLException("Comment not found.");

            if (comment.CreatedBy.HasValue && comment.CreatedBy.Value != userId)
                throw new GraphQLException("Unauthorized to edit this comment.");

            if (string.IsNullOrWhiteSpace(commentText))
                throw new GraphQLException("Comment cannot be empty.");

            comment.CommentText = commentText.Trim();

            await context.SaveChangesAsync();
            await context.Entry(comment).Reference(c => c.Author).LoadAsync();

            return comment;
        }

        [Authorize]
        public async Task<bool> DeleteWorkItemComment(
            Guid commentId,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = GetUserId(claimsPrincipal);
            var comment = await context.WorkItemComments.FindAsync(commentId);

            if (comment == null)
                throw new GraphQLException("Comment not found.");

            if (comment.CreatedBy.HasValue && comment.CreatedBy.Value != userId)
                throw new GraphQLException("Unauthorized to delete this comment.");

            context.WorkItemComments.Remove(comment);

            await context.SaveChangesAsync();

            return true;
        }

        [Authorize]
        public async Task<List<WorkItem>> MoveWorkItem(
    Guid workItemId,
    Guid targetColumnId,
    int targetIndex,
    [Service] AppDbContext context)
        {
            var item = await context.WorkItems.FindAsync(workItemId);
            if (item == null)
                throw new GraphQLException("Work item not found");

            // 🔹 Get ALL items in target column
            var targetItems = await context.WorkItems
                .Where(w => w.BoardColumnId == targetColumnId && w.Id != workItemId)
                .OrderBy(w => w.Order)
                .ThenBy(w => w.Id)
                .ToListAsync();

            // 🔹 Remove item from old column list
            var sourceItems = await context.WorkItems
                .Where(w => w.BoardColumnId == item.BoardColumnId && w.Id != workItemId)
                .OrderBy(w => w.Order)
                .ToListAsync();

            // 🔹 Reorder source column
            for (int i = 0; i < sourceItems.Count; i++)
            {
                sourceItems[i].Order = i;
            }

            // 🔹 Move item to new column
            item.BoardColumnId = targetColumnId;

            // 🔹 Insert into target position
            if (targetIndex < 0) targetIndex = 0;
            if (targetIndex > targetItems.Count) targetIndex = targetItems.Count;

            targetItems.Insert(targetIndex, item);

            // 🔹 Reorder target column
            for (int i = 0; i < targetItems.Count; i++)
            {
                targetItems[i].Order = i;
            }

            await context.SaveChangesAsync();

            return targetItems;
        }

        [Authorize]
        public async Task<bool> DeleteBoardColumn(
            Guid columnId,
            Guid targetColumnId,
            [Service] AppDbContext context)
        {
            var column = await context.BoardColumns.FindAsync(columnId);
            if (column == null) throw new GraphQLException("Column not found.");
            if (column.IsSystem) throw new GraphQLException("Cannot delete system columns.");

            var targetColumn = await context.BoardColumns.FindAsync(targetColumnId);
            if (targetColumn == null) throw new GraphQLException("Target column not found.");

            // Move items
            var itemsToMove = await context.WorkItems
                .Where(wi => wi.BoardColumnId == columnId)
                .ToListAsync();

            foreach (var item in itemsToMove)
            {
                item.BoardColumnId = targetColumnId;
            }

            context.BoardColumns.Remove(column);
            await context.SaveChangesAsync();

            // Re-order remaining columns
            var columns = await context.BoardColumns
                .Where(c => c.SpaceId == column.SpaceId)
                .OrderBy(c => c.Order)
                .ToListAsync();

            for (int i = 0; i < columns.Count; i++)
            {
                columns[i].Order = i;
            }

            await context.SaveChangesAsync();
            return true;
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
        public async Task<bool> DeleteWorkItem(
    Guid workItemId,
    [Service] AppDbContext context)
        {
            var workItem = await context.WorkItems.FindAsync(workItemId);

            if (workItem == null)
                throw new GraphQLException("Work item not found.");

            context.WorkItems.Remove(workItem);

            await context.SaveChangesAsync();

            return true;
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
            {
                var workItemToUpdate = items[i];
                if (workItemToUpdate != null)
                {
                    workItemToUpdate.Order = i;
                }
            }

            await context.SaveChangesAsync();

            return items;
        }

        // --- SPRINT MANAGEMENT ---

        [Authorize]
        public async Task<Sprint> CreateSprint(
            CreateSprintInput input,
            [Service] AppDbContext context)
        {
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
        public async Task<Sprint> UpdateSprint(
            Guid sprintId,
            UpdateSprintInput input,
            [Service] AppDbContext context)
        {
            var sprint = await context.Sprints.FindAsync(sprintId);
            if (sprint == null) throw new GraphQLException("Sprint not found.");

            if (input.Name.HasValue) sprint.Name = input.Name.Value ?? sprint.Name;
            if (input.Goal.HasValue) sprint.Goal = input.Goal.Value;
            if (input.StartDate.HasValue) sprint.StartDate = input.StartDate.Value;
            if (input.EndDate.HasValue) sprint.EndDate = input.EndDate.Value;
            if (input.Duration.HasValue) sprint.Duration = input.Duration.Value;
            if (input.Status.HasValue && input.Status.Value.HasValue) sprint.Status = input.Status.Value.Value;

            await context.SaveChangesAsync();
            return sprint;
        }

        [Authorize]
        public async Task<Sprint> DeleteSprint(
            Guid sprintId,
            [Service] AppDbContext context)
        {
            var sprint = await context.Sprints.FindAsync(sprintId);
            if (sprint == null) throw new GraphQLException("Sprint not found.");

            context.Sprints.Remove(sprint);
            await context.SaveChangesAsync();
            return sprint;
        }

        [Authorize]
        public async Task<Sprint> StartSprint(
            Guid sprintId,
            [Service] AppDbContext context)
        {
            var sprint = await context.Sprints.FindAsync(sprintId);
            if (sprint == null) throw new GraphQLException("Sprint not found.");

            sprint.Status = SprintStatus.ACTIVE;
            sprint.StartDate = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return sprint;
        }

        [Authorize]
        public async Task<Sprint> CompleteSprint(
            Guid sprintId,
            [Service] AppDbContext context)
        {
            var sprint = await context.Sprints.FindAsync(sprintId);
            if (sprint == null) throw new GraphQLException("Sprint not found.");

            sprint.Status = SprintStatus.COMPLETED;
            sprint.EndDate = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return sprint;
        }

        // --- SUBTASK & SPRINT HELPERS ---

        [Authorize]
        public async Task<WorkItem> CreateSubtask(
            Guid parentWorkItemId,
            CreateWorkItemInput input,
            [Service] AppDbContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var userId = GetUserId(claimsPrincipal);
            var parentItem = await context.WorkItems.FindAsync(parentWorkItemId);
            if (parentItem == null) throw new GraphQLException("Parent not found.");

            if (!parentItem.SpaceId.HasValue)
                throw new GraphQLException("Parent item is not associated with a space.");

            var space = await context.Spaces
                .Include(s => s.BoardColumns)
                .FirstOrDefaultAsync(s => s.Id == parentItem.SpaceId.Value);
            if (space == null) throw new GraphQLException("Space associated with parent item not found.");

            var firstColumn = space.BoardColumns.OrderBy(c => c.Order).FirstOrDefault();
            if (firstColumn == null) throw new GraphQLException("Space has no columns.");

            var subtask = new WorkItem {
                Summary = input.Summary,
                Key = $"{space.Key}-{await GetNextWorkItemKey(context, space.Id)}",
                ReporterId = userId,
                ParentWorkItemId = parentWorkItemId,
                BoardColumnId = firstColumn.Id,
                SpaceId = space.Id,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await context.WorkItems.AddAsync(subtask);
            await context.SaveChangesAsync();
            return subtask;
        }

        private async Task<int> GetNextWorkItemKey(AppDbContext context, Guid spaceId)
        {
            var spaceKey = await context.Spaces
                .Where(s => s.Id == spaceId)
                .Select(s => s.Key)
                .SingleAsync();

            var prefix = $"{spaceKey}-";
            var keys = await context.WorkItems
                .Where(wi => wi.SpaceId == spaceId && wi.Key.StartsWith(prefix))
                .Select(wi => wi.Key)
                .ToListAsync();

            var max = 0;
            foreach (var key in keys)
            {
                if (key.Length <= prefix.Length) continue;

                var suffix = key.Substring(prefix.Length);
                if (int.TryParse(suffix, out var parsed) && parsed > max)
                {
                    max = parsed;
                }
            }

            return max + 1;
        }
    }
}
