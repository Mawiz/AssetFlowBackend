-- Phase 5 Preventive Maintenance — manual migration (review before production).
-- DO NOT run from automated tooling; backup database first.

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MaintenanceTypes')
BEGIN
    CREATE TABLE MaintenanceTypes (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId INT NULL,
        Name NVARCHAR(200) NOT NULL,
        Code NVARCHAR(50) NOT NULL,
        Description NVARCHAR(1000) NULL,
        SortOrder INT NOT NULL DEFAULT 0,
        CreatedOn DATETIME2 NOT NULL,
        CreatedById INT NULL,
        ModifiedOn DATETIME2 NULL,
        ModifiedById INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
    CREATE UNIQUE INDEX IX_MaintenanceTypes_TenantId_Code ON MaintenanceTypes(TenantId, Code) WHERE TenantId IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MaintenanceChecklists')
BEGIN
    CREATE TABLE MaintenanceChecklists (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId INT NULL,
        Name NVARCHAR(200) NOT NULL,
        Code NVARCHAR(50) NOT NULL,
        Description NVARCHAR(1000) NULL,
        MaintenanceTypeId INT NULL,
        Version INT NOT NULL DEFAULT 1,
        CreatedOn DATETIME2 NOT NULL,
        CreatedById INT NULL,
        ModifiedOn DATETIME2 NULL,
        ModifiedById INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_MaintenanceChecklists_MaintenanceTypes FOREIGN KEY (MaintenanceTypeId) REFERENCES MaintenanceTypes(Id)
    );
    CREATE UNIQUE INDEX IX_MaintenanceChecklists_TenantId_Code ON MaintenanceChecklists(TenantId, Code) WHERE TenantId IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MaintenanceChecklistItems')
BEGIN
    CREATE TABLE MaintenanceChecklistItems (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaintenanceChecklistId INT NOT NULL,
        ItemText NVARCHAR(500) NOT NULL,
        Description NVARCHAR(1000) NULL,
        ResponseType INT NOT NULL,
        IsRequired BIT NOT NULL DEFAULT 1,
        SortOrder INT NOT NULL DEFAULT 0,
        CreatedOn DATETIME2 NOT NULL,
        CreatedById INT NULL,
        ModifiedOn DATETIME2 NULL,
        ModifiedById INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_MaintenanceChecklistItems_Checklists FOREIGN KEY (MaintenanceChecklistId) REFERENCES MaintenanceChecklists(Id) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MaintenanceChecklistItemOptions')
BEGIN
    CREATE TABLE MaintenanceChecklistItemOptions (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MaintenanceChecklistItemId INT NOT NULL,
        OptionText NVARCHAR(200) NOT NULL,
        SortOrder INT NOT NULL DEFAULT 0,
        CreatedOn DATETIME2 NOT NULL,
        CreatedById INT NULL,
        ModifiedOn DATETIME2 NULL,
        ModifiedById INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_MaintenanceChecklistItemOptions_Items FOREIGN KEY (MaintenanceChecklistItemId) REFERENCES MaintenanceChecklistItems(Id) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MaintenanceSchedules')
BEGIN
    CREATE TABLE MaintenanceSchedules (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId INT NULL,
        AssetId INT NOT NULL,
        MaintenanceTypeId INT NOT NULL,
        MaintenanceChecklistId INT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        RecurrenceType INT NOT NULL,
        IntervalValue INT NOT NULL DEFAULT 1,
        DayOfWeek INT NULL,
        StartDate DATETIME2 NOT NULL,
        EndDate DATETIME2 NULL,
        NextDueDate DATETIME2 NULL,
        NextDueOperatingHours DECIMAL(18,2) NULL,
        NextDueCycles DECIMAL(18,2) NULL,
        LastGeneratedDate DATETIME2 NULL,
        LastOccurrenceDate DATETIME2 NULL,
        LastCompletedDate DATETIME2 NULL,
        ResponsibleUserId INT NULL,
        GenerationHorizonDays INT NOT NULL DEFAULT 90,
        CreatedOn DATETIME2 NOT NULL,
        CreatedById INT NULL,
        ModifiedOn DATETIME2 NULL,
        ModifiedById INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_MaintenanceSchedules_Assets FOREIGN KEY (AssetId) REFERENCES Assets(Id),
        CONSTRAINT FK_MaintenanceSchedules_MaintenanceTypes FOREIGN KEY (MaintenanceTypeId) REFERENCES MaintenanceTypes(Id),
        CONSTRAINT FK_MaintenanceSchedules_Checklists FOREIGN KEY (MaintenanceChecklistId) REFERENCES MaintenanceChecklists(Id),
        CONSTRAINT FK_MaintenanceSchedules_Users FOREIGN KEY (ResponsibleUserId) REFERENCES ApplicationUsers(Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PreventiveMaintenanceOccurrences')
BEGIN
    CREATE TABLE PreventiveMaintenanceOccurrences (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId INT NULL,
        MaintenanceScheduleId INT NOT NULL,
        AssetId INT NOT NULL,
        MaintenanceTypeId INT NOT NULL,
        MaintenanceChecklistId INT NULL,
        ChecklistVersion INT NULL,
        ScheduledDate DATETIME2 NULL,
        DueDate DATETIME2 NOT NULL,
        DueOperatingHours DECIMAL(18,2) NULL,
        DueCycles DECIMAL(18,2) NULL,
        Status INT NOT NULL,
        StartedAt DATETIME2 NULL,
        StartedByUserId INT NULL,
        CompletedAt DATETIME2 NULL,
        CompletedByUserId INT NULL,
        Remarks NVARCHAR(2000) NULL,
        WorkOrderId INT NULL,
        CreatedOn DATETIME2 NOT NULL,
        CreatedById INT NULL,
        ModifiedOn DATETIME2 NULL,
        ModifiedById INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_PMO_Schedules FOREIGN KEY (MaintenanceScheduleId) REFERENCES MaintenanceSchedules(Id),
        CONSTRAINT FK_PMO_Assets FOREIGN KEY (AssetId) REFERENCES Assets(Id),
        CONSTRAINT FK_PMO_MaintenanceTypes FOREIGN KEY (MaintenanceTypeId) REFERENCES MaintenanceTypes(Id),
        CONSTRAINT FK_PMO_Checklists FOREIGN KEY (MaintenanceChecklistId) REFERENCES MaintenanceChecklists(Id)
    );
    CREATE UNIQUE INDEX IX_PMO_Tenant_Schedule_ScheduledDate ON PreventiveMaintenanceOccurrences(TenantId, MaintenanceScheduleId, ScheduledDate) WHERE ScheduledDate IS NOT NULL;
    CREATE UNIQUE INDEX IX_PMO_Tenant_Schedule_DueHours ON PreventiveMaintenanceOccurrences(TenantId, MaintenanceScheduleId, DueOperatingHours) WHERE DueOperatingHours IS NOT NULL;
    CREATE UNIQUE INDEX IX_PMO_Tenant_Schedule_DueCycles ON PreventiveMaintenanceOccurrences(TenantId, MaintenanceScheduleId, DueCycles) WHERE DueCycles IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PreventiveMaintenanceOccurrenceChecklistItems')
BEGIN
    CREATE TABLE PreventiveMaintenanceOccurrenceChecklistItems (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        PreventiveMaintenanceOccurrenceId INT NOT NULL,
        SourceChecklistItemId INT NULL,
        ItemText NVARCHAR(500) NOT NULL,
        Description NVARCHAR(1000) NULL,
        ResponseType INT NOT NULL,
        IsRequired BIT NOT NULL,
        SortOrder INT NOT NULL,
        OptionsJson NVARCHAR(4000) NULL,
        CreatedOn DATETIME2 NOT NULL,
        CreatedById INT NULL,
        ModifiedOn DATETIME2 NULL,
        ModifiedById INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_PMOChecklistItems_Occurrences FOREIGN KEY (PreventiveMaintenanceOccurrenceId) REFERENCES PreventiveMaintenanceOccurrences(Id) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PreventiveMaintenanceChecklistResponses')
BEGIN
    CREATE TABLE PreventiveMaintenanceChecklistResponses (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        PreventiveMaintenanceOccurrenceId INT NOT NULL,
        OccurrenceChecklistItemId INT NOT NULL,
        ResponseValue NVARCHAR(500) NULL,
        NumericValue DECIMAL(18,2) NULL,
        Remarks NVARCHAR(1000) NULL,
        CreatedOn DATETIME2 NOT NULL,
        CreatedById INT NULL,
        ModifiedOn DATETIME2 NULL,
        ModifiedById INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_PMOResp_Occurrences FOREIGN KEY (PreventiveMaintenanceOccurrenceId) REFERENCES PreventiveMaintenanceOccurrences(Id) ON DELETE CASCADE,
        CONSTRAINT FK_PMOResp_ChecklistItems FOREIGN KEY (OccurrenceChecklistItemId) REFERENCES PreventiveMaintenanceOccurrenceChecklistItems(Id)
    );
    CREATE UNIQUE INDEX IX_PMOResp_OccurrenceChecklistItemId ON PreventiveMaintenanceChecklistResponses(OccurrenceChecklistItemId);
END
GO

-- After schema: apply EF migration or insert new Resource seed rows (MaintenanceType, MaintenanceChecklist, MaintenanceSchedule, PreventiveMaintenance features 100-103 and actions 104-119) and assign to roles.
