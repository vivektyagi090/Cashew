import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../core/services/admin.service';

@Component({
  selector: 'app-master-records',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-8 animate-in fade-in slide-in-from-bottom-5 duration-700">
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
           <h1 class="text-3xl font-serif font-black text-royal-dark">Master Records</h1>
           <p class="text-sm text-royal-brown/60 mt-1 font-medium">Manage your products and categories catalog</p>
        </div>
        <button class="btn-premium btn-premium-primary" (click)="openAddModal()">
          <span>➕ Add New {{activeTab() === 'products' ? 'Product' : 'Category'}}</span>
        </button>
      </div>

      <div class="flex gap-2 border-b border-royal-border/50 pb-px">
        <button (click)="activeTab.set('products')" 
                class="px-6 py-3 font-bold text-sm tracking-wide transition-all relative"
                [class.text-royal-gold]="activeTab() === 'products'"
                [class.text-royal-brown/40]="activeTab() !== 'products'">
          Product Master
          <div *ngIf="activeTab() === 'products'" class="absolute bottom-0 left-0 right-0 h-1 bg-royal-gold rounded-t-full"></div>
        </button>
        <button (click)="activeTab.set('categories')" 
                class="px-6 py-3 font-bold text-sm tracking-wide transition-all relative"
                [class.text-royal-gold]="activeTab() === 'categories'"
                [class.text-royal-brown/40]="activeTab() !== 'categories'">
          Category Master
          <div *ngIf="activeTab() === 'categories'" class="absolute bottom-0 left-0 right-0 h-1 bg-royal-gold rounded-t-full"></div>
        </button>
      </div>

      <div class="rich-card !p-0 overflow-hidden border-royal-border/40">
        <!-- Products Master -->
        <div *ngIf="activeTab() === 'products'" class="divide-y divide-royal-border/30">
          <div class="p-6 bg-royal-cream/10">
            <div class="relative max-w-md">
                <span class="absolute left-4 top-1/2 -translate-y-1/2 text-royal-brown/30">🔍</span>
                <input type="text" [(ngModel)]="productSearch" placeholder="Search by name or SKU..." 
                       class="w-full pl-12 pr-5 py-3 rounded-xl border border-royal-border/60 bg-white shadow-sm focus:outline-none focus:border-royal-gold focus:ring-4 focus:ring-royal-gold/5 transition-all" />
            </div>
          </div>
          
          <div class="overflow-x-auto">
            <table class="w-full text-left border-collapse">
            <thead>
              <tr class="bg-royal-cream/30">
                <th class="premium-table-header">ID</th>
                <th class="premium-table-header">Name</th>
                <th class="premium-table-header">Category</th>
                <th class="premium-table-header">Price Details</th>
                <th class="premium-table-header">Stock</th>
                <th class="premium-table-header">Status</th>
                <th class="premium-table-header">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-royal-border/30">
              <tr *ngFor="let p of filteredProducts()" class="hover:bg-royal-cream/10 transition-colors group">
                <td class="premium-table-cell font-mono text-xs text-royal-brown/40">#{{p.productId}}</td>
                <td class="premium-table-cell">
                    <div class="flex items-center gap-3">
                        <div class="w-8 h-8 rounded-lg bg-royal-cream flex items-center justify-center text-xs">📦</div>
                        <div>
                            <div class="font-bold text-royal-dark">{{p.name}}</div>
                            <div class="text-[10px] text-royal-brown/40 font-mono">SKU-{{p.productId}}001</div>
                        </div>
                    </div>
                </td>
                <td class="premium-table-cell">
                    <span class="px-2.5 py-1 rounded-md bg-royal-cream text-[10px] font-bold text-royal-brown/60 uppercase">{{p.categoryName}}</span>
                </td>
                <td class="premium-table-cell">
                    <div class="text-xs font-semibold text-royal-brown/60 line-through">₹{{p.costPrice}}</div>
                    <div class="text-base font-black text-royal-dark">₹{{p.price}}</div>
                </td>
                <td class="premium-table-cell">
                    <div class="flex items-center gap-2">
                        <div class="h-1.5 w-12 bg-royal-border rounded-full overflow-hidden">
                            <div class="h-full bg-royal-gold" [style.width.%]="p.stockQty > 100 ? 100 : p.stockQty"></div>
                        </div>
                        <span class="font-bold text-sm">{{p.stockQty}}</span>
                    </div>
                </td>
                <td class="premium-table-cell">
                  <span class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-wider"
                        [class.bg-emerald-100]="p.isActive" [class.text-emerald-700]="p.isActive"
                        [class.bg-red-100]="!p.isActive" [class.text-red-700]="!p.isActive">
                    <span class="w-1.5 h-1.5 rounded-full" [class.bg-emerald-500]="p.isActive" [class.bg-red-500]="!p.isActive"></span>
                    {{p.isActive ? 'Active' : 'Hidden'}}
                  </span>
                </td>
                <td class="premium-table-cell">
                  <div class="flex items-center gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                    <button class="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-royal-gold hover:text-white transition-all bg-white shadow-sm border border-royal-border" (click)="editProduct(p)">✏️</button>
                    <button class="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-red-500 hover:text-white transition-all bg-white shadow-sm border border-royal-border" (click)="deleteProduct(p.productId)">🗑️</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
          </div>
        </div>

        <!-- Categories Master -->
        <div *ngIf="activeTab() === 'categories'" class="animate-in slide-in-from-right-5 duration-500">
          <div class="overflow-x-auto">
            <table class="w-full text-left border-collapse">
              <thead>
                <tr class="bg-royal-cream/30">
                  <th class="premium-table-header">ID</th>
                  <th class="premium-table-header">Category Name</th>
                  <th class="premium-table-header">Description</th>
                  <th class="premium-table-header">Status</th>
                  <th class="premium-table-header">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-royal-border/30">
                <tr *ngFor="let c of categories()" class="hover:bg-royal-cream/10 transition-colors group">
                  <td class="premium-table-cell font-mono text-xs text-royal-brown/40">#{{c.categoryId}}</td>
                  <td class="premium-table-cell">
                     <div class="font-bold text-royal-dark text-base">{{c.name}}</div>
                  </td>
                  <td class="premium-table-cell">
                      <div class="max-w-xs truncate text-royal-brown/60 text-xs">{{c.description}}</div>
                  </td>
                  <td class="premium-table-cell">
                    <span class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-wider"
                          [class.bg-emerald-100]="c.isActive" [class.text-emerald-700]="c.isActive"
                          [class.bg-red-100]="!c.isActive" [class.text-red-700]="!c.isActive">
                      <span class="w-1.5 h-1.5 rounded-full" [class.bg-emerald-500]="c.isActive" [class.bg-red-500]="!c.isActive"></span>
                      {{c.isActive ? 'Active' : 'Disabled'}}
                    </span>
                  </td>
                  <td class="premium-table-cell">
                    <div class="flex items-center gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                        <button class="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-royal-gold hover:text-white transition-all bg-white shadow-sm border border-royal-border" (click)="editCategory(c)">✏️</button>
                        <button class="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-red-500 hover:text-white transition-all bg-white shadow-sm border border-royal-border" (click)="deleteCategory(c.categoryId)">🗑️</button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Product Modal -->
      <div class="fixed inset-0 z-[100] flex items-center justify-center p-4 sm:p-6" *ngIf="showProductModal">
        <div class="absolute inset-0 bg-royal-dark/60 backdrop-blur-sm animate-in fade-in duration-300" (click)="showProductModal = false"></div>
        <div class="relative w-full max-w-2xl bg-white rounded-3xl shadow-2xl shadow-royal-dark/40 overflow-hidden animate-in zoom-in-95 duration-300">
          <div class="bg-royal-dark px-8 py-6 flex justify-between items-center">
            <h3 class="text-xl font-serif font-bold text-white">{{editMode ? 'Edit' : 'Add'}} Product Master</h3>
            <button class="text-white/60 hover:text-white text-2xl transition-colors" (click)="showProductModal = false">×</button>
          </div>
          <div class="p-8 space-y-6">
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
              <div class="space-y-1.5">
                <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Product Name</label>
                <input type="text" [(ngModel)]="currentProduct.name" class="premium-input-field" placeholder="e.g. Roasted Cashew 500g" />
              </div>
              <div class="space-y-1.5">
                <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Category</label>
                <select [(ngModel)]="currentProduct.categoryId" class="premium-input-field">
                  <option *ngFor="let c of categories()" [value]="c.categoryId">{{c.name}}</option>
                </select>
              </div>
            </div>
            <div class="grid grid-cols-1 sm:grid-cols-3 gap-6">
              <div class="space-y-1.5">
                <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Cost Price (₹)</label>
                <input type="number" [(ngModel)]="currentProduct.costPrice" class="premium-input-field" />
              </div>
              <div class="space-y-1.5">
                <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Sale Price (₹)</label>
                <input type="number" [(ngModel)]="currentProduct.price" class="premium-input-field" />
              </div>
              <div class="space-y-1.5">
                <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Initial Stock</label>
                <input type="number" [(ngModel)]="currentProduct.stockQty" class="premium-input-field" [disabled]="editMode" />
              </div>
            </div>
            <div class="space-y-1.5">
                <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Description</label>
                <textarea [(ngModel)]="currentProduct.description" class="premium-input-field" rows="3" placeholder="Describe the quality, batch, and packaging details..."></textarea>
            </div>
            <div class="flex flex-wrap gap-8 pt-2">
                <label class="flex items-center gap-3 cursor-pointer group">
                  <div class="relative">
                      <input type="checkbox" [(ngModel)]="currentProduct.isActive" class="peer sr-only" />
                      <div class="w-10 h-6 bg-royal-border rounded-full transition-colors peer-checked:bg-emerald-500"></div>
                      <div class="absolute left-1 top-1 w-4 h-4 bg-white rounded-full transition-transform peer-checked:translate-x-4"></div>
                  </div>
                  <span class="text-sm font-bold text-royal-dark group-hover:text-royal-gold transition-colors">Active on Store</span>
                </label>
                <label class="flex items-center gap-3 cursor-pointer group">
                    <div class="relative">
                        <input type="checkbox" [(ngModel)]="currentProduct.isFeatured" class="peer sr-only" />
                        <div class="w-10 h-6 bg-royal-border rounded-full transition-colors peer-checked:bg-royal-gold"></div>
                        <div class="absolute left-1 top-1 w-4 h-4 bg-white rounded-full transition-transform peer-checked:translate-x-4"></div>
                    </div>
                    <span class="text-sm font-bold text-royal-dark group-hover:text-royal-gold transition-colors">Featured Product</span>
                </label>
            </div>
          </div>
          <div class="p-8 bg-royal-cream/30 border-t border-royal-border/40 flex justify-end gap-4">
            <button class="btn-premium py-2 px-6 text-royal-brown/60 font-black hover:text-royal-dark" (click)="showProductModal = false">Cancel</button>
            <button class="btn-premium btn-premium-primary !rounded-2xl" (click)="saveProduct()">Save Changes</button>
          </div>
        </div>
      </div>

      <!-- Category Modal -->
      <div class="fixed inset-0 z-[100] flex items-center justify-center p-4 sm:p-6" *ngIf="showCategoryModal">
        <div class="absolute inset-0 bg-royal-dark/60 backdrop-blur-sm animate-in fade-in duration-300" (click)="showCategoryModal = false"></div>
        <div class="relative w-full max-w-lg bg-white rounded-3xl shadow-2xl shadow-royal-dark/40 overflow-hidden animate-in zoom-in-95 duration-300">
           <div class="bg-royal-gold px-8 py-6 flex justify-between items-center">
            <h3 class="text-xl font-serif font-bold text-white">{{editMode ? 'Edit' : 'Add'}} Category</h3>
            <button class="text-white/60 hover:text-white text-2xl transition-colors" (click)="showCategoryModal = false">×</button>
          </div>
          <div class="p-8 space-y-6">
            <div class="space-y-1.5">
              <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Category Name</label>
              <input type="text" [(ngModel)]="currentCategory.name" class="premium-input-field" placeholder="e.g. Export Quality Cashews" />
            </div>
            <div class="space-y-1.5">
              <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Description</label>
              <textarea [(ngModel)]="currentCategory.description" class="premium-input-field" rows="4"></textarea>
            </div>
          </div>
          <div class="p-8 bg-royal-cream/30 border-t border-royal-border/40 flex justify-end gap-4">
            <button class="btn-premium py-2 px-6 text-royal-brown/60 font-black hover:text-royal-dark" (click)="showCategoryModal = false">Cancel</button>
            <button class="btn-premium btn-premium-primary !rounded-2xl" (click)="saveCategory()">Save Category</button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class MasterRecordsComponent implements OnInit {
  private adminService = inject(AdminService);
  
  activeTab = signal<'products' | 'categories'>('products');
  products = signal<any[]>([]);
  categories = signal<any[]>([]);
  productSearch = '';

  showProductModal = false;
  showCategoryModal = false;
  editMode = false;
  
  currentProduct: any = this.resetProduct();
  currentCategory: any = this.resetCategory();

  ngOnInit() {
    this.loadAllData();
  }

  loadAllData() {
    this.adminService.getInventoryMaster().subscribe(data => this.products.set(data));
    this.adminService.getCategories().subscribe(data => this.categories.set(data));
  }

  filteredProducts() {
    const term = this.productSearch.toLowerCase();
    return this.products().filter(p => p.name.toLowerCase().includes(term) || p.categoryName.toLowerCase().includes(term));
  }

  resetProduct() {
    return { name: '', categoryId: 1, costPrice: 0, price: 0, stockQty: 0, description: '', isActive: true, isFeatured: false, imageUrl: '' };
  }

  resetCategory() {
    return { name: '', description: '', isActive: true };
  }

  openAddModal() {
    this.editMode = false;
    if (this.activeTab() === 'products') {
      this.currentProduct = this.resetProduct();
      this.showProductModal = true;
    } else {
      this.currentCategory = this.resetCategory();
      this.showCategoryModal = true;
    }
  }

  editProduct(p: any) {
    this.editMode = true;
    this.currentProduct = { ...p };
    this.showProductModal = true;
  }

  editCategory(c: any) {
    this.editMode = true;
    this.currentCategory = { ...c };
    this.showCategoryModal = true;
  }

  saveProduct() {
    const obs = this.editMode 
      ? this.adminService.updateProduct(this.currentProduct)
      : this.adminService.createProduct(this.currentProduct);
    
    obs.subscribe(() => {
      alert(`Product ${this.editMode ? 'updated' : 'created'} successfully.`);
      this.showProductModal = false;
      this.loadAllData();
    });
  }

  saveCategory() {
    const obs = this.editMode 
      ? this.adminService.updateCategory(this.currentCategory)
      : this.adminService.createCategory(this.currentCategory);
    
    obs.subscribe(() => {
      alert(`Category ${this.editMode ? 'updated' : 'created'} successfully.`);
      this.showCategoryModal = false;
      this.loadAllData();
    });
  }

  deleteProduct(id: number) {
    if (confirm('Are you sure you want to delete this product?')) {
      this.adminService.deleteProduct(id).subscribe(() => {
        alert('Product deleted.');
        this.loadAllData();
      });
    }
  }

  deleteCategory(id: number) {
    if (confirm('Are you sure you want to delete this category? (Note: Products in this category may be affected)')) {
      this.adminService.deleteCategory(id).subscribe(() => {
        alert('Category deleted.');
        this.loadAllData();
      });
    }
  }
}
