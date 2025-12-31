using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayGoHub.Domain.Entities;

namespace PayGoHub.Infrastructure.Data.Configurations;

public class InstallationConfiguration : IEntityTypeConfiguration<Installation>
{
    public void Configure(EntityTypeBuilder<Installation> builder)
    {
        builder.ToTable("installations");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");

        builder.Property(i => i.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(i => i.DeviceId)
            .HasColumnName("device_id");

        builder.Property(i => i.SystemType)
            .HasColumnName("system_type")
            .HasMaxLength(50);

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.ScheduledDate)
            .HasColumnName("scheduled_date");

        builder.Property(i => i.CompletedDate)
            .HasColumnName("completed_date");

        builder.Property(i => i.Location)
            .HasColumnName("location")
            .HasMaxLength(500);

        builder.Property(i => i.TechnicianName)
            .HasColumnName("technician_name")
            .HasMaxLength(200);

        builder.Property(i => i.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(i => i.CreatedAt).HasColumnName("created_at");
        builder.Property(i => i.UpdatedAt).HasColumnName("updated_at");
        builder.Property(i => i.DeletedAt).HasColumnName("deleted_at");

        builder.Ignore(i => i.IsDeleted);

        builder.HasOne(i => i.Customer)
            .WithMany(c => c.Installations)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Device)
            .WithOne(d => d.Installation)
            .HasForeignKey<Installation>(i => i.DeviceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(i => i.CustomerId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.ScheduledDate);
    }
}
