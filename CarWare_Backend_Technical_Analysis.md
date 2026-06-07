# CarWare Backend - Complete Technical Analysis

**Repository**: CarWaree/carware-backend  
**Language**: C#  
**Framework**: .NET 8 with ASP.NET Core  
**Description**: Backend API for managing vehicles, services, and maintenance centers

---

## TABLE OF CONTENTS

1. Overall System Architecture
2. Project Structure Explanation
3. Full System Flow
4. Database Design
5. API Endpoints
6. Design Patterns & Best Practices
7. Simple Real-Life Explanation
8. Interview Preparation - Q&A
9. Common Gotchas & Tips

---

## 1. OVERALL SYSTEM ARCHITECTURE

### Purpose of the Project

CarWare is a **vehicle maintenance management and booking platform** that connects car owners with service centers. It provides:

- **For Users**: Vehicle management, appointment booking, maintenance tracking, and service request handling
- **For Service Centers**: Booking management, service catalog, time slot management, and dashboard analytics
- **Multi-role Authentication**: User, CenterAdmin, and other roles with role-based access control

### High-Level Architecture (Clean Architecture Pattern)

```
┌─────────────────────────────────────────────────────────────────┐
│                    API LAYER (Controllers)                      │
│  AuthController │ VehicleController │ AppointmentController │   │
└────────────────────┬────────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────────┐
│           APPLICATION LAYER (Services & DTOs)                   │
│  ├─ AuthService        ├─ VehicleService    ├─ NotificationSvc │
│  ├─ RoleService        ├─ AppointmentSvc    ├─ DashboardSvc    │
│  └─ ProfileService     └─ ServiceRequestSvc ├─ SetupService    │
└────────────────────┬────────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────────┐
│            DOMAIN LAYER (Entities & Interfaces)                 │
│  - ApplicationUser  - ServiceRequest   - MaintenanceReminder    │
│  - Vehicle          - Appointment      - Notification          │
│  - ServiceCenter    - Device Token     - Payment               │
└────────────────────┬────────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────────┐
│         INFRASTRUCTURE LAYER (Data Access)                      │
│  ├─ UnitOfWork        ├─ GenericRepository  ├─ DbContext       │
│  └─ Specific Repos    └─ EF Core Configs    └─ SQL Server      │
└─────────────────────────────────────────────────────────────────┘
```

### Technologies Used

**Core Framework**
- .NET 8
- ASP.NET Core Web API

**Database & ORM**
- Entity Framework Core (EF Core)
- SQL Server
- Identity Framework (for auth)

**Authentication & Authorization**
- JWT Bearer Token
- Google OAuth 2.0
- Role-Based Access Control (RBAC)

**Caching & Background Jobs**
- Distributed Memory Cache (for OTP, tokens)
- Hangfire (for background jobs)

**Communication**
- Firebase Cloud Messaging (FCM) for push notifications
- Email sender service

**Additional Libraries**
- AutoMapper (for DTOs)
- Swagger/OpenAPI (for API documentation)

---

## 2. PROJECT STRUCTURE EXPLANATION

### Folder Structure & Responsibilities

```
CarWare.API/
├── Program.cs                    # DI Container, Middleware setup
├── Controllers/                  # HTTP endpoints
├── Middlewares/                 # Custom middleware
├── Errors/                      # Global error handling & DTOs
└── wwwroot/                     # Static files

CarWare.Domain/
├── Entities/                    # Database models
├── Interfaces/                  # Repository contracts
├── Enums/                       # Business enums
└── Common/                      # Shared utilities

CarWare.Application/
├── Services/                    # Business logic
├── DTOs/                        # Data Transfer Objects
├── Interfaces/                  # Service contracts
├── Mapping/                     # AutoMapper profiles
└── Common/                      # Helpers, caching, JWT

CarWare.Infrastructure/
├── Context/                     # EF Core DbContext
├── Repositories/                # Data access implementations
├── UnitOfWork/                  # Transaction manager
├── Configurations/              # EF Core entity mappings
├── Seed/                        # Database seeding
└── Migrations/                  # EF Core migrations
```

