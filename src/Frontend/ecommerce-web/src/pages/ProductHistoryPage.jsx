import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { searchOrders } from '../services/ordersApi';
import { useAuth } from '../hooks/useAuth';
import { formatCurrency } from '../utils/formatCurrency';
import { productTypeIcon, productTypeLabel } from '../utils/productType';
import { Icon } from '../components/Icon';

export function ProductHistoryPage() {
  const { user } = useAuth();
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [copiedId, setCopiedId] = useState(null);
  const [revealed, setRevealed] = useState(() => new Set());

  useEffect(() => {
    searchOrders({ customerId: user.id, status: 'Completed', pageSize: 100 })
      .then((result) => setOrders(result.items))
      .finally(() => setLoading(false));
  }, [user.id]);

  function toggleReveal(itemId) {
    setRevealed((current) => {
      const next = new Set(current);
      if (next.has(itemId)) next.delete(itemId); else next.add(itemId);
      return next;
    });
  }

  function handleCopy(item) {
    navigator.clipboard?.writeText(item.accessKey).then(() => {
      setCopiedId(item.id);
      setTimeout(() => setCopiedId(null), 1500);
    });
  }

  const items = orders.flatMap((order) => order.items.map((item) => ({ ...item, order })));

  return (
    <div className="page">
      <h1>My Products</h1>
      <p className="page-subtitle">Every access key and delivered asset from your completed purchases.</p>

      {loading && <p>Loading...</p>}
      {!loading && items.length === 0 && (
        <p>Nothing delivered yet — <Link to="/products">browse the catalog</Link> to buy your first product.</p>
      )}

      {items.map((item) => (
        <div key={item.id} className="purchase-card">
          <div className="purchase-card-header">
            <h3>{item.productName}</h3>
            <span className={`badge-type badge-type-${item.productType.toLowerCase()}`} style={{ position: 'static' }}>
              <Icon name={productTypeIcon(item.productType)} size={12} /> {productTypeLabel(item.productType)}
            </span>
          </div>
          <p className="purchase-card-meta">
            Order {item.order.orderNumber} · {formatCurrency(item.totalPrice)} · {new Date(item.order.updatedAt).toLocaleDateString()}
          </p>

          {item.accessKey && (
            <div className="key-chip">
              <span className="key-chip-value">
                {revealed.has(item.id) ? item.accessKey : '•'.repeat(28)}
              </span>
              <button onClick={() => toggleReveal(item.id)} title={revealed.has(item.id) ? 'Hide' : 'Reveal'}>
                <Icon name={revealed.has(item.id) ? 'chevron' : 'key'} size={15} />
              </button>
              <button onClick={() => handleCopy(item)} title="Copy">
                <Icon name={copiedId === item.id ? 'check' : 'copy'} size={15} />
              </button>
            </div>
          )}

          <div className="purchase-card-actions">
            {item.assetUrl && (
              <a href={item.assetUrl} target="_blank" rel="noreferrer" className="button button-ghost button-sm">
                <Icon name="external" size={14} /> Open asset
              </a>
            )}
            <Link to={`/orders/${item.order.id}`} className="link-button">View order</Link>
          </div>
        </div>
      ))}
    </div>
  );
}
