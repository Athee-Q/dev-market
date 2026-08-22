import { useCallback, useEffect, useRef, useState } from 'react';
import { createUpiQr, getUpiQrStatus } from '../services/paymentsApi';
import { Icon } from './Icon';

const POLL_MS = 2500;

/**
 * Headless UPI payment: shows a scannable QR (minted by CreateUpiQr) and polls CheckUpiQrPayment
 * until Razorpay reports a captured payment against it — there's no client-side callback for a QR
 * scan the way Razorpay Checkout's `handler` provides one. Calls onPaid() once settled.
 */
export function UpiQrPanel({ orderId, onPaid, onClose }) {
  const [qr, setQr] = useState(null);
  const [error, setError] = useState(null);
  const [secondsLeft, setSecondsLeft] = useState(null);
  const pollRef = useRef(null);

  const requestQr = useCallback(async () => {
    setError(null);
    setQr(null);
    try {
      setQr(await createUpiQr(orderId));
    } catch {
      setError('Could not generate a UPI QR code. Please try again.');
    }
  }, [orderId]);

  useEffect(() => { requestQr(); }, [requestQr]);

  // Poll for payment while a live (non-expired) QR is on screen.
  useEffect(() => {
    if (!qr) return undefined;

    pollRef.current = setInterval(async () => {
      try {
        const payment = await getUpiQrStatus(orderId);
        if (payment.status === 'Succeeded') {
          clearInterval(pollRef.current);
          onPaid();
        }
      } catch {
        // transient — keep polling
      }
    }, POLL_MS);

    return () => clearInterval(pollRef.current);
  }, [qr, orderId, onPaid]);

  // Local countdown to expiry, independent of the poll — purely cosmetic urgency, the backend is
  // the source of truth for whether a QR is actually still usable.
  useEffect(() => {
    if (!qr) return undefined;
    const tick = () => setSecondsLeft(Math.max(0, Math.round((new Date(qr.expiresAt) - Date.now()) / 1000)));
    tick();
    const interval = setInterval(tick, 1000);
    return () => clearInterval(interval);
  }, [qr]);

  const expired = secondsLeft === 0;
  const minutes = secondsLeft !== null ? String(Math.floor(secondsLeft / 60)).padStart(2, '0') : '--';
  const seconds = secondsLeft !== null ? String(secondsLeft % 60).padStart(2, '0') : '--';

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-card qr-modal" onClick={(e) => e.stopPropagation()}>
        <h2><Icon name="key" size={18} style={{ marginRight: 8, verticalAlign: -3 }} />Pay with UPI</h2>

        {error && <p className="error-text">{error}</p>}

        {!error && !qr && <p>Generating your QR code...</p>}

        {qr && !expired && (
          <div className="qr-panel">
            <img src={qr.imageUrl} alt="Scan to pay with any UPI app" className="qr-image" />
            <p className="qr-hint">
              <Icon name="clock" size={13} style={{ marginRight: 5, verticalAlign: -2 }} />
              Scan with GPay, PhonePe, Paytm, or any UPI app · expires in {minutes}:{seconds}
            </p>
            <p className="qr-waiting"><span className="qr-dot" />Waiting for payment...</p>
          </div>
        )}

        {qr && expired && (
          <div className="qr-panel">
            <p className="error-text">This QR code has expired.</p>
            <button className="button" onClick={requestQr}>Generate a new QR</button>
          </div>
        )}

        <div className="modal-actions">
          <button type="button" className="button button-ghost" onClick={onClose}>Close</button>
        </div>
      </div>
    </div>
  );
}
