import { Link } from 'react-router-dom';
import { useCart } from '../hooks/useCart';
import { formatCurrency } from '../utils/formatCurrency';

export function CartPage() {
  const { cart, updateItem, removeItem, loading } = useCart();

  if (loading) return <div className="page"><p>Loading...</p></div>;

  return (
    <div className="page">
      <h1>Your Cart</h1>

      {cart.items.length === 0 && (
        <p>
          Your cart is empty. <Link to="/products">Browse products</Link>.
        </p>
      )}

      {cart.items.length > 0 && (
        <>
          <table className="cart-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>Unit Price</th>
                <th>Quantity</th>
                <th>Total</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {cart.items.map((item) => (
                <tr key={item.productId}>
                  <td>{item.productName}</td>
                  <td>{formatCurrency(item.unitPrice)}</td>
                  <td>
                    <input
                      type="number"
                      min={0}
                      value={item.quantity}
                      onChange={(e) => updateItem(item.productId, Math.max(0, Number(e.target.value)))}
                    />
                  </td>
                  <td>{formatCurrency(item.totalPrice)}</td>
                  <td>
                    <button className="link-button" onClick={() => removeItem(item.productId)}>
                      Remove
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className="cart-summary">
            <strong>Total: {formatCurrency(cart.totalAmount)}</strong>
            <Link to="/checkout" className="button">
              Checkout
            </Link>
          </div>
        </>
      )}
    </div>
  );
}
