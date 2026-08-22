namespace Customer.Domain;

public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string Phone { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }

    private readonly List<Address> _addresses = [];
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

    private Customer() { }

    public Customer(string name, string email, string phone) : this(Guid.NewGuid(), name, email, phone) { }

    /// <summary>
    /// Used when the id must match an id already minted elsewhere — specifically, the Identity
    /// Service UserId a Customer row is created for off UserRegisteredEvent (see
    /// UserRegisteredConsumer), so "customer id" and "authenticated user id" stay the same GUID
    /// everywhere downstream.
    /// </summary>
    public Customer(Guid id, string name, string email, string phone)
    {
        Id = id;
        Name = name;
        Email = email;
        Phone = phone;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateProfile(string name, string email, string phone)
    {
        Name = name;
        Email = email;
        Phone = phone;
    }

    public Address AddAddress(string addressLine1, string city, string state, string postalCode, string country)
    {
        var address = new Address(Id, addressLine1, city, state, postalCode, country);
        _addresses.Add(address);
        return address;
    }
}
