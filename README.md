# Carware Backend Solution

This repository contains the backend solution for the **CarWare** application, built using **C#/.NET** and structured according to **Clean Architecture** principles. This ensures separation of concerns, testability, and maintainability.

---

## 1. Architecture Overview


The solution is divided into four main projects (layers) to ensure minimal coupling and high cohesion:

| Layer                  | Purpose                                                      | References                      |
| ---------------------- | ------------------------------------------------------------ | ------------------------------- |
| **Domain** 🏛️         | Core business entities, value objects, and interfaces        | None (independent core)         |
| **Application** ⚙️     | Business logic, use cases (Commands/Queries), DTOs           | `Domain`                        |
| **Infrastructure** 🏗️ | EF Core DbContext, repositories, Identity, external services | `Domain`                        |
| **API** 🌐             | Web API controllers, DI setup, configuration, middleware     | `Application`, `Infrastructure` |


---

## 2. Development Workflow & Guidelines

### 2.1 Commit Message Standard (Conventional Commits)

Commit messages must follow:

- `feat`: New feature or major enhancement  
  Example: `feat: implement vehicle tracking endpoint`

- `fix`: Bug fix  
  Example: `fix: correct VIN validation logic`

- `refactor`: Code restructuring without behavior change  
  Example: `refactor: simplify login service logic`

### 2.2 Pull Request

- Always pull latest `develop` before creating a branch.  

---

## 3. Getting Started

### 3.1 Clone Repository

git clone https://github.com/your-repo/CarWare.Backend.git

---

## 3.2 Key Packages

Install the following packages via **NuGet Package Manager**:

- **`Microsoft.EntityFrameworkCore.SqlServer`**  
  SQL Server provider for **Entity Framework Core**.

- **`Microsoft.EntityFrameworkCore.Tools`**  
  Enables **EF Core migrations** and database management tools.

- **`Microsoft.AspNetCore.Identity.EntityFrameworkCore`**  
  Integrates **ASP.NET Identity** for authentication and user management.

- **`Microsoft.AspNetCore.Authentication.JwtBearer`**  
  Provides support for **JWT token-based authentication**.


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

```powershell
Add-Migration InitialCreate        # Core tables
Add-Migration IdentityInitial      # Identity tables
```
### Step 2: Update Database

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
