import { Directive, ElementRef, Input, OnChanges, SimpleChanges, inject, HostListener } from '@angular/core';

@Directive({
  selector: 'canvas[appCanvasScrubber]',
  standalone: true
})
export class CanvasScrubberDirective implements OnChanges {
  @Input({ required: true }) imageSequence: string[] = [];
  @Input({ required: true }) currentFrame: number = 0;

  private el = inject(ElementRef<HTMLCanvasElement>);
  private ctx: CanvasRenderingContext2D | null = null;
  private imageCache = new Map<number, HTMLImageElement>();
  private lastRenderedFrame = -1;

  async ngOnChanges(changes: SimpleChanges) {
    if (changes['imageSequence']) {
      this.preloadImages();
    }

    if (changes['currentFrame'] || changes['imageSequence']) {
      this.render();
    }
  }

  @HostListener('window:resize')
  onWindowResize() {
    this.lastRenderedFrame = -1; // force re-render on resize
    this.render();
  }

  private preloadImages() {
    this.imageCache.clear();
    this.imageSequence.forEach((url, index) => {
      const img = new Image();
      img.src = url;
      img.onload = () => {
        this.imageCache.set(index, img);
        if (index === Math.round(this.currentFrame)) {
          this.render();
        }
      };
    });
  }

  private render() {
    const canvas = this.el.nativeElement;
    if (!this.ctx) {
      this.ctx = canvas.getContext('2d');
    }

    const frameIndex = Math.min(
      this.imageSequence.length - 1,
      Math.max(0, Math.round(this.currentFrame))
    );

    if (frameIndex === this.lastRenderedFrame && this.ctx) return;

    const img = this.imageCache.get(frameIndex);
    if (!img || !this.ctx) return;

    // Handle canvas sizing
    this.syncCanvasSize(canvas);

    // Draw frame (center contain logic)
    const rect = canvas.getBoundingClientRect();
    const width = rect.width || 1;
    const height = rect.height || 1;

    const imgRatio = img.width / img.height;
    const canvasRatio = width / height;

    let drawWidth = width;
    let drawHeight = height;
    let offsetX = 0;
    let offsetY = 0;

    if (imgRatio > canvasRatio) {
      drawWidth = width;
      drawHeight = width / imgRatio;
      offsetY = (height - drawHeight) / 2;
    } else {
      drawHeight = height;
      drawWidth = height * imgRatio;
      offsetX = (width - drawWidth) / 2;
    }

    this.ctx.clearRect(0, 0, width, height);
    this.ctx.drawImage(img, offsetX, offsetY, drawWidth, drawHeight);

    this.lastRenderedFrame = frameIndex;
  }

  private syncCanvasSize(canvas: HTMLCanvasElement) {
    const dpr = window.devicePixelRatio || 1;
    const rect = canvas.getBoundingClientRect();

    if (canvas.width !== Math.floor(rect.width * dpr)) {
      canvas.width = rect.width * dpr;
      canvas.height = rect.height * dpr;
      this.ctx?.scale(dpr, dpr);
    }
  }
}
