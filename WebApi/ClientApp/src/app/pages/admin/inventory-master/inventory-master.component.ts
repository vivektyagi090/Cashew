import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../core/services/admin.service';

@Component({
  selector: 'app-inventory-master',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-8 animate-in fade-in slide-in-from-bottom-5 duration-700">
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
           <h1 class="text-3xl font-sans font-black text-white tracking-tighter uppercase italic">Inventory <span class="text-solar-gold">Master</span></h1>
           <p class="text-sm text-slate-500 mt-1 font-medium">Real-time stock audit and warehouse management</p>
        </div>
        <button class="btn-premium-primary !rounded-2xl flex items-center gap-2" (click)="showMovementModal = true">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          <span class="text-xs font-black uppercase tracking-widest">Stock Movement</span>
        </button>
      </div>

      <div class="flex flex-col md:flex-row gap-6 items-center justify-between">
        <div class="relative w-full max-w-md group">
            <span class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-600 group-focus-within:text-solar-gold transition-colors">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            </span>
            <input type="text" [(ngModel)]="searchTerm" placeholder="Search by SKU or name..." 
                   class="w-full pl-12 pr-5 py-3 rounded-2xl border border-white/5 bg-slate-900/50 text-white placeholder:text-slate-600 focus:outline-none focus:border-solar-gold/50 focus:bg-slate-900 transition-all text-sm font-medium" />
        </div>
        <div class="flex items-center gap-4">
            <div class="flex items-center gap-2 px-4 py-2 rounded-xl bg-red-500/5 border border-red-500/10">
                <div class="w-1.5 h-1.5 rounded-full bg-red-500 animate-ping"></div>
                <span class="text-[10px] font-black uppercase tracking-wider text-red-500">Low Stock Alert</span>
            </div>
            <div class="flex items-center gap-2 px-4 py-2 rounded-xl bg-solar-gold/5 border border-solar-gold/10">
                <div class="w-1.5 h-1.5 rounded-full bg-solar-gold"></div>
                <span class="text-[10px] font-black uppercase tracking-wider text-solar-gold">Audit Verified</span>
            </div>
        </div>
      </div>

      <div class="rich-card !p-0 overflow-hidden border-white/5">
        <div class="overflow-x-auto">
            <table class="w-full text-left">
              <thead>
                <tr class="bg-white/[0.01]">
                  <th class="premium-table-header">ID</th>
                  <th class="premium-table-header">Product Details</th>
                  <th class="premium-table-header">Warehouse Qty</th>
                  <th class="premium-table-header">Unit Economics</th>
                  <th class="premium-table-header">Status</th>
                  <th class="premium-table-header">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-white/5">
                <tr *ngFor="let p of filteredProducts()" class="hover:bg-white/[0.02] transition-colors group">
                  <td class="premium-table-cell font-mono text-xs text-slate-600">#{{p.productId}}</td>
                  <td class="premium-table-cell">
                    <div class="flex items-center gap-4">
                         <div class="w-11 h-11 rounded-xl bg-slate-800 flex items-center justify-center text-solar-gold">
                           <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16Z"/><polyline points="3.27 6.96 12 12.01 20.73 6.96"/><line x1="12" y1="22.08" x2="12" y2="12"/></svg>
                         </div>
                         <div>
                             <div class="font-black text-white text-sm tracking-tight">{{p.name}}</div>
                             <div class="text-[9px] px-2 py-0.5 rounded bg-white/5 text-slate-500 font-black uppercase w-fit mt-1 tracking-widest border border-white/5">{{p.categoryName}}</div>
                         </div>
                    </div>
                  </td>
                  <td class="premium-table-cell">
                    <div class="flex items-center gap-3">
                        <div class="text-xl font-black" [class.text-red-500]="p.stockQty < 20" [class.text-white]="p.stockQty >= 20">{{p.stockQty}}</div>
                        <div *ngIf="p.stockQty < 20" class="text-[9px] font-black uppercase tracking-tighter text-red-500 bg-red-500/10 px-2 py-1 rounded border border-red-500/20">Critical</div>
                    </div>
                  </td>
                  <td class="premium-table-cell">
                    <div class="flex flex-col gap-1">
                        <div class="text-[10px] text-slate-500 font-bold uppercase tracking-tight">Cost: <span class="text-slate-300">₹{{p.costPrice}}</span></div>
                        <div class="text-[10px] text-slate-500 font-bold uppercase tracking-tight">Sale: <span class="text-solar-gold">₹{{p.price}}</span></div>
                        <div class="text-[11px] font-black text-emerald-400 mt-1">Margin: +₹{{p.price - p.costPrice}}</div>
                    </div>
                  </td>
                  <td class="premium-table-cell">
                    <span class="inline-flex items-center gap-2 px-3 py-1.5 rounded-lg text-[9px] font-black uppercase tracking-widest"
                          [class.bg-emerald-500/10]="p.isActive" [class.text-emerald-400]="p.isActive" [class.border]="p.isActive" [class.border-emerald-500/20]="p.isActive"
                          [class.bg-white/5]="!p.isActive" [class.text-slate-600]="!p.isActive" [class.border]="!p.isActive" [class.border-white/5]="!p.isActive">
                      <span class="w-1.5 h-1.5 rounded-full" [class.bg-emerald-500]="p.isActive" [class.bg-slate-700]="!p.isActive"></span>
                      {{p.isActive ? 'Optimal' : 'Halted'}}
                    </span>
                  </td>
                  <td class="premium-table-cell">
                    <div class="flex items-center gap-2 opacity-0 group-hover:opacity-100 transition-all duration-300 transform translate-x-2 group-hover:translate-x-0">
                      <button (click)="movementData.productId = p.productId; showMovementModal = true"
                              class="w-9 h-9 flex items-center justify-center rounded-lg bg-slate-800 border border-white/5 text-slate-400 hover:text-solar-gold hover:border-solar-gold/40 hover:bg-slate-700 transition-all">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a2 2 0 0 1 2.83 0l1.17 1.17a2 2 0 0 1 0 2.83L21.7 13.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 1 1-1.4 1.4L20.3 16.1a1 1 0 0 0-1.4 0l-3.77 3.77a2 2 0 0 1-2.83 0l-1.17-1.17a2 2 0 0 1 0-2.83L12.7 14.3a1 1 0 0 0 0-1.4l-1.6-1.6a1 1 0 0 1 1.4-1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a2 2 0 0 0 0-2.83l-1.17-1.17a2 2 0 0 0-2.83 0L14.7 6.3z"/></svg>
                      </button>
                      <button (click)="openDamageModal(p)"
                              class="w-9 h-9 flex items-center justify-center rounded-lg bg-red-500/10 border border-red-500/20 text-red-500 hover:bg-red-500 hover:text-white transition-all">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3Z"/><path d="M12 9v4"/><path d="M12 17h.01"/></svg>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
        </div>
      </div>

      <!-- Stock Movement Modal -->
      <div class="fixed inset-0 z-[100] flex items-center justify-center p-4 sm:p-6" *ngIf="showMovementModal">
        <div class="absolute inset-0 bg-slate-950/80 backdrop-blur-md animate-in fade-in duration-500" (click)="showMovementModal = false"></div>
        <div class="relative w-full max-w-xl bg-slate-900 rounded-[2.5rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-500 border border-white/5">
          <div class="bg-gradient-to-r from-slate-800 to-slate-900 px-10 py-8 flex justify-between items-center border-b border-white/5">
            <div>
              <h3 class="text-2xl font-sans font-black text-white uppercase italic tracking-tighter">Inventory <span class="text-solar-gold">Shift</span></h3>
              <p class="text-xs text-slate-500 font-bold uppercase tracking-widest mt-1">Direct Stock Adjustment Master</p>
            </div>
            <button class="w-10 h-10 rounded-full border border-white/10 text-white/40 hover:text-white hover:border-white animate-transition" (click)="showMovementModal = false">×</button>
          </div>
          <div class="p-10 space-y-8">
            <div class="space-y-3">
              <label class="text-[10px] font-black uppercase tracking-widest text-slate-500">Master Product Select</label>
              <select [(ngModel)]="movementData.productId" class="w-full bg-slate-950 border border-white/5 rounded-2xl px-6 py-4 text-white text-sm font-bold focus:outline-none focus:border-solar-gold transition-all appearance-none cursor-pointer">
                <option *ngFor="let p of products()" [value]="p.productId">{{p.name}} (Current: {{p.stockQty}})</option>
              </select>
            </div>
            <div class="grid grid-cols-2 gap-8">
              <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-500">Adjustment Type</label>
                <select [(ngModel)]="movementData.type" class="w-full bg-slate-950 border border-white/5 rounded-2xl px-6 py-4 text-white text-sm font-bold focus:outline-none focus:border-solar-gold transition-all appearance-none cursor-pointer">
                  <option value="Purchase">Stock In (Purchase)</option>
                  <option value="Adjustment">Audit Correction</option>
                  <option value="Internal Use">Factory Usage</option>
                </select>
              </div>
              <div class="space-y-3">
                <label class="text-[10px] font-black uppercase tracking-widest text-slate-500">Audit Quantity</label>
                <input type="number" [(ngModel)]="movementData.quantity" class="w-full bg-slate-950 border border-white/5 rounded-2xl px-6 py-4 text-white text-sm font-black focus:outline-none focus:border-solar-gold transition-all" placeholder="0" />
              </div>
            </div>
            <div class="space-y-3">
              <label class="text-[10px] font-black uppercase tracking-widest text-slate-500">Entry Logs / Manifest Reference</label>
              <textarea [(ngModel)]="movementData.remarks" class="w-full bg-slate-950 border border-white/5 rounded-2xl px-6 py-4 text-white text-sm font-medium focus:outline-none focus:border-solar-gold transition-all" rows="3" placeholder="Reference batch numbers..."></textarea>
            </div>
          </div>
          <div class="p-10 bg-black/20 border-t border-white/5 flex flex-col gap-4">
            <button class="btn-premium-primary w-full py-5 rounded-2xl shadow-solar" (click)="confirmMovement()">
               Commit Master Inventory Update
            </button>
            <button class="text-[10px] font-black uppercase tracking-widest text-slate-600 hover:text-white transition-colors" (click)="showMovementModal = false">Cancel Transaction</button>
          </div>
        </div>
      </div>

      <!-- Damage Modal -->
      <div class="fixed inset-0 z-[100] flex items-center justify-center p-4 sm:p-6" *ngIf="selectedProduct">
        <div class="absolute inset-0 bg-red-950/80 backdrop-blur-md animate-in fade-in duration-500" (click)="selectedProduct = null"></div>
        <div class="relative w-full max-w-lg bg-slate-900 rounded-[2.5rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-500 border border-red-500/20">
           <div class="bg-gradient-to-r from-red-600 to-red-800 px-10 py-8 flex justify-between items-center">
              <div>
                  <h3 class="text-2xl font-sans font-black text-white uppercase italic tracking-tighter">Loss <span class="text-red-200">Mitigation</span></h3>
                  <p class="text-[10px] font-black uppercase tracking-widest text-red-200/60 mt-1">Logging Damage: {{selectedProduct.name}}</p>
              </div>
              <button class="w-10 h-10 rounded-full border border-white/20 text-white/50 hover:text-white" (click)="selectedProduct = null">×</button>
          </div>
          <div class="p-10 space-y-8">
            <p class="text-xs text-slate-400 leading-relaxed font-bold uppercase tracking-tight italic">
              "This transaction will permanently subtract quantity from warehouse stock and log a financial deficit in the audit trail."
            </p>
            <div class="grid grid-cols-2 gap-8">
                <div class="space-y-3">
                    <label class="text-[10px] font-black uppercase tracking-widest text-slate-500">Deficit Quantity</label>
                    <input type="number" [(ngModel)]="damageQty" class="w-full bg-slate-950 border border-red-500/20 rounded-2xl px-6 py-4 text-white text-sm font-black focus:outline-none focus:border-red-500 transition-all" />
                </div>
                <div class="space-y-3">
                    <label class="text-[10px] font-black uppercase tracking-widest text-slate-500">Root Defect Cause</label>
                    <select [(ngModel)]="damageReason" class="w-full bg-slate-950 border border-red-500/20 rounded-2xl px-6 py-4 text-white text-sm font-bold focus:outline-none focus:border-red-500 transition-all appearance-none cursor-pointer">
                        <option value="Broken Seal">Tampered Seal</option>
                        <option value="Infected">Biological Hazard</option>
                        <option value="Expired">Maturity Threshold</option>
                        <option value="Quality Issue">QC Rejection</option>
                    </select>
                </div>
            </div>
            <div class="p-6 rounded-2xl bg-red-500/5 border border-red-500/10 flex items-center justify-between">
                <div>
                  <div class="text-[10px] font-black text-red-500 uppercase tracking-widest mb-1">Fiscal Impact Velocity</div>
                  <div class="text-2xl font-sans font-black text-white">₹{{damageQty * selectedProduct.costPrice | number}} Deficit</div>
                </div>
                <div class="w-12 h-12 rounded-full bg-red-500/20 flex items-center justify-center text-red-500">
                  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 2v20"/><path d="m17 5-5-3-5 3"/><path d="m17 19-5 3-5-3"/><path d="m2 17 3-5-3-5"/><path d="m22 17-3-5 3-5"/></svg>
                </div>
            </div>
          </div>
          <div class="p-10 bg-black/20 border-t border-white/5 flex flex-col gap-4">
             <button class="w-full py-5 bg-red-600 text-white font-black uppercase tracking-widest text-sm rounded-2xl hover:bg-red-500 transition-all shadow-xl shadow-red-900/20" (click)="confirmDamage()">Log Loss & Adjust Stock</button>
             <button class="text-[10px] font-black uppercase tracking-widest text-slate-600 hover:text-white transition-colors" (click)="selectedProduct = null">Cancel Event</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    @reference "../../../../styles.css";
    .premium-table-header { @apply px-6 py-5 text-[10px] font-black uppercase tracking-[0.2em] text-slate-500; }
    .premium-table-cell { @apply px-6 py-5 text-sm; }
  `]
})
export class InventoryMasterComponent implements OnInit {
  private adminService = inject(AdminService);
  products = signal<any[]>([]);
  searchTerm = '';
  showMovementModal = false;
  movementData = {
    productId: 0,
    quantity: 0,
    type: 'Purchase',
    remarks: ''
  };
  
  // Damage Modal
  selectedProduct: any = null;
  damageQty = 1;
  damageReason = 'Quality Issue';

  ngOnInit() {
    this.loadInventory();
  }

  loadInventory() {
    this.adminService.getInventoryMaster().subscribe(data => this.products.set(data));
  }

  filteredProducts() {
    const term = this.searchTerm.toLowerCase();
    return this.products().filter(p => 
      p.name.toLowerCase().includes(term) || 
      p.categoryName.toLowerCase().includes(term)
    );
  }

  openDamageModal(product: any) {
    this.selectedProduct = product;
    this.damageQty = 1;
  }

  confirmDamage() {
    if (!this.selectedProduct) return;
    const damage = {
      productId: this.selectedProduct.productId,
      quantity: this.damageQty,
      reason: this.damageReason,
      lossAmount: this.damageQty * this.selectedProduct.costPrice,
      loggedInBy: 1 // Admin Vivek
    };

    this.adminService.logDamage(damage).subscribe(() => {
      alert('Damage logged and stock adjusted.');
      this.selectedProduct = null;
      this.loadInventory();
    });
  }

  confirmMovement() {
    if (this.movementData.productId === 0 || this.movementData.quantity === 0) {
      alert('Please select a product and entering a non-zero quantity.');
      return;
    }

    this.adminService.logMovement(this.movementData).subscribe({
      next: () => {
        alert('Inventory Master updated successfully.');
        this.showMovementModal = false;
        this.movementData = { productId: 0, quantity: 0, type: 'Purchase', remarks: '' };
        this.loadInventory();
      },
      error: () => alert('Failed to update inventory. Please verify product selection.')
    });
  }
}
