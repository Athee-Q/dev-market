import { NavLink, Outlet } from 'react-router-dom';
import { Icon } from './Icon';

const LINKS = [
  { to: '/admin', end: true, icon: 'dashboard', label: 'Dashboard' },
  { to: '/admin/products', icon: 'box', label: 'Products' },
  { to: '/admin/orders', icon: 'package', label: 'Orders' },
  { to: '/admin/users', icon: 'user', label: 'Users' },
];

export function AdminLayout() {
  return (
    <div className="admin-shell">
      <aside className="admin-sidebar">
        <span className="admin-sidebar-title">Admin Console</span>
        {LINKS.map((link) => (
          <NavLink key={link.to} to={link.to} end={link.end} className="admin-nav-link">
            <Icon name={link.icon} size={16} />
            {link.label}
          </NavLink>
        ))}
      </aside>
      <div className="admin-content">
        <Outlet />
      </div>
    </div>
  );
}