### Why This Structure?

✅ **Clean Architecture** - Separation of concerns  
✅ **SOLID Principles** - Each layer has single responsibility  
✅ **Testability** - Services can be tested in isolation  
✅ **Maintainability** - Clear dependency flow  
✅ **Scalability** - Easy to add new features  

---

## 3. FULL SYSTEM FLOW (VERY IMPORTANT)

### 🔐 AUTHENTICATION & AUTHORIZATION FLOW

#### A. Registration Flow

```
Frontend Request (POST /api/auth/register)
    ↓
1. AuthController receives RegisterDto
   - Username, Email, Password, FirstName, LastName
    ↓
2. AuthService.RegisterAsync()
   ├─ Validate: username not taken
   ├─ Validate: email not registered
   └─ Map RegisterDto → ApplicationUser
    ↓
3. UserManager.CreateAsync()
   ├─ Hash password with bcrypt
   ├─ Create user in database
   └─ EmailConfirmed = false
    ↓
4. UserManager.AddToRoleAsync(user, "User")
   ├─ Assign default "User" role
   └─ Store in AspNetUserRoles table
    ↓
5. SendEmailOtpAsync()
   ├─ Generate 6-digit OTP
   ├─ Store in DistributedCache (validity: 3 min)
   ├─ Send email with OTP
   └─ Setup failed attempt tracking
    ↓
Response: { userId, email, "Please verify your email" }
```

#### B. Email Verification Flow

```
Frontend Request (POST /api/auth/verify-email-otp)
    ↓
1. VerifyEmailOtpAsync()
   ├─ Find user by email
   ├─ Check if already verified
   ├─ Get OTP from cache
   └─ Track failed attempts (max 5 before lock)
    ↓
2. Validate OTP
   ├─ Compare cached OTP with provided OTP
   ├─ If mismatch: increment failed attempts
   └─ If matches: clear cache
    ↓
3. Mark email verified
   ├─ user.EmailConfirmed = true
   └─ UserManager.UpdateAsync(user)
    ↓
4. Generate tokens
   ├─ Create JWT token via JwtTokenGenerator
   │  ├─ Claims: UserId, Email, FirstName, LastName
   │  ├─ Roles from UserManager.GetRolesAsync()
   │  ├─ Expiration: JWT:AccessTokenDurationMinutes
   │  └─ Sign with SymmetricSecurityKey
   └─ Create RefreshToken
       ├─ Store in RefreshTokens collection
       ├─ Expiration: 7 days
       └─ Can be revoked (RevokedOn tracking)
    ↓
Response: { accessToken, refreshToken }
```

#### C. Login Flow

```
Frontend Request (POST /api/auth/login)
    ↓
1. Find user by email OR username
    ↓
2. Validate password
   ├─ SignInManager.CheckPasswordSignInAsync()
   ├─ Handles: locked out accounts, password hashing
   └─ Returns: succeeded/failed/locked status
    ↓
3. Check email verification
   ├─ if (!user.EmailConfirmed)
   └─ Return error: "Please verify email"
    ↓
4. Generate tokens & roles
   ├─ Get user roles: UserManager.GetRolesAsync()
   ├─ Create refresh token
   ├─ Create JWT token with roles as claims
   └─ Store refresh token in RefreshTokens collection
    ↓
Response: {
  isAuthenticated: true,
  accessToken, refreshToken, roles, firstName, lastName, email
}
```

#### D. JWT Token Usage & Authorization

