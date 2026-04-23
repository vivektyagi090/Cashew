import {
  Component,
  ElementRef,
  OnInit,
  OnDestroy,
  ViewChild,
  signal,
  computed,
  effect,
  HostListener,
  inject,
  PLATFORM_ID,
} from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import {
  fromEvent,
  Subject,
  takeUntil,
  throttleTime,
  animationFrameScheduler,
} from 'rxjs';
import { trigger, transition, style, animate, state } from '@angular/animations';

@Component({
  selector: 'app-cashew-scrolly',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cashew-scrolly.component.html',
  styleUrls: ['./cashew-scrolly.component.css'],
  animations: [
    trigger('fadeInOut', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(30px)' }),
        animate('800ms cubic-bezier(0.23, 1, 0.32, 1)', style({ opacity: 1, transform: 'translateY(0)' })),
      ]),
      transition(':leave', [
        animate('600ms ease-in', style({ opacity: 0, transform: 'translateY(-30px)' })),
      ]),
    ]),
  ],
})
export class CashewScrollyComponent implements OnInit, OnDestroy {
  @ViewChild('canvas', { static: true }) canvasRef!: ElementRef<HTMLCanvasElement>;

  private destroy$ = new Subject<void>();
  private images: HTMLImageElement[] = [];
  private platformId = inject(PLATFORM_ID);
  
  // Constants
  private readonly TOTAL_FRAMES = 232; // Using all 232 frames for ultra-smoothness
  private readonly LERP_STIFFNESS = 0.1;
  private readonly LERP_DAMPING = 0.05;

  // Signals
  scrollProgress = signal(0);
  smoothedProgress = signal(0);
  loadedCount = signal(0);
  isLoaded = signal(false);
  
  currentFrame = computed(() => {
    return Math.floor(this.smoothedProgress() * (this.TOTAL_FRAMES - 1));
  });

  loadingPercentage = computed(() => {
    return Math.floor((this.loadedCount() / this.TOTAL_FRAMES) * 100);
  });

  // State for scrollytelling beats
  activeBeat = computed(() => {
    const p = this.scrollProgress();
    if (p >= 0 && p < 0.20) return 'A';
    if (p >= 0.25 && p < 0.45) return 'B';
    if (p >= 0.50 && p < 0.70) return 'C';
    if (p >= 0.75 && p <= 1.0) return 'D';
    return null;
  });

  constructor() {
    // Effect to redraw when frame changes
    effect(() => {
      const frame = this.currentFrame();
      if (this.isLoaded()) {
        this.drawFrame(frame);
      }
    });
  }

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.preloadImages();
      this.initScrollListener();
      this.startAnimationLoop();
    }
  }

  private preloadImages() {
    for (let i = 1; i <= this.TOTAL_FRAMES; i++) {
      const img = new Image();
      const frameIndex = i.toString().padStart(3, '0');
      img.src = `/animation_image/ezgif-frame-${frameIndex}.jpg`;
      img.onload = () => {
        this.loadedCount.update(c => c + 1);
        if (this.loadedCount() === this.TOTAL_FRAMES) {
          this.isLoaded.set(true);
          this.drawFrame(0);
        }
      };
      this.images.push(img);
    }
  }

  private initScrollListener() {
    fromEvent(window, 'scroll')
      .pipe(
        throttleTime(10, animationFrameScheduler),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.updateScrollProgress();
      });
    
    // Initial check
    this.updateScrollProgress();
  }

  private updateScrollProgress() {
    const container = document.querySelector('.cashew-scroll-zone');
    if (!container) return;

    const rect = container.getBoundingClientRect();
    const viewportHeight = window.innerHeight;
    const scrollHeight = rect.height - viewportHeight;
    const scrolled = -rect.top;
    
    const progress = Math.max(0, Math.min(1, scrolled / scrollHeight));
    this.scrollProgress.set(progress);
  }

  private rafId: number | null = null;
  private startAnimationLoop() {
    const loop = () => {
      const target = this.scrollProgress();
      const current = this.smoothedProgress();
      const diff = target - current;

      if (Math.abs(diff) > 0.0001) {
        // LERP calculation
        this.smoothedProgress.set(current + diff * this.LERP_STIFFNESS);
      } else {
        this.smoothedProgress.set(target);
      }

      this.rafId = requestAnimationFrame(loop);
    };
    this.rafId = requestAnimationFrame(loop);
  }

  private drawFrame(index: number) {
    const canvas = this.canvasRef.nativeElement;
    const ctx = canvas.getContext('2d');
    if (!ctx || !this.images[index]) return;

    const img = this.images[index];
    
    // High-Performance Responsive Scaling (Cover)
    const cw = window.innerWidth;
    const ch = window.innerHeight;
    const pixelRatio = window.devicePixelRatio || 1;
    
    canvas.width = cw * pixelRatio;
    canvas.height = ch * pixelRatio;
    ctx.scale(pixelRatio, pixelRatio);

    const imgRatio = img.width / img.height;
    const canvasRatio = cw / ch;

    let drawW, drawH, drawX, drawY;

    // Cover Algorithm: fill the entire screen while maintaining aspect ratio
    if (imgRatio < canvasRatio) {
      // Screen is wider than image relative to height (Desktop/Ultrawide)
      drawW = cw;
      drawH = cw / imgRatio;
    } else {
      // Screen is taller than image relative to width (Mobile/Vertical)
      drawH = ch;
      drawW = ch * imgRatio;
    }

    drawX = (cw - drawW) / 2;
    drawY = (ch - drawH) / 2;

    ctx.clearRect(0, 0, cw, ch);
    ctx.drawImage(img, drawX, drawY, drawW, drawH);
  }

  @HostListener('window:resize')
  onResize() {
    if (this.isLoaded()) {
      this.drawFrame(this.currentFrame());
    }
    this.updateScrollProgress();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.rafId) {
      cancelAnimationFrame(this.rafId);
    }
    this.images = [];
  }
}
