import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { useToast } from '../hooks/useToast';
import { Icon } from '../components/Icon';

export function RegisterPage() {
  const { register } = useAuth();
  const { addToast } = useToast();
  const navigate = useNavigate();
  const [form, setForm] = useState({ fullName: '', email: '', password: '' });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  async function handleSubmit(e) {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      const result = await register(form.email, form.fullName, form.password);
      addToast(`Welcome to DevMarket, ${result.user.fullName.split(' ')[0]}`, 'success');
      navigate('/', { replace: true });
    } catch (err) {
      setError(err.response?.status === 409 ? 'An account with that email already exists.' : 'Could not create your account — please try again.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="page-narrow">
      <div className="auth-card">
        <div className="auth-mark"><Icon name="rocket" size={22} /></div>
        <h1>Create your account</h1>
        <p className="page-subtitle">Buy software licenses, API keys, and project bundles.</p>

        <form className="form" style={{ maxWidth: 'none' }} onSubmit={handleSubmit}>
          <label>
            Full name
            <input required value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} />
          </label>
          <label>
            Email
            <input required type="email" autoComplete="email" value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })} />
          </label>
          <label>
            Password
            <input required type="password" autoComplete="new-password" minLength={8} value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })} />
            <span className="form-hint">At least 8 characters.</span>
          </label>

          {error && <p className="error-text">{error}</p>}

          <button className="button button-block" type="submit" disabled={submitting}>
            {submitting ? 'Creating account...' : 'Create account'}
          </button>
        </form>

        <p className="auth-switch">
          Already have an account? <Link to="/login">Sign in</Link>
        </p>
      </div>
    </div>
  );
}
