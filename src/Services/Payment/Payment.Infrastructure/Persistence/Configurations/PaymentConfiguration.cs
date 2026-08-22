using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Payment.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Domain.Payment>
{
    public void Configure(EntityTypeBuilder<Domain.Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.OrderNumber).HasMaxLength(40).IsRequired();
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.RazorpayOrderId).HasMaxLength(50).IsRequired();
        builder.Property(p => p.RazorpayPaymentId).HasMaxLength(50);
        builder.Property(p => p.FailureReason).HasMaxLength(500);
        builder.Property(p => p.RazorpayQrCodeId).HasMaxLength(50);
        builder.Property(p => p.UpiQrImageUrl).HasMaxLength(500);

        // Enforces idempotency at the database level: one payment row per order (see Domain.Payment).
        builder.HasIndex(p => p.OrderId).IsUnique();
        builder.HasIndex(p => p.RazorpayOrderId).IsUnique();
    }
}
