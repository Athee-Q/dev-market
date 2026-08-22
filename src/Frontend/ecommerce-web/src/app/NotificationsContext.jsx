import { createContext, useCallback, useEffect, useMemo, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { getApiBaseUrl } from '../services/apiClient';
import { getUnreadCount, listNotifications, markNotificationRead } from '../services/notificationsApi';
import { getAuth } from '../services/authStorage';
import { useAuth } from '../hooks/useAuth';
import { useToast } from '../hooks/useToast';

export const NotificationsContext = createContext(null);

/**
 * Owns the SignalR connection to Notification Service's /hubs/notifications (§12) and keeps the
 * bell's unread count in sync. The hub is JWT-authenticated (see Notification.Api's Program.cs) —
 * the token travels as a query string param since a WebSocket upgrade can't carry an Authorization
 * header — and the server auto-joins the caller's own group from the token (see NotificationHub),
 * so the client no longer sends its own id anywhere.
 */
export function NotificationsProvider({ children }) {
  const { user } = useAuth();
  const { addToast } = useToast();
  const [unreadCount, setUnreadCount] = useState(0);
  const [notifications, setNotifications] = useState([]);
  const [connectionState, setConnectionState] = useState('disconnected');
  const connectionRef = useRef(null);

  const refresh = useCallback(async () => {
    if (!user) return;
    const [count, list] = await Promise.all([getUnreadCount(), listNotifications(false)]);
    setUnreadCount(count);
    setNotifications(list);
  }, [user]);

  const markAsRead = useCallback(async (id) => {
    await markNotificationRead(id);
    setNotifications((current) => current.map((n) => (n.id === id ? { ...n, isRead: true } : n)));
    setUnreadCount((current) => Math.max(0, current - 1));
  }, []);

  useEffect(() => {
    if (!user) return undefined;

    refresh().catch(() => {});

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${getApiBaseUrl()}/hubs/notifications`, {
        accessTokenFactory: () => getAuth()?.accessToken ?? '',
      })
      .withAutomaticReconnect()
      .build();

    connection.on('notificationReceived', (notification) => {
      setNotifications((current) => [notification, ...current]);
      setUnreadCount((current) => current + 1);
      addToast(notification.message, 'info');
    });

    connection.onreconnecting(() => setConnectionState('reconnecting'));
    connection.onreconnected(() => setConnectionState('connected'));
    connection.onclose(() => setConnectionState('disconnected'));

    connection
      .start()
      .then(() => setConnectionState('connected'))
      .catch(() => setConnectionState('disconnected'));

    connectionRef.current = connection;

    return () => {
      connection.stop();
      connectionRef.current = null;
    };
  }, [user, refresh, addToast]);

  const value = useMemo(
    () => ({ unreadCount, notifications, connectionState, refresh, markAsRead }),
    [unreadCount, notifications, connectionState, refresh, markAsRead],
  );

  return <NotificationsContext.Provider value={value}>{children}</NotificationsContext.Provider>;
}
