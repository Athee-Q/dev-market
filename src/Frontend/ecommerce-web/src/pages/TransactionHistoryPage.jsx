import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { searchPayments } from '../services/paymentsApi';
import { useAuth } from '../hooks/useAuth';
import { formatCurrency } from '../utils/formatCurrency';
import { Icon } from '../components/Icon';

export function TransactionHistoryPage() {
  const { isAdmin } = useAuth();
  const [customerFilter, setCustomerFilter] = useState('');
  const [result, setResult] = useState({ items: [], totalCount: 0 });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    searchPayments({ customerId: isAdmin && customerFilter ? customerFilter : undefined, pageSize: 50 })
      .then(setResult)
      .finally(() => setLoading(false));
  }, [isAdmin, customerFilter]);

  return (
    <div className="page">
      <div className="page-header-row">
        <div>
          <h1>Transactions</h1>
          <p className="page-subtitle">{isAdmin ? 'Every payment across all customers.' : 'Your payment history.'}</p>
        </div>
      </div>

      {isAdmin && (
        <input
          className="search-input"
          placeholder="Filter by customer id..."
          value={customerFilter}
          onChange={(e) => setCustomerFilter(e.target.value)}
        />
      )}

      {loading && <p>Loading...</p>}
      {!loading && result.items.length === 0 && <p>No transactions yet.</p>}

      {!loading && result.items.length > 0 && (
        <div className="data-table-wrap">
          <table className="data-table">
            <thead>
              <tr>
                <th>Order</th>
                <th>Amount</th>
                <th>Status</th>
                <th>Date</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {result.items.map((payment) => (
                <tr key={payment.id}>
                  <td className="mono">{payment.orderNumber}</td>
                  <td>{formatCurrency(payment.amount)}</td>
                  <td><span className={`status-pill status-${payment.status.toLowerCase()}`}>{payment.status}</span></td>
                  <td>{new Date(payment.createdAt).toLocaleString()}</td>
                  <td>
                    <Link to={`/orders/${payment.orderId}`} className="link-button">
                      <Icon name="external" size={13} /> Order
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
