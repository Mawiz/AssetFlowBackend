# Phase 9 — History & Cost Management (completion)

## Backend

### Created
- `AssetFlow.Data/Entities/History/MaintenanceCostRecord.cs`
- `AssetFlow.Data/Entities/History/WorkOrderLaborRecord.cs`
- `AssetFlow.Services/Dto/History/AssetHistoryDtos.cs`
- `AssetFlow.Services/Core/History/AssetHistoryService.cs` (read projections from PM, Issues, WO, PartReplacement, costs, status history)
- `AssetFlow.Services/Core/History/MaintenanceCostService.cs`
- `AssetFlow.Services/Contracts/IAssetHistoryService.cs`, `IMaintenanceCostService.cs`
- `AssetFlow.API/Controllers/AssetHistoryController.cs`
- `AssetFlow.API/Controllers/MaintenanceCostController.cs`
- `docs/PHASE9_MANUAL_MIGRATION.sql`

### Modified
- `Enums.cs` — `MaintenanceCostType`, `AssetHistoryEventType`
- `ApplicationDbContext.cs` — DbSets + indexes
- `Permissions.cs`, `RoleEnumSeed.cs` (features 124–125, actions 156–160)
- `Program.cs` — DI

### Reused (no duplicates)
- WorkOrder + StatusHistory, AssetIssue, PM Occurrences, PartReplacement, AssetComponent, PartTransaction (read-only where applicable)

### Audit log
- No AuditLog entity existed in codebase; **not** added (per “do not create a second AuditLog” when none exists). Service History tab covers operational timeline.

## Frontend

### Created
- `model/asset-history.ts`
- `services/asset-history-service.ts`, `maintenance-cost-service.ts`
- `asset-history-panel.component.ts/html` — summary cards, timeline, maintenance/breakdown/WO/parts/downtime/cost sub-tabs

### Modified
- `asset-detail-component` — **Service History** tab
- `permissions.ts` — `AssetHistory`, `CostManagement`

## Calculations
- **WO total** = labor records + cost rows by type (Parts/Labor/External/Other)
- **Asset cost** = sum for asset (+ labor records on asset WOs)
- **Downtime** = Issue.ReportedAt → WO.AssetRestoredAt / CompletedAt / ClosedAt / Issue.ResolvedAt
- **Part life** = days between InstalledAt and RemovedAt on projections

## Tests
- No test project in solution; manual QA recommended.

## Confirmations
- No migrations created/run
- No seed/dummy data added
- Phase 10/11 not implemented
- Existing WO / PM / Issue / PartReplacement flows unchanged
