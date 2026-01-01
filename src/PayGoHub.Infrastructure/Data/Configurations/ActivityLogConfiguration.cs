using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayGoHub.Domain.Entities;

namespace PayGoHub.Infrastructure.Data.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("activity_logs");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.ActivityType)
            .HasColumnName("action_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Title)
            .HasColumnName("title")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(a => a.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .HasColumnName("entity_id");

        builder.Property(a => a.EntityIdentifier)
            .HasColumnName("entity_identifier")
            .HasMaxLength(255);

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasMaxLength(50);

        builder.Property(a => a.PerformedBy)
            .HasColumnName("performed_by")
            .HasMaxLength(255);

        builder.Property(a => a.IconClass)
            .HasColumnName("icon")
            .HasMaxLength(50);

        builder.Property(a => a.ColorClass)
            .HasColumnName("color_class")
            .HasMaxLength(50);

        builder.Property(a => a.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");
        builder.Property(a => a.DeletedAt).HasColumnName("deleted_at");

        builder.Ignore(a => a.IsDeleted);

        builder.HasIndex(a => a.EntityType);
        builder.HasIndex(a => a.EntityId);
        builder.HasIndex(a => a.EntityIdentifier);
        builder.HasIndex(a => a.CreatedAt);
    }
}
