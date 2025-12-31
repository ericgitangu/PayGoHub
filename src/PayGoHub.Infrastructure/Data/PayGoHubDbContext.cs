using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PayGoHub.Domain.Entities;

namespace PayGoHub.Infrastructure.Data;

public class PayGoHubDbContext : DbContext
{
    public PayGoHubDbContext(DbContextOptions<PayGoHubDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        // Suppress pending model changes warning for production deployments
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    // Existing entities
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Installation> Installations => Set<Installation>();
    public DbSet<Device> Devices => Set<Device>();

    // M-Services entities
    public DbSet<ApiClient> ApiClients => Set<ApiClient>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<MomoPaymentTransaction> MomoPaymentTransactions => Set<MomoPaymentTransaction>();
    public DbSet<DeviceCommand> DeviceCommands => Set<DeviceCommand>();
    public DbSet<Token> Tokens => Set<Token>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PayGoHubDbContext).Assembly);

        // Global soft delete filter - existing entities
        modelBuilder.Entity<Customer>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Payment>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Loan>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Installation>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Device>().HasQueryFilter(e => e.DeletedAt == null);

        // Global soft delete filter - m-services entities
        modelBuilder.Entity<ApiClient>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Provider>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<MomoPaymentTransaction>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<DeviceCommand>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Token>().HasQueryFilter(e => e.DeletedAt == null);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
