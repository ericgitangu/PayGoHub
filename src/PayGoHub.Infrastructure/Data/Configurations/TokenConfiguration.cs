using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayGoHub.Domain.Entities;

namespace PayGoHub.Infrastructure.Data.Configurations;

public class TokenConfiguration : IEntityTypeConfiguration<Token>
{
    public void Configure(EntityTypeBuilder<Token> builder)
    {
        builder.ToTable("tokens");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");

        builder.Property(t => t.DeviceIdentifier)
            .HasColumnName("device_identifier")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.TokenValue)
            .HasColumnName("token_value")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Command)
            .HasColumnName("command")
            .HasMaxLength(50)
            .HasDefaultValue("unlock_relative");

        builder.Property(t => t.Payload)
            .HasColumnName("payload")
            .HasMaxLength(100);

        builder.Property(t => t.SequenceNumber)
            .HasColumnName("sequence_number")
            .IsRequired();

        builder.Property(t => t.Encoding)
            .HasColumnName("encoding")
            .HasMaxLength(50);

        builder.Property(t => t.DaysCredit)
            .HasColumnName("days_credit");

        builder.Property(t => t.ValidFrom)
            .HasColumnName("valid_from");

        builder.Property(t => t.ValidUntil)
            .HasColumnName("valid_until");

        builder.Property(t => t.IsUsed)
            .HasColumnName("is_used")
            .HasDefaultValue(false);

        builder.Property(t => t.UsedAt)
            .HasColumnName("used_at");

        builder.Property(t => t.PaymentId)
            .HasColumnName("payment_id");

        builder.Property(t => t.ApiClientId)
            .HasColumnName("api_client_id")
            .IsRequired();

        builder.Property(t => t.CreatedAt).HasColumnName("created_at");
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");
        builder.Property(t => t.DeletedAt).HasColumnName("deleted_at");

        builder.Ignore(t => t.IsDeleted);

        // Relationships
        builder.HasOne(t => t.ApiClient)
            .WithMany(a => a.Tokens)
            .HasForeignKey(t => t.ApiClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(t => t.DeviceIdentifier);
        builder.HasIndex(t => t.TokenValue).IsUnique();
        builder.HasIndex(t => t.ApiClientId);
        builder.HasIndex(t => new { t.DeviceIdentifier, t.SequenceNumber });
    }
}
