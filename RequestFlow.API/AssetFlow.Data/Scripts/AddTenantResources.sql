-- Run after pulling tenant-permission changes, or use: dotnet ef migrations add AddTenantResources
IF OBJECT_ID(N'dbo.TenantResources', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TenantResources] (
        [Id] int NOT NULL IDENTITY,
        [TenantId] int NOT NULL,
        [ResourceId] int NOT NULL,
        CONSTRAINT [PK_TenantResources] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TenantResources_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TenantResources_Resources_ResourceId] FOREIGN KEY ([ResourceId]) REFERENCES [Resources] ([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [IX_TenantResources_TenantId_ResourceId] ON [TenantResources] ([TenantId], [ResourceId]);
END
