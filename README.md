# CarWare Backend Solution

This repository contains the backend solution for the **CarWare** application, built using **C#/.NET** and structured according to **Clean Architecture** principles. This ensures separation of concerns, testability, and maintainability.

---

## 1. Architecture Overview

The solution is divided into four main projects (layers) to ensure minimal coupling and high cohesion:

- **CarWare.Domain**  
  Core business entities, value objects, and interfaces. (Core Layer)

- **CarWare.Application**  
  Business logic, use cases (Commands/Queries), DTOs, and repository interfaces. References: `CarWare.Domain`

- **CarWare.Infrastructure**  
  Implementation of external dependencies (Entity Framework Context, Repositories, Identity). References: `CarWare.Domain`

- **CarWare.API**  
  Presentation layer (Web API Controllers), Dependency Injection setup, and configuration. References: `CarWare.Application`, `CarWare.Infrastructure`

---

## 2. Development Workflow & Guidelines

### 2.1 Commit Message Standard (Conventional Commits)

Commit messages must follow:

```
<type>: <subject>
```

- `feat`: New feature or major enhancement  
  Example: `feat: implement vehicle tracking endpoint`

- `fix`: Bug fix  
  Example: `fix: correct VIN validation logic`

- `refactor`: Code restructuring without behavior change  
  Example: `refactor: simplify login service logic`

### 2.2 Pull Request & Clean Code

- Always pull latest `develop` before creating a branch.  

---

## 3. Getting Started

### 3.1 Clone Repository

Clone the repo via Visual Studio or GitHub and open the solution.

---

## 3.2 Key Packages

Install these packages via **NuGet Package Manager**:

-   Microsoft.EntityFrameworkCore.SqlServer  - SQL Server provider for EF Core.

- Microsoft.EntityFrameworkCore.Tools  - Enables migrations and database updates.

- Microsoft.AspNetCore.Identity.EntityFrameworkCore  - Integrates ASP.NET Identity for authentication and user management.

- Microsoft.AspNetCore.Authentication.JwtBearer  - Enables JWT token-based authentication.

---

## 3.3 Configuration (`appsettings.json`)

Add **ConnectionStrings** and **JWT settings** to `CarWare.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\mssqllocaldb;Database=CarWareDb;Trusted_Connection=True;MultipleActiveResultSets=true"
},
"Jwt": {
  "Key": "YOUR_SECRET_KEY_HERE",
  "Issuer": "CarWareAPI",
  "Audience": "CarWareAPIUsers",
  "DurationInMinutes": 60
}
```

> Replace `YOUR_SECRET_KEY_HERE` with a strong secret key.

---

## 3.4 Database & Identity Migrations (Package Manager Console)

Open **Package Manager Console (PMC)** in Visual Studio and select the **Default Project** as `CarWare.Infrastructure`.

### Step 1: Add Migrations

**For SQL database (core tables):**

```powershell
Add-Migration InitialCreate
```

**For Identity tables (users, roles):**

```powershell
Add-Migration IdentityInitial
```

> You can separate migrations by feature if needed.

### Step 2: Update Database

**Apply SQL migration:**
```powershell
Update-Database
```

---
## 3.5 SQL Server Setup (`Program.cs`)

**Register SQL Server**

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
```
---

---
## 3.6 Identity & JWT Setup (`Program.cs`)

**Register Identity Services:**

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
```

**Configure JWT Authentication:**

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
```
---

**Happy coding! 🚗💻**