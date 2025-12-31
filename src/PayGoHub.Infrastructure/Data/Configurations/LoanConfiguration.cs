using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayGoHub.Domain.Entities;

namespace PayGoHub.Infrastructure.Data.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("loans");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");

        builder.Property(l => l.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(l => l.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(l => l.InterestRate)
            .HasColumnName("interest_rate")
            .HasPrecision(5, 2);

        builder.Property(l => l.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.IssuedDate)
            .HasColumnName("issued_date");

        builder.Property(l => l.DueDate)
            .HasColumnName("due_date");

        builder.Property(l => l.RemainingBalance)
            .HasColumnName("remaining_balance")
            .HasPrecision(18, 2);

        builder.Property(l => l.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(l => l.CreatedAt).HasColumnName("created_at");
        builder.Property(l => l.UpdatedAt).HasColumnName("updated_at");
        builder.Property(l => l.DeletedAt).HasColumnName("deleted_at");

        builder.Ignore(l => l.IsDeleted);

        builder.HasOne(l => l.Customer)
            .WithMany(c => c.Loans)
            .HasForeignKey(l => l.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.CustomerId);
        builder.HasIndex(l => l.Status);
    }
}
