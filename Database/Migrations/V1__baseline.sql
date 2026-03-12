-- =============================================================
-- Flyway Baseline Migration - WorknestDB
-- Generated: 2026-03-12 13:11:04
-- This snapshot captures the existing schema without changes.
-- =============================================================

-- Table: [dbo].[__EFMigrationsHistory]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory] (
        [MigrationId] NVARCHAR(150) NOT NULL,
        [ProductVersion] NVARCHAR(32) NOT NULL
    );
END
GO

-- Table: [dbo].[Activities]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Activities' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Activities] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [WorkItemId] UNIQUEIDENTIFIER NOT NULL,
        [Field] NVARCHAR(MAX) NOT NULL,
        [OldValue] NVARCHAR(MAX) NULL,
        [NewValue] NVARCHAR(MAX) NULL,
        [AuthorId] UNIQUEIDENTIFIER NOT NULL,
        [CreatedDate] DATETIME2(7) NOT NULL
    );
END
GO

-- Table: [dbo].[AspNetRoleClaims]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoleClaims' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RoleId] UNIQUEIDENTIFIER NOT NULL,
        [ClaimType] NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL
    );
END
GO

-- Table: [dbo].[AspNetRoles]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetRoles' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[AspNetRoles] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(256) NULL,
        [NormalizedName] NVARCHAR(256) NULL,
        [ConcurrencyStamp] NVARCHAR(MAX) NULL
    );
END
GO

-- Table: [dbo].[AspNetUserClaims]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserClaims' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [ClaimType] NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL
    );
END
GO

-- Table: [dbo].[AspNetUserLogins]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserLogins' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins] (
        [LoginProvider] NVARCHAR(450) NOT NULL,
        [ProviderKey] NVARCHAR(450) NOT NULL,
        [ProviderDisplayName] NVARCHAR(MAX) NULL,
        [UserId] UNIQUEIDENTIFIER NOT NULL
    );
END
GO

-- Table: [dbo].[AspNetUserRoles]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserRoles' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles] (
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [RoleId] UNIQUEIDENTIFIER NOT NULL
    );
END
GO

-- Table: [dbo].[AspNetUsers]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUsers' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[AspNetUsers] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [FullName] NVARCHAR(MAX) NULL,
        [UserName] NVARCHAR(256) NULL,
        [NormalizedUserName] NVARCHAR(256) NULL,
        [Email] NVARCHAR(256) NULL,
        [NormalizedEmail] NVARCHAR(256) NULL,
        [EmailConfirmed] BIT NOT NULL,
        [PasswordHash] NVARCHAR(MAX) NULL,
        [SecurityStamp] NVARCHAR(MAX) NULL,
        [ConcurrencyStamp] NVARCHAR(MAX) NULL,
        [PhoneNumber] NVARCHAR(MAX) NULL,
        [PhoneNumberConfirmed] BIT NOT NULL,
        [TwoFactorEnabled] BIT NOT NULL,
        [LockoutEnd] DATETIMEOFFSET(7) NULL,
        [LockoutEnabled] BIT NOT NULL,
        [AccessFailedCount] INT NOT NULL,
        [JobTitle] NVARCHAR(MAX) NULL
    );
END
GO

-- Table: [dbo].[AspNetUserTokens]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUserTokens' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens] (
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [LoginProvider] NVARCHAR(450) NOT NULL,
        [Name] NVARCHAR(450) NOT NULL,
        [Value] NVARCHAR(MAX) NULL
    );
END
GO

-- Table: [dbo].[BoardColumns]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BoardColumns' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[BoardColumns] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(MAX) NOT NULL,
        [Order] INT NOT NULL,
        [IsSystem] BIT NOT NULL,
        [SpaceId] UNIQUEIDENTIFIER NOT NULL
    );
END
GO

-- Table: [dbo].[Comments]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Comments' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Comments] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Content] NVARCHAR(MAX) NOT NULL,
        [CreatedDate] DATETIME2(7) NOT NULL,
        [WorkItemId] UNIQUEIDENTIFIER NOT NULL,
        [AuthorId] UNIQUEIDENTIFIER NOT NULL
    );
