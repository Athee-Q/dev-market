namespace Customer.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Domain.Customer?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);
    Task AddAsync(Domain.Customer customer, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
