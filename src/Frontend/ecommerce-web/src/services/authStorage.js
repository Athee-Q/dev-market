// Plain (non-React) storage for the current session, shared by AuthContext (which wraps this in
// state for components) and apiClient's interceptors (which run outside the React tree and can't
// use context/hooks). Single source of truth so the two never drift.
const STORAGE_KEY = 'ecommerce.auth';

export function getAuth() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

export function setAuth(auth) {
  if (auth) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(auth));
  } else {
    localStorage.removeItem(STORAGE_KEY);
  }
}

export function clearAuth() {
  localStorage.removeItem(STORAGE_KEY);
}

/** Builds the stored shape from a Register/Login/RefreshToken response. */
export function authFromResult(result) {
  return {
    accessToken: result.accessToken,
    refreshToken: result.refreshToken,
    accessTokenExpiresAt: result.accessTokenExpiresAt,
    user: {
      id: result.userId,
      email: result.email,
      fullName: result.fullName,
      roles: result.roles,
      permissions: result.permissions,
    },
  };
}