```
Every Protected Request
    ↓
1. Frontend sends: Authorization: Bearer {jwt-token}
    ↓
2. JwtBearer Middleware intercepts
   ├─ Extract token from header
   ├─ Validate signature using SymmetricSecurityKey
   ├─ Check: Issuer, Audience, Expiration
   └─ Extract claims → HttpContext.User
    ↓
3. Controller method executes
   ├─ [Authorize] checks authentication
   ├─ [Authorize(Roles = "CENTERADMIN")] checks role
   └─ Get current user: User.FindFirst(ClaimTypes...)
    ↓
Resource accessed OR 401 Unauthorized / 403 Forbidden
```

#### E. Password Reset Flow

```
User forgot password
    ↓
1. ForgetPassword - Send OTP via email
   ├─ Generate OTP
   ├─ Cache key: CacheKeys.ResetOtp(userId)
   ├─ Validity: 3 minutes
   └─ Setup attempt tracking (max 5)
    ↓
2. VerifyOtpAsync - Verify OTP
   ├─ Validate OTP against cache
   ├─ Generate reset token via UserManager
   └─ Store reset token + userId in cache (10 min)
    ↓
3. ResetPasswordAsync - Reset password
   ├─ Retrieve reset token from cache
   ├─ UserManager.ResetPasswordAsync()
   ├─ Clear cache
   └─ Password reset successful
    ↓
Response: "Password reset successfully"
```

### 📱 APPOINTMENT BOOKING FLOW

```
User wants to book vehicle maintenance
    ↓
1. Get available service centers
   └─ Returns: List<ServiceCenterDto>
    ↓
2. Get services offered by selected center
   └─ Query ProviderServices table
    ↓
3. Get available time slots
   ├─ Query Slots table (filtered by center)
   ├─ Return: Day of week + start/end times
   └─ Example: Monday 09:00-10:00, 10:00-11:00
    ↓
4. Create appointment
   ├─ POST /api/appointment
   ├─ Include: VehicleId, ServiceCenterId, ServiceId, Date, TimeSlot
   └─ UserId extracted from JWT claim
    ↓
5. Store in database
   ├─ Create Appointment entity
   │  ├─ Status = AppointmentStatus.Pending
   │  └─ Store all relationships
   └─ UnitOfWork.CompleteAsync() → SaveChanges()
    ↓
Response: { appointmentId, status: "Pending" }
```

### 🔧 SERVICE REQUEST WORKFLOW

#### Overview: The core business process

```
Appointment → ServiceRequest (multi-step workflow)
                     ↓
        Pending → Accepted → Completed
           ↓         ↓           ↓
        Rejected at any step, all with reasons/notes
```

#### Accept Service Request

```
CenterAdmin clicks "Accept" on pending request
    ↓
PATCH /api/service-requests/{id}/accept
{
  "technicianId": "tech-123",
  "estimatedCompletion": "2025-06-15T14:30:00Z",
  "estimatedCost": 250.00
}
    ↓
Update ServiceRequest:
├─ ServiceStatus = Accepted
├─ TechnicianId = provided technician
├─ EstimatedCompletion = provided date
├─ EstimatedCost = provided amount
└─ AcceptedAt = DateTime.UtcNow
    ↓
Response: "Request accepted successfully"
```

#### Complete Service Request

```
Technician finishes the work
    ↓
PATCH /api/service-requests/{id}/complete
{
  "technicianNotes": "Oil change completed. Filter replaced.",
  "finalCost": 250.00
}
    ↓
Update ServiceRequest:
├─ ServiceStatus = Completed
├─ TechnicianNotes = provided notes
├─ TotalPrice = finalCost
├─ CompletedAt = DateTime.UtcNow
└─ Save to database
    ↓
Dashboard stats update:
├─ "Today's Requests" counter incremented
├─ "Today's Income" sum updated
└─ Calendar view refreshed
    ↓
Response: "Request completed successfully"
```

---

## 4. DATABASE DESIGN

### Main Entities & Relationships

**Core Users**
- ApplicationUser (IdentityUser)
  - Collections: ServiceRequests, Vehicles, Appointments, Notifications, DeviceTokens

