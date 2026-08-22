namespace Customer.Application.Dto;

public record AddressDto(
    Guid Id,
    string AddressLine1,
    string City,
    string State,
    string PostalCode,
    string Country);

public record CustomerDto(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<AddressDto> Addresses)
{
    public static CustomerDto FromDomain(Domain.Customer c) => new(
        c.Id, c.Name, c.Email, c.Phone, c.CreatedAt,
        c.Addresses.Select(a => new AddressDto(a.Id, a.AddressLine1, a.City, a.State, a.PostalCode, a.Country)).ToList());
}
