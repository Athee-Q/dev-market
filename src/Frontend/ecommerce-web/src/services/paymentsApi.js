import { apiClient } from './apiClient';

// Returns null (rather than throwing) on 404 — Payment Service creates the payment record
// asynchronously off OrderConfirmedEvent, so "not yet ready" is a normal, expected state right
// after an order is confirmed, not an error.
export async function getPaymentForOrder(orderId) {
  try {
    const { data } = await apiClient.get(`/api/payments/order/${orderId}`);
    return data;
  } catch (err) {
    if (err.response?.status === 404) return null;
    throw err;
  }
}

export async function verifyPayment({ orderId, razorpayOrderId, razorpayPaymentId, razorpaySignature }) {
  const { data } = await apiClient.post('/api/payments/verify', {
    orderId,
    razorpayOrderId,
    razorpayPaymentId,
    razorpaySignature,
  });
  return data;
}

// Transaction History page. Non-admins are always pinned server-side to their own payments
// regardless of what customerId is passed here.
export async function searchPayments({ customerId, status, page = 1, pageSize = 20 } = {}) {
  const { data } = await apiClient.get('/api/payments', { params: { customerId, status, page, pageSize } });
  return data;
}

// Admin only (Permissions.PaymentsManage) — Admin dashboard revenue tile.
export async function getRevenueSummary() {
  const { data } = await apiClient.get('/api/payments/summary');
  return data;
}

// Headless UPI QR flow — an alternative to opening Razorpay Checkout (see paymentsApi.getPaymentForOrder
// + OrderDetailsPage's "Pay Now"). createUpiQr mints/reuses a QR; getUpiQrStatus is polled while it's on screen.
export async function createUpiQr(orderId) {
  const { data } = await apiClient.post(`/api/payments/order/${orderId}/upi-qr`);
  return data;
}

export async function getUpiQrStatus(orderId) {
  const { data } = await apiClient.get(`/api/payments/order/${orderId}/upi-qr/status`);
  return data;
}
