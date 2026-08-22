import { apiClient } from './apiClient';

export async function getCart() {
  const { data } = await apiClient.get('/api/cart');
  return data;
}

export async function addCartItem(productId, quantity) {
  const { data } = await apiClient.post('/api/cart/items', { productId, quantity });
  return data;
}

export async function updateCartItem(productId, quantity) {
  const { data } = await apiClient.put(`/api/cart/items/${productId}`, { quantity });
  return data;
}

export async function removeCartItem(productId) {
  const { data } = await apiClient.delete(`/api/cart/items/${productId}`);
  return data;
}
