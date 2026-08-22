using Microsoft.EntityFrameworkCore;

namespace Customer.Infrastructure.Persistence;

public class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Customer> Customers => Set<Domain.Customer>();
    public DbSet<Domain.Address> Addresses => Set<Domain.Address>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerDbContext).Assembly);
    }
}
