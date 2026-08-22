import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { cancelOrder, getOrder } from '../services/ordersApi';
import { getPaymentForOrder, verifyPayment } from '../services/paymentsApi';
import { useAuth } from '../hooks/useAuth';
import { useToast } from '../hooks/useToast';
import { loadRazorpayCheckout } from '../utils/loadRazorpayCheckout';
import { formatCurrency } from '../utils/formatCurrency';
import { productTypeLabel } from '../utils/productType';
import { Icon } from '../components/Icon';
import { UpiQrPanel } from '../components/UpiQrPanel';

const CANCELLABLE_STATUSES = new Set(['Pending', 'Confirmed']);
const PAYABLE_STATUSES = new Set(['Confirmed', 'PaymentFailed']);
const SETTLED_STATUSES = new Set(['Completed', 'PaymentFailed']);

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

export function OrderDetailsPage() {
  const { id } = useParams();
  const { user } = useAuth();
  const { addToast } = useToast();
  const [order, setOrder] = useState(null);
  const [cancelling, setCancelling] = useState(false);
  const [paying, setPaying] = useState(false);
  const [payError, setPayError] = useState(null);
  const [copiedId, setCopiedId] = useState(null);
  const [showUpiQr, setShowUpiQr] = useState(false);

  useEffect(() => {
    getOrder(id).then(setOrder);
  }, [id]);

  async function handleCancel() {
    setCancelling(true);
    try {
      setOrder(await cancelOrder(id));
    } finally {
      setCancelling(false);
    }
  }

  // Order Service completes the order asynchronously (PaymentSucceededEvent → MassTransit →
  // CompleteAfterPaymentAsync), so right after Verify returns the order is often still
  // "Confirmed" — poll briefly rather than assume the response reflects the final state.
  async function pollUntilSettled() {
    for (let attempt = 0; attempt < 8; attempt++) {
      await sleep(1500);
      const latest = await getOrder(id);
      setOrder(latest);
      if (SETTLED_STATUSES.has(latest.status)) return;
    }
  }

  async function handlePay() {
    setPaying(true);
    setPayError(null);
    try {
      const payment = await getPaymentForOrder(id);
      if (!payment) {
        setPayError('Payment is still being prepared for this order — please try again in a few seconds.');
        setPaying(false);
        return;
      }

      await loadRazorpayCheckout();

      const razorpay = new window.Razorpay({
        key: payment.razorpayKeyId,
        amount: Math.round(payment.amount * 100),
        currency: payment.currency,
        name: 'DevMarket',
        description: `Order ${order.orderNumber}`,
        order_id: payment.razorpayOrderId,
        prefill: user?.fullName ? { name: user.fullName, email: user.email } : undefined,
        theme: { color: '#7c6ffd' },
        modal: {
          ondismiss: () => setPaying(false),
        },
        // UPI itself needs no backend change — Checkout already shows every method enabled on
        // the Razorpay account (Dashboard → Settings → Payment Methods; on by default in test
        // mode). This just makes UPI (intent apps + VPA collect + QR) the first, prominent tab
        // instead of one option buried in a long list — see Razorpay's Checkout "config" docs.
        config: {
          display: {
            blocks: {
              upi: {
                name: 'Pay via UPI',
                instruments: [{ method: 'upi' }],
              },
              other: {
                name: 'Other payment methods',
                instruments: [
                  { method: 'card' },
                  { method: 'netbanking' },
                  { method: 'wallet' },
                ],
              },
            },
            sequence: ['block.upi', 'block.other'],
            preferences: { show_default_blocks: false },
          },
        },
        handler: async (response) => {
          try {
            await verifyPayment({
              orderId: id,
              razorpayOrderId: response.razorpay_order_id,
              razorpayPaymentId: response.razorpay_payment_id,
              razorpaySignature: response.razorpay_signature,
            });
            addToast('Payment verified — confirming your order...', 'success');
            await pollUntilSettled();
          } catch {
            setPayError('Payment verification failed. Please try again.');
          } finally {
            setPaying(false);
          }
        },
      });

      razorpay.on('payment.failed', (response) => {
        setPayError(response.error?.description || 'Payment failed.');
        setPaying(false);
      });

      razorpay.open();
    } catch {
      setPayError('Could not start the payment. Please try again.');
      setPaying(false);
    }
  }

  function handleCopy(item) {
    navigator.clipboard?.writeText(item.accessKey).then(() => {
      setCopiedId(item.id);
      setTimeout(() => setCopiedId(null), 1500);
    });
  }

  async function handleUpiPaid() {
    setShowUpiQr(false);
    addToast('Payment received — confirming your order...', 'success');
    await pollUntilSettled();
  }

  if (!order) return <div className="page"><p>Loading...</p></div>;

  return (
    <div className="page">
      <h1>{order.orderNumber}</h1>
      <p className={`status-pill status-${order.status.toLowerCase()}`}>{order.status}</p>
      <p className="page-subtitle">Placed {new Date(order.createdAt).toLocaleString()}</p>

      <table className="cart-table">
        <thead>
          <tr>
            <th>Product</th>
            <th>Type</th>
            <th>Unit Price</th>
            <th>Qty</th>
            <th>Total</th>
          </tr>
        </thead>
        <tbody>
          {order.items.map((item) => (
            <tr key={item.id}>
              <td>{item.productName}</td>
              <td>{productTypeLabel(item.productType)}</td>
              <td>{formatCurrency(item.unitPrice)}</td>
              <td>{item.quantity}</td>
              <td>{formatCurrency(item.totalPrice)}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <p className="checkout-total">Total: {formatCurrency(order.totalAmount)}</p>

      {order.status === 'Completed' && (
        <>
          <h2 style={{ fontSize: '1.05rem' }}>Delivered access</h2>
          {order.items.map((item) => (
            <div key={item.id} className="purchase-card">
              <div className="purchase-card-header">
                <h3>{item.productName}</h3>
                <span className={`badge-type badge-type-${item.productType.toLowerCase()}`} style={{ position: 'static' }}>
                  {productTypeLabel(item.productType)}
                </span>
              </div>
              {item.accessKey && (
                <div className="key-chip">
                  <span className="key-chip-value">{item.accessKey}</span>
                  <button onClick={() => handleCopy(item)} title="Copy">
                    <Icon name={copiedId === item.id ? 'check' : 'copy'} size={15} />
                  </button>
                </div>
              )}
              {item.assetUrl && (
                <div className="purchase-card-actions">
                  <a href={item.assetUrl} target="_blank" rel="noreferrer" className="button button-ghost button-sm">
                    <Icon name="external" size={14} /> Open asset
                  </a>
                </div>
              )}
            </div>
          ))}
        </>
      )}

      {payError && <p className="error-text">{payError}</p>}

      <div className="order-actions">
        {PAYABLE_STATUSES.has(order.status) && (
          <button className="button" onClick={handlePay} disabled={paying}>
            {paying ? 'Processing payment...' : order.status === 'PaymentFailed' ? 'Retry Payment' : 'Pay Now'}
          </button>
        )}

        {PAYABLE_STATUSES.has(order.status) && (
          <button className="button button-ghost" onClick={() => setShowUpiQr(true)}>
            <Icon name="key" size={15} /> Pay with UPI
          </button>
        )}

        {CANCELLABLE_STATUSES.has(order.status) && (
          <button className="button button-danger" onClick={handleCancel} disabled={cancelling}>
            {cancelling ? 'Cancelling...' : 'Cancel Order'}
          </button>
        )}
      </div>

      {showUpiQr && (
        <UpiQrPanel orderId={id} onPaid={handleUpiPaid} onClose={() => setShowUpiQr(false)} />
      )}
    </div>
  );
}
