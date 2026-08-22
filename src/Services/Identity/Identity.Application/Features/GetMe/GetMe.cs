using ECommerce.BuildingBlocks.Application.Mediator;
using Identity.Application.Abstractions;
using Identity.Application.Dto;

namespace Identity.Application.Features.GetMe;

public static class GetMe
{
    public record Query(Guid UserId) : IRequest<UserDto?>;

    public class Handler(IUserRepository users, IRoleRepository roles) : IRequestHandler<Query, UserDto?>
    {
        public async Task<UserDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await users.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null) return null;

            var userRoles = await roles.GetByIdsAsync(user.UserRoles.Select(ur => ur.RoleId), cancellationToken);
            return UserDto.FromDomain(user, userRoles.Select(r => r.Name).ToList());
        }
    }
}
