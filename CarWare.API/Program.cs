using CarWare.API.Middlewares;
using CarWare.Application.Common.helper;
using CarWare.Application.Common.Security;
using CarWare.Application.Interfaces;
using CarWare.Application.Mapping;
using CarWare.Application.Services;
using CarWare.Application.Services.ServiceRequests;
using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
using CarWare.Infrastructure.Repositories;
using CarWare.Infrastructure.Seed;
using CarWare.Infrastructure.UnitOfWork;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

namespace CarWare.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Kestrel Ports
            //builder.WebHost.ConfigureKestrel(options =>
            //{
            //    options.Configure(builder.Configuration.GetSection("Kestrel"));
            //    options.ListenAnyIP(8080);
            //    options.ListenAnyIP(8081, listenOptions =>
            //    {
            //        listenOptions.UseHttps("/https/carware.pfx", "123456");
            //    });
            //});

            // builder.WebHost.ConfigureKestrel(options =>
            // {
            //     options.ListenAnyIP(8080);
            // });

            var jwtKey = builder.Configuration["JWT:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("JWT Key is missing");

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Register Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            // Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // JWT Settings
            builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));

            // JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JWT").Get<JWT>();
            var key = Encoding.UTF8.GetBytes(jwtSettings!.Key);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            })
            .AddGoogle("Google", googleOptions =>
            {
                googleOptions.ClientId = builder.Configuration["Google:ClientId"];
                googleOptions.ClientSecret = builder.Configuration["Google:ClientSecret"];
                googleOptions.CallbackPath = "/auth/google-callback";
            });

            // HttpContext accessor
            builder.Services.AddHttpContextAccessor();

            // Hangfire
            builder.Services.AddHangfire(config =>
                config.UseSqlServerStorage(
                    builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddHangfireServer();

            // Memory cache
            builder.Services.AddDistributedMemoryCache();

            // Custom services
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IFcmService, FcmService>();

            // Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Role
            builder.Services.AddScoped<IRoleService, RoleService>();

            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<IVehicleService, VehicleService>();
            builder.Services.AddScoped<IMaintenanceTypeService, MaintenanceTypeService>();
            builder.Services.AddScoped<IServiceCenterService, ServiceCenterService>();
            builder.Services.AddScoped<IMaintenanceReminderService, MaintenanceReminderService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IProfileService, ProfileService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<ISetupService, SetupService>();
            builder.Services.AddScoped<IServiceRequestQueryService, ServiceRequestQueryService>();
            builder.Services.AddScoped<IServiceRequestWorkflowService, ServiceRequestWorkflowService>();
            builder.Services.AddScoped<IServiceRequestHistoryService, ServiceRequestHistoryService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IOtpGenerator, OtpGenerator>();
            builder.Services.AddScoped<CarImageService>();
            builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

            // AutoMapper
            builder.Services.AddAutoMapper(typeof(AuthProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // Jobs
            builder.Services.AddScoped<NotificationJobs>();
            builder.Services.AddScoped<MaintenanceReminderJob>();

            // Firebase
            // NOTE: This currently expects the credential file to exist at runtime.
            // We will move this to secure configuration/environment-based credentials in a later phase.
            var firebasePath = Path.Combine(Directory.GetCurrentDirectory(), "Firebase", "firebase-key.json");

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(firebasePath)
            });

            // CORS: allow only explicitly configured frontend origins.
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                ?? Array.Empty<string>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CarWareCorsPolicy", policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            var app = builder.Build();

            // Custom Middleware
            app.UseMiddleware<ExceptionMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Apply the single, explicitly configured CORS policy.
            app.UseCors("CarWareCorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            #region Update Database
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var logger = services.GetRequiredService<ILogger<Program>>();

                    // Apply pending migrations and seed data.
                    await context.Database.MigrateAsync();
                    await StoreContextSeed.SeedAsync(context);

                    logger.LogInformation("Database migrated and seeded successfully.");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Error during migration or seeding.");
                }
            }
            #endregion

            // Create roles when the app starts.
            await CreateRolesAsync(app);

            // NOTE: Hangfire Dashboard authorization is a separate security-baseline task.
            // Do not expose this dashboard publicly in production until an authorization filter is added.
            app.UseHangfireDashboard("/hangfire");

            RecurringJob.AddOrUpdate<MaintenanceReminderJob>(
                "maintenance-reminders",
                job => job.Execute(),
                "*/5 * * * *");

            app.MapControllers();

            app.Run();
        }

        // Helper for seeding roles
        private static async Task CreateRolesAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "SUPERADMIN", "CENTERADMIN", "USER" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
