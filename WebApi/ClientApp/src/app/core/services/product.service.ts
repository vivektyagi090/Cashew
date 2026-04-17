import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Product {
  productId: number; categoryId: number; categoryName: string;
  name: string; description: string; price: number; originalPrice?: number;
  stockQty: number; imageUrl?: string; rating: number; reviewCount: number;
  brand?: string; isActive: boolean; isFeatured: boolean; createdAt: string;
  discountPercent?: number;
}

export interface ProductListResponse {
  products: Product[]; totalCount: number; page: number; pageSize: number; totalPages: number;
}

export interface ProductListRequest {
  categoryId?: number; search?: string; minPrice?: number; maxPrice?: number;
  sortBy?: string; page?: number; pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class ProductService {
  private http = inject(HttpClient);

  getProducts(req: ProductListRequest = {}): Observable<ProductListResponse> {
    let params = new HttpParams();
    if (req.categoryId) params = params.set('categoryId', req.categoryId);
    if (req.search)     params = params.set('search', req.search);
    if (req.minPrice)   params = params.set('minPrice', req.minPrice);
    if (req.maxPrice)   params = params.set('maxPrice', req.maxPrice);
    if (req.sortBy)     params = params.set('sortBy', req.sortBy);
    if (req.page)       params = params.set('page', req.page);
    if (req.pageSize)   params = params.set('pageSize', req.pageSize ?? 12);
    return this.http.get<ProductListResponse>('/api/product', { params });
  }

  getFeatured(): Observable<Product[]> {
    return this.http.get<Product[]>('/api/product/featured');
  }

  getById(id: number): Observable<Product> {
    return this.http.get<Product>(`/api/product/${id}`);
  }
}
