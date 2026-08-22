import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

/** Guards Admin-only routes (currently just /admin/*) — assumes RequireAuth already ran higher up the tree. */
export function RequireRole({ role = 'Admin' }) {
  const { user } = useAuth();
  if (!user?.roles?.includes(role)) return <Navigate to="/" replace />;
  return <Outlet />;
}
