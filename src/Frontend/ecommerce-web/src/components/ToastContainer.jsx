import { useToast } from '../hooks/useToast';

export function ToastContainer() {
  const { toasts, removeToast } = useToast();

  return (
    <div className="toast-container">
      {toasts.map((toast) => (
        <div key={toast.id} className={`toast toast-${toast.kind}`} onClick={() => removeToast(toast.id)}>
          {toast.message}
        </div>
      ))}
    </div>
  );
}
