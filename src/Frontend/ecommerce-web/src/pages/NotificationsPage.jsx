import { useState } from 'react';
import { useNotifications } from '../hooks/useNotifications';

const TABS = ['All', 'Unread', 'Read'];

export function NotificationsPage() {
  const { notifications, markAsRead, refresh } = useNotifications();
  const [tab, setTab] = useState('All');

  const filtered = notifications.filter((n) => {
    if (tab === 'Unread') return !n.isRead;
    if (tab === 'Read') return n.isRead;
    return true;
  });

  return (
    <div className="page">
      <div className="page-header-row">
        <h1>Notifications</h1>
        <button className="link-button" onClick={() => refresh()}>Refresh</button>
      </div>

      <div className="tabs">
        {TABS.map((t) => (
          <button key={t} className={`tab ${tab === t ? 'tab-active' : ''}`} onClick={() => setTab(t)}>
            {t}
          </button>
        ))}
      </div>

      <ul className="notification-list">
        {filtered.map((n) => (
          <li key={n.id} className={n.isRead ? '' : 'notification-unread'}>
            <div>
              <p>{n.message}</p>
              <span className="notification-meta">{new Date(n.createdAt).toLocaleString()}</span>
            </div>
            {!n.isRead && (
              <button className="link-button" onClick={() => markAsRead(n.id)}>
                Mark as read
              </button>
            )}
          </li>
        ))}
      </ul>

      {filtered.length === 0 && <p>No notifications here.</p>}
    </div>
  );
}
