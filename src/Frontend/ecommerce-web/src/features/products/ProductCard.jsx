import { Link } from 'react-router-dom';
import { formatCurrency } from '../../utils/formatCurrency';
import { productGradient, productInitials } from '../../utils/productColor';
import { productTypeIcon, productTypeLabel, pricingModelLabel } from '../../utils/productType';
import { Icon } from '../../components/Icon';

export function ProductCard({ product }) {
  return (
    <Link to={`/products/${product.id}`} className="product-card">
      <div className="product-media" style={{ background: productGradient(product.id) }}>
        <span className={`badge-type badge-type-${product.productType.toLowerCase()} product-type-badge`}>
          <Icon name={productTypeIcon(product.productType)} size={12} />
          {productTypeLabel(product.productType)}
        </span>
        <span>{productInitials(product.name)}</span>
        {!product.isActive && <span className="badge-muted product-media-badge">Unavailable</span>}
      </div>
      <div className="product-card-body">
        <h3>{product.name}</h3>
        <p className="product-sku">{product.sku}</p>
        <p className="product-price">
          {formatCurrency(product.price)}
          {product.pricingModel !== 'OneTime' && <span style={{ color: 'var(--color-text-faint)', fontWeight: 500 }}> {pricingModelLabel(product.pricingModel)}</span>}
        </p>
      </div>
    </Link>
  );
}
