using Customer.Application.Abstractions;
using Customer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Customer.Infrastructure.Repositories;

public class CustomerRepository(CustomerDbContext db) : ICustomerRepository
{
    public Task<Domain.Customer?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Customers.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct) =>
        db.Customers.AnyAsync(c => c.Email == email, ct);

    public async Task AddAsync(Domain.Customer customer, CancellationToken ct) =>
        await db.Customers.AddAsync(customer, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