**Vehicle Management**
- Brand (e.g., "Toyota")
- Model (e.g., "Camry", linked to Brand)
- Vehicle (Name, Year, Color, linked to User, Brand, Model)

**Maintenance Tracking**
- MaintenanceType (e.g., "Oil Change", "Brake Fluid")
- MaintenanceReminder (DueDate, NextDueDate, RepeatUnit, RepeatEvery)

**Service Centers**
- ServiceCenter (Name, Location, Phone, WorkingFrom, WorkingTo)
- Slot (DayOfWeek, StartTime, EndTime, IsActive)
- ProviderServices (junction: ServiceCenter ↔ MaintenanceType)

**Appointments & Requests**
- Appointment (Date, TimeSlot, Status, linked to User, Vehicle, ServiceCenter)
- ServiceRequest (workflow tracking, linked to Appointment, User, Technician)
- ServiceRequestItem (junction with MaintenanceType, includes Description)

**Payments & Notifications**
- Payment (AppointmentId, Amount, Method, Status)
- Notification (Title, Body, Type, Channel, IsRead, IsSent)
- DeviceToken (FCM registration token, Platform, IsActive)

### Why This Design?

✅ **Normalization** - Reduces data duplication  
✅ **Referential Integrity** - Foreign keys enforce relationships  
✅ **Workflow Tracking** - ServiceRequest has full lifecycle tracking  
✅ **Multi-tenancy Support** - ServiceCenter isolation for admins  
✅ **Audit Trail** - CreatedAt, UpdatedAt, AcceptedAt, etc.  

---

## 5. API ENDPOINTS

### Authentication Endpoints

| Method | Endpoint | Auth | Purpose |
|--------|----------|------|---------|
| POST | `/api/auth/register` | ❌ | Register new user |
| POST | `/api/auth/verify-email-otp` | ❌ | Verify email OTP |
| POST | `/api/auth/resend-email-otp` | ❌ | Resend verification OTP |
| POST | `/api/auth/login` | ❌ | Login with credentials |
| POST | `/api/auth/forgot-password` | ❌ | Request password reset |
| POST | `/api/auth/verify-otp` | ❌ | Verify reset OTP |
| POST | `/api/auth/reset-password` | ❌ | Reset password |
| POST | `/api/auth/refresh-token` | ❌ | Get new access token |
| POST | `/api/auth/change-password` | ✅ | Change own password |
| POST | `/api/auth/google-login` | ❌ | Google OAuth login |

### Vehicle Endpoints

| Method | Endpoint | Auth | Purpose |
|--------|----------|------|---------|
| GET | `/api/vehicles` | ✅ | Get my vehicles |
| POST | `/api/vehicles` | ✅ | Create vehicle |
| GET | `/api/vehicles/{id}` | ✅ | Get vehicle details |
| PUT | `/api/vehicles/{id}` | ✅ | Update vehicle |
| DELETE | `/api/vehicles/{id}` | ✅ | Delete vehicle |

### Appointment Endpoints

| Method | Endpoint | Auth | Purpose |
|--------|----------|------|---------|
| GET | `/api/appointment/my` | ✅ | Get my appointments |
| POST | `/api/appointment` | ✅ | Create appointment |
| GET | `/api/service-centers` | ✅ | Get all centers |
| GET | `/api/service-centers/{id}/slots` | ✅ | Get center slots |
| GET | `/api/service-centers/{id}/services` | ✅ | Get center services |

### Service Request Endpoints (Center Admins)

| Method | Endpoint | Auth | Purpose |
|--------|----------|------|---------|
| GET | `/api/service-requests` | ✅ CENTERADMIN | List requests (with filters) |
| GET | `/api/service-requests/{id}` | ✅ CENTERADMIN | Get request details |
| PATCH | `/api/service-requests/{id}/accept` | ✅ CENTERADMIN | Accept request |
| PATCH | `/api/service-requests/{id}/reject` | ✅ CENTERADMIN | Reject request |
| PATCH | `/api/service-requests/{id}/complete` | ✅ CENTERADMIN | Complete request |

