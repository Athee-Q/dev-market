import { useEffect, useState } from 'react';
import { getCustomer, updateCustomer } from '../services/customersApi';
import { useAuth } from '../hooks/useAuth';
import { useToast } from '../hooks/useToast';
import { Icon } from '../components/Icon';

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

export function ProfilePage() {
  const { user, logout } = useAuth();
  const { addToast } = useToast();
  const [form, setForm] = useState({ name: '', email: '', phone: '' });
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    let cancelled = false;

    // The Customer profile row is created asynchronously off UserRegisteredEvent (see
    // UserRegisteredConsumer) — right after registration it may not exist yet for a moment.
    async function load() {
      for (let attempt = 0; attempt < 6; attempt++) {
        try {
          const customer = await getCustomer(user.id);
          if (!cancelled) setForm({ name: customer.name, email: customer.email, phone: customer.phone });
          return;
        } catch (err) {
          if (err.response?.status !== 404 || attempt === 5) throw err;
          await sleep(1000);
        }
      }
    }

    load().catch(() => { if (!cancelled) setError('Could not load your profile yet — try refreshing in a moment.'); })
      .finally(() => { if (!cancelled) setLoading(false); });

    return () => { cancelled = true; };
  }, [user.id]);

  async function handleSubmit(e) {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      await updateCustomer(user.id, form);
      addToast('Profile updated', 'success');
    } catch {
      setError('Could not save your profile — check the details and try again.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="page">
      <h1>Profile</h1>

      <div className="callout">
        <p>
          Signed in as <strong>{user.email}</strong>
          {user.roles?.includes('Admin') && <span className="role-pill">Admin</span>}
        </p>
        <button className="link-button" onClick={logout}>
          <Icon name="logout" size={15} /> Sign out
        </button>
      </div>

      {loading && <p>Loading...</p>}

      {!loading && (
        <form className="form" onSubmit={handleSubmit}>
          <label>
            Name
            <input required value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
          </label>
          <label>
            Email
            <input required type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
          </label>
          <label>
            Phone
            <input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
          </label>

          {error && <p className="error-text">{error}</p>}

          <button className="button" type="submit" disabled={submitting}>
            {submitting ? 'Saving...' : 'Save changes'}
          </button>
        </form>
      )}
    </div>
  );
}
