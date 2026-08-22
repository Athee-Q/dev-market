import { apiClient } from './apiClient';

export async function getCustomer(id) {
  const { data } = await apiClient.get(`/api/customers/${id}`);
  return data;
}

export async function createCustomer(payload) {
  const { data } = await apiClient.post('/api/customers', payload);
  return data;
}

export async function updateCustomer(id, payload) {
  const { data } = await apiClient.put(`/api/customers/${id}`, payload);
  return data;
}
