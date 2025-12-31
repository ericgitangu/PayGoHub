using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayGoHub.Domain.Entities;

namespace PayGoHub.Infrastructure.Data.Configurations;

public class ApiClientConfiguration : IEntityTypeConfiguration<ApiClient>
{
    public void Configure(EntityTypeBuilder<ApiClient> builder)
    {
        builder.ToTable("api_clients");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.ApiKeyHash)
            .HasColumnName("api_key_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(a => a.AllowedScopes)
            .HasColumnName("allowed_scopes")
            .HasColumnType("text[]");

        builder.Property(a => a.AllowedProviders)
            .HasColumnName("allowed_providers")
            .HasColumnType("text[]");

        builder.Property(a => a.LastUsedAt)
            .HasColumnName("last_used_at");

        builder.Property(a => a.RateLimitPerMinute)
            .HasColumnName("rate_limit_per_minute")
            .HasDefaultValue(100);

        builder.Property(a => a.IpWhitelist)
            .HasColumnName("ip_whitelist")
            .HasMaxLength(1000);

        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");
        builder.Property(a => a.DeletedAt).HasColumnName("deleted_at");

        builder.Ignore(a => a.IsDeleted);

        builder.HasIndex(a => a.ApiKeyHash).IsUnique();
        builder.HasIndex(a => a.Name);
    }
}
