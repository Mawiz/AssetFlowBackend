# Phase 8 — Part Replacement & Installation (completion)

## Scope delivered

- **PartReplacement** entity and **PartTransactions** extensions (`WorkOrderId`, `AssetId`, `PartReplacementId`).
- **PartReplacementService**: validate, serial lookup, replace (transactional), list by work order.
- **PartReplacementController** + **IPartReplacementService** DI.
- Permissions: feature **123**, actions **151–155** (`PartReplacement.View/Create/Validate/Replace/Delete`).
- **Frontend**: models/services, Work Order drawer section **Parts / replacements**, dialog for engineer replace flow.

## Out of scope (as requested)

- EF migrations executed by tooling, seed runs, automated tests, QR camera SDK.

## Manual steps

1. Apply `docs/PHASE8_MANUAL_MIGRATION.sql` on your database.
2. Run permission seed (or assign feature 123 / actions 151–155 to engineer/manager roles).
3. QA: assigned engineer on WO in **Accepted / In Progress / Waiting for parts / Reopened** replaces a serialized or batch part; verify inventory, asset component, and issue transaction.

## API (base `api/PartReplacement`)

| Method | Route | Permission |
|--------|--------|------------|
| POST | Validate | PartReplacement.Validate |
| GET | LookupSerial | PartReplacement.Validate |
| POST | Replace | PartReplacement.Replace |
| GET | ByWorkOrder/{id} | PartReplacement.View |
| GET | {id} | PartReplacement.View |

## Build status

- `AssetFlow.Services`: compiles (0 errors).
- `ng build`: succeeds.
- Full API build may fail if Visual Studio / running API locks output DLLs; stop the debugger and rebuild.