---

## 6. DESIGN PATTERNS & BEST PRACTICES

### 1. Repository Pattern

The Repository pattern abstracts database access, making it easy to mock for testing and change implementations without affecting business logic.

```csharp
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int Id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public interface IServiceRequestRepository : IGenericRepository<ServiceRequest>
{
    IQueryable<ServiceRequest> GetByCenterId(int centerId);
    Task<int> CountTodayByCenterIdAsync(int centerId);
}
```

### 2. Unit of Work Pattern

Manages transactions across multiple repositories, ensuring atomic commits.

```csharp
public interface IUnitOfWork : IAsyncDisposable
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
    IVehicleRepository VehicleRepository { get; }
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}
```

### 3. Dependency Injection Pattern

Enables loose coupling, testability, and centralized configuration.

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### 4. Service Layer Pattern

Controllers are thin; services contain business logic for reusability.

### 5. DTO (Data Transfer Object) Pattern

Decouples domain models from API contracts, improving security and flexibility.

### 6. Result Pattern (for error handling)

Explicit error handling without exceptions for business logic.

```csharp
public class Result<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}
```

### 7. JWT with Claims

Stateless authentication with role-based authorization built into tokens.

### 8. AutoMapper

Reduces boilerplate for DTO transformations.

### 9. EF Core Configurations

Explicit relationship definitions prevent accidental cascades.

### 10. Caching Pattern

OTP and tokens use distributed memory cache with automatic expiration.

---

## 7. SIMPLE REAL-LIFE EXPLANATION

### 🚗 For a Junior Developer

Imagine **CarWare is like Uber for car maintenance**:

1. **User signs up** → App creates account, sends OTP to verify email
2. **User adds their car** → Stores: Brand (Toyota), Model (Camry), Year, Color
3. **User searches for service** → Sees available service centers + available time slots
4. **User books appointment** → "I want oil change at Provider 1, Monday 10 AM"

**Behind the scenes:**
- Creates an "Appointment" record (booking confirmed)
- Automatically creates a "ServiceRequest" (workflow tracking)
- Service center admin sees it in their dashboard

**Service center admin logs in** → Dashboard shows:
- "Pending requests: 5" (new bookings waiting for acceptance)
- Today's income: $1,250 (completed jobs)
- Calendar: appointments for the week

**Admin reviews the request** → Clicks "Accept"
- Assigns a technician
- Sets estimated time: "Will be done Monday 11 AM"
- Sets estimated cost: "$150"

**Customer is notified** (via push notification or email): "Your booking confirmed!"

**Technician works on the car** → When done, clicks "Complete"
- Updates status to "Completed"
- Records actual cost: "$140" (less than estimate)
- Adds notes: "Oil changed, filter replaced"

**Customer sees history** → "Oil Change - Completed - $140"

**System sends maintenance reminder** → "Next oil change due in 5,000 km"

---

## 8. INTERVIEW PREPARATION - Q&A

### Q1: How does the authentication system work?

**Answer:**
The system uses JWT (JSON Web Tokens) with refresh token rotation:

1. **Registration**: User provides credentials → password hashed with bcrypt → OTP sent to email
2. **Email Verification**: User verifies OTP → email marked as confirmed → JWT + Refresh token generated
3. **Login**: Credentials validated → JWT created with claims (userId, roles, email) → both tokens returned
4. **Each Request**: JWT verified using SymmetricSecurityKey → claims extracted → user identified
5. **Token Refresh**: When JWT expires, client sends refresh token → new JWT issued → old refresh token revoked

**Why this is good**: Stateless (no session storage), secure (tokens are cryptographically signed), scalable (no server-side session lookup).

---

### Q2: Explain the service request workflow

**Answer:**
Service request has 4 states:

```
Pending → Accepted → Completed
   ↓         ↓
Rejected (terminal)
```

