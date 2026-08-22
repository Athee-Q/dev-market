import { useContext } from 'react';
import { NotificationsContext } from '../app/NotificationsContext';

export function useNotifications() {
  const ctx = useContext(NotificationsContext);
  if (!ctx) throw new Error('useNotifications must be used within a NotificationsProvider');
  return ctx;
}
