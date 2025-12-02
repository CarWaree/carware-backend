using CarWare.API.Middlewares;
using CarWare.Application.Interfaces;
using CarWare.Application.Mapping;
using CarWare.Application.Services;
using CarWare.Domain;
using CarWare.Domain.Entities;
using CarWare.Domain.helper;
using CarWare.Domain.Interfaces;
using CarWare.Infrastructure.Context;
using CarWare.Infrastructure.Repositories;
using CarWare.Infrastructure.Seed;
using CarWare.Infrastructure.Services;
using CarWare.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CarWare.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.UseUrls("https://localhost:7136", "http://0.0.0.0:7136");

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
                options.Password.RequiredLength = 4;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
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
                googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                googleOptions.CallbackPath = "/auth/google-callback"; 
            });

            //Memory Cashe
            builder.Services.AddDistributedMemoryCache();

            //Custom Service [Email Sender]
            builder.Services.AddScoped<IEmailSender, EmailSender>();

            //Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            //vehicleservice
            builder.Services.AddScoped<IVehicleService, VehicleService>();

            //maintenancetypeservice
            builder.Services.AddScoped<IMaintenanceTypeService, MaintenanceTypeService>();


            //autoMapper
            builder.Services.AddAutoMapper(typeof(AuthProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

            //service center
            builder.Services.AddScoped<IServiceCenterService, ServiceCenterService>();
            //maintenence reminder 
            builder.Services.AddScoped<IMaintenanceReminderService, MaintenanceReminderService>();


            //CORS
            var MyAllowSpecificOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyAllowSpecificOrigins",
                                  policy =>
                                  {
                                      policy
                                            .WithOrigins("AllowedOrigins") //your frontend URLs
                                            .AllowAnyHeader()
                                            .AllowAnyMethod()
                                            .AllowCredentials(); //if i want to use cookies/auth
                                  });
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

                // ✅ Apply pending migrations
                await context.Database.MigrateAsync();

                // ✅ Run vehicle seeder (using your JSON file)
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

            app.UseCors("MyAllowSpecificOrigins");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        //Helper method to seed roles
        private static async Task CreateRolesAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "ADMIN", "USER" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
