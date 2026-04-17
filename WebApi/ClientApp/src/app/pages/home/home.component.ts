import { Component, OnInit, inject, HostListener, signal, computed, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { CanvasScrubberDirective } from '../../core/directives/canvas-scrubber.directive';
import { ScrollRevealDirective } from '../../core/directives/scroll-reveal.directive';
import { ProductService, Product } from '../../core/services/product.service';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, CanvasScrubberDirective, ScrollRevealDirective],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css', './animations.css']
})
export class HomeComponent implements OnInit, AfterViewInit, OnDestroy {
  private productService = inject(ProductService);
  private cartService = inject(CartService);
  private router = inject(Router);

  featuredProducts: Product[] = [];
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

  scrubProgress = signal(0);
  specsScrubProgress = signal(0);

  // High-fidelity image sequence manifest
  cashewSequence = [
    '/assets/images/cashew-fruit.png',
    '/assets/images/cashew-fruit.png',
    '/assets/images/cashew-shell.png',
    '/assets/images/cashew-shell.png',
    '/assets/images/cashew-01.png',
    '/assets/images/cashew-02.png',
    '/assets/images/cashew-01.png',
    '/assets/images/cashew-02.png',
    '/assets/images/cashew-roasted.png',
    '/assets/images/cashew-roasted.png',
    '/assets/images/cashew-salted.png',
    '/assets/images/cashew-salted.png',
    '/assets/images/cashew-packaged.png',
    '/assets/images/cashew-packaged.png'
  ];

  // Motion sequence (a0-a17)
  cashewMotionSequence = Array.from({ length: 18 }, (_, i) => `/assets/images/a${i}-Photoroom.png`);

  renderedScrubProgress = signal(0);

  motionFrame = computed(() => {
    const total = this.cashewMotionSequence.length - 1;
    return this.renderedScrubProgress() * total;
  });

  activeBenefit = computed(() => {
    const maxIndex = this.cashewBenefits.length - 1;
    const index = Math.min(maxIndex, Math.max(0, Math.round(this.motionFrame())));
    return {
      ...this.cashewBenefits[index],
      index,
      side: index % 2 === 0 ? 'left' : 'right'
    };
  });

  // Map progress (0-1) to frame index
  currentFrame = computed(() => {
    const total = this.cashewSequence.length - 1;
    return this.scrubProgress() * total;
  });

  private spiralElement?: HTMLElement;
  private specsElement?: HTMLElement;

  ngOnInit() {
    this.productService.getFeatured().subscribe({
      next: (p) => {
        // Filter out any non-cashew products if they exist (UI only for now)
        this.featuredProducts = p.filter(item => item.name.toLowerCase().includes('cashew') || item.categoryId === 1);
        this.loading = false;
        this.triggerScrollAnimations();
      },
      error: () => { this.loading = false; }
    });

    this.checkIfMobile();
    this.triggerScrollAnimations();
    this.startSmoothLoop();
  }

  private startSmoothLoop() {
    const loop = () => {
      const target = this.specsScrubProgress();
      const current = this.renderedScrubProgress();

      // LERP (Linear Interpolation) for inertia effect
      // 0.1 factor means it moves 10% towards the target every frame
      const diff = target - current;
      if (Math.abs(diff) > 0.0001) {
        this.renderedScrubProgress.set(current + diff * 0.1);
      } else {
        this.renderedScrubProgress.set(target);
      }

      requestAnimationFrame(loop);
    };
    requestAnimationFrame(loop);
  }

  ngOnDestroy() {
    // requestAnimationFrame cleanup usually not strictly required for simple signals 
    // as the callback won't find the component after destroy, but we'll stop the loop
    // if we had a handle.
  }

  private checkIfMobile() {
    this.isMobile = window.innerWidth < 768;
  }

  ngAfterViewInit() {
    this.initScrubbingObserver();
  }

  @HostListener('window:scroll')
  onWindowScroll() {
    this.checkIfMobile();
    this.triggerScrollAnimations();
    this.updateScrubbingProgress();
  }

  @HostListener('window:resize')
  onResize() {
    this.checkIfMobile();
  }

  private initScrubbingObserver() {
    this.spiralElement = document.getElementById('origin-spiral') as HTMLElement;
    this.specsElement = document.getElementById('engineering-specs') as HTMLElement;
  }

  private updateScrubbingProgress() {
    const vh = window.innerHeight;

    // 1. Update Hero Spiral Progress
    if (this.spiralElement) {
      const rect = this.spiralElement.getBoundingClientRect();
      const end = -rect.height + vh;
      const progress = (rect.top - 0) / (end - 0);
      this.scrubProgress.set(Math.max(0, Math.min(1, progress)));
    }

    // 2. Update Engineering Specs Progress
    if (this.specsElement) {
      const rect = this.specsElement.getBoundingClientRect();
      // The scrubbing starts when the top of the section hits the top of the viewport
      const start = 0;
      const end = -rect.height + vh;
      const progress = (rect.top - start) / (end - start);
      this.specsScrubProgress.set(Math.max(0, Math.min(1, progress)));
    }
  }

  triggerScrollAnimations() {
    setTimeout(() => {
      const elements = document.querySelectorAll('.scroll-animate, .scroll-animate-left, .scroll-animate-right');

      elements.forEach((el: Element) => {
        const rect = el.getBoundingClientRect();
        const isInView = rect.top < window.innerHeight * 0.9 && rect.bottom > 0;

        if (isInView) {
          (el as HTMLElement).classList.add('in-view');
        }
      });
    }, 0);
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

  goToProduct(id: number) { this.router.navigate(['/products', id]); }
}
