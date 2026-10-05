-- Phase 9 History & Cost — manual migration (review before production).
-- Permissions: feature 124 AssetHistory, 125 CostManagement, actions 156-160.

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MaintenanceCostRecords')
BEGIN
    CREATE TABLE MaintenanceCostRecords (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId INT NULL,
        AssetId INT NOT NULL,
        WorkOrderId INT NULL,
        CostType INT NOT NULL,
        Description NVARCHAR(500) NOT NULL DEFAULT '',
        Amount DECIMAL(18,2) NOT NULL,
        Currency NVARCHAR(10) NOT NULL DEFAULT 'USD',
        CostDate DATETIME2 NOT NULL,
        PartReplacementId INT NULL,
        PartId INT NULL,
        Quantity DECIMAL(18,2) NULL,
        UnitCost DECIMAL(18,2) NULL,
        LaborRecordId INT NULL,
        ExternalServiceDescription NVARCHAR(500) NOT NULL DEFAULT '',
        ReferenceNumber NVARCHAR(100) NOT NULL DEFAULT '',
        Notes NVARCHAR(2000) NOT NULL DEFAULT '',
        CreatedByUserId INT NOT NULL,
        CreatedOn DATETIME2 NOT NULL,
        CreatedById INT NULL,
        ModifiedOn DATETIME2 NULL,
        ModifiedById INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_MaintenanceCostRecords_Assets FOREIGN KEY (AssetId) REFERENCES Assets(Id),
        CONSTRAINT FK_MaintenanceCostRecords_WorkOrders FOREIGN KEY (WorkOrderId) REFERENCES WorkOrders(Id)
    );
    CREATE INDEX IX_MaintenanceCostRecords_TenantId ON MaintenanceCostRecords(TenantId);
    CREATE INDEX IX_MaintenanceCostRecords_AssetId ON MaintenanceCostRecords(AssetId);
    CREATE INDEX IX_MaintenanceCostRecords_WorkOrderId ON MaintenanceCostRecords(WorkOrderId);
    CREATE INDEX IX_MaintenanceCostRecords_CostDate ON MaintenanceCostRecords(CostDate);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WorkOrderLaborRecords')
BEGIN
    CREATE TABLE WorkOrderLaborRecords (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId INT NULL,
        WorkOrderId INT NOT NULL,
        UserId INT NOT NULL,
        StartTime DATETIME2 NOT NULL,
        EndTime DATETIME2 NOT NULL,
        DurationMinutes INT NOT NULL,
        HourlyRate DECIMAL(18,2) NULL,
        LaborCost DECIMAL(18,2) NOT NULL,
        Notes NVARCHAR(2000) NOT NULL DEFAULT '',
        CreatedOn DATETIME2 NOT NULL,
        CreatedById INT NULL,
        ModifiedOn DATETIME2 NULL,
        ModifiedById INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_WorkOrderLaborRecords_WorkOrders FOREIGN KEY (WorkOrderId) REFERENCES WorkOrders(Id)
    );
    CREATE INDEX IX_WorkOrderLaborRecords_WorkOrderId ON WorkOrderLaborRecords(WorkOrderId);
    CREATE INDEX IX_WorkOrderLaborRecords_TenantId ON WorkOrderLaborRecords(TenantId);
END
GO
