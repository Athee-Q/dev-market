using Microsoft.EntityFrameworkCore;

namespace Order.Infrastructure.Persistence;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Order> Orders => Set<Domain.Order>();
    public DbSet<Domain.OrderItem> OrderItems => Set<Domain.OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
    }
}
