using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using FluentValidation;
using Identity.Application.Abstractions;
using Identity.Application.Dto;

namespace Identity.Application.Features.Login;

public static class Login
{
    public record Command(string Email, string Password) : IRequest<AuthResultDto>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class Handler(
        IUserRepository users,
        IRoleRepository roles,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher passwordHasher,
        ITokenService tokenService) : IRequestHandler<Command, AuthResultDto>
    {
        public async Task<AuthResultDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await users.GetByEmailAsync(request.Email, cancellationToken);

            // Deliberately the same error for "no such user" and "wrong password" — doesn't
            // reveal whether an email is registered.
            if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAppException("Invalid email or password.");

            var userRoles = await roles.GetByIdsAsync(user.UserRoles.Select(ur => ur.RoleId), cancellationToken);
            return await TokenIssuer.IssueAsync(user, userRoles, tokenService, refreshTokens, cancellationToken);
        }
    }
}