- **Pending**: Just created, awaiting service center review
- **Accepted**: Service center assigned technician, estimated cost/time set
- **Completed**: Work done, final cost recorded, technician notes added
- **Rejected**: Center decided not to proceed

Each state change is tracked with timestamps for analytics and history.

---

### Q3: How is role-based access control implemented?

**Answer:**
The system uses ASP.NET Identity + JWT Claims:

1. **Roles exist**: "User", "CENTERADMIN", etc. (stored in AspNetRoles table)
2. **Users assigned roles**: Via UserManager.AddToRoleAsync()
3. **Extra claims**: For CENTERADMIN, a claim "ServiceCenterId" is added
4. **JWT includes roles**: When token created, all roles are added as "role" claims
5. **Authorization checks**: [Authorize(Roles = "CENTERADMIN")]

---

### Q4: How do you prevent N+1 queries?

**Answer:**
Using **explicit eager loading** in repositories:

```csharp
// ❌ N+1 problem - separate query per item
foreach (var req in requests)
    var user = req.User;

// ✅ Solution - Include related data upfront
var requests = await _context.ServiceRequests
    .Include(x => x.User)
    .Include(x => x.Vehicle)
    .Include(x => x.ServiceCenter)
    .Include(x => x.ServiceRequestServices)
        .ThenInclude(s => s.MaintenanceType)
    .ToListAsync();
```

Only 1 query with joins instead of N+1 queries.

---

### Q5: How is caching used for OTP?

**Answer:**
```csharp
// Store OTP with automatic expiration
await _cache.SetStringAsync(
    $"email_verify_otp:{userId}",
    otp,
    new DistributedCacheEntryOptions 
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
    }
);
```

Fast access, auto-expires, no cleanup code needed.

---

### Q6: How are permissions scoped per service center?

**Answer:**
```csharp
private int CenterId => 
    _currentUserService.ServiceCenterId 
    ?? throw new Exception("ServiceCenterId not found");

// Only queries this center's data
var requests = _unitOfWork.ServiceRequestRepository
    .GetByCenterId(CenterId);
```

The ServiceCenterId claim is set server-side during login, can't be modified by client.

---

### Q7: What happens when an appointment is created?

**Answer:**
```csharp
var appointment = new Appointment
{
    Date = dto.Date,
    TimeSlot = dto.TimeSlot,
    Status = AppointmentStatus.Pending,
    UserId = userId,
    VehicleId = dto.VehicleId,
    ServiceCenterId = dto.ServiceCenterId,
    ServiceId = dto.ServiceId
};
await _unitOfWork.AppointmentRepository.AddAsync(appointment);
await _unitOfWork.CompleteAsync();
```

