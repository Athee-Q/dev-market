namespace Customer.Domain;

public class Address
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string AddressLine1 { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string State { get; private set; } = default!;
    public string PostalCode { get; private set; } = default!;
    public string Country { get; private set; } = default!;

    private Address() { }

    public Address(Guid customerId, string addressLine1, string city, string state, string postalCode, string country)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        AddressLine1 = addressLine1;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }
}
