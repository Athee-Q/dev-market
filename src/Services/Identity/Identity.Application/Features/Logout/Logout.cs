using ECommerce.BuildingBlocks.Application.Mediator;
using FluentValidation;
using Identity.Application.Abstractions;

namespace Identity.Application.Features.Logout;

public static class Logout
{
    public record Command(string RefreshToken) : IRequest<Unit>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator() => RuleFor(x => x.RefreshToken).NotEmpty();
    }

    public class Handler(IRefreshTokenRepository refreshTokens, ITokenService tokenService) : IRequestHandler<Command, Unit>
    {
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var hash = tokenService.HashToken(request.RefreshToken);
            var existing = await refreshTokens.GetByHashAsync(hash, cancellationToken);
            if (existing is not null && existing.IsActive)
            {
                existing.Revoke();
                await refreshTokens.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}
