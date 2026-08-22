using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Customer.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Domain.Customer>
{
    public void Configure(EntityTypeBuilder<Domain.Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Phone).HasMaxLength(30).IsRequired();

        builder.HasIndex(c => c.Email).IsUnique();

        // Addresses is a read-only collection backed by the private `_addresses` field;
        // EF Core's default field-access convention maps it without extra configuration.
        builder.Navigation(c => c.Addresses).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class AddressConfiguration : IEntityTypeConfiguration<Domain.Address>
{
    public void Configure(EntityTypeBuilder<Domain.Address> builder)
    {
        builder.ToTable("Addresses");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AddressLine1).HasMaxLength(300).IsRequired();
        builder.Property(a => a.City).HasMaxLength(100).IsRequired();
        builder.Property(a => a.State).HasMaxLength(100).IsRequired();
        builder.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Country).HasMaxLength(100).IsRequired();

        builder.HasOne<Domain.Customer>()
            .WithMany(c => c.Addresses)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
