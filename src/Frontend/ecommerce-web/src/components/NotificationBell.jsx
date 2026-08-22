import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useNotifications } from '../hooks/useNotifications';
import { Icon } from './Icon';

export function NotificationBell() {
  const { unreadCount, connectionState } = useNotifications();
  const [open, setOpen] = useState(false);

  return (
    <div className="notification-bell">
      <button
        className="icon-button"
        onClick={() => setOpen((o) => !o)}
        aria-label="Notifications"
        title={`Realtime: ${connectionState}`}
      >
        <Icon name="bell" size={17} />
        {unreadCount > 0 && <span className="badge">{unreadCount > 99 ? '99+' : unreadCount}</span>}
        <span className={`status-dot status-${connectionState}`} />
      </button>
      {open && (
        <div className="dropdown">
          <Link to="/notifications" onClick={() => setOpen(false)}>
            View all notifications
          </Link>
        </div>
      )}
    </div>
  );
}
