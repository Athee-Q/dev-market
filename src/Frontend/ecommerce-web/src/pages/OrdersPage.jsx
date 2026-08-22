import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { searchOrders } from '../services/ordersApi';
import { useAuth } from '../hooks/useAuth';
import { formatCurrency } from '../utils/formatCurrency';

export function OrdersPage() {
  const { user } = useAuth();
  const [result, setResult] = useState({ items: [] });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    searchOrders({ customerId: user.id }).then(setResult).finally(() => setLoading(false));
  }, [user.id]);

  return (
    <div className="page">
      <h1>Orders</h1>

      {loading && <p>Loading...</p>}
      {!loading && result.items.length === 0 && <p>You haven't placed any orders yet.</p>}

      <ul className="order-list">
        {result.items.map((order) => (
          <li key={order.id}>
            <Link to={`/orders/${order.id}`}>
              <span className="order-number">{order.orderNumber}</span>
              <span className={`status-pill status-${order.status.toLowerCase()}`}>{order.status}</span>
              <span>{formatCurrency(order.totalAmount)}</span>
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}
