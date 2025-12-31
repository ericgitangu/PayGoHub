using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayGoHub.Domain.Entities;

namespace PayGoHub.Infrastructure.Data.Configurations;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("providers");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.ProviderKey)
            .HasColumnName("provider_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Country)
            .HasColumnName("country")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasDefaultValue("KES");

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(p => p.ConfigurationJson)
            .HasColumnName("configuration_json")
            .HasColumnType("jsonb");

        builder.Property(p => p.MinAmountSubunit)
            .HasColumnName("min_amount_subunit")
            .HasDefaultValue(100);

        builder.Property(p => p.MaxAmountSubunit)
            .HasColumnName("max_amount_subunit")
            .HasDefaultValue(100000000);

        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");

        builder.Ignore(p => p.IsDeleted);

        builder.HasIndex(p => p.ProviderKey).IsUnique();
        builder.HasIndex(p => p.Country);
    }
}
