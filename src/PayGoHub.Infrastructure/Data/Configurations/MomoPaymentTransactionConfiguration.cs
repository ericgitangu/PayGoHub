using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayGoHub.Domain.Entities;

namespace PayGoHub.Infrastructure.Data.Configurations;

public class MomoPaymentTransactionConfiguration : IEntityTypeConfiguration<MomoPaymentTransaction>
{
    public void Configure(EntityTypeBuilder<MomoPaymentTransaction> builder)
    {
        builder.ToTable("momo_payment_transactions");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");

        builder.Property(t => t.Reference)
            .HasColumnName("reference")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.AmountSubunit)
            .HasColumnName("amount_subunit")
            .IsRequired();

        builder.Property(t => t.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasDefaultValue("KES");

        builder.Property(t => t.BusinessAccount)
            .HasColumnName("business_account")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.ProviderKey)
            .HasColumnName("provider_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.SenderPhoneNumber)
            .HasColumnName("sender_phone_number")
            .HasMaxLength(20);

        builder.Property(t => t.ProviderTx)
            .HasColumnName("provider_tx")
            .HasMaxLength(100);

        builder.Property(t => t.MomoepId)
            .HasColumnName("momoep_id")
            .HasMaxLength(100);

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.CustomerName)
            .HasColumnName("customer_name")
            .HasMaxLength(200);

        builder.Property(t => t.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(500);

        builder.Property(t => t.ValidatedAt)
            .HasColumnName("validated_at");

        builder.Property(t => t.ConfirmedAt)
            .HasColumnName("confirmed_at");

        builder.Property(t => t.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(100);

        builder.Property(t => t.SecondaryProviderTx)
            .HasColumnName("secondary_provider_tx")
            .HasMaxLength(100);

        builder.Property(t => t.TransactionAt)
            .HasColumnName("transaction_at");

        builder.Property(t => t.SenderName)
            .HasColumnName("sender_name")
            .HasMaxLength(200);

        builder.Property(t => t.TransactionKind)
            .HasColumnName("transaction_kind")
            .HasMaxLength(50);

        builder.Property(t => t.CreatedAt).HasColumnName("created_at");
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");
        builder.Property(t => t.DeletedAt).HasColumnName("deleted_at");

        builder.Ignore(t => t.IsDeleted);

        // Indexes
        builder.HasIndex(t => new { t.Reference, t.ProviderKey });
        builder.HasIndex(t => new { t.ProviderTx, t.MomoepId })
            .IsUnique()
            .HasFilter("provider_tx IS NOT NULL");
        builder.HasIndex(t => t.IdempotencyKey)
            .IsUnique()
            .HasFilter("idempotency_key IS NOT NULL");
        builder.HasIndex(t => t.Status);
    }
}
