# 🚗 carware_backend 

A professional **ASP.NET Core Web API** backend for the **CarWare** platform, designed using **Clean Architecture** principles to ensure scalability, maintainability, and testability.

---

## 📌 Overview

CarWare Backend provides RESTful APIs to manage vehicles, users, maintenance schedules, and related business operations. The solution follows **Clean Architecture**, enforcing a clear separation of concerns and a dependency flow toward the core domain.

**Key Goals:**
- High maintainability & scalability
- Testable business logic
- Clear separation of responsibilities
- Secure authentication using JWT

---

## 🏗️ Architecture

The solution is organized into four main layers:

| Layer | Description | Depends On |
|------|-------------|-----------|
| **Domain** 🏛️ | Core business entities, value objects, enums, and interfaces | None |
| **Application** ⚙️ | Business logic, use cases, DTOs, validation, CQRS | Domain |
| **Infrastructure** 🏗️ | Database access, EF Core, Identity, external services | Domain, Application |
| **API** 🌐 | Controllers, middleware, configuration, dependency injection | Application |

---

## 🧠 Design Patterns & Practices

- Clean Architecture
- Repository Pattern
- Unit of Work
- Dependency Injection
- DTO Mapping
- SOLID Principles
- RESTful API design

---

## 🔐 Authentication & Security

- ASP.NET Core Identity
- JWT Bearer Authentication
- Role-based authorization
- Secure password hashing

---

## 🛠️ Tech Stack

- **.NET 8 / ASP.NET Core Web API**
- **Entity Framework Core**
- **SQL Server**
- **ASP.NET Core Identity**
- **JWT Authentication**
- **Swagger / OpenAPI**

---

## 📦 NuGet Packages

Key packages used in the project:

- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Swashbuckle.AspNetCore`

---

## 🚀 Getting Started

### 1️⃣ Clone the Repository

```bash
git clone https://github.com/your-repo/CarWare.Backend.git
cd CarWare.Backend
```

### 2️⃣ Configure Database

Update `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=CarWareDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 3️⃣ Apply Migrations

```bash
dotnet ef database update
```

### 4️⃣ Run the Application

```bash
dotnet run
```

Swagger UI:
```
https://localhost:{port}/swagger
```

---

## 🔄 Development Workflow

### ✅ Branching Strategy

- `main` → Production-ready code
- `develop` → Active development
- `feature/*` → New features
- `bugfix/*` → Bug fixes

---

## 📝 Commit Message Convention

This project follows **Conventional Commits**:

- `feat:` New feature
- `fix:` Bug fix
- `refactor:` Code restructuring
- `docs:` Documentation updates
- `test:` Adding or updating tests

**Examples:**
```
feat: add vehicle maintenance endpoint
fix: resolve token expiration issue
refactor: improve repository abstraction
```

---

## 🧪 Testing (Planned / Optional)

- Unit testing with xUnit
- Integration testing for APIs
- In-memory database for tests

---

## 📂 Project Structure

```text
CarWare.Backend
│
├── CarWare.Domain
├── CarWare.Application
├── CarWare.Infrastructure
└── CarWare.API
```

---

## 📄 License

This project is licensed under the MIT License.

---

## 👨‍💻 Author

**CarWare Backend Team**  
Built with ❤️ using ASP.NET Core

---

**Happy Coding! 🚀**

