import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface CartItem {
  cartItemId: number; cartId: number; productId: number;
  productName: string; productImageUrl?: string; quantity: number;
  price: number; totalPrice: number;
}

export interface Cart {
  cartId: number; userId: number; items: CartItem[];
  totalAmount: number; totalItems: number;
}

@Injectable({ providedIn: 'root' })
export class CartService {
  private http = inject(HttpClient);

  private _cart = signal<Cart | null>(null);
  cart = this._cart.asReadonly();
  itemCount = computed(() => this._cart()?.totalItems ?? 0);

  loadCart() {
    this.http.get<Cart>('/api/cart').subscribe(c => this._cart.set(c));
  }

  addToCart(productId: number, quantity = 1) {
    return this.http.post('/api/cart/add', { productId, quantity });
  }

  updateItem(cartItemId: number, quantity: number) {
    return this.http.put('/api/cart/update', { cartItemId, quantity });
  }

  removeItem(cartItemId: number) {
    return this.http.delete(`/api/cart/item/${cartItemId}`);
  }

  clearCart() {
    return this.http.delete('/api/cart/clear');
  }
}
