# Carware Backend Solution

This repository contains the backend solution for the **CarWare** application, built using **C#/.NET** and structured according to **Clean Architecture** principles. This ensures separation of concerns, testability, and maintainability.

---

## 1. Architecture Overview


The solution is divided into four main projects (layers) to ensure minimal coupling and high cohesion:

| Layer                  | Purpose                                                       | References                     |
| ---------------------- | ------------------------------------------------------------ | ------------------------------- |
| **Domain** 🏛️         | Core business entities, value objects, and interfaces         | None (independent core)         |
| **Infrastructure** 🏗️ | EF Core DbContext, repositories, Identity, external services  | `Domain`                        |
| **Application** ⚙️     | Business logic, use cases (Commands/Queries), DTOs           | `Domain, Infrastructure`        |
| **API** 🌐             | Web API controllers, DI setup, configuration, middleware     | `Application`                   |

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

**Happy coding! 🚗💻**
