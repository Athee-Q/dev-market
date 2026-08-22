using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using FluentValidation;
using Identity.Application.Abstractions;
using Identity.Application.Dto;

namespace Identity.Application.Features.AssignRole;

/// <summary>Admin-only — see Permissions.UsersManage, enforced at the endpoint.</summary>
public static class AssignRole
{
    public record Command(Guid UserId, string RoleName) : IRequest<UserDto?>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator() => RuleFor(x => x.RoleName).NotEmpty();
    }

    public class Handler(IUserRepository users, IRoleRepository roles) : IRequestHandler<Command, UserDto?>
    {
        public async Task<UserDto?> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await users.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null) return null;

            var role = await roles.GetByNameAsync(request.RoleName, cancellationToken)
                ?? throw new NotFoundException($"Role '{request.RoleName}' does not exist.");

            user.AssignRole(role.Id);
            await users.SaveChangesAsync(cancellationToken);

            var userRoles = await roles.GetByIdsAsync(user.UserRoles.Select(ur => ur.RoleId), cancellationToken);
            return UserDto.FromDomain(user, userRoles.Select(r => r.Name).ToList());
        }
    }
}
