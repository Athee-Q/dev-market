using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using Identity.Application.Abstractions;
using Identity.Application.Dto;

namespace Identity.Application.Features.ListUsers;

/// <summary>Admin-only — see Permissions.UsersManage, enforced at the endpoint.</summary>
public static class ListUsers
{
    public record Query(int Page = 1, int PageSize = 20) : IRequest<PagedResult<UserDto>>;

    public class Handler(IUserRepository users, IRoleRepository roles) : IRequestHandler<Query, PagedResult<UserDto>>
    {
        public async Task<PagedResult<UserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var (items, total) = await users.SearchAsync(request.Page, request.PageSize, cancellationToken);
            var roleById = (await roles.GetAllAsync(cancellationToken)).ToDictionary(r => r.Id, r => r.Name);

            var dtos = items
                .Select(u => UserDto.FromDomain(u, u.UserRoles.Select(ur => roleById.GetValueOrDefault(ur.RoleId, "?")).ToList()))
                .ToList();

            return new PagedResult<UserDto>(dtos, request.Page, request.PageSize, total);
        }
    }
}