END
GO

-- Table: [dbo].[SpaceMembers]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SpaceMembers' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[SpaceMembers] (
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [SpaceId] UNIQUEIDENTIFIER NOT NULL,
        [Role] NVARCHAR(MAX) NOT NULL
    );
END
GO

-- Table: [dbo].[Spaces]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Spaces' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Spaces] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(MAX) NOT NULL,
        [Key] NVARCHAR(MAX) NOT NULL,
        [Type] NVARCHAR(MAX) NOT NULL,
        [OwnerId] UNIQUEIDENTIFIER NOT NULL
    );
END
GO

-- Table: [dbo].[Sprints]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Sprints' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[Sprints] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Name] NVARCHAR(MAX) NOT NULL,
        [Goal] NVARCHAR(MAX) NULL,
        [StartDate] DATETIME2(7) NULL,
        [EndDate] DATETIME2(7) NULL,
        [Duration] NVARCHAR(MAX) NULL,
        [Status] NVARCHAR(MAX) NOT NULL,
        [SpaceId] UNIQUEIDENTIFIER NOT NULL
    );
END
GO

-- Table: [dbo].[WorkItemLinks]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WorkItemLinks' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[WorkItemLinks] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Type] NVARCHAR(MAX) NOT NULL,
        [SourceId] UNIQUEIDENTIFIER NOT NULL,
        [TargetId] UNIQUEIDENTIFIER NOT NULL
    );
END
GO

-- Table: [dbo].[WorkItems]
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WorkItems' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[WorkItems] (
        [Id] UNIQUEIDENTIFIER NOT NULL,
        [Key] NVARCHAR(MAX) NOT NULL,
        [Summary] NVARCHAR(MAX) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [Priority] NVARCHAR(MAX) NOT NULL,
        [StoryPoints] INT NULL,
        [DueDate] DATETIME2(7) NULL,
        [CreatedDate] DATETIME2(7) NOT NULL,
        [UpdatedDate] DATETIME2(7) NOT NULL,
        [Flagged] BIT NOT NULL,
        [ReporterId] UNIQUEIDENTIFIER NOT NULL,
        [BoardColumnId] UNIQUEIDENTIFIER NULL,
        [AssigneeId] UNIQUEIDENTIFIER NULL,
        [SprintId] UNIQUEIDENTIFIER NULL,
        [ParentWorkItemId] UNIQUEIDENTIFIER NULL,
        [EpicId] UNIQUEIDENTIFIER NULL,
        [SpaceId] UNIQUEIDENTIFIER NULL DEFAULT ('00000000-0000-0000-0000-000000000000'),
        [Order] INT NOT NULL DEFAULT ((0)),
        [Type] INT NOT NULL DEFAULT ((0))
    );
END
GO

-- =============================================================
-- PRIMARY KEY CONSTRAINTS
-- =============================================================

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_WorkItemLinks')
    ALTER TABLE [dbo].[WorkItemLinks] ADD CONSTRAINT [PK_WorkItemLinks] PRIMARY KEY CLUSTERED ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_Activities')
    ALTER TABLE [dbo].[Activities] ADD CONSTRAINT [PK_Activities] PRIMARY KEY CLUSTERED ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK___EFMigrationsHistory')
    ALTER TABLE [dbo].[__EFMigrationsHistory] ADD CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId]);
GO

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_AspNetRoles')
    ALTER TABLE [dbo].[AspNetRoles] ADD CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_AspNetUsers')
    ALTER TABLE [dbo].[AspNetUsers] ADD CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_AspNetRoleClaims')
    ALTER TABLE [dbo].[AspNetRoleClaims] ADD CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_AspNetUserClaims')
    ALTER TABLE [dbo].[AspNetUserClaims] ADD CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_AspNetUserLogins')
    ALTER TABLE [dbo].[AspNetUserLogins] ADD CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider], [ProviderKey]);
GO

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_AspNetUserRoles')
    ALTER TABLE [dbo].[AspNetUserRoles] ADD CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId], [RoleId]);
