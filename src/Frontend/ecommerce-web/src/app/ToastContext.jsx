import { createContext, useCallback, useMemo, useState } from 'react';

export const ToastContext = createContext(null);

let nextId = 1;

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);

  const removeToast = useCallback((id) => {
    setToasts((current) => current.filter((t) => t.id !== id));
  }, []);

  const addToast = useCallback((message, kind = 'info') => {
    const id = nextId++;
    setToasts((current) => [...current, { id, message, kind }]);
    setTimeout(() => removeToast(id), 5000);
  }, [removeToast]);

  const value = useMemo(() => ({ toasts, addToast, removeToast }), [toasts, addToast, removeToast]);

  return <ToastContext.Provider value={value}>{children}</ToastContext.Provider>;
}
