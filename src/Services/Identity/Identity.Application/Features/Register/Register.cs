using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.Contracts.Common;
using ECommerce.Contracts.Events;
using FluentValidation;
using Identity.Application.Abstractions;
using Identity.Application.Dto;

namespace Identity.Application.Features.Register;

public static class Register
{
    public record Command(string Email, string FullName, string Password) : IRequest<AuthResultDto>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8).WithMessage("Password must be at least 8 characters.");
        }
    }

    public class Handler(
        IUserRepository users,
        IRoleRepository roles,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEventPublisher eventPublisher) : IRequestHandler<Command, AuthResultDto>
    {
        public async Task<AuthResultDto> Handle(Command request, CancellationToken cancellationToken)
        {
            if (await users.EmailExistsAsync(request.Email, cancellationToken))
                throw new ConflictException($"An account with email '{request.Email}' already exists.");

            var customerRole = await roles.GetByNameAsync(Roles.Customer, cancellationToken)
                ?? throw new InvalidOperationException($"Role '{Roles.Customer}' is not seeded — Identity Service failed to start correctly.");

            var user = new Domain.User(request.Email, request.FullName, passwordHasher.Hash(request.Password));
            user.AssignRole(customerRole.Id);

            await users.AddAsync(user, cancellationToken);
            await users.SaveChangesAsync(cancellationToken);

            // Customer Service consumes this to create the linked Customer profile — see
            // UserRegisteredEvent's doc comment (Id == UserId everywhere downstream).
            await eventPublisher.PublishAsync(
                new UserRegisteredEvent(user.Id, user.Email, user.FullName, user.CreatedAt), cancellationToken);

            return await TokenIssuer.IssueAsync(user, [customerRole], tokenService, refreshTokens, cancellationToken);
        }
    }
}
