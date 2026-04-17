import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { OrderService, PlaceOrderRequest } from '../../core/services/order.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css'
})
export class CheckoutComponent {
  private orderService = inject(OrderService);
  private router       = inject(Router);

  step = 1;
  loading = false;
  success = false;
  orderId: number | null = null;

  form: PlaceOrderRequest = {
    shippingAddress: '', city: '', state: '', zipCode: '',
    paymentMethod: 'COD', notes: ''
  };

  paymentMethods = [
    { val: 'COD',    label: '💵 Cash on Delivery' },
    { val: 'UPI',    label: '📱 UPI' },
    { val: 'CARD',   label: '💳 Credit / Debit Card' },
    { val: 'NETBANK',label: '🏦 Net Banking' },
  ];

  nextStep() { if (this.step < 3) this.step++; }
  prevStep() { if (this.step > 1) this.step--; }

  placeOrder() {
    this.loading = true;
    this.orderService.placeOrder(this.form).subscribe({
      next: (res) => {
        this.loading = false;
        this.success = true;
        this.orderId = res.orderId;
      },
      error: () => { this.loading = false; }
    });
  }

  goToOrders() { this.router.navigate(['/orders']); }
}
