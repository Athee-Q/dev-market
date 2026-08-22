import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { searchOrders } from '../../services/ordersApi';
import { formatCurrency } from '../../utils/formatCurrency';
import { Icon } from '../../components/Icon';

const STATUSES = ['Pending', 'Confirmed', 'PaymentFailed', 'Cancelled', 'Completed'];

export function AdminOrdersPage() {
  const [status, setStatus] = useState('');
  const [result, setResult] = useState({ items: [], totalCount: 0 });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    searchOrders({ status: status || undefined, pageSize: 50 }).then(setResult).finally(() => setLoading(false));
  }, [status]);

  return (
    <div>
      <div className="page-header-row">
        <div>
          <h1>Orders</h1>
          <p className="page-subtitle">Every order placed across the marketplace.</p>
        </div>
      </div>

      <div className="filter-tabs">
        <button className={`filter-tab ${!status ? 'active' : ''}`} onClick={() => setStatus('')}>All</button>
        {STATUSES.map((s) => (
          <button key={s} className={`filter-tab ${status === s ? 'active' : ''}`} onClick={() => setStatus(s)}>{s}</button>
        ))}
      </div>

      {loading && <p>Loading...</p>}
      {!loading && result.items.length === 0 && <p>No orders match this filter.</p>}

      {!loading && result.items.length > 0 && (
        <div className="data-table-wrap">
          <table className="data-table">
            <thead>
              <tr>
                <th>Order</th>
                <th>Customer</th>
                <th>Total</th>
                <th>Status</th>
                <th>Placed</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {result.items.map((order) => (
                <tr key={order.id}>
                  <td className="mono">{order.orderNumber}</td>
                  <td className="mono">{order.customerId.slice(0, 8)}...</td>
                  <td>{formatCurrency(order.totalAmount)}</td>
                  <td><span className={`status-pill status-${order.status.toLowerCase()}`}>{order.status}</span></td>
                  <td>{new Date(order.createdAt).toLocaleDateString()}</td>
                  <td>
                    <Link to={`/orders/${order.id}`} className="link-button">
                      <Icon name="external" size={13} /> View
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
