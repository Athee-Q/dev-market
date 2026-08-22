import { useState } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { NotificationBell } from './NotificationBell';
import { ToastContainer } from './ToastContainer';
import { Icon } from './Icon';
import { useCart } from '../hooks/useCart';
import { useAuth } from '../hooks/useAuth';

export function Layout() {
  const { itemCount } = useCart();
  const { user, isAdmin, logout } = useAuth();
  const navigate = useNavigate();
  const [menuOpen, setMenuOpen] = useState(false);

  async function handleLogout() {
    setMenuOpen(false);
    await logout();
    navigate('/login');
  }

  return (
    <div className="app-shell">
      <header className="app-header">
        <div className="app-header-inner">
          <NavLink to="/" className="brand">
            <span className="brand-mark"><Icon name="layers" size={16} /></span>
            DevMarket
          </NavLink>
          <nav className="main-nav">
            <NavLink to="/products"><Icon name="box" size={15} />Catalog</NavLink>
            {user && <NavLink to="/orders"><Icon name="package" size={15} />Orders</NavLink>}
            {user && <NavLink to="/my-products"><Icon name="key" size={15} />My Products</NavLink>}
            {user && <NavLink to="/transactions"><Icon name="card" size={15} />Transactions</NavLink>}
            {isAdmin && <NavLink to="/admin"><Icon name="shield" size={15} />Admin</NavLink>}
          </nav>
          <div className="header-actions">
            <NavLink to="/cart" className="cart-link">
              <Icon name="cart" size={16} /> {itemCount > 0 && <span className="badge">{itemCount}</span>}
            </NavLink>
            {user && <NotificationBell />}

            {user ? (
              <div className="user-menu">
                <button className="user-chip" onClick={() => setMenuOpen((o) => !o)}>
                  <span className="user-avatar">{user.fullName?.[0]?.toUpperCase() ?? '?'}</span>
                  {user.fullName?.split(' ')[0]}
                  <Icon name="chevron" size={13} />
                </button>
                {menuOpen && (
                  <div className="dropdown">
                    <span className="user-menu-email">{user.email}</span>
                    <NavLink to="/profile" onClick={() => setMenuOpen(false)}>
                      <Icon name="user" size={14} style={{ marginRight: 6 }} />Profile
                    </NavLink>
                    <button onClick={handleLogout}>
                      <Icon name="logout" size={14} style={{ marginRight: 6 }} />Sign out
                    </button>
                  </div>
                )}
              </div>
            ) : (
              <NavLink to="/login" className="button button-sm">Sign in</NavLink>
            )}
          </div>
        </div>
      </header>

      <main className="app-content">
        <Outlet />
      </main>

      <ToastContainer />
    </div>
  );
}
