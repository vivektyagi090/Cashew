import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService, Product, ProductListResponse } from '../../core/services/product.service';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css'
})
export class ProductsComponent implements OnInit {
  private productService = inject(ProductService);
  private cartService    = inject(CartService);
  private route          = inject(ActivatedRoute);
  private router         = inject(Router);

  products: Product[] = [];
  totalCount = 0; totalPages = 0; currentPage = 1;
  loading = true;
  viewMode: 'grid' | 'list' = 'grid';

  filters = { categoryId: 0, search: '', minPrice: 0, maxPrice: 0, sortBy: '' };
  sortOptions = [
    { label: 'Newest First', val: '' },
    { label: 'Price: Low to High', val: 'price_asc' },
    { label: 'Price: High to Low', val: 'price_desc' },
    { label: 'Top Rated', val: 'rating' },
  ];

  categories = [
    { id: 0, name: 'All Collection', icon: '🥜' },
    { id: 1, name: 'Raw Cashews',    icon: '📦' },
    { id: 2, name: 'Roasted & Salted', icon: '🧂' },
    { id: 3, name: 'Flavoured',      icon: '🌶️' },
    { id: 4, name: 'Gift Packs',     icon: '🎁' },
    { id: 5, name: 'Cashew Butter',  icon: '🫙' },
    { id: 6, name: 'Organic',        icon: '🌿' },
  ];

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.filters.search           = params['search']   || '';
      this.filters.categoryId       = +params['category'] || 0;
      this.currentPage              = 1;
      this.load();
    });
  }

  load() {
    this.loading = true;
    const req: any = { page: this.currentPage, pageSize: 12, sortBy: this.filters.sortBy };
    if (this.filters.categoryId) req.categoryId = this.filters.categoryId;
    if (this.filters.search)     req.search = this.filters.search;
    if (this.filters.minPrice)   req.minPrice = this.filters.minPrice;
    if (this.filters.maxPrice)   req.maxPrice = this.filters.maxPrice;

    this.productService.getProducts(req).subscribe({
      next: (res) => {
        this.products   = res.products;
        this.totalCount = res.totalCount;
        this.totalPages = res.totalPages;
        this.loading    = false;
      },
      error: () => this.loading = false
    });
  }

  applyFilters() { this.currentPage = 1; this.load(); }
  resetFilters() { this.filters = { categoryId: 0, search: '', minPrice: 0, maxPrice: 0, sortBy: '' }; this.applyFilters(); }
  changePage(p: number) { this.currentPage = p; this.load(); window.scrollTo(0, 0); }
  pagesArray() { return Array.from({ length: this.totalPages }, (_, i) => i + 1); }

  addToCart(p: Product, e: Event) {
    e.stopPropagation();
    this.cartService.addToCart(p.productId).subscribe({ next: () => this.cartService.loadCart() });
  }
  goToProduct(id: number) { this.router.navigate(['/products', id]); }
  starsArray(r: number) { return Array.from({ length: 5 }, (_, i) => i < Math.round(r)); }
}
