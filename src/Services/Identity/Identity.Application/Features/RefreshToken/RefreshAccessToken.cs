using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using FluentValidation;
using Identity.Application.Abstractions;
using Identity.Application.Dto;

namespace Identity.Application.Features.RefreshToken;

/// <summary>
/// Named RefreshAccessToken (not RefreshToken) to avoid colliding with Identity.Domain.RefreshToken
/// — the same "endpoint namespace shadows the domain type" gotcha the README calls out for Product's
/// SearchProducts, sidestepped here by simply not reusing the entity's name for the slice.
/// </summary>
public static class RefreshAccessToken
{
    public record Command(string RefreshToken) : IRequest<AuthResultDto>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator() => RuleFor(x => x.RefreshToken).NotEmpty();
    }

    public class Handler(
        IUserRepository users,
        IRoleRepository roles,
        IRefreshTokenRepository refreshTokens,
        ITokenService tokenService) : IRequestHandler<Command, AuthResultDto>
    {
        public async Task<AuthResultDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var hash = tokenService.HashToken(request.RefreshToken);
            var existing = await refreshTokens.GetByHashAsync(hash, cancellationToken);
            if (existing is null || !existing.IsActive)
                throw new UnauthorizedAppException("Refresh token is invalid or expired.");

            var user = await users.GetByIdAsync(existing.UserId, cancellationToken)
                ?? throw new UnauthorizedAppException("Refresh token is invalid or expired.");

            var userRoles = await roles.GetByIdsAsync(user.UserRoles.Select(ur => ur.RoleId), cancellationToken);
            var result = await TokenIssuer.IssueAsync(user, userRoles, tokenService, refreshTokens, cancellationToken);

            // Rotate: revoke the token that was just used, chaining to the one just issued so
            // reuse of an already-rotated token is at least detectable later.
            existing.Revoke(tokenService.HashToken(result.RefreshToken));
            await refreshTokens.SaveChangesAsync(cancellationToken);

            return result;
        }
    }
}
