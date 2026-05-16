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

            builder.WebHost.UseUrls("https://localhost:7136", "http://0.0.0.0:7136");

            var jwtKey = builder.Configuration["JWT:Key"];

            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("JWT Key is missing");

            //Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Register Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            //Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            //JWT Settings
            builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));

            //JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JWT").Get<JWT>();
            var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

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

            //Hangfire 
            builder.Services.AddHangfire(config =>
                config.UseSqlServerStorage(
                    builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddHangfireServer();

            //Memory Cashe
            builder.Services.AddDistributedMemoryCache();

            //Custom Service [Email Sender]
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IFcmService, FcmService>(); 

            //Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            //Role
            builder.Services.AddScoped<IRoleService, RoleService>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            //vehicleservice
            builder.Services.AddScoped<IVehicleService, VehicleService>();
            //maintenancetypeservice
            builder.Services.AddScoped<IMaintenanceTypeService, MaintenanceTypeService>();
            //service center
            builder.Services.AddScoped<IServiceCenterService, ServiceCenterService>();
            //maintenence reminder 
            builder.Services.AddScoped<IMaintenanceReminderService, MaintenanceReminderService>();
            //Appointment
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            //Profile
            builder.Services.AddScoped<IProfileService, ProfileService>();
            //Service Requests
            builder.Services.AddScoped<IServiceRequestQueryService, ServiceRequestQueryService>();
            builder.Services.AddScoped<IServiceRequestWorkflowService, ServiceRequestWorkflowService>();
            //History
            builder.Services.AddScoped<IServiceRequestHistoryService, ServiceRequestHistoryService>();
            //Notification
            builder.Services.AddScoped<INotificationService, NotificationService>();
            //Otp Generator
            builder.Services.AddScoped<IOtpGenerator, OtpGenerator>();
            //JWT Token
            builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            //Refresh Token
            builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
            //autoMapper
            builder.Services.AddAutoMapper(typeof(AuthProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
            //Jobs
            builder.Services.AddScoped<NotificationJobs>();
            builder.Services.AddScoped<MaintenanceReminderJob>();
            //Firebase
            var firebasePath = Path.Combine(Directory.GetCurrentDirectory(), "Firebase", "firebase-key.json");

            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(firebasePath)
            });

            //CORS
            var MyAllowSpecificOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyAllowSpecificOrigins",
                                  policy =>
                                  {
                                      policy
                                            .WithOrigins("http://localhost:5173") 
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

            //update Database 
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            var LoggerFactory = services.GetRequiredService<ILoggerFactory>();

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var logger = services.GetRequiredService<ILogger<Program>>();

                //  Apply pending migrations
                await context.Database.MigrateAsync();

                //  Run vehicle seeder (using your JSON file)
                await StoreContextSeed.SeedAsync(context);

                logger.LogInformation("Database migrated & seeded successfully ");
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, " Error during migration or seeding");
            }

            //Create roles when the app starts
            await CreateRolesAsync(app);

            //Custom Middleware
            app.UseMiddleware<ExceptionMiddleware>();

            //Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCors("MyAllowSpecificOrigins");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHangfireDashboard("/hangfire");
            RecurringJob.AddOrUpdate<MaintenanceReminderJob>(
                "maintenance-reminders",
                        job => job.Execute(),
                        "*/5 * * * *"
            );

            app.MapControllers();

            app.Run();
        }

        //Helper For Seeding Roles
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
