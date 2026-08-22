import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { useToast } from '../hooks/useToast';
import { Icon } from '../components/Icon';

export function LoginPage() {
  const { login } = useAuth();
  const { addToast } = useToast();
  const navigate = useNavigate();
  const location = useLocation();
  const [form, setForm] = useState({ email: '', password: '' });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  async function handleSubmit(e) {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      const result = await login(form.email, form.password);
      addToast(`Welcome back, ${result.user.fullName.split(' ')[0]}`, 'success');
      navigate(location.state?.from?.pathname ?? '/', { replace: true });
    } catch (err) {
      setError(err.response?.status === 401 ? 'Incorrect email or password.' : 'Could not sign in — please try again.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="page-narrow">
      <div className="auth-card">
        <div className="auth-mark"><Icon name="key" size={22} /></div>
        <h1>Sign in</h1>
        <p className="page-subtitle">Access your purchases, transactions, and issued keys.</p>

        <form className="form" style={{ maxWidth: 'none' }} onSubmit={handleSubmit}>
          <label>
            Email
            <input required type="email" autoComplete="email" value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })} />
          </label>
          <label>
            Password
            <input required type="password" autoComplete="current-password" value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })} />
          </label>

          {error && <p className="error-text">{error}</p>}

          <button className="button button-block" type="submit" disabled={submitting}>
            {submitting ? 'Signing in...' : 'Sign in'}
          </button>
        </form>

        <p className="auth-switch">
          New here? <Link to="/register">Create an account</Link>
        </p>
      </div>
    </div>
  );
}
