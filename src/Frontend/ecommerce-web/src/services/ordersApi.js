import { apiClient } from './apiClient';

export async function searchOrders({ customerId, status, page = 1, pageSize = 20 } = {}) {
  const { data } = await apiClient.get('/api/orders', { params: { customerId, status, page, pageSize } });
  return data;
}

export async function getOrder(id) {
  const { data } = await apiClient.get(`/api/orders/${id}`);
  return data;
}

// CustomerId is no longer passed by the client — Order Service derives it from the caller's JWT.
export async function createOrder(items) {
  const { data } = await apiClient.post('/api/orders', { items });
  return data;
}

export async function cancelOrder(id) {
  const { data } = await apiClient.post(`/api/orders/${id}/cancel`);
  return data;
}
