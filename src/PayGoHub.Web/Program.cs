using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using PayGoHub.Application.Interfaces;
using PayGoHub.Infrastructure.Data;
using PayGoHub.Infrastructure.Seed;
using PayGoHub.Infrastructure.Services;
using PayGoHub.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

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

// Add HTTP client for M2M callbacks
builder.Services.AddHttpClient("M2MCallback", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add MVC and API controllers
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<PayGoHubDbContext>();
        await context.Database.MigrateAsync();
        await DbSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Configure the HTTP request pipeline.
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
