import { Component, OnInit, inject, HostListener, signal, computed, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ScrollRevealDirective } from '../../core/directives/scroll-reveal.directive';
import { ProductService, Product } from '../../core/services/product.service';
import { CartService } from '../../core/services/cart.service';
import { CashewScrollyComponent } from '../../component/cashew-scrolly/cashew-scrolly.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, CashewScrollyComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css', './animations.css']
})
export class HomeComponent implements OnInit, AfterViewInit, OnDestroy {
  private productService = inject(ProductService);
  private cartService = inject(CartService);
  private router = inject(Router);

  featuredProducts: Product[] = [];
  activeProduct = signal<Product | null>(null);
  loading = true;
  isMobile = false;
  readonly Math = Math;

  categories = [
    { id: 1, name: 'Konkan Raw', icon: 'seed', image: 'https://images.unsplash.com/photo-1604084849174-7b97a71cbbf3?w=400', desc: 'Directly sourced from the laterite soil estates of Maharashtra' },
    { id: 2, name: 'Roasted Gold', icon: 'flame', image: 'https://images.unsplash.com/photo-1575218823251-f42f2e7a7f9b?w=400', desc: 'Craft-roasted in small batches to preserve Konkan aroma' },
    { id: 3, name: 'Spicy Masala', icon: 'sparkles', image: 'https://images.unsplash.com/photo-1600147131759-880e94a6185f?w=400', desc: 'Infused with authentic Maharashtra spice blends' },
    { id: 4, name: 'Gift Architect', icon: 'gift', image: 'https://images.unsplash.com/photo-1549465220-1a8b9238cd48?w=400', desc: 'Bespoke artisanal cashew arrangements for master moments' },
    { id: 5, name: 'Cashew Butter', icon: 'droplets', image: 'https://images.unsplash.com/photo-1589927986089-35812378533a?w=400', desc: 'Velvety, stone-ground Konkan cashews with a hint of honey' },
    { id: 6, name: 'Eco Harvest', icon: 'leaf', image: 'https://images.unsplash.com/photo-1608797178974-15b35a64ede9?w=400', desc: 'Sustainable, earth-conscious yields from Maharashtra' },
  ];

  harvestJourney = [
    { step: '01', title: 'Laterite Genesis', desc: 'Sourced from the mineral-rich laterite estates of coastal Maharashtra.' },
    { step: '02', title: 'Estate Scouting', desc: 'Only 3% of legacy Konkan harvests meet our artisanal VVKBMS grade.' },
    { step: '03', title: 'Master Sifting', desc: 'Every single nut is hand-inspected for weight, color, and PFA purity.' },
    { step: '04', title: 'Stone Roasting', desc: 'Traditional slow-roasting using laterite-clay methods for amber perfection.' },
    { step: '05', title: 'Golden Seasoning', desc: 'Infused with hand-harvested sea salt within the golden hour of roasting.' },
    { step: '06', title: 'The Final Seal', desc: 'Vacuum-sealed at the source in luxury matte-finish anaerobic packs.' },
  ];

  testimonials = [
    { name: 'Amit Kulkarni', loc: 'Pune', text: 'The W180 jumbo cashews are outstanding. Fresh from Konkan!', rating: 5 },
    { name: 'Sneha Patil', loc: 'Kolhapur', text: 'Authentic taste. Best Maharashtra cashews online.', rating: 5 },
    { name: 'Vijay Deshmukh', loc: 'Mumbai', text: 'The salt & pepper cashews are perfect for my evening snack.', rating: 5 },
  ];

