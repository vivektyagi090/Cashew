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
  public authStore = inject(AuthStore) as InstanceType<typeof AuthStore>;
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
    { 
      id: 1, 
      name: 'Pure Raw', 
      icon: 'seed', 
      image: 'https://images.unsplash.com/photo-1604084849174-7b97a71cbbf3?w=400', 
      desc: 'Directly sourced from the laterite soil estates of Maharashtra' 
    },
    { 
      id: 2, 
      name: 'Roasted Elite', 
      icon: 'flame', 
      image: 'https://images.unsplash.com/photo-1575218823251-f42f2e7a7f9b?w=400', 
      desc: 'Craft-roasted in small batches to preserve Konkan aroma' 
    },
    { 
      id: 3, 
      name: 'Spicy Fusion', 
      icon: 'sparkles', 
      image: 'https://images.unsplash.com/photo-1600147131759-880e94a6185f?w=400', 
      desc: 'Infused with authentic Maharashtra spice blends' 
    },
    { 
      id: 4, 
      name: 'Luxury Gifts', 
      icon: 'gift', 
      image: 'https://images.unsplash.com/photo-1549465220-1a8b9238cd48?w=400', 
      desc: 'Bespoke artisanal cashew arrangements for master moments' 
    },
    { 
      id: 5, 
      name: 'Creamy Butter', 
      icon: 'droplets', 
      image: 'https://images.unsplash.com/photo-1589927986089-35812378533a?w=400', 
      desc: 'Velvety, stone-ground Konkan cashews with a hint of honey' 
    },
    { 
      id: 6, 
      name: 'Eco Organic', 
      icon: 'leaf', 
      image: 'https://images.unsplash.com/photo-1608797178974-15b35a64ede9?w=400', 
      desc: 'Sustainable, earth-conscious yields from Maharashtra' 
    },
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
