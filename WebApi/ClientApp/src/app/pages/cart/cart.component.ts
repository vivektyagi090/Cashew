import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { CartService, Cart } from '../../core/services/cart.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css'
})
export class CartComponent implements OnInit {
  private cartService = inject(CartService);
  private router = inject(Router);

  cart: Cart | null = null;
  loading = true;

  ngOnInit() {
    this.cartService.loadCart();
    // Subscribe via a local observable
    this.loading = false;
  }

  get cartData() { return this.cartService.cart(); }

  increase(itemId: number, qty: number) {
    this.cartService.updateItem(itemId, qty + 1).subscribe({ next: () => this.cartService.loadCart() });
  }

  decrease(itemId: number, qty: number) {
    if (qty <= 1) { this.remove(itemId); return; }
    this.cartService.updateItem(itemId, qty - 1).subscribe({ next: () => this.cartService.loadCart() });
  }

  remove(itemId: number) {
    this.cartService.removeItem(itemId).subscribe({ next: () => this.cartService.loadCart() });
  }

  checkout() { this.router.navigate(['/checkout']); }
}
