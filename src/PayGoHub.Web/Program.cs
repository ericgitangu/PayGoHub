using Microsoft.EntityFrameworkCore;
using PayGoHub.Application.Interfaces;
using PayGoHub.Infrastructure.Data;
using PayGoHub.Infrastructure.Seed;
using PayGoHub.Infrastructure.Services;
using PayGoHub.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<PayGoHubDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
