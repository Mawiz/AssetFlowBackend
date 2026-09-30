-- Phase 4 Spare Parts inventory model — manual migration script (review before running on production).
-- Backup database first. Adjust schema/table names if your EF history differs.

-- 1) PartInventoryBatches
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PartInventoryBatches')
BEGIN
    CREATE TABLE PartInventoryBatches (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TenantId INT NULL,
        PartInventoryId INT NOT NULL,
        PartId INT NOT NULL,
        BatchReference NVARCHAR(50) NOT NULL,
        SupplierId INT NULL,
        ReceivedDate DATETIME2 NULL,
        ExpiryDate DATETIME2 NULL,
        ExpectedLifeValue INT NULL,
        ExpectedLifeUnit INT NULL,
        TotalQuantity DECIMAL(18,2) NOT NULL DEFAULT 0,
        AvailableQuantity DECIMAL(18,2) NOT NULL DEFAULT 0,
        FaultyQuantity DECIMAL(18,2) NOT NULL DEFAULT 0,
        QuarantineQuantity DECIMAL(18,2) NOT NULL DEFAULT 0,
        IssuedQuantity DECIMAL(18,2) NOT NULL DEFAULT 0,
        Notes NVARCHAR(1000) NULL,
        CreatedOn DATETIME2 NOT NULL,
        CreatedById INT NULL,
        ModifiedOn DATETIME2 NULL,
        ModifiedById INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_PartInventoryBatches_PartInventories FOREIGN KEY (PartInventoryId) REFERENCES PartInventories(Id),
        CONSTRAINT FK_PartInventoryBatches_Parts FOREIGN KEY (PartId) REFERENCES Parts(Id),
        CONSTRAINT FK_PartInventoryBatches_Suppliers FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
    );
    CREATE UNIQUE INDEX IX_PartInventoryBatches_TenantId_BatchReference ON PartInventoryBatches(TenantId, BatchReference) WHERE TenantId IS NOT NULL;
END
GO

-- 2) PartTransactionSerials
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PartTransactionSerials')
BEGIN
    CREATE TABLE PartTransactionSerials (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        PartTransactionId INT NOT NULL,
        PartSerialNumberId INT NOT NULL,
        Quantity DECIMAL(18,2) NOT NULL DEFAULT 1,
        CreatedOn DATETIME2 NOT NULL,
        CreatedById INT NULL,
        ModifiedOn DATETIME2 NULL,
        ModifiedById INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_PartTransactionSerials_PartTransactions FOREIGN KEY (PartTransactionId) REFERENCES PartTransactions(Id) ON DELETE CASCADE,
        CONSTRAINT FK_PartTransactionSerials_PartSerialNumbers FOREIGN KEY (PartSerialNumberId) REFERENCES PartSerialNumbers(Id)
    );
END
GO

-- 3) PartInventories — add aggregate columns (if migrating from legacy per-serial rows)
IF COL_LENGTH('PartInventories', 'TotalQuantity') IS NULL
    ALTER TABLE PartInventories ADD TotalQuantity DECIMAL(18,2) NOT NULL CONSTRAINT DF_PI_Total DEFAULT 0;
IF COL_LENGTH('PartInventories', 'FaultyQuantity') IS NULL
    ALTER TABLE PartInventories ADD FaultyQuantity DECIMAL(18,2) NOT NULL CONSTRAINT DF_PI_Faulty DEFAULT 0;
IF COL_LENGTH('PartInventories', 'QuarantineQuantity') IS NULL
    ALTER TABLE PartInventories ADD QuarantineQuantity DECIMAL(18,2) NOT NULL CONSTRAINT DF_PI_Quarantine DEFAULT 0;
IF COL_LENGTH('PartInventories', 'IssuedQuantity') IS NULL
    ALTER TABLE PartInventories ADD IssuedQuantity DECIMAL(18,2) NOT NULL CONSTRAINT DF_PI_Issued DEFAULT 0;
GO

-- Optional: map legacy QuantityAvailable -> AvailableQuantity / TotalQuantity before dropping old columns
-- UPDATE PartInventories SET AvailableQuantity = QuantityAvailable, TotalQuantity = QuantityAvailable WHERE TotalQuantity = 0;

-- 4) PartTransactions — new FK columns
IF COL_LENGTH('PartTransactions', 'PartInventoryId') IS NULL
    ALTER TABLE PartTransactions ADD PartInventoryId INT NULL;
IF COL_LENGTH('PartTransactions', 'PartInventoryBatchId') IS NULL
    ALTER TABLE PartTransactions ADD PartInventoryBatchId INT NULL;
IF COL_LENGTH('PartTransactions', 'Reason') IS NULL
    ALTER TABLE PartTransactions ADD Reason NVARCHAR(500) NULL;
IF COL_LENGTH('PartTransactions', 'IssuedToUserId') IS NULL
    ALTER TABLE PartTransactions ADD IssuedToUserId INT NULL;
IF COL_LENGTH('PartTransactions', 'ReturnedFromUserId') IS NULL
    ALTER TABLE PartTransactions ADD ReturnedFromUserId INT NULL;
GO

-- 5) PartSerialNumbers — batch + expiry
IF COL_LENGTH('PartSerialNumbers', 'PartInventoryBatchId') IS NULL
    ALTER TABLE PartSerialNumbers ADD PartInventoryBatchId INT NULL;
IF COL_LENGTH('PartSerialNumbers', 'ExpiryDate') IS NULL
    ALTER TABLE PartSerialNumbers ADD ExpiryDate DATETIME2 NULL;
IF COL_LENGTH('PartSerialNumbers', 'ExpectedLifeValue') IS NULL
    ALTER TABLE PartSerialNumbers ADD ExpectedLifeValue INT NULL;
IF COL_LENGTH('PartSerialNumbers', 'ExpectedLifeUnit') IS NULL
    ALTER TABLE PartSerialNumbers ADD ExpectedLifeUnit INT NULL;
GO

-- 6) After data migration: drop legacy PartInventories columns/indexes
-- DROP INDEX IX_PartInventories_TenantId_PartId_LocationId_PartSerialNumberId ON PartInventories;
-- ALTER TABLE PartInventories DROP CONSTRAINT FK_PartInventories_PartSerialNumbers_PartSerialNumberId;
-- ALTER TABLE PartInventories DROP COLUMN PartSerialNumberId, PartTransactionId, QuantityReserved, Status;
-- CREATE UNIQUE INDEX IX_PartInventories_TenantId_PartId_LocationId ON PartInventories(TenantId, PartId, LocationId) WHERE TenantId IS NOT NULL;
