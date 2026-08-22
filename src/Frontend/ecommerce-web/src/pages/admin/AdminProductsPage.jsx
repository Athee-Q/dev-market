import { useEffect, useState } from 'react';
import { createProduct, searchProducts, updateProduct } from '../../services/productsApi';
import { PRODUCT_TYPES, PRICING_MODELS, productTypeLabel, pricingModelLabel, categoryIdForType } from '../../utils/productType';
import { formatCurrency } from '../../utils/formatCurrency';
import { Icon } from '../../components/Icon';

const EMPTY_FORM = {
  name: '', description: '', price: '', sku: '', assetUrl: '',
  productType: 'License', pricingModel: 'OneTime', isActive: true,
};

export function AdminProductsPage() {
  const [result, setResult] = useState({ items: [], totalCount: 0 });
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(null); // null = closed, {} = new, {...product} = editing
  const [form, setForm] = useState(EMPTY_FORM);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  function load() {
    setLoading(true);
    searchProducts({ pageSize: 100 }).then(setResult).finally(() => setLoading(false));
  }

  useEffect(load, []);

  function openCreate() {
    setForm(EMPTY_FORM);
    setError(null);
    setEditing({});
  }

  function openEdit(product) {
    setForm({
      name: product.name, description: product.description, price: product.price, sku: product.sku,
      assetUrl: product.assetUrl ?? '', productType: product.productType, pricingModel: product.pricingModel,
      isActive: product.isActive,
    });
    setError(null);
    setEditing(product);
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      const payload = {
        categoryId: categoryIdForType(form.productType),
        name: form.name,
        description: form.description,
        price: Number(form.price),
        sku: form.sku,
        productType: form.productType,
        pricingModel: form.pricingModel,
        assetUrl: form.assetUrl || null,
        isActive: form.isActive,
      };
      if (editing.id) {
        await updateProduct(editing.id, payload);
      } else {
        await createProduct(payload);
      }
      setEditing(null);
      load();
    } catch (err) {
      setError(err.response?.data?.detail ?? 'Could not save the product.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div>
      <div className="page-header-row">
        <div>
          <h1>Products</h1>
          <p className="page-subtitle">Licenses, API access, SaaS plans, and projects.</p>
        </div>
        <button className="button" onClick={openCreate}><Icon name="plus" size={15} />Add Product</button>
      </div>

      {loading && <p>Loading...</p>}

      {!loading && (
        <div className="data-table-wrap">
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Type</th>
                <th>Price</th>
                <th>SKU</th>
                <th>Status</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {result.items.map((product) => (
                <tr key={product.id}>
                  <td>{product.name}</td>
                  <td>{productTypeLabel(product.productType)}</td>
                  <td>{formatCurrency(product.price)}{product.pricingModel !== 'OneTime' ? ` ${pricingModelLabel(product.pricingModel)}` : ''}</td>
                  <td className="mono">{product.sku}</td>
                  <td><span className={`status-pill ${product.isActive ? 'status-confirmed' : ''}`}>{product.isActive ? 'Active' : 'Inactive'}</span></td>
                  <td className="table-actions">
                    <button className="link-button" onClick={() => openEdit(product)}>Edit</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {editing && (
        <div className="modal-backdrop" onClick={() => setEditing(null)}>
          <div className="modal-card" onClick={(e) => e.stopPropagation()}>
            <h2>{editing.id ? 'Edit product' : 'Add product'}</h2>
            <form className="form-grid" onSubmit={handleSubmit}>
              <label className="span-2">
                Name
                <input required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              </label>
              <label className="span-2">
                Description
                <textarea required rows={3} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
              </label>
              <label>
                Type
                <select value={form.productType} onChange={(e) => setForm({ ...form, productType: e.target.value })}>
                  {PRODUCT_TYPES.map((t) => <option key={t} value={t}>{productTypeLabel(t)}</option>)}
                </select>
              </label>
              <label>
                Billing
                <select value={form.pricingModel} onChange={(e) => setForm({ ...form, pricingModel: e.target.value })}>
                  {PRICING_MODELS.map((m) => <option key={m} value={m}>{pricingModelLabel(m)}</option>)}
                </select>
              </label>
              <label>
                Price (INR)
                <input required type="number" min="0" step="0.01" value={form.price} onChange={(e) => setForm({ ...form, price: e.target.value })} />
              </label>
              <label>
                SKU
                <input required value={form.sku} onChange={(e) => setForm({ ...form, sku: e.target.value })} />
              </label>
              <label className="span-2">
                Asset URL <span className="form-hint">(repo / download / docs link delivered on purchase)</span>
                <input type="url" placeholder="https://..." value={form.assetUrl} onChange={(e) => setForm({ ...form, assetUrl: e.target.value })} />
              </label>
              <label className="span-2 checkbox-row">
                <input type="checkbox" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />
                Active (visible in the catalog)
              </label>

              {error && <p className="error-text span-2">{error}</p>}

              <div className="modal-actions span-2">
                <button type="button" className="button button-ghost" onClick={() => setEditing(null)}>Cancel</button>
                <button type="submit" className="button" disabled={submitting}>
                  {submitting ? 'Saving...' : editing.id ? 'Save changes' : 'Create product'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
