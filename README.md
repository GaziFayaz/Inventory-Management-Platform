# Inventory Management Platform

Inventory Management Platform is an ASP.NET Core Web API for managing inventories, inventory access, custom inventory fields, admin users, authentication, and image storage. The API uses PostgreSQL through Entity Framework Core and includes policy-based authorization for owner, admin, authenticated, and inventory-write workflows.

## Features

- ASP.NET Core API with controller-based endpoints and OpenAPI support in development.
- PostgreSQL persistence using Entity Framework Core and Npgsql.
- ASP.NET Core Identity user management with role-based admin access.
- Google authentication integration with JSON API-friendly 401/403 responses.
- Admin user management services and controllers.
- Inventory creation, listing, settings updates, access control, and custom-field management.
- Policy-based authorization for authenticated users, admins, inventory owners, and inventory writers.
- Cloudinary-backed image storage for inventory media.
- Centralized API response shape and global exception handling.
- Database migrations and idempotent admin seeding on startup.

## Tech Stack

- C#, .NET 10, ASP.NET Core Web API
- Entity Framework Core 10, Npgsql PostgreSQL provider
- ASP.NET Core Identity, Google authentication, policy-based authorization
- CloudinaryDotNet for image storage
- OpenAPI, ProblemDetails, forwarded headers, CORS
- Docker project settings, Visual Studio solution structure

## Getting Started

Prerequisites:

- .NET 10 SDK
- PostgreSQL database
- Cloudinary account for image uploads

Restore dependencies:

```bash
dotnet restore "Inventory Management Platform.sln"
```

Configure local settings using user secrets or `appsettings.Development.json`. At minimum, provide a PostgreSQL connection string and Cloudinary credentials.

Apply migrations:

```bash
dotnet ef database update --project "Inventory Management Platform"
```

Run the API:

```bash
dotnet run --project "Inventory Management Platform"
```

Build the solution:

```bash
dotnet build "Inventory Management Platform.sln"
```

When running in development, the OpenAPI endpoint is mapped by the application startup pipeline.

## Environment Variables

The app uses ASP.NET Core configuration, so values can come from environment variables, user secrets, or `appsettings.*.json`. Do not commit real secret values.

```env
ConnectionStrings__DefaultConnection=
FrontendUrl=

Cloudinary__CloudName=
Cloudinary__ApiKey=
Cloudinary__ApiSecret=
Cloudinary__Folder=inventories

Authentication__Google__ClientId=
Authentication__Google__ClientSecret=
```
