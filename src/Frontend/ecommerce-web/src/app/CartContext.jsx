import { createContext, useCallback, useEffect, useMemo, useState } from 'react';
import * as cartApi from '../services/cartApi';
import { useAuth } from '../hooks/useAuth';

export const CartContext = createContext(null);

export function CartProvider({ children }) {
  const { user } = useAuth();
  const [cart, setCart] = useState({ customerId: null, items: [], totalAmount: 0 });
  const [loading, setLoading] = useState(false);

  const refresh = useCallback(async () => {
    if (!user) return;
    setLoading(true);
    try {
      setCart(await cartApi.getCart());
    } finally {
      setLoading(false);
    }
  }, [user]);

  useEffect(() => {
    if (user) refresh().catch(() => {});
  }, [user, refresh]);

  const addItem = useCallback(async (productId, quantity) => {
    setCart(await cartApi.addCartItem(productId, quantity));
  }, []);

  const updateItem = useCallback(async (productId, quantity) => {
    const updated = await cartApi.updateCartItem(productId, quantity);
    setCart(updated ?? { customerId: user?.id ?? null, items: [], totalAmount: 0 });
  }, [user]);

  const removeItem = useCallback(async (productId) => {
    setCart(await cartApi.removeCartItem(productId));
  }, []);

  const itemCount = cart.items?.reduce((sum, i) => sum + i.quantity, 0) ?? 0;

  const value = useMemo(
    () => ({ cart, itemCount, loading, refresh, addItem, updateItem, removeItem }),
    [cart, itemCount, loading, refresh, addItem, updateItem, removeItem],
  );

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}
