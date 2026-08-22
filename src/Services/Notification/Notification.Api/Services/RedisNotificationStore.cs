using System.Text.Json;
using Notification.Api.Models;
using StackExchange.Redis;

namespace Notification.Api.Services;

/// <summary>
/// Redis layout (§11):
///   notification:{id}                 -> JSON blob for one notification (TTL'd)
///   notifications:{customerId}        -> sorted set of notification ids, score = created-at unix time,
///                                         trimmed to a recent-history window
///   notifications:{customerId}:unread -> set of unread notification ids
/// This is transient, fast-access state, not the system of record — see §11 "Do not store the
/// authoritative order transaction only in Redis" (notifications aren't the order transaction,
/// so this is fine for them specifically).
/// </summary>
public class RedisNotificationStore(IConnectionMultiplexer redis) : INotificationStore
{
    private const int RecentHistoryWindow = 100;
    private static readonly TimeSpan NotificationTtl = TimeSpan.FromDays(30);

    private IDatabase Db => redis.GetDatabase();
    private static string NotificationKey(Guid id) => $"notification:{id}";
    private static string IndexKey(Guid customerId) => $"notifications:{customerId}";
    private static string UnreadKey(Guid customerId) => $"notifications:{customerId}:unread";

    public async Task<NotificationDto> AddAsync(Guid customerId, string message, string type, CancellationToken ct)
    {
        var notification = new NotificationDto(Guid.NewGuid(), customerId, message, type, IsRead: false, DateTimeOffset.UtcNow);

        await Db.StringSetAsync(NotificationKey(notification.Id), JsonSerializer.Serialize(notification), NotificationTtl);

        var score = notification.CreatedAt.ToUnixTimeSeconds();
        await Db.SortedSetAddAsync(IndexKey(customerId), notification.Id.ToString(), score);
        await Db.SortedSetRemoveRangeByRankAsync(IndexKey(customerId), 0, -(RecentHistoryWindow + 1));

        await Db.SetAddAsync(UnreadKey(customerId), notification.Id.ToString());

        return notification;
    }

    public async Task<IReadOnlyCollection<NotificationDto>> ListAsync(Guid customerId, bool onlyUnread, CancellationToken ct)
    {
        var ids = await Db.SortedSetRangeByRankAsync(IndexKey(customerId), 0, -1, Order.Descending);
        if (ids.Length == 0) return [];

        HashSet<string>? unreadIds = null;
        if (onlyUnread)
        {
            var unread = await Db.SetMembersAsync(UnreadKey(customerId));
            unreadIds = unread.Select(v => v.ToString()).ToHashSet();
        }

        var keys = ids.Select(id => (RedisKey)NotificationKey(Guid.Parse((string)id!))).ToArray();
        var values = await Db.StringGetAsync(keys);

        var results = new List<NotificationDto>();
        for (var i = 0; i < ids.Length; i++)
        {
            if (!values[i].HasValue) continue;
            if (unreadIds is not null && !unreadIds.Contains(ids[i]!)) continue;
            results.Add(JsonSerializer.Deserialize<NotificationDto>((string)values[i]!)!);
        }

        return results;
    }

    public Task<long> UnreadCountAsync(Guid customerId, CancellationToken ct) =>
        Db.SetLengthAsync(UnreadKey(customerId));

    public async Task<bool> MarkReadAsync(Guid customerId, Guid notificationId, CancellationToken ct)
    {
        var value = await Db.StringGetAsync(NotificationKey(notificationId));
        if (!value.HasValue) return false;

        var notification = JsonSerializer.Deserialize<NotificationDto>((string)value!)! with { IsRead = true };
        await Db.StringSetAsync(NotificationKey(notificationId), JsonSerializer.Serialize(notification), NotificationTtl);
        await Db.SetRemoveAsync(UnreadKey(customerId), notificationId.ToString());

        return true;
    }
}
