import { apiClient } from './apiClient';

export async function login(email, password) {
  const { data } = await apiClient.post('/api/identity/login', { email, password });
  return data;
}

export async function register(email, fullName, password) {
  const { data } = await apiClient.post('/api/identity/register', { email, fullName, password });
  return data;
}

export async function refresh(refreshToken) {
  const { data } = await apiClient.post('/api/identity/refresh', { refreshToken });
  return data;
}

export async function logout(refreshToken) {
  await apiClient.post('/api/identity/logout', { refreshToken });
}

export async function getMe() {
  const { data } = await apiClient.get('/api/identity/me');
  return data;
}

// Admin-only (Permissions.UsersManage)
export async function listUsers({ page = 1, pageSize = 20 } = {}) {
  const { data } = await apiClient.get('/api/identity/users', { params: { page, pageSize } });
  return data;
}

export async function assignRole(userId, roleName) {
  const { data } = await apiClient.post(`/api/identity/users/${userId}/roles`, { roleName });
  return data;
}
