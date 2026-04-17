import { Component, inject, HostListener, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, NavigationEnd } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthStore } from '../../core/state/auth.store';
import { CartService } from '../../core/services/cart.service';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-navbar',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent {
  private authStore = inject(AuthStore) as InstanceType<typeof AuthStore>;
  private cartService = inject(CartService);
  private router = inject(Router);

  isLoggedIn = this.authStore.isAuthenticated;
  cartCount  = this.cartService.itemCount;
  searchQuery = '';
  mobileMenuOpen = false;
  accountMenuOpen = false;

  currentUrl = signal('');
  isLoginPage = computed(() => this.currentUrl().includes('/login'));

  megaMenuOpen = false;
  isMobile = window.innerWidth < 768;

  constructor() {
    this.currentUrl.set(window.location.pathname);
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.currentUrl.set(event.urlAfterRedirects);
    });
  }

  @HostListener('window:resize')
  onResize() {
    this.isMobile = window.innerWidth < 768;
  }

  categories = [
    { name: 'Pure Raw',        icon: 'seed',     id: 1 },
    { name: 'Roasted Elite',   icon: 'flame',    id: 2 },
    { name: 'Spicy Fusion',    icon: 'sparkles', id: 3 },
    { name: 'Luxury Gifts',    icon: 'gift',     id: 4 },
    { name: 'Creamy Butter',   icon: 'droplets', id: 5 },
    { name: 'Eco Organic',     icon: 'leaf',     id: 6 },
  ];

  search() {
    if (this.searchQuery.trim()) {
      this.router.navigate(['/products'], { queryParams: { search: this.searchQuery.trim() } });
      this.searchQuery = '';
    }
  }

  browseCategory(id: number) {
    this.router.navigate(['/products'], { queryParams: { category: id } });
  }

  logout() {
    this.authStore.logout();
    this.router.navigate(['/login']);
    this.accountMenuOpen = false;
  }

  toggleMobileMenu() { this.mobileMenuOpen = !this.mobileMenuOpen; }
  toggleAccountMenu() { this.accountMenuOpen = !this.accountMenuOpen; }
  toggleMegaMenu() { this.megaMenuOpen = !this.megaMenuOpen; }
}
