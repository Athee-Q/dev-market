using ECommerce.BuildingBlocks.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Api.Services;

namespace Notification.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController(INotificationStore store) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] bool onlyUnread, CancellationToken ct) =>
        Ok(await store.ListAsync(User.GetUserId(), onlyUnread, ct));

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken ct) =>
        Ok(new { count = await store.UnreadCountAsync(User.GetUserId(), ct) });

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var marked = await store.MarkReadAsync(User.GetUserId(), id, ct);
        return marked ? NoContent() : NotFound();
    }
}
