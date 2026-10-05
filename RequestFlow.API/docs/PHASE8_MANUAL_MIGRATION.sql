-- Phase 8 Part Replacement — manual migration (review before production).
-- DO NOT run from automated tooling; backup database first.
-- Permissions feature 123 / actions 151–155 are in RoleEnumSeed; apply via your usual seed process if needed.

-- 1) PartTransactions — work order / asset / replacement linkage
IF COL_LENGTH('PartTransactions', 'WorkOrderId') IS NULL
    ALTER TABLE PartTransactions ADD WorkOrderId INT NULL;
IF COL_LENGTH('PartTransactions', 'AssetId') IS NULL
    ALTER TABLE PartTransactions ADD AssetId INT NULL;
IF COL_LENGTH('PartTransactions', 'PartReplacementId') IS NULL
    ALTER TABLE PartTransactions ADD PartReplacementId INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PartTransactions_WorkOrderId')
    CREATE INDEX IX_PartTransactions_WorkOrderId ON PartTransactions(WorkOrderId) WHERE WorkOrderId IS NOT NULL;
GO

-- 2) PartReplacements
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PartReplacements')
BEGIN
    CREATE TABLE PartReplacements (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId INT NULL,
        WorkOrderId INT NOT NULL,
        AssetId INT NOT NULL,
        OldAssetComponentId INT NULL,
        OldPartNumber NVARCHAR(100) NOT NULL DEFAULT '',
        OldSerialNumber NVARCHAR(100) NOT NULL DEFAULT '',
        OldPartSerialNumberId INT NULL,
        NewPartId INT NOT NULL,
        NewPartSerialNumberId INT NULL,
        NewPartInventoryBatchId INT NULL,
        NewAssetComponentId INT NULL,
        Quantity DECIMAL(18,2) NOT NULL DEFAULT 1,
        InstalledByUserId INT NOT NULL,
        InstalledAt DATETIME2 NOT NULL,
        RemovedByUserId INT NULL,
        RemovedAt DATETIME2 NULL,
        RemovalReason NVARCHAR(2000) NOT NULL DEFAULT '',
        FailureReason NVARCHAR(2000) NOT NULL DEFAULT '',
        FromLocationId INT NULL,
        InstallationLocation NVARCHAR(500) NOT NULL DEFAULT '',
        Remarks NVARCHAR(2000) NOT NULL DEFAULT '',
        PartTransactionId INT NULL,
        CreatedOn DATETIME2 NOT NULL,
        CreatedById INT NULL,
        ModifiedOn DATETIME2 NULL,
        ModifiedById INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_PartReplacements_WorkOrders FOREIGN KEY (WorkOrderId) REFERENCES WorkOrders(Id),
        CONSTRAINT FK_PartReplacements_Assets FOREIGN KEY (AssetId) REFERENCES Assets(Id),
        CONSTRAINT FK_PartReplacements_OldAssetComponents FOREIGN KEY (OldAssetComponentId) REFERENCES AssetComponents(Id),
        CONSTRAINT FK_PartReplacements_NewAssetComponents FOREIGN KEY (NewAssetComponentId) REFERENCES AssetComponents(Id),
        CONSTRAINT FK_PartReplacements_NewParts FOREIGN KEY (NewPartId) REFERENCES Parts(Id),
        CONSTRAINT FK_PartReplacements_NewPartSerialNumbers FOREIGN KEY (NewPartSerialNumberId) REFERENCES PartSerialNumbers(Id),
        CONSTRAINT FK_PartReplacements_OldPartSerialNumbers FOREIGN KEY (OldPartSerialNumberId) REFERENCES PartSerialNumbers(Id),
        CONSTRAINT FK_PartReplacements_NewPartInventoryBatches FOREIGN KEY (NewPartInventoryBatchId) REFERENCES PartInventoryBatches(Id),
        CONSTRAINT FK_PartReplacements_PartTransactions FOREIGN KEY (PartTransactionId) REFERENCES PartTransactions(Id),
        CONSTRAINT FK_PartReplacements_FromLocations FOREIGN KEY (FromLocationId) REFERENCES Locations(Id)
    );

    CREATE INDEX IX_PartReplacements_WorkOrderId ON PartReplacements(WorkOrderId);
    CREATE INDEX IX_PartReplacements_AssetId ON PartReplacements(AssetId);
    CREATE INDEX IX_PartReplacements_TenantId ON PartReplacements(TenantId) WHERE TenantId IS NOT NULL;
END
GO

-- Optional FKs on PartTransactions (after PartReplacements exists)
IF COL_LENGTH('PartTransactions', 'WorkOrderId') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PartTransactions_WorkOrders')
    ALTER TABLE PartTransactions ADD CONSTRAINT FK_PartTransactions_WorkOrders
        FOREIGN KEY (WorkOrderId) REFERENCES WorkOrders(Id);
GO
