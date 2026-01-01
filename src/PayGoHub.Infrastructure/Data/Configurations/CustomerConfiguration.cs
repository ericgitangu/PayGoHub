using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayGoHub.Domain.Entities;

namespace PayGoHub.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(c => c.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Region)
            .HasColumnName("region")
            .HasMaxLength(100);

        builder.Property(c => c.District)
            .HasColumnName("district")
            .HasMaxLength(100);

        builder.Property(c => c.Address)
            .HasColumnName("address")
            .HasMaxLength(500);

        builder.Property(c => c.Country)
            .HasColumnName("country")
            .HasMaxLength(10)
            .HasDefaultValue("KE");

        builder.Property(c => c.Currency)
            .HasColumnName("currency")
            .HasMaxLength(10)
            .HasDefaultValue("KES");

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.AccountNumber)
            .HasColumnName("account_number")
            .HasMaxLength(100);

        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");
        builder.Property(c => c.DeletedAt).HasColumnName("deleted_at");

        builder.Ignore(c => c.FullName);
        builder.Ignore(c => c.IsDeleted);

        builder.HasIndex(c => c.Email);
        builder.HasIndex(c => c.PhoneNumber);
        builder.HasIndex(c => c.Region);
    }
}
