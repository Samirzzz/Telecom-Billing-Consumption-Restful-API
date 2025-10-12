using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TelecomBilling.Api.Data;
using TelecomBilling.Api.Services;
using TelecomBilling.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddXmlSerializerFormatters(); // Add XML support for SOAP

// Add Entity Framework
builder.Services.AddDbContext<TelecomBillingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

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
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add AutoMapper - commented out for now to avoid conflicts
// builder.Services.AddAutoMapper(typeof(Program));

// Add custom services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IConsumptionService, ConsumptionService>();
builder.Services.AddScoped<ITariffRuleService, TariffRuleService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Telecom Billing API", 
        Version = "v1",
        Description = "A comprehensive Telecom Billing & Consumption RESTful API with JWT Authentication"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add custom formatters
builder.Services.AddControllers(options =>
{
    options.OutputFormatters.Add(new TelecomBilling.Api.Formatters.SoapXmlFormatter());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Telecom Billing API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Keep the simple endpoint for testing
app.MapGet("/", () => "Hello World 👋 from Telecom Billing API - Visit /swagger for API documentation");

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TelecomBillingDbContext>();
    context.Database.EnsureCreated();
    
    // Seed initial data
    await SeedDataAsync(context);
}

app.Run();

static async Task SeedDataAsync(TelecomBillingDbContext context)
{
        // Seed users (who are also subscribers)
        if (!context.Users.Any())
        {
            var samirUser = new User
            {
                Username = "samir",
                Email = "samir@example.com",
                PasswordHash = HashPassword("1234"), // In production, use a proper password hashing
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                // Subscriber fields
                Name = "Samir Ahmed",
                PhoneNumber = "+1234567890",
                PlanType = "Premium",
                Country = "USA",
                IsRoaming = false
            };

            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = HashPassword("admin123"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                // Subscriber fields
                Name = "System Administrator",
                PhoneNumber = "+0987654321",
                PlanType = "Enterprise",
                Country = "USA",
                IsRoaming = false
            };

            context.Users.AddRange(samirUser, adminUser);
            await context.SaveChangesAsync(); // Save users first
        }

        // Get the actual user IDs from the database
        var samirUserFromDb = await context.Users.FirstOrDefaultAsync(u => u.Username == "samir");
        var adminUserFromDb = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");


    // Seed sample tariff rules
    if (!context.TariffRules.Any())
    {
        var tariffRules = new List<TariffRule>
        {
            new TariffRule
            {
                Name = "Premium Plan Rates",
                PlanType = "Premium",
                VoicePeakRate = 0.05m,
                VoiceOffPeakRate = 0.03m,
                DataRate = 0.01m,
                SMSRate = 0.10m,
                RoamingVoiceRate = 0.15m,
                RoamingDataRate = 0.05m,
                RoamingSMSRate = 0.25m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new TariffRule
            {
                Name = "Basic Plan Rates",
                PlanType = "Basic",
                VoicePeakRate = 0.08m,
                VoiceOffPeakRate = 0.05m,
                DataRate = 0.02m,
                SMSRate = 0.15m,
                RoamingVoiceRate = 0.20m,
                RoamingDataRate = 0.08m,
                RoamingSMSRate = 0.30m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.TariffRules.AddRange(tariffRules);
        await context.SaveChangesAsync(); // Save tariff rules
    }

        // Seed sample usage records
        if (!context.UsageRecords.Any() && samirUserFromDb != null && adminUserFromDb != null)
        {
            var usageRecords = new List<UsageRecord>
            {
                new UsageRecord
                {
                    UserId = samirUserFromDb.Id, // samir user
                    Timestamp = DateTime.UtcNow.AddDays(-1),
                    CallMinutes = 45,
                    DataMB = 250,
                    SMSCount = 12,
                    IsPeakTime = true,
                    IsRoaming = false,
                    CreatedAt = DateTime.UtcNow
                },
                new UsageRecord
                {
                    UserId = samirUserFromDb.Id, // samir user
                    Timestamp = DateTime.UtcNow.AddDays(-2),
                    CallMinutes = 30,
                    DataMB = 180,
                    SMSCount = 8,
                    IsPeakTime = false,
                    IsRoaming = true,
                    CreatedAt = DateTime.UtcNow
                },
                new UsageRecord
                {
                    UserId = adminUserFromDb.Id, // admin user
                    Timestamp = DateTime.UtcNow.AddDays(-1),
                    CallMinutes = 20,
                    DataMB = 120,
                    SMSCount = 5,
                    IsPeakTime = true,
                    IsRoaming = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.UsageRecords.AddRange(usageRecords);
            await context.SaveChangesAsync(); // Save usage records
        }
}

static string HashPassword(string password)
{
    using var sha256 = System.Security.Cryptography.SHA256.Create();
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(hashedBytes);
}
