# Menu Module Refactoring Report

Date: 2026-06-21

## Scope

Analyzed and refactored the DineFlow Menu module areas:

- `MenuItem`
- `MenuCategory` / `Category`
- `MenuAddonGroup`
- `MenuAddonOption`
- `MenuItemAddonGroup`
- Addon group option mapping
- Stock management
- Availability management
- Customer menu ordering and addon validation flow

## Changes Made

### 1. Removed Unused / Overloaded Service Helpers

Removed private service helper implementations that mixed validation and business state transitions:

- `MenuItemService.Validate`
- `MenuItemService.ApplyStockRule`
- `MenuItemService.TouchRowVersion`
- `MenuItemService.ValidateOrderRequests`
- `CategoryService.Validate`
- `MenuAddonService.ValidateGroup`
- `MenuAddonService.ValidateOption`
- `MenuAddonService.ValidateGroupOption`
- `MenuAddonService.ValidateMenuItemAddonGroup`

The behavior was preserved by moving the same checks into validators or domain methods.

### 2. Duplicated Logic Reduced

Consolidated stock and availability transitions into `MenuItem` domain methods:

- `SetStockQuantity`
- `SetSaleAvailability`
- `ReserveStock`
- `RestoreStock`
- `ApplyStockAvailabilityRule`
- `EnsureCanBeOrdered`
- `MarkHidden`

This removes repeated service-side branching around tracked stock, zero quantity, availability, and row-version refresh.

### 3. Business-Oriented Method Names

Renamed internal orchestration helpers to describe POS behavior:

- `EnsureMenuItemCanBeOrdered`
- `EnsureAddonGroupSelectionsSatisfyRules`
- `ResolveSelectedAddons`
- `EnsureAddonDoesNotLinkToParentItem`
- `EnsureLinkedAddonItemCanBeOrdered`
- `GetActiveAddonGroupMappings`

Public service method names were kept stable to avoid API/controller behavior changes.

### 4. Clean Architecture Folder Organization

Added a validation boundary under:

- `src/DineFlow.Services/Menu/Validation`

Validation classes:

- `CategoryValidator`
- `MenuItemValidator`
- `MenuAddonValidator`
- `OrderRequestValidator`

Domain behavior lives in `DineFlow.BusinessObjects.Menu.MenuItem`; service classes now orchestrate repositories, validators, and domain methods.

### 5. Validation Separated From Services

Field-level and request-shape validation was moved out of service classes:

- Category save validation -> `CategoryValidator`
- Menu item save validation -> `MenuItemValidator`
- Addon/group/mapping validation -> `MenuAddonValidator`
- Order request validation -> `OrderRequestValidator`

Services still enforce repository-backed rules such as uniqueness and existence checks.

### 6. Business Rules Moved Into Domain Methods

Moved behavior that depends only on `MenuItem` state into the entity:

- Tracked stock cannot be manually updated for non-stock items.
- Tracked stock quantity must be non-null and non-negative.
- Zero tracked stock forces `IsAvailable = false`.
- Enabling availability is blocked for inactive items.
- Enabling availability is blocked when tracked stock is zero.
- Reserving stock deducts quantity and refreshes row version.
- Restoring stock adds quantity and refreshes row version without auto-enabling availability.
- Orderability checks include active item, available item, active category, standalone rules, and stock quantity.

### 7. Constants Added

Created `MenuBusinessMessages` for repeated core business messages used by services and domain methods.

### 8. Commented Code

Scanned menu service/domain files for commented code. No commented code blocks were found in the touched module files.

### 9. Long Service Methods Optimized

Split the old order-preparation workflow into smaller methods. Service methods in the reviewed menu service files are now under 50 lines.

### 10. POS Rules Preserved

The following critical rules were preserved:

- Inactive category hides customer menu items.
- Inactive/unavailable menu items cannot be ordered.
- Addon-only items are not shown as standalone customer menu items.
- Addon option must belong to the selected item group.
- Required addon groups enforce min/max selection.
- Linked addon menu items require stock and orderability.
- Addons without linked menu items do not deduct stock.
- Multiple order lines normalize stock deduction.
- Successful orders deduct main item and linked addon stock.
- Cancelled orders restore main item and linked addon stock.
- Price snapshots remain stable after admin price changes.
- Required groups without available options block ordering.
- Default addon options auto-apply unless the group was touched by the customer.
- Each addon group allows only one default option.
- Addon group `MaxSelect` must be at least 1.
- Duplicate linked addon options are rejected.
- Staff cannot change master menu data but can operate stock.

## Verification

Command run:

```powershell
dotnet test --no-restore
```

Result: passed.