  cashewBenefits = [
    { title: 'Cardio Vitality', text: 'Harnessing deep-estate oleic profiles to safeguard your heart\'s natural rhythm and vigor.' },
    { title: 'Skeletal Integrity', text: 'High magnesium and copper content help maintain bone density and skeletal integrity.' },
    { title: 'Brain Boost', text: 'Packed with healthy fats and minerals that support cognitive function and mental clarity.' },
    { title: 'Skin Glow', text: 'Copper and antioxidants promote collagen production for youthful, vibrant skin elasticity.' },
    { title: 'Weight Support', text: 'High protein and fiber content help increase satiety and support healthy metabolism.' },
    { title: 'Eye Protection', text: 'Contains Zeaxanthin, an antioxidant that protects the retina from UV damage.' },
    { title: 'Immune Power', text: 'Significant zinc levels help boost the immune system and support rapid healing.' },
    { title: 'Blood Health', text: 'Iron and copper work together to help the body form and use red blood cells.' },
    { title: 'Nerve Function', text: 'Magnesium helps regulate nerve impulses and maintains a balanced nervous system.' },
    { title: 'Muscle Tonic', text: 'Essential minerals support muscle contraction and post-workout recovery phases.' },
    { title: 'Energy Reserve', text: 'Healthy fats provide a sustained energy source throughout your active day.' },
    { title: 'Hair Vitality', text: 'Copper helps maintain hair pigment and adds a natural, healthy shine.' },
    { title: 'Oral Health', text: 'Phosphorus is essential for the healthy development of teeth and gums.' },
    { title: 'Stress Relief', text: 'Magnesium plays a role in managing cortisol and promoting overall relaxation.' },
    { title: 'Gut Wellness', text: 'Dietary fiber supports healthy digestion and promotes a balanced gut microbiome.' },
    { title: 'Gallstone Guard', text: 'Regular consumption is linked to a lower risk of developing painful gallstones.' },
    { title: 'Longevity Support', text: 'Rich in antioxidants that combat oxidative stress and cellular aging processes.' },
    { title: 'Pure Versatility', text: 'A nutrient-dense addition that enhances both savory dishes and sweet treats.' }
  ];

  renderedHeroProgress = signal(0);
  renderedSpecsProgress = signal(0);

  private spiralElement?: HTMLElement;
  private specsElement?: HTMLElement;
  private initialVh = 0;

  ngOnInit() {
    this.initialVh = window.innerHeight;
    this.loadProducts();
    this.checkIfMobile();
    this.triggerScrollAnimations();
  }

  private loadProducts() {
    this.productService.getFeatured().subscribe({
      next: (p) => {
        let filtered: Product[] = [];
        if (p && p.length > 0) {
          filtered = p.filter(item => item.name.toLowerCase().includes('cashew') || item.categoryId === 1);
        }

        if (filtered.length > 0) {
          this.featuredProducts = filtered;
        } else {
          this.setFallbackData();
        }
        this.loading = false;
        this.triggerScrollAnimations();
      },
      error: () => {
        this.setFallbackData();
        this.loading = false;
        this.triggerScrollAnimations();
      }
    });
  }

