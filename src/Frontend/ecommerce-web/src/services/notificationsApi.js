import { apiClient } from './apiClient';

export async function listNotifications(onlyUnread = false) {
  const { data } = await apiClient.get('/api/notifications', { params: { onlyUnread } });
  return data;
}

export async function getUnreadCount() {
  const { data } = await apiClient.get('/api/notifications/unread-count');
  return data.count;
}

export async function markNotificationRead(id) {
  await apiClient.post(`/api/notifications/${id}/read`);
}
