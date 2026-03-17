# Inventory Functionality and Endpoints

This document describes the current backend implementation for the **Inventory** feature.

All responses follow [API Response Convention](api-response-convention.md). Authorization rules are based on [Authorization](authorization.md).

---

## Scope (Current Phase)

Implemented:
- Inventory CRUD (create, list, read, update settings, delete)
- Inventory access list management (get, add users, remove users)
- Inventory custom fields management (get, set)
- Optimistic locking on mutable operations

Not implemented in this phase:
- Custom ID generation/format editor logic for items

---

## Architecture

| Layer | File | Responsibility |
|---|---|---|
| Controller | `Features/Inventory/InventoriesController.cs` | HTTP routes, auth checks, resource authorization |
| Service interface | `Features/Inventory/IInventoryService.cs` | Inventory feature contract |
| Service implementation | `Features/Inventory/InventoryService.cs` | Validation, persistence, optimistic locking, mapping |
| Contracts | `Contracts/Inventory/*.cs` | Request/response DTOs |
| Data model | `Models/Inventory.cs` | Inventory entity including custom field slots |

---

## Authorization Model for Inventory Endpoints

| Endpoint | Access |
|---|---|
| `GET /inventories` | Anonymous |
| `GET /inventories/{id}` | Anonymous |
| `GET /inventories/{id}/fields` | Anonymous |
| `POST /inventories` | Authenticated (`Authenticated`) |
| `PUT /inventories/{id}/settings` | Owner or Admin (resource-based `OwnerOrAdmin`) |
| `GET /inventories/{id}/access` | Owner or Admin (resource-based `OwnerOrAdmin`) |
| `POST /inventories/{id}/access/add` | Owner or Admin (resource-based `OwnerOrAdmin`) |
| `POST /inventories/{id}/access/remove` | Owner or Admin (resource-based `OwnerOrAdmin`) |
| `PUT /inventories/{id}/fields` | Owner or Admin (resource-based `OwnerOrAdmin`) |
| `DELETE /inventories/{id}` | Owner or Admin (resource-based `OwnerOrAdmin`) |

---

## Optimistic Locking

Mutable endpoints use the Postgres `xmin` concurrency token:
- Client sends `version` in request body.
- Service sets EF original concurrency value to request version.
- On conflict, EF throws `DbUpdateConcurrencyException`.
- Global exception handler returns `409` with `errorCode = conflict.optimistic_lock`.

Applies to:
- `PUT /inventories/{id}/settings`
- `POST /inventories/{id}/access/add`
- `POST /inventories/{id}/access/remove`
- `PUT /inventories/{id}/fields`

---

## DTO Overview

### Core inventory
- `CreateInventoryRequest`
- `InventoryDto`
- `InventoryListResponse`
- `UpdateInventorySettingsRequest`

### Access list
- `UpdateInventoryAccessRequest` (emails + version)
- `InventoryAccessResponse` (inventoryId + emails + version)

### Custom fields
- `CustomFieldType`
- `CustomFieldDto`
- `UpdateInventoryCustomFieldsRequest`
- `InventoryCustomFieldsResponse`

---

## Endpoint Details

### 1) `POST /inventories`

Creates a new inventory owned by the current user.

**Auth:** `Authenticated`

**Request**
```json
{
  "title": "Office Assets",
  "descriptionMd": "Markdown text",
  "imageUrl": "https://cdn.example.com/img.png",
  "categoryId": 1,
  "isPublic": false,
  "tagNames": ["office", "assets"]
}
```

**Success:** `201` with `InventoryDto`

**Errors**
- `400 inventory.invalid_title`
- `400 inventory.category_not_found`
- `401 auth.unauthorized`
- `403 auth.blocked`

---

### 2) `GET /inventories`

Returns paginated inventories.

**Auth:** anonymous

**Query params**
- `ownerId` (optional)
- `page`
- `pageSize`

**Success:** `200` with `InventoryListResponse`

---

### 3) `GET /inventories/{id}`

Returns one inventory.

**Auth:** anonymous

**Success:** `200` with `InventoryDto`

**Errors**
- `404 inventory.not_found`

---

### 4) `PUT /inventories/{id}/settings`

Full replacement of inventory settings payload (title/description/image/category/isPublic/tags) with optimistic locking.

**Auth:** owner/admin

**Request**
```json
{
  "title": "Updated title",
  "descriptionMd": "Updated md",
  "imageUrl": "https://cdn.example.com/new.png",
  "categoryId": 2,
  "isPublic": true,
  "tagNames": ["hardware", "it"],
  "version": 12345
}
```

**Success:** `200` with updated `InventoryDto`

**Errors**
- `400 inventory.invalid_title`
- `400 inventory.category_not_found`
- `404 inventory.not_found`
- `403 auth.forbidden`
- `409 conflict.optimistic_lock`

---

### 5) `GET /inventories/{id}/access`

Returns current write-access users (emails) and current version.

**Auth:** owner/admin

**Success:** `200` with `InventoryAccessResponse`

**Errors**
- `404 inventory.not_found`
- `403 auth.forbidden`

---

### 6) `POST /inventories/{id}/access/add`

Adds users to write-access list by email.

**Auth:** owner/admin

**Request**
```json
{
  "emails": ["user1@example.com", "user2@example.com"],
  "version": 12345
}
```

**Success:** `200` with updated `InventoryAccessResponse`

**Errors**
- `404 inventory.not_found`
- `404 inventory.access_user_not_found`
- `403 auth.forbidden`
- `409 conflict.optimistic_lock`

---

### 7) `POST /inventories/{id}/access/remove`

Removes users from write-access list by email.

**Auth:** owner/admin

**Request**
```json
{
  "emails": ["user1@example.com"],
  "version": 12345
}
```

**Success:** `200` with updated `InventoryAccessResponse`

**Errors**
- `404 inventory.not_found`
- `404 inventory.access_user_not_found`
- `403 auth.forbidden`
- `409 conflict.optimistic_lock`

---

### 8) `GET /inventories/{id}/fields`

Returns custom field definitions for the inventory (slot-based representation).

**Auth:** anonymous

**Success:** `200` with `InventoryCustomFieldsResponse`

**Errors**
- `404 inventory.not_found`

---

### 9) `PUT /inventories/{id}/fields`

Sets custom fields as a full replacement of provided field list, using optimistic locking.

**Auth:** owner/admin

**Request**
```json
{
  "fields": [
    {
      "type": "String",
      "slot": 1,
      "enabled": true,
      "title": "Serial",
      "description": "Device serial number",
      "showInTable": true,
      "orderIndex": 1
    }
  ],
  "version": 12345
}
```

**Validation rules**
- `slot` must be in range 1..3
- no duplicate `(type, slot)` in one request

**Success:** `200` with updated `InventoryCustomFieldsResponse`

**Errors**
- `400` duplicate slot/type or invalid slot
- `404 inventory.not_found`
- `403 auth.forbidden`
- `409 conflict.optimistic_lock`

---

### 10) `DELETE /inventories/{id}`

Deletes inventory. Dependent rows are handled by DB cascade rules.

**Auth:** owner/admin

**Success**
```json
{ "success": true, "status": 200, "data": null }
```

**Errors**
- `404 inventory.not_found`
- `403 auth.forbidden`

---

## Notes for Frontend Integration

- For all update operations, always send the latest `version` returned by the server.
- On `409 conflict.optimistic_lock`, refetch resource and retry with fresh version.
- `PUT` endpoints in this feature are modeled as replacement-style updates.
- Access list endpoints identify users by email (not user id).