GO

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_AspNetUserTokens')
    ALTER TABLE [dbo].[AspNetUserTokens] ADD CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED ([UserId], [LoginProvider], [Name]);
GO

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_Spaces')
    ALTER TABLE [dbo].[Spaces] ADD CONSTRAINT [PK_Spaces] PRIMARY KEY CLUSTERED ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_BoardColumns')
    ALTER TABLE [dbo].[BoardColumns] ADD CONSTRAINT [PK_BoardColumns] PRIMARY KEY CLUSTERED ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_SpaceMembers')
    ALTER TABLE [dbo].[SpaceMembers] ADD CONSTRAINT [PK_SpaceMembers] PRIMARY KEY CLUSTERED ([SpaceId], [UserId]);
GO

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_Sprints')
    ALTER TABLE [dbo].[Sprints] ADD CONSTRAINT [PK_Sprints] PRIMARY KEY CLUSTERED ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_WorkItems')
    ALTER TABLE [dbo].[WorkItems] ADD CONSTRAINT [PK_WorkItems] PRIMARY KEY CLUSTERED ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_Comments')
    ALTER TABLE [dbo].[Comments] ADD CONSTRAINT [PK_Comments] PRIMARY KEY CLUSTERED ([Id]);
GO

-- =============================================================
-- FOREIGN KEY CONSTRAINTS
-- =============================================================

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_WorkItemLinks_WorkItems_SourceId')
    ALTER TABLE [dbo].[WorkItemLinks] ADD CONSTRAINT [FK_WorkItemLinks_WorkItems_SourceId] FOREIGN KEY ([SourceId]) REFERENCES [dbo].[WorkItems] ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_WorkItemLinks_WorkItems_TargetId')
    ALTER TABLE [dbo].[WorkItemLinks] ADD CONSTRAINT [FK_WorkItemLinks_WorkItems_TargetId] FOREIGN KEY ([TargetId]) REFERENCES [dbo].[WorkItems] ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_WorkItems_WorkItems_EpicId')
    ALTER TABLE [dbo].[WorkItems] ADD CONSTRAINT [FK_WorkItems_WorkItems_EpicId] FOREIGN KEY ([EpicId]) REFERENCES [dbo].[WorkItems] ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_WorkItems_Spaces_SpaceId')
    ALTER TABLE [dbo].[WorkItems] ADD CONSTRAINT [FK_WorkItems_Spaces_SpaceId] FOREIGN KEY ([SpaceId]) REFERENCES [dbo].[Spaces] ([Id]) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Activities_AspNetUsers_AuthorId')
    ALTER TABLE [dbo].[Activities] ADD CONSTRAINT [FK_Activities_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [dbo].[AspNetUsers] ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Activities_WorkItems_WorkItemId')
    ALTER TABLE [dbo].[Activities] ADD CONSTRAINT [FK_Activities_WorkItems_WorkItemId] FOREIGN KEY ([WorkItemId]) REFERENCES [dbo].[WorkItems] ([Id]) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AspNetRoleClaims_AspNetRoles_RoleId')
    ALTER TABLE [dbo].[AspNetRoleClaims] ADD CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AspNetUserClaims_AspNetUsers_UserId')
    ALTER TABLE [dbo].[AspNetUserClaims] ADD CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AspNetUserLogins_AspNetUsers_UserId')
    ALTER TABLE [dbo].[AspNetUserLogins] ADD CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AspNetUserRoles_AspNetRoles_RoleId')
    ALTER TABLE [dbo].[AspNetUserRoles] ADD CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AspNetUserRoles_AspNetUsers_UserId')
    ALTER TABLE [dbo].[AspNetUserRoles] ADD CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AspNetUserTokens_AspNetUsers_UserId')
    ALTER TABLE [dbo].[AspNetUserTokens] ADD CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Spaces_AspNetUsers_OwnerId')
    ALTER TABLE [dbo].[Spaces] ADD CONSTRAINT [FK_Spaces_AspNetUsers_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[AspNetUsers] ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_BoardColumns_Spaces_SpaceId')
    ALTER TABLE [dbo].[BoardColumns] ADD CONSTRAINT [FK_BoardColumns_Spaces_SpaceId] FOREIGN KEY ([SpaceId]) REFERENCES [dbo].[Spaces] ([Id]) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SpaceMembers_AspNetUsers_UserId')
    ALTER TABLE [dbo].[SpaceMembers] ADD CONSTRAINT [FK_SpaceMembers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SpaceMembers_Spaces_SpaceId')
    ALTER TABLE [dbo].[SpaceMembers] ADD CONSTRAINT [FK_SpaceMembers_Spaces_SpaceId] FOREIGN KEY ([SpaceId]) REFERENCES [dbo].[Spaces] ([Id]) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Sprints_Spaces_SpaceId')
    ALTER TABLE [dbo].[Sprints] ADD CONSTRAINT [FK_Sprints_Spaces_SpaceId] FOREIGN KEY ([SpaceId]) REFERENCES [dbo].[Spaces] ([Id]) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_WorkItems_AspNetUsers_AssigneeId')
    ALTER TABLE [dbo].[WorkItems] ADD CONSTRAINT [FK_WorkItems_AspNetUsers_AssigneeId] FOREIGN KEY ([AssigneeId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE SET NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_WorkItems_AspNetUsers_ReporterId')
    ALTER TABLE [dbo].[WorkItems] ADD CONSTRAINT [FK_WorkItems_AspNetUsers_ReporterId] FOREIGN KEY ([ReporterId]) REFERENCES [dbo].[AspNetUsers] ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_WorkItems_BoardColumns_BoardColumnId')
    ALTER TABLE [dbo].[WorkItems] ADD CONSTRAINT [FK_WorkItems_BoardColumns_BoardColumnId] FOREIGN KEY ([BoardColumnId]) REFERENCES [dbo].[BoardColumns] ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_WorkItems_Sprints_SprintId')
    ALTER TABLE [dbo].[WorkItems] ADD CONSTRAINT [FK_WorkItems_Sprints_SprintId] FOREIGN KEY ([SprintId]) REFERENCES [dbo].[Sprints] ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_WorkItems_WorkItems_ParentWorkItemId')
    ALTER TABLE [dbo].[WorkItems] ADD CONSTRAINT [FK_WorkItems_WorkItems_ParentWorkItemId] FOREIGN KEY ([ParentWorkItemId]) REFERENCES [dbo].[WorkItems] ([Id]);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Comments_AspNetUsers_AuthorId')
    ALTER TABLE [dbo].[Comments] ADD CONSTRAINT [FK_Comments_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Comments_WorkItems_WorkItemId')
    ALTER TABLE [dbo].[Comments] ADD CONSTRAINT [FK_Comments_WorkItems_WorkItemId] FOREIGN KEY ([WorkItemId]) REFERENCES [dbo].[WorkItems] ([Id]) ON DELETE CASCADE;
GO

-- =============================================================
-- INDEXES (non-PK)
-- =============================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkItemLinks_SourceId' AND object_id = OBJECT_ID('dbo.WorkItemLinks'))
    CREATE NONCLUSTERED INDEX [IX_WorkItemLinks_SourceId] ON [dbo].[WorkItemLinks] ([SourceId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkItemLinks_TargetId' AND object_id = OBJECT_ID('dbo.WorkItemLinks'))
    CREATE NONCLUSTERED INDEX [IX_WorkItemLinks_TargetId] ON [dbo].[WorkItemLinks] ([TargetId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Activities_AuthorId' AND object_id = OBJECT_ID('dbo.Activities'))
    CREATE NONCLUSTERED INDEX [IX_Activities_AuthorId] ON [dbo].[Activities] ([AuthorId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Activities_WorkItemId' AND object_id = OBJECT_ID('dbo.Activities'))
    CREATE NONCLUSTERED INDEX [IX_Activities_WorkItemId] ON [dbo].[Activities] ([WorkItemId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'RoleNameIndex' AND object_id = OBJECT_ID('dbo.AspNetRoles'))
    CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles] ([NormalizedName]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'EmailIndex' AND object_id = OBJECT_ID('dbo.AspNetUsers'))
    CREATE NONCLUSTERED INDEX [EmailIndex] ON [dbo].[AspNetUsers] ([NormalizedEmail]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UserNameIndex' AND object_id = OBJECT_ID('dbo.AspNetUsers'))
    CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers] ([NormalizedUserName]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetRoleClaims_RoleId' AND object_id = OBJECT_ID('dbo.AspNetRoleClaims'))
    CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims] ([RoleId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUserClaims_UserId' AND object_id = OBJECT_ID('dbo.AspNetUserClaims'))
    CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims] ([UserId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUserLogins_UserId' AND object_id = OBJECT_ID('dbo.AspNetUserLogins'))
    CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins] ([UserId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUserRoles_RoleId' AND object_id = OBJECT_ID('dbo.AspNetUserRoles'))
    CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles] ([RoleId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Spaces_OwnerId' AND object_id = OBJECT_ID('dbo.Spaces'))
    CREATE NONCLUSTERED INDEX [IX_Spaces_OwnerId] ON [dbo].[Spaces] ([OwnerId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BoardColumns_SpaceId' AND object_id = OBJECT_ID('dbo.BoardColumns'))
    CREATE NONCLUSTERED INDEX [IX_BoardColumns_SpaceId] ON [dbo].[BoardColumns] ([SpaceId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SpaceMembers_UserId' AND object_id = OBJECT_ID('dbo.SpaceMembers'))
    CREATE NONCLUSTERED INDEX [IX_SpaceMembers_UserId] ON [dbo].[SpaceMembers] ([UserId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Sprints_SpaceId' AND object_id = OBJECT_ID('dbo.Sprints'))
    CREATE NONCLUSTERED INDEX [IX_Sprints_SpaceId] ON [dbo].[Sprints] ([SpaceId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkItems_AssigneeId' AND object_id = OBJECT_ID('dbo.WorkItems'))
    CREATE NONCLUSTERED INDEX [IX_WorkItems_AssigneeId] ON [dbo].[WorkItems] ([AssigneeId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkItems_BoardColumnId' AND object_id = OBJECT_ID('dbo.WorkItems'))
    CREATE NONCLUSTERED INDEX [IX_WorkItems_BoardColumnId] ON [dbo].[WorkItems] ([BoardColumnId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkItems_ParentWorkItemId' AND object_id = OBJECT_ID('dbo.WorkItems'))
    CREATE NONCLUSTERED INDEX [IX_WorkItems_ParentWorkItemId] ON [dbo].[WorkItems] ([ParentWorkItemId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkItems_ReporterId' AND object_id = OBJECT_ID('dbo.WorkItems'))
    CREATE NONCLUSTERED INDEX [IX_WorkItems_ReporterId] ON [dbo].[WorkItems] ([ReporterId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkItems_SprintId' AND object_id = OBJECT_ID('dbo.WorkItems'))
    CREATE NONCLUSTERED INDEX [IX_WorkItems_SprintId] ON [dbo].[WorkItems] ([SprintId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkItems_EpicId' AND object_id = OBJECT_ID('dbo.WorkItems'))
    CREATE NONCLUSTERED INDEX [IX_WorkItems_EpicId] ON [dbo].[WorkItems] ([EpicId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkItems_SpaceId' AND object_id = OBJECT_ID('dbo.WorkItems'))
    CREATE NONCLUSTERED INDEX [IX_WorkItems_SpaceId] ON [dbo].[WorkItems] ([SpaceId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Comments_AuthorId' AND object_id = OBJECT_ID('dbo.Comments'))
    CREATE NONCLUSTERED INDEX [IX_Comments_AuthorId] ON [dbo].[Comments] ([AuthorId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Comments_WorkItemId' AND object_id = OBJECT_ID('dbo.Comments'))
    CREATE NONCLUSTERED INDEX [IX_Comments_WorkItemId] ON [dbo].[Comments] ([WorkItemId]);
GO

