import { useEffect, useState } from 'react';
import { getRevenueSummary, searchPayments } from '../../services/paymentsApi';
import { searchOrders } from '../../services/ordersApi';
import { searchProducts } from '../../services/productsApi';
import { listUsers } from '../../services/authApi';
import { formatCurrency } from '../../utils/formatCurrency';
import { Icon } from '../../components/Icon';
import { Sparkline } from '../../components/Sparkline';

export function AdminDashboardPage() {
  const [summary, setSummary] = useState(null);
  const [orderCount, setOrderCount] = useState(null);
  const [productCount, setProductCount] = useState(null);
  const [userCount, setUserCount] = useState(null);
  const [trend, setTrend] = useState([]);

  useEffect(() => {
    getRevenueSummary().then(setSummary);
    searchOrders({ pageSize: 1 }).then((r) => setOrderCount(r.totalCount));
    searchProducts({ pageSize: 1 }).then((r) => setProductCount(r.totalCount));
    listUsers({ pageSize: 1 }).then((r) => setUserCount(r.totalCount));
    searchPayments({ status: 'Succeeded', pageSize: 12 })
      .then((r) => setTrend(r.items.map((p) => p.amount).reverse()));
  }, []);

  return (
    <div>
      <h1>Dashboard</h1>
      <p className="page-subtitle">A snapshot of the marketplace.</p>

      <div className="stat-grid">
        <div className="stat-tile">
          <div className="stat-tile-label"><Icon name="card" size={15} />Revenue</div>
          <p className="stat-tile-value">{summary ? formatCurrency(summary.totalRevenue) : '—'}</p>
          <p className="stat-tile-meta">{summary ? `${summary.succeededPaymentCount} successful payments` : 'Loading...'}</p>
          <Sparkline values={trend} />
        </div>
        <div className="stat-tile">
          <div className="stat-tile-label"><Icon name="package" size={15} />Orders</div>
          <p className="stat-tile-value">{orderCount ?? '—'}</p>
          <p className="stat-tile-meta">Across all customers</p>
        </div>
        <div className="stat-tile">
          <div className="stat-tile-label"><Icon name="box" size={15} />Products</div>
          <p className="stat-tile-value">{productCount ?? '—'}</p>
          <p className="stat-tile-meta">Listed in the catalog</p>
        </div>
        <div className="stat-tile">
          <div className="stat-tile-label"><Icon name="user" size={15} />Users</div>
          <p className="stat-tile-value">{userCount ?? '—'}</p>
          <p className="stat-tile-meta">Registered accounts</p>
        </div>
      </div>
    </div>
  );
}
