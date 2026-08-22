import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getProduct } from '../services/productsApi';
import { formatCurrency } from '../utils/formatCurrency';
import { productGradient, productInitials } from '../utils/productColor';
import { productTypeIcon, productTypeLabel, pricingModelLabel } from '../utils/productType';
import { useCart } from '../hooks/useCart';
import { useAuth } from '../hooks/useAuth';
import { useToast } from '../hooks/useToast';
import { Icon } from '../components/Icon';

export function ProductDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
  const { addItem } = useCart();
  const { addToast } = useToast();
  const [product, setProduct] = useState(null);
  const [quantity, setQuantity] = useState(1);
  const [error, setError] = useState(null);

  useEffect(() => {
    getProduct(id).then(setProduct).catch(() => setError(true));
  }, [id]);

  async function handleAddToCart() {
    if (!user) {
      navigate('/login');
      return;
    }
    await addItem(id, quantity);
    addToast(`Added ${quantity} × ${product.name} to cart`, 'success');
  }

  if (error) return <div className="page"><p className="error-text">Product not found.</p></div>;
  if (!product) return <div className="page"><p>Loading...</p></div>;

  return (
    <div className="page">
      <div className="product-detail">
        <div className="product-media product-detail-media" style={{ background: productGradient(product.id) }}>
          <span className={`badge-type badge-type-${product.productType.toLowerCase()} product-type-badge`}>
            <Icon name={productTypeIcon(product.productType)} size={13} />
            {productTypeLabel(product.productType)}
          </span>
          <span>{productInitials(product.name)}</span>
          {!product.isActive && <span className="badge-muted product-media-badge">Unavailable</span>}
        </div>

        <div className="product-detail-info">
          <p className="product-sku">{product.sku}</p>
          <h1>{product.name}</h1>
          <p className="product-price-large">{formatCurrency(product.price)}</p>
          {product.pricingModel !== 'OneTime' && (
            <p className="product-pricing-model">Billed {pricingModelLabel(product.pricingModel)}</p>
          )}
          <p className="product-description">{product.description}</p>

          <div className="add-to-cart-row">
            <input
              type="number"
              min={1}
              value={quantity}
              onChange={(e) => setQuantity(Math.max(1, Number(e.target.value)))}
            />
            <button className="button" onClick={handleAddToCart} disabled={!product.isActive}>
              <Icon name="cart" size={15} /> Add to Cart
            </button>
          </div>
          {!product.isActive && <p className="error-text">This product is not currently available.</p>}
        </div>
      </div>
    </div>
  );
}
