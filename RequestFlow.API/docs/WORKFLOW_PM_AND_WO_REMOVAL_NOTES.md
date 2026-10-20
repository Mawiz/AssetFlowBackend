# Workflow PM start & Work Order removal (manual DB steps)

## New permission
- `PreventiveMaintenance.Start` — seed resource id **175** (assign to roles in ACL after deploy).

## Maintenance workflow
- PM **Generate** no longer starts workflows.
- `POST /api/PreventiveMaintenance/{id}/StartWorkflow` (requires `PreventiveMaintenance.Start`) calls `InitiateIfMappedAsync` for `ReferenceItemType = Maintenance`, `ReferenceItemId = occurrence id`.
- When workflow master completes, occurrence **Status** is set to **Completed** (CompletedAt/By unchanged).

## Work Orders removed from API/UI
- Drop or ignore Work Order tables when you migrate.
- Repoint **PartReplacement** / **MaintenanceCostRecord** to `PreventiveMaintenanceOccurrenceId` and/or `AssetIssueId` (code may still reference legacy columns until you alter schema).
- Remove `WorkOrderId` from `AssetIssues` and `PreventiveMaintenanceOccurrences` when ready.

## Analytics
- Dashboard uses **OpenWorkflowTasks** and **WorkflowStageDistribution** instead of open work orders.
