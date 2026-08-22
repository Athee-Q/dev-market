import { Link } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { Icon } from '../components/Icon';

export function DashboardPage() {
  const { user, isAdmin } = useAuth();

  return (
    <div className="page">
      <div className="hero">
        <span className="hero-eyebrow"><Icon name="rocket" size={13} /> Software &amp; API marketplace</span>
        <h1>{user ? `Welcome back, ${user.fullName.split(' ')[0]}` : 'Ship faster with ready-made access'} 👋</h1>
        <p>
          Browse licenses, API keys, SaaS plans and project bundles — buy instantly and get a
          delivered credential on the spot. Built on an independent-services platform under the hood.
        </p>
        <Link to="/products" className="button">Browse Catalog</Link>
      </div>

      {!user && (
        <div className="callout">
          <p>Sign in to buy, track purchases, and manage issued keys.</p>
          <Link to="/login" className="button">Sign in</Link>
        </div>
      )}

      <div className="dashboard-grid">
        <Link to="/products" className="dashboard-card">
          <div className="dashboard-card-icon"><Icon name="box" /></div>
          <h2>Catalog</h2>
          <p>Licenses, API access, SaaS plans, projects</p>
        </Link>
        <Link to="/cart" className="dashboard-card">
          <div className="dashboard-card-icon"><Icon name="cart" /></div>
          <h2>Cart</h2>
          <p>Review items before checkout</p>
        </Link>
        {user && (
          <Link to="/my-products" className="dashboard-card">
            <div className="dashboard-card-icon"><Icon name="key" /></div>
            <h2>My Products</h2>
            <p>Access keys and delivered assets</p>
          </Link>
        )}
        {user && (
          <Link to="/orders" className="dashboard-card">
            <div className="dashboard-card-icon"><Icon name="package" /></div>
            <h2>Orders</h2>
            <p>Track order status</p>
          </Link>
        )}
        {user && (
          <Link to="/transactions" className="dashboard-card">
            <div className="dashboard-card-icon"><Icon name="card" /></div>
            <h2>Transactions</h2>
            <p>Payment history</p>
          </Link>
        )}
        {user && (
          <Link to="/notifications" className="dashboard-card">
            <div className="dashboard-card-icon"><Icon name="bell" /></div>
            <h2>Notifications</h2>
            <p>Order confirmations and updates</p>
          </Link>
        )}
        {isAdmin && (
          <Link to="/admin" className="dashboard-card">
            <div className="dashboard-card-icon"><Icon name="shield" /></div>
            <h2>Admin Console</h2>
            <p>Manage catalog, orders, and users</p>
          </Link>
        )}
      </div>
    </div>
  );
}
