using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Product.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Domain.Product>
{
    public void Configure(EntityTypeBuilder<Domain.Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000).IsRequired();
        builder.Property(p => p.SKU).HasMaxLength(64).IsRequired();
        builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
        builder.Property(p => p.AssetUrl).HasMaxLength(2000);

        // Stored as strings, not ints — readable straight in the DB and stable if enum members
        // are ever reordered.
        builder.Property(p => p.ProductType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(p => p.PricingModel).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.HasIndex(p => p.SKU).IsUnique();
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.ProductType);
    }
}
