import { createContext, useCallback, useMemo, useState } from 'react';
import * as authApi from '../services/authApi';
import { authFromResult, clearAuth, getAuth, setAuth } from '../services/authStorage';

export const AuthContext = createContext(null);

/** Real JWT auth (replaces the old CustomerContext placeholder) — see authStorage.js for the shared storage shape. */
export function AuthProvider({ children }) {
  const [auth, setAuthState] = useState(() => getAuth());

  const applyResult = useCallback((result) => {
    const next = authFromResult(result);
    setAuth(next);
    setAuthState(next);
    return next;
  }, []);

  const login = useCallback(async (email, password) => {
    const result = await authApi.login(email, password);
    return applyResult(result);
  }, [applyResult]);

  const register = useCallback(async (email, fullName, password) => {
    const result = await authApi.register(email, fullName, password);
    return applyResult(result);
  }, [applyResult]);

  const logout = useCallback(async () => {
    const current = getAuth();
    clearAuth();
    setAuthState(null);
    if (current?.refreshToken) {
      authApi.logout(current.refreshToken).catch(() => {});
    }
  }, []);

  const isAdmin = auth?.user?.roles?.includes('Admin') ?? false;

  const value = useMemo(
    () => ({
      user: auth?.user ?? null,
      accessToken: auth?.accessToken ?? null,
      isAuthenticated: Boolean(auth?.accessToken),
      isAdmin,
      login,
      register,
      logout,
    }),
    [auth, isAdmin, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
