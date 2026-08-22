import axios from 'axios';
import { authFromResult, clearAuth, getAuth, setAuth } from './authStorage';

// The frontend only ever talks to the Gateway (§13/§15) — never to individual services directly.
const baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

export const apiClient = axios.create({ baseURL });

apiClient.interceptors.request.use((config) => {
  const auth = getAuth();
  if (auth?.accessToken) {
    config.headers.Authorization = `Bearer ${auth.accessToken}`;
  }
  return config;
});

// A 401 means the access token expired (or was never valid) — try exactly one silent refresh
// using the stored refresh token, then replay the original request. A second 401 (or no refresh
// token at all) means the session is really over: clear it and bounce to /login.
let refreshInFlight = null;

async function refreshSession() {
  const auth = getAuth();
  if (!auth?.refreshToken) throw new Error('No refresh token');

  // axios.create's interceptor above doesn't apply to a bare axios.post, so a mid-refresh 401
  // from an unrelated request never recurses into this function.
  const { data } = await axios.post(`${baseURL}/api/identity/refresh`, { refreshToken: auth.refreshToken });
  const next = authFromResult(data);
  setAuth(next);
  return next;
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const { config, response } = error;
    if (response?.status !== 401 || config._retried || config.url?.includes('/api/identity/')) {
      return Promise.reject(error);
    }

    config._retried = true;
    try {
      refreshInFlight ??= refreshSession().finally(() => { refreshInFlight = null; });
      const next = await refreshInFlight;
      config.headers.Authorization = `Bearer ${next.accessToken}`;
      return apiClient(config);
    } catch {
      clearAuth();
      if (typeof window !== 'undefined' && window.location.pathname !== '/login') {
        window.location.assign('/login');
      }
      return Promise.reject(error);
    }
  },
);

export function getApiBaseUrl() {
  return baseURL;
}
