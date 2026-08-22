import { useEffect, useState } from 'react';
import { searchProducts } from '../services/productsApi';
import { ProductCard } from '../features/products/ProductCard';
import { PRODUCT_TYPES, productTypeLabel } from '../utils/productType';
import { Icon } from '../components/Icon';

export function ProductsPage() {
  const [search, setSearch] = useState('');
  const [productType, setProductType] = useState(null);
  const [result, setResult] = useState({ items: [], totalCount: 0 });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    searchProducts({ search: search || undefined, productType: productType || undefined, isActive: true })
      .then((data) => { if (!cancelled) setResult(data); })
      .catch((err) => { if (!cancelled) setError(err); })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, [search, productType]);

  return (
    <div className="page">
      <h1>Catalog</h1>
      <p className="page-subtitle">Licenses, API access, SaaS plans, and project bundles — instant delivery on purchase.</p>

      <div className="search-row">
        <input
          className="search-input"
          placeholder="Search the catalog..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      <div className="filter-tabs">
        <button className={`filter-tab ${!productType ? 'active' : ''}`} onClick={() => setProductType(null)}>
          All
        </button>
        {PRODUCT_TYPES.map((type) => (
          <button key={type} className={`filter-tab ${productType === type ? 'active' : ''}`} onClick={() => setProductType(type)}>
            {productTypeLabel(type)}
          </button>
        ))}
      </div>

      {loading && <p><Icon name="clock" size={14} style={{ verticalAlign: -2, marginRight: 6 }} />Loading...</p>}
      {error && <p className="error-text">Could not load the catalog.</p>}

      <div className="product-grid">
        {result.items.map((product) => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>

      {!loading && result.items.length === 0 && <p>No products found.</p>}
    </div>
  );
}
