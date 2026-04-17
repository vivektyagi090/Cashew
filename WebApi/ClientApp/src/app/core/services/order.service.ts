import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PlaceOrderRequest {
  shippingAddress: string; city: string; state: string;
  zipCode: string; paymentMethod: string; notes?: string;
}

export interface Order {
  orderId: number; userId: number; orderDate: string; status: string;
  totalAmount: number; shippingAddress: string; city: string;
  state: string; zipCode: string; paymentMethod: string;
  items: OrderItem[];
}

export interface OrderItem {
  orderItemId: number; productId: number; productName: string;
  quantity: number; unitPrice: number; totalPrice: number;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private http = inject(HttpClient);

  getMyOrders(): Observable<Order[]> {
    return this.http.get<Order[]>('/api/order/my');
  }

  getOrder(id: number): Observable<Order> {
    return this.http.get<Order>(`/api/order/${id}`);
  }

  placeOrder(request: PlaceOrderRequest): Observable<{ message: string; orderId: number }> {
    return this.http.post<{ message: string; orderId: number }>('/api/order/place', request);
  }
}