  private setFallbackData() {
    this.featuredProducts = [
      {
        productId: 101, categoryId: 1, categoryName: 'Premium Raw',
        name: 'Konkan King W180', description: 'The absolute pinnacle of cashew harvests. Jumbo, cream-white, and buttery. Traced to legacy Maharashtra estates.',
        price: 1299, originalPrice: 1800, discountPercent: 28,
        imageUrl: '/assets/images/cashew-packaged.png', isFeatured: true, stockQty: 5, rating: 5, reviewCount: 124, isActive: true, createdAt: ''
      },
      {
        productId: 102, categoryId: 2, categoryName: 'Roasted Gold',
        name: 'Stone-Roasted Salted', description: 'Expertly stone-roasted for 4 hours and infused with hand-harvested sea salt. The definitive crunchy snack.',
        price: 899, originalPrice: 1200, discountPercent: 25,
        imageUrl: '/assets/images/cashew-salted.png', isFeatured: true, stockQty: 15, rating: 5, reviewCount: 89, isActive: true, createdAt: ''
      },
      {
        productId: 103, categoryId: 3, categoryName: 'Spiced Fusion',
        name: 'Masala Fusion Fire', description: 'A bold blend of Konkan chilies and 12-spice masala. Not for the faint-hearted. Artisanal spice coating.',
        price: 949, originalPrice: 1100, discountPercent: 14,
        imageUrl: '/assets/images/cashew-02.png', isFeatured: true, stockQty: 8, rating: 4.8, reviewCount: 56, isActive: true, createdAt: ''
      },
      {
        productId: 104, categoryId: 4, categoryName: 'Artisanal Sweets',
        name: 'Honey Bliss Glaze', description: 'Slow-glazed in organic forest honey with a touch of vanilla. A sweet, nutrient-dense artisanal treat.',
        price: 1150, originalPrice: 1500, discountPercent: 23,
        imageUrl: '/assets/images/a3-Photoroom.png', isFeatured: true, stockQty: 12, rating: 5, reviewCount: 42, isActive: true, createdAt: ''
      },
      {
        productId: 105, categoryId: 3, categoryName: 'Spiced Fusion',
        name: 'Peri Peri Punch', description: 'A zesty and fiery peri-peri seasoning that brings a modern twist to the classic Konkan cashew.',
        price: 999, originalPrice: 1250, discountPercent: 20,
        imageUrl: '/assets/images/a4-Photoroom.png', isFeatured: true, stockQty: 20, rating: 4.9, reviewCount: 67, isActive: true, createdAt: ''
      },
      {
        productId: 106, categoryId: 2, categoryName: 'Roasted Gold',
        name: 'Black Pepper Premium', description: 'Freshly cracked Malabar black pepper paired with slow-roasted cashews for a sophisticated flavor profile.',
        price: 1050, originalPrice: 1300, discountPercent: 19,
        imageUrl: '/assets/images/cashew-roasted.png', isFeatured: true, stockQty: 10, rating: 5, reviewCount: 34, isActive: true, createdAt: ''
      },
      {
        productId: 107, categoryId: 5, categoryName: 'Luxury Blends',
        name: 'Saffron Infused Elite', description: 'Hand-picked saffron strands infused into premium jumbo cashews. A truly royal Maharashtra experience.',
        price: 2499, originalPrice: 3000, discountPercent: 16,
        imageUrl: '/assets/images/cashew-fruit.png', isFeatured: true, stockQty: 3, rating: 5, reviewCount: 12, isActive: true, createdAt: ''
      },
      {
        productId: 108, categoryId: 4, categoryName: 'Artisanal Sweets',
        name: 'Caramel Clusters', description: 'Crunchy cashew clusters bound by organic jaggery and a hint of Himalayan pink salt.',
        price: 1200, originalPrice: 1500, discountPercent: 20,
        imageUrl: '/assets/images/cashew-shell.png', isFeatured: true, stockQty: 25, rating: 4.7, reviewCount: 45, isActive: true, createdAt: ''
      }
    ];
  }

  ngOnDestroy() {
  }

  private checkIfMobile() {
    this.isMobile = window.innerWidth < 768;
  }

  ngAfterViewInit() {
  }

  @HostListener('window:scroll')
  onWindowScroll() {
    this.triggerScrollAnimations();
  }

  triggerScrollAnimations() {
    // Small delay to ensure DOM is ready
    const elements = document.querySelectorAll('.scroll-animate, .scroll-animate-left, .scroll-animate-right, .reveal-element');

    elements.forEach((el: Element) => {
      const rect = el.getBoundingClientRect();
      // Trigger when element is 10% into the viewport
      const isInView = rect.top < window.innerHeight * 0.9 && rect.bottom > 0;

      if (isInView) {
        (el as HTMLElement).classList.add('in-view');
      }
    });
  }

  scrollTo(sectionId: string) {
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  browseCategory(id: number) {
    this.router.navigate(['/products'], { queryParams: { category: id } });
  }

  addToCart(p: Product, e: Event) {
    e.stopPropagation();
    this.cartService.addToCart(p.productId).subscribe({ next: () => this.cartService.loadCart() });
  }

  buyNow(p: Product, e: Event) {
    e.stopPropagation();
    this.cartService.addToCart(p.productId).subscribe({
      next: () => {
        this.cartService.loadCart();
        this.router.navigate(['/cart']);
      }
    });
  }

  goToProduct(id: number) { this.router.navigate(['/products', id]); }

  toggleDetails(p: Product, e: Event) {
    e.stopPropagation();
    if (this.activeProduct()?.productId === p.productId) {
      this.activeProduct.set(null);
    } else {
      this.activeProduct.set(p);
    }
  }

  scrollCarousel(id: string, direction: number) {
    const el = document.getElementById(id);
    if (el) {
      const scrollAmount = direction * 400;
      el.scrollBy({ left: scrollAmount, behavior: 'smooth' });
    }
  }

  closeDetails() {
    this.activeProduct.set(null);
  }
}
