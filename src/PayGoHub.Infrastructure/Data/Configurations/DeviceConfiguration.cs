using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayGoHub.Domain.Entities;

namespace PayGoHub.Infrastructure.Data.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.SerialNumber)
            .HasColumnName("serial_number")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.Model)
            .HasColumnName("model")
            .HasMaxLength(100);

        builder.Property(d => d.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.InstallationId)
            .HasColumnName("installation_id");

        builder.Property(d => d.BatteryHealth)
            .HasColumnName("battery_health");

        builder.Property(d => d.LastSyncDate)
            .HasColumnName("last_sync_date");

        builder.Property(d => d.CreatedAt).HasColumnName("created_at");
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at");
        builder.Property(d => d.DeletedAt).HasColumnName("deleted_at");

        builder.Ignore(d => d.IsDeleted);

        builder.HasIndex(d => d.SerialNumber).IsUnique();
        builder.HasIndex(d => d.Status);
    }
}
