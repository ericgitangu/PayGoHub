using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayGoHub.Domain.Entities;

namespace PayGoHub.Infrastructure.Data.Configurations;

public class DeviceCommandConfiguration : IEntityTypeConfiguration<DeviceCommand>
{
    public void Configure(EntityTypeBuilder<DeviceCommand> builder)
    {
        builder.ToTable("device_commands");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.DeviceIdentifier)
            .HasColumnName("device_identifier")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.IdentifierKind)
            .HasColumnName("identifier_kind")
            .HasMaxLength(20)
            .HasDefaultValue("serial");

        builder.Property(c => c.CommandName)
            .HasColumnName("command_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.CommandDetails)
            .HasColumnName("command_details")
            .HasColumnType("jsonb");

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.CallbackUrl)
            .HasColumnName("callback_url")
            .HasMaxLength(500);

        builder.Property(c => c.DeviceResponse)
            .HasColumnName("device_response")
            .HasColumnType("jsonb");

        builder.Property(c => c.SentAt)
            .HasColumnName("sent_at");

        builder.Property(c => c.ExecutedAt)
            .HasColumnName("executed_at");

        builder.Property(c => c.CallbackDeliveredAt)
            .HasColumnName("callback_delivered_at");

        builder.Property(c => c.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(500);

        builder.Property(c => c.RetryCount)
            .HasColumnName("retry_count")
            .HasDefaultValue(0);

        builder.Property(c => c.ApiClientId)
            .HasColumnName("api_client_id")
            .IsRequired();

        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");
        builder.Property(c => c.DeletedAt).HasColumnName("deleted_at");

        builder.Ignore(c => c.IsDeleted);

        // Relationships
        builder.HasOne(c => c.ApiClient)
            .WithMany(a => a.DeviceCommands)
            .HasForeignKey(c => c.ApiClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(c => new { c.DeviceIdentifier, c.Status });
        builder.HasIndex(c => c.ApiClientId);
        builder.HasIndex(c => c.Status);
    }
}
