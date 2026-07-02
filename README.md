# Task Management API

A RESTful Web API built with **.NET 10** and **MySQL** for managing tasks. Features JWT authentication, a background job that auto-expires overdue tasks, pagination, global exception handling, and Swagger UI.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Prerequisites](#prerequisites)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Database Setup](#database-setup)
- [Configuration](#configuration)
- [Steps to Run](#steps-to-run)
- [API Endpoints](#api-endpoints)
- [Request & Response Examples](#request--response-examples)
- [Background Job](#background-job)
- [Assumptions](#assumptions)

---

## Project Overview

The Task Management API allows users to:

- Register and log in with JWT-based authentication
- Create, read, update, and delete tasks
- Filter tasks by status and paginate results
- Automatically expire overdue pending tasks via a background job that runs every **1 minute**

---

## Prerequisites

| Tool | Version |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 |
| [MySQL Server](https://dev.mysql.com/downloads/mysql/) | 8.0 or later |
| [Visual Studio](https://visualstudio.microsoft.com/) | 2022 / 2026 (with ASP.NET workload) |
| [Git](https://git-scm.com/) | Any recent version |

---

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 10 Web API |
| ORM | Entity Framework Core 10 |
| Database | MySQL 8.x |
| Authentication | JWT Bearer Tokens |
| Password Hashing | BCrypt.Net-Next |
| API Documentation | Swagger (Swashbuckle 6.9) |
| Background Job | `BackgroundService` (Hosted Service) |

### NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `MySql.EntityFrameworkCore` | 10.0.7 | MySQL EF Core provider |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.9 | JWT authentication |
| `System.IdentityModel.Tokens.Jwt` | 8.19.1 | JWT token generation |
| `BCrypt.Net-Next` | 4.2.0 | Password hashing |
| `Swashbuckle.AspNetCore` | 6.9.0 | Swagger UI |

---

## Project Structure

```
Task Management API/
├── Controllers/
│   ├── AuthController.cs          # POST /api/auth/register, /api/auth/login
│   └── TasksController.cs         # CRUD endpoints for tasks
├── Services/
│   ├── AuthService.cs             # Register, login, JWT generation
│   └── TaskService.cs             # Task business logic
├── Repositories/
│   └── TaskRepository.cs          # EF Core data access
├── Interfaces/
│   ├── IAuthService.cs
│   ├── ITaskService.cs
│   └── ITaskRepository.cs
├── Models/
│   ├── User.cs                    # Users table (manual, no Identity)
│   ├── TaskItem.cs                # Tasks table
│   └── TaskStatus.cs              # Enum: Pending, Completed, Expired
├── DTOs/
│   ├── ApiResponse.cs             # Generic API response wrapper
│   ├── PaginatedResponse.cs       # Pagination wrapper
│   ├── CreateTaskDto.cs
│   ├── UpdateTaskDto.cs
│   ├── TaskResponseDto.cs
│   ├── RegisterDto.cs
│   ├── LoginDto.cs
│   └── AuthResponseDto.cs
├── Data/
│   └── ApplicationDbContext.cs    # EF Core DbContext (Users + Tasks)
├── BackgroundJobs/
│   └── ExpiredTaskBackgroundService.cs  # Runs every 1 minute
├── Middleware/
│   └── GlobalExceptionHandlerMiddleware.cs
├── Properties/
│   └── launchSettings.json
├── Program.cs
├── appsettings.json
└── .gitignore
```

---

## Database Setup

### 1. Create the Database

Connect to MySQL and run:

```sql
CREATE DATABASE TaskManagement;
```

### 2. Tables

The tables are created automatically via **EF Core migrations**. Run the following commands from the project directory:

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

> If `dotnet ef` is not installed, install it first:
> ```powershell
> dotnet tool install --global dotnet-ef
> ```

### 3. Table Schemas (auto-generated)

**Users**

| Column | Type | Notes |
|---|---|---|
| Id | INT | PK, Auto Increment |
| FullName | VARCHAR(100) | Required |
| Email | VARCHAR(200) | Required, Unique |
| PasswordHash | TEXT | BCrypt hashed |
| CreatedAt | DATETIME | Auto set |

**Tasks**

| Column | Type | Notes |
|---|---|---|
| Id | INT | PK, Auto Increment |
| Title | VARCHAR(200) | Required |
| Description | TEXT | Optional |
| DueDate | DATETIME | Required, must be future |
| Status | VARCHAR(20) | Pending / Completed / Expired |
| CreatedAt | DATETIME | Auto set |
| UpdatedAt | DATETIME | Auto updated |

---

## Configuration

Update `appsettings.json` with your MySQL credentials and JWT secret:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=TaskManagement;User=root;Password=your_password_here;"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "TaskManagementAPI",
    "Audience": "TaskManagementAPIUsers",
    "ExpirationInMinutes": 60
  }
}
```

> ⚠️ Never commit real credentials. Use `appsettings.Local.json` or environment variables for secrets in production.

---

## Steps to Run

### 1. Clone the Repository

```powershell
git clone https://github.com/AdithRavindran0fficial/Task-Management-API.git
cd "Task Management API"
```

### 2. Restore Packages

```powershell
dotnet restore
```

### 3. Configure the Database

Edit `appsettings.json` with your MySQL connection string, then run migrations:

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Run the Application

```powershell
dotnet run
```

Or press **F5** in Visual Studio. The browser will automatically open to:

```
http://localhost:5006/swagger
```

---

## API Endpoints

### Auth (Public)

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and receive a JWT token |

### Tasks (Requires JWT)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/tasks` | Get all tasks (filter + paginate) |
| GET | `/api/tasks/{id}` | Get task by Id |
| POST | `/api/tasks` | Create a new task |
| PUT | `/api/tasks/{id}` | Update an existing task |
| DELETE | `/api/tasks/{id}` | Delete a task |

#### Query Parameters for `GET /api/tasks`

| Parameter | Type | Default | Description |
|---|---|---|---|
| `status` | string | - | Filter by `Pending`, `Completed`, or `Expired` |
| `pageNumber` | int | 1 | Page number |
| `pageSize` | int | 10 | Items per page (max 100) |

---

## Request & Response Examples

### Register

**POST** `/api/auth/register`

```json
{
  "fullName": "Adith Ravindran",
  "email": "adith@example.com",
  "password": "Pass@123"
}
```

**Response**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "User registered successfully.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",
    "expiration": "2025-01-01T13:00:00Z",
    "email": "adith@example.com",
    "fullName": "Adith Ravindran"
  }
}
```

### Login

**POST** `/api/auth/login`

```json
{
  "email": "adith@example.com",
  "password": "Pass@123"
}
```

### Create Task

**POST** `/api/tasks`
_Header:_ `Authorization: Bearer <token>`

```json
{
  "title": "Complete project report",
  "description": "Write and submit the final report",
  "dueDate": "2025-12-31T18:00:00Z"
}
```

**Response**
```json
{
  "success": true,
  "statusCode": 201,
  "message": "Task created successfully.",
  "data": {
    "id": 1,
    "title": "Complete project report",
    "description": "Write and submit the final report",
    "dueDate": "2025-12-31T18:00:00Z",
    "status": "Pending",
    "createdAt": "2025-07-02T10:00:00Z",
    "updatedAt": "2025-07-02T10:00:00Z"
  }
}
```

### Get All Tasks (with filter & pagination)

**GET** `/api/tasks?status=Pending&pageNumber=1&pageSize=5`

**Response**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Tasks retrieved successfully.",
  "data": {
    "data": [ { "id": 1, "title": "...", "status": "Pending" } ],
    "pageNumber": 1,
    "pageSize": 5,
    "totalCount": 12,
    "totalPages": 3,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### Error Response

```json
{
  "success": false,
  "statusCode": 404,
  "message": "Task with Id 99 not found.",
  "data": null
}
```

---

## Background Job

`ExpiredTaskBackgroundService` runs **every 1 minute** automatically on startup.

**Logic:**
```
IF task.Status == "Pending" AND task.DueDate < DateTime.UtcNow
    SET task.Status = "Expired"
```

No manual trigger is needed — it runs silently in the background as a hosted service.

---

## Assumptions

1. **All DateTime values are in UTC.** Clients should send and expect UTC timestamps.
2. **DueDate must be in the future** when creating or updating a task. Past dates are rejected with `400 Bad Request`.
3. **Passwords are hashed with BCrypt** and are never stored in plain text.
4. **No ASP.NET Core Identity** is used. User management is handled manually with a custom `Users` table.
5. **JWT tokens expire in 60 minutes** by default (configurable in `appsettings.json`).
6. **All task endpoints require a valid JWT token.** Auth endpoints (`/register`, `/login`) are public.
7. **Status values are case-insensitive** on input (`pending`, `Pending`, `PENDING` all work).
8. **The background job uses a scoped service factory** to avoid DI lifetime conflicts with `DbContext`.
9. **Swagger UI is only enabled in the Development environment.**
10. **Page size is capped at 100** to prevent excessive data retrieval.
