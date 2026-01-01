using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PayGoHub.Application.Interfaces;
using PayGoHub.Infrastructure.Data;
using PayGoHub.Infrastructure.Seed;
using PayGoHub.Infrastructure.Services;
using PayGoHub.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure forwarded headers for Fly.io proxy (MUST be before other middleware)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Load environment variables from .env file
var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}

// Add DbContext
builder.Services.AddDbContext<PayGoHubDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Google OAuth Authentication
var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
    ?? builder.Configuration["Authentication:Google:ClientId"]
    ?? "";
var googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
    ?? builder.Configuration["Authentication:Google:ClientSecret"]
    ?? "";

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.LoginPath = "/Account/Landing";
    options.AccessDeniedPath = "/Account/Landing";
})
.AddGoogle(options =>
{
    options.ClientId = googleClientId;
    options.ClientSecret = googleClientSecret;
    options.CallbackPath = "/signin-google";
    options.SaveTokens = true;
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Events.OnCreatingTicket = async context =>
    {
        // Add profile picture to claims
        var picture = context.User.GetProperty("picture").GetString();
        if (!string.IsNullOrEmpty(picture))
        {
            context.Identity?.AddClaim(new System.Security.Claims.Claim("picture", picture));
        }
    };
});

// Add existing services
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IInstallationService, InstallationService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();

// Add M-services
builder.Services.AddScoped<IMomoPaymentService, MomoPaymentService>();
builder.Services.AddScoped<IM2MCommandService, M2MCommandService>();
builder.Services.AddScoped<ITokenGenerationService, TokenGenerationService>();
builder.Services.AddScoped<IMegaSmsService, MegaSmsService>();

// Add Activity logging
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();

// Add HTTP client for M2M callbacks
builder.Services.AddHttpClient("M2MCallback", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add MVC and API controllers
builder.Services.AddControllersWithViews();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PayGoHub API",
        Version = "v1",
        Description = "PayGoHub - M-Services Integration Backend for Solarium\n\n" +
                      "Provides endpoints for MoMo payments, M2M device commands, and token generation.",
        Contact = new OpenApiContact
        {
            Name = "PayGoHub Team",
            Email = "dev@plugintheworld.com"
        }
    });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key for authentication. Use 'API-KEY' or 'X-API-Key' header.",
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<PayGoHubDbContext>();

    // Try to apply migrations, but continue even if they fail (for already-applied schemas)
    try
    {
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Npgsql.PostgresException ex) when (ex.SqlState == "42701" || ex.SqlState == "42P07")
    {
        // 42701 = column already exists, 42P07 = table already exists
        logger.LogWarning("Some migrations were skipped (schema already exists): {Message}", ex.Message);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
    }

    // Always try to seed data
    try
    {
        await DbSeeder.SeedAsync(context);
        logger.LogInformation("Database seeding completed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
// MUST be first - handle forwarded headers from Fly.io proxy
app.UseForwardedHeaders();

// Enable Swagger in all environments for API documentation
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PayGoHub API v1");
    options.RoutePrefix = "api-docs";
    options.DocumentTitle = "PayGoHub API Documentation";
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Add authentication middleware
app.UseAuthentication();

// API key authentication for /api routes
app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.UseMiddleware<ApiKeyAuthenticationMiddleware>();
});

app.UseAuthorization();

app.MapStaticAssets();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// Map API controllers
app.MapControllers();

// Dashboard route
app.MapControllerRoute(
    name: "dashboard",
    pattern: "dashboard",
    defaults: new { controller = "Home", action = "Index" });

// Standard MVC routes for all controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
