import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCart } from '../hooks/useCart';
import { useToast } from '../hooks/useToast';
import { createOrder } from '../services/ordersApi';
import { formatCurrency } from '../utils/formatCurrency';

export function CheckoutPage() {
  const { cart, updateItem, refresh } = useCart();
  const { addToast } = useToast();
  const navigate = useNavigate();
  const [placing, setPlacing] = useState(false);
  const [error, setError] = useState(null);

  async function handlePlaceOrder() {
    setPlacing(true);
    setError(null);
    try {
      const items = cart.items.map((i) => ({ productId: i.productId, quantity: i.quantity }));
      const order = await createOrder(items);

      // Cart Service has no bulk-clear endpoint (§8) — remove each line individually.
      await Promise.all(cart.items.map((i) => updateItem(i.productId, 0)));
      await refresh();

      addToast(`Order ${order.orderNumber} confirmed — continue to payment.`, 'success');
      navigate(`/orders/${order.id}`);
    } catch {
      setError('Could not place the order. Please try again.');
    } finally {
      setPlacing(false);
    }
  }

  if (cart.items.length === 0) {
    return <div className="page"><p>Your cart is empty.</p></div>;
  }

  return (
    <div className="page">
      <h1>Checkout</h1>

      <ul className="checkout-summary">
        {cart.items.map((item) => (
          <li key={item.productId}>
            {item.productName} × {item.quantity} — {formatCurrency(item.totalPrice)}
          </li>
        ))}
      </ul>
      <p className="checkout-total">Total: {formatCurrency(cart.totalAmount)}</p>

      {error && <p className="error-text">{error}</p>}

      <button className="button" onClick={handlePlaceOrder} disabled={placing}>
        {placing ? 'Placing order...' : 'Place Order'}
      </button>
    </div>
  );
}
