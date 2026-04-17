import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderService, Order } from '../../core/services/order.service';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
<div class="orders-page fade-in">
  <div class="container">
    <h1 class="page-title">My Orders 📦</h1>

    <div class="loading-wrap" *ngIf="loading">
      <div class="order-card card skeleton-card" *ngFor="let i of [1,2,3]">
        <div class="skeleton" style="height:80px"></div>
      </div>
    </div>

    <div class="empty-orders" *ngIf="!loading && !orders.length">
      <div class="empty-icon">📦</div>
      <h2>No orders yet</h2>
      <p>You haven't placed any orders yet.</p>
      <a routerLink="/products" class="btn btn-primary btn-lg mt-2">Start Shopping →</a>
    </div>

    <div class="orders-list" *ngIf="!loading && orders.length">
      <div class="order-card card" *ngFor="let order of orders">
        <div class="order-header">
          <div>
            <div class="order-id">Order #{{ order.orderId }}</div>
            <div class="order-date">{{ order.orderDate | date:'mediumDate' }}</div>
          </div>
          <div class="header-right">
            <span class="badge" [ngClass]="statusClass(order.status)">{{ order.status }}</span>
            <div class="order-total">₹{{ order.totalAmount | number }}</div>
          </div>
        </div>
        <div class="order-items">
          <div class="order-item" *ngFor="let item of order.items">
            <span class="item-n">{{ item.productName }}</span>
            <span class="item-q">× {{ item.quantity }}</span>
            <span class="item-p">₹{{ item.totalPrice | number }}</span>
          </div>
        </div>
        <div class="order-footer">
          <span class="delivery-addr">📍 {{ order.shippingAddress }}, {{ order.city }}</span>
          <span class="pay-method">{{ order.paymentMethod }}</span>
        </div>
      </div>
    </div>
  </div>
</div>
`,
  styles: [`
.orders-page { padding: 32px 0 60px; }
.page-title  { margin-bottom: 28px; }
.empty-orders { text-align:center; padding:80px 20px; }
.empty-icon { font-size:3.5rem; margin-bottom:16px; }
.empty-orders h2 { margin-bottom:8px; }
.orders-list { display:flex; flex-direction:column; gap:20px; }
.order-card { padding:24px; }
.order-header { display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:16px; }
.order-id    { font-weight:700; font-size:1rem; }
.order-date  { font-size:0.82rem; color:var(--text-muted); margin-top:3px; }
.header-right { display:flex; flex-direction:column; align-items:flex-end; gap:6px; }
.order-total { font-size:1.1rem; font-weight:700; color:var(--brand-dark); }
.order-items { border-top:1px solid var(--border); padding-top:14px; display:flex; flex-direction:column; gap:8px; }
.order-item  { display:flex; gap:12px; align-items:center; font-size:0.9rem; }
.item-n      { flex:1; color:var(--text-primary); }
.item-q      { color:var(--text-muted); }
.item-p      { font-weight:600; min-width:80px; text-align:right; }
.order-footer{ display:flex; justify-content:space-between; font-size:0.82rem; color:var(--text-muted); margin-top:14px; padding-top:14px; border-top:1px solid var(--border); }
.pay-method  { font-weight:600; color:var(--text-secondary); }
  `]
})
export class OrdersComponent implements OnInit {
  private orderService = inject(OrderService);
  orders: Order[] = [];
  loading = true;

  ngOnInit() {
    this.orderService.getMyOrders().subscribe({
      next: (o) => { this.orders = o; this.loading = false; },
      error: ()  => { this.loading = false; }
    });
  }

  statusClass(status: string) {
    const map: Record<string, string> = {
      'Pending':    'badge-warning',
      'Processing': 'badge-info',
      'Shipped':    'badge-info',
      'Delivered':  'badge-success',
      'Cancelled':  'badge-danger',
    };
    return map[status] || 'badge-neutral';
  }
}
