import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './AuthContext';
import { ToastProvider } from './ToastContext';
import { CartProvider } from './CartContext';
import { NotificationsProvider } from './NotificationsContext';
import { Layout } from '../components/Layout';
import { AdminLayout } from '../components/AdminLayout';
import { RequireAuth } from '../components/RequireAuth';
import { RequireRole } from '../components/RequireRole';
import { DashboardPage } from '../pages/DashboardPage';
import { ProductsPage } from '../pages/ProductsPage';
import { ProductDetailsPage } from '../pages/ProductDetailsPage';
import { CartPage } from '../pages/CartPage';
import { CheckoutPage } from '../pages/CheckoutPage';
import { OrdersPage } from '../pages/OrdersPage';
import { OrderDetailsPage } from '../pages/OrderDetailsPage';
import { ProfilePage } from '../pages/ProfilePage';
import { NotificationsPage } from '../pages/NotificationsPage';
import { LoginPage } from '../pages/LoginPage';
import { RegisterPage } from '../pages/RegisterPage';
import { TransactionHistoryPage } from '../pages/TransactionHistoryPage';
import { ProductHistoryPage } from '../pages/ProductHistoryPage';
import { AdminDashboardPage } from '../pages/admin/AdminDashboardPage';
import { AdminProductsPage } from '../pages/admin/AdminProductsPage';
import { AdminOrdersPage } from '../pages/admin/AdminOrdersPage';
import { AdminUsersPage } from '../pages/admin/AdminUsersPage';

export default function App() {
  return (
    <AuthProvider>
      <ToastProvider>
        <CartProvider>
          <NotificationsProvider>
            <BrowserRouter>
              <Routes>
                <Route element={<Layout />}>
                  <Route path="/" element={<DashboardPage />} />
                  <Route path="/products" element={<ProductsPage />} />
                  <Route path="/products/:id" element={<ProductDetailsPage />} />
                  <Route path="/login" element={<LoginPage />} />
                  <Route path="/register" element={<RegisterPage />} />

                  <Route element={<RequireAuth />}>
                    <Route path="/cart" element={<CartPage />} />
                    <Route path="/checkout" element={<CheckoutPage />} />
                    <Route path="/orders" element={<OrdersPage />} />
                    <Route path="/orders/:id" element={<OrderDetailsPage />} />
                    <Route path="/my-products" element={<ProductHistoryPage />} />
                    <Route path="/transactions" element={<TransactionHistoryPage />} />
                    <Route path="/notifications" element={<NotificationsPage />} />
                    <Route path="/profile" element={<ProfilePage />} />

                    <Route element={<RequireRole role="Admin" />}>
                      <Route path="/admin" element={<AdminLayout />}>
                        <Route index element={<AdminDashboardPage />} />
                        <Route path="products" element={<AdminProductsPage />} />
                        <Route path="orders" element={<AdminOrdersPage />} />
                        <Route path="users" element={<AdminUsersPage />} />
                      </Route>
                    </Route>
                  </Route>
                </Route>
              </Routes>
            </BrowserRouter>
          </NotificationsProvider>
        </CartProvider>
      </ToastProvider>
    </AuthProvider>
  );
}
