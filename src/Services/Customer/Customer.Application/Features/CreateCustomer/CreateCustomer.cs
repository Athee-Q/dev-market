using ECommerce.BuildingBlocks.Application.Mediator;
using Customer.Application.Abstractions;
using Customer.Application.Dto;
using ECommerce.Contracts.Common;
using FluentValidation;

namespace Customer.Application.Features.CreateCustomer;

public static class CreateCustomer
{
    public record AddressInput(string AddressLine1, string City, string State, string PostalCode, string Country);

    public record Command(string Name, string Email, string Phone, IReadOnlyCollection<AddressInput>? Addresses) : IRequest<CustomerDto>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        }
    }

    public class Handler(ICustomerRepository repository) : IRequestHandler<Command, CustomerDto>
    {
        public async Task<CustomerDto> Handle(Command request, CancellationToken cancellationToken)
        {
            if (await repository.EmailExistsAsync(request.Email, cancellationToken))
                throw new ConflictException($"A customer with email '{request.Email}' already exists.");

            var customer = new Domain.Customer(request.Name, request.Email, request.Phone);
            foreach (var address in request.Addresses ?? [])
                customer.AddAddress(address.AddressLine1, address.City, address.State, address.PostalCode, address.Country);

            await repository.AddAsync(customer, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            return CustomerDto.FromDomain(customer);
        }
    }
}
