import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface AdminStats {
  totalRevenue: number;
  totalProfit: number;
  totalOrders: number;
  pendingDeliveries: number;
  lowStockAlerts: number;
  recentOrders: RecentOrder[];
}

export interface RecentOrder {
  orderId: number;
  customerName: string;
  amount: number;
  status: string;
  date: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5203/api/AdminDashboard';

  getStats(): Observable<AdminStats> {
    return this.http.get<AdminStats>(`${this.apiUrl}/stats`);
  }

  getInventoryMaster(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/inventory-master`);
  }

  getMovements(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/movements`);
  }

  logMovement(movement: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/movement`, movement);
  }

  logDamage(damage: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/damage`, damage);
  }

  getReturns(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/returns`);
  }

  logReturn(returnItem: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/return`, returnItem);
  }

  processReturn(returnItem: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/returns/process`, returnItem);
  }

  getLogistics(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/logistics`);
  }

  updateLogistics(logistics: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/logistics/update`, logistics);
  }

  // --- Master Records CRUD ---

  getCategories(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/categories`);
  }

  createCategory(category: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/category`, category);
  }

  updateCategory(category: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/category`, category);
  }

  deleteCategory(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/category/${id}`);
  }

  createProduct(product: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/product`, product);
  }

  updateProduct(product: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/product`, product);
  }

  deleteProduct(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/product/${id}`);
  }
}