Flow: Appointment (user's booking) → ServiceRequest (internal workflow for center).

---

### Q8: How does the notification system work?

**Answer:**
```csharp
await _notificationService.SendAsync(new SendNotificationDto
{
    UserId = userId,
    Title = "Appointment Confirmed",
    Body = "Your appointment at provider 1 on Monday 10 AM is confirmed",
    Channel = NotificationChannel.Push
});
```

Multi-channel: Push (mobile), Email, In-app. Tracks IsSent and SentAt timestamps.

---

### Q9: How is data validation done?

**Answer:**
Multiple layers:

1. **DTO Validation** (client-side + server-side)
   - [Required], [StringLength], custom attributes

2. **Service Layer Validation**
   - Business logic checks (vehicle ownership, date validity)

3. **Database Constraints**
   - NOT NULL, MAX length, unique constraints

Layered approach prevents invalid data at every stage.

---

### Q10: How does the Unit of Work pattern help?

**Answer:**
Multiple operations, single transaction:

```csharp
var request = await _unitOfWork.ServiceRequestRepository.GetByIdAsync(requestId);
request.ServiceStatus = ServiceRequestStatus.Completed;
_unitOfWork.ServiceRequestRepository.Update(request);

var payment = new Payment { ... };
await _unitOfWork.Repository<Payment>().AddAsync(payment);

var notification = new Notification { ... };
await _unitOfWork.NotificationRepository.AddAsync(notification);

await _unitOfWork.CompleteAsync(); // All or nothing - single commit
```

**Benefits**: If any operation fails, all rollback. Single SaveChanges() call. Atomicity guaranteed.

---

### Q11: How do you handle errors globally?

**Answer:**
```csharp
public class Result<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}

// In services:
return Result<AppointmentDto>.Fail("Vehicle not found");

// In controllers:
if (!result.Success)
    return BadRequest(new ApiResponse(400, result.Error));
```

Explicit error handling, no exceptions bubbling up, consistent API responses.

---

### Q12: What about security - how are passwords handled?

**Answer:**
```csharp
// Identity Framework handles password hashing
await _userManager.CreateAsync(user, password);
// - Password hashed with PBKDF2
// - Salt included automatically
// - Hash stored, NOT plain password

// Password verification
var result = await _signInManager.CheckPasswordSignInAsync(user, password, true);
// "true" = lock account after failed attempts (brute force protection)
```

Never store plain passwords. Use framework's built-in hashing. Enforce strong requirements.

---

### Q13: How do you test a service?

**Answer:**
```csharp
[TestClass]
public class AppointmentServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private AppointmentService _service;
    
    [TestInitialize]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new AppointmentService(_unitOfWorkMock.Object);
    }
    
    [TestMethod]
    public async Task CreateAppointment_WithInvalidVehicle_ReturnsFail()
    {
        var dto = new CreateAppointmentDto { VehicleId = 999 };
        _unitOfWorkMock.Setup(x => x.VehicleRepository.GetByIdAsync(999))
            .ReturnsAsync((Vehicle)null);
        
        var result = await _service.AddAppointmentAsync(dto, "user-1");
        
        Assert.IsFalse(result.Success);
    }
}
```

DI benefit: Can inject mocks, test in isolation, no database needed.

---

### Q14: How is pagination handled?

**Answer:**
```csharp
public class ServiceRequestQueryParams
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
}

var skip = (page - 1) * limit;
var data = await query
    .Skip(skip)
    .Take(limit)
    .ToListAsync();

var total = await query.CountAsync();
```

Only fetches requested page from database - efficient and scalable.

---

### Q15: Biggest challenge & solution?

**Answer:**
**Challenge**: Managing complex service request workflow across multiple entities.

**Solution**:
1. Single workflow service handles state transitions
2. Result pattern for explicit success/failure messages
3. Unit of Work for transactions
4. Notifications for event-driven design
5. Specialized repositories with proper eager loading

---

## 9. COMMON GOTCHAS & TIPS

| Issue | Solution |
|-------|----------|
| N+1 queries | Use `.Include()` in repositories |
| Stale data | Always refresh after updates in same transaction |
| Sensitive data in logs | Don't log passwords/tokens |
| Token expiration | Client should refresh before expiration |
| Race conditions | Use transactions (UnitOfWork) |
| Missing validation | Validate at multiple layers |
| Unbounded queries | Always add pagination |
| No audit trail | Track CreatedAt, UpdatedAt, and workflow timestamps |

---

## SUMMARY

**Technology Stack**: .NET 8, EF Core, SQL Server, JWT, Firebase FCM, AutoMapper

**Key Takeaways**:
- Clean architecture with clear separation of concerns
- JWT-based stateless authentication with refresh tokens
- Multi-role authorization with service center scoping
- Complex workflow tracking for service requests
- Multi-channel notification system
- Comprehensive error handling with Result pattern
- Database optimization through eager loading and pagination
- Testable design through dependency injection

**For Technical Discussions**: Focus on architecture decisions, authentication flow, service request workflow, and design patterns used throughout the system.

---

Generated: 2026-06-07  
Repository: CarWaree/carware-backend  
Language: C# (.NET 8)
