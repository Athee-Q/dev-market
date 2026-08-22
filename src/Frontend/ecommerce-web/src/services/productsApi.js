import { apiClient } from './apiClient';

export async function searchProducts({ search, categoryId, isActive, productType, page = 1, pageSize = 20 } = {}) {
  const { data } = await apiClient.get('/api/products', {
    params: { search, categoryId, isActive, productType, page, pageSize },
  });
  return data;
}

export async function getProduct(id) {
  const { data } = await apiClient.get(`/api/products/${id}`);
  return data;
}

export async function createProduct(payload) {
  const { data } = await apiClient.post('/api/products', payload);
  return data;
}

export async function updateProduct(id, payload) {
  const { data } = await apiClient.put(`/api/products/${id}`, payload);
  return data;
}
