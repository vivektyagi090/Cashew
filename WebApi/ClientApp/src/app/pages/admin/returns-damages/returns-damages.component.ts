import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../core/services/admin.service';

@Component({
  selector: 'app-returns-damages',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-8 animate-in fade-in slide-in-from-bottom-5 duration-700">
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
           <h1 class="text-3xl font-serif font-black text-royal-dark">Reverse Logistics</h1>
           <p class="text-sm text-royal-brown/60 mt-1 font-medium">Customer returns & internal damage audit management</p>
        </div>
        <div class="flex gap-3">
          <button class="btn-premium px-6 py-2.5 bg-white border border-royal-border text-royal-brown/60 font-bold hover:border-royal-gold hover:text-royal-gold transition-all" (click)="loadData()">🔄 Refresh</button>
          <button class="btn-premium btn-premium-primary px-6" (click)="openReturnModal()">
            <span>➕ Log Manual Return</span>
          </button>
        </div>
      </div>

      <div class="flex items-center gap-1 p-1 bg-royal-cream/30 rounded-2xl w-fit border border-royal-border/40">
        <button (click)="activeTab = 'returns'" 
                class="px-8 py-2.5 rounded-xl text-xs font-black uppercase tracking-widest transition-all duration-300"
                [class.bg-royal-dark]="activeTab === 'returns'" [class.text-white]="activeTab === 'returns'" [class.shadow-xl]="activeTab === 'returns'"
                [class.text-royal-brown/40]="activeTab !== 'returns'" [class.hover:text-royal-dark]="activeTab !== 'returns'">
            Customer Returns
        </button>
        <button (click)="activeTab = 'damages'" 
                class="px-8 py-2.5 rounded-xl text-xs font-black uppercase tracking-widest transition-all duration-300"
                [class.bg-royal-dark]="activeTab === 'damages'" [class.text-white]="activeTab === 'damages'" [class.shadow-xl]="activeTab === 'damages'"
                [class.text-royal-brown/40]="activeTab !== 'damages'" [class.hover:text-royal-dark]="activeTab !== 'damages'">
            Internal Damages
        </button>
      </div>

      <div class="rich-card !p-0 overflow-hidden border-royal-border/40 min-h-[400px]">
        <!-- Returns Tab -->
        <div *ngIf="activeTab === 'returns'" class="animate-in fade-in slide-in-from-left-5 duration-500">
          <div class="overflow-x-auto">
            <table class="w-full text-left">
              <thead>
                <tr class="bg-royal-cream/20">
                  <th class="premium-table-header">Return #</th>
                  <th class="premium-table-header">Manifest Reference</th>
                  <th class="premium-table-header">Product Details</th>
                  <th class="premium-table-header">Refund Credit</th>
                  <th class="premium-table-header">Audit Status</th>
                  <th class="premium-table-header">Review Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-royal-border/30">
                <tr *ngFor="let r of returns()" class="hover:bg-royal-cream/5 transition-colors group">
                  <td class="premium-table-cell font-mono text-[10px] text-royal-brown/40">ID-{{r.returnId}}</td>
                  <td class="premium-table-cell font-bold text-royal-dark">#{{r.orderId}}</td>
                  <td class="premium-table-cell">
                    <div class="flex flex-col">
                        <span class="font-bold text-royal-dark">{{r.productName}}</span>
                        <span class="text-[10px] text-royal-brown/40 font-medium">Qty: {{r.quantity}} units</span>
                    </div>
                  </td>
                  <td class="premium-table-cell font-black text-royal-brown">₹{{r.refundAmount | number}}</td>
                  <td class="premium-table-cell">
                    <span class="inline-flex items-center gap-1.5 px-3 py-1 rounded-lg text-[9px] font-black uppercase tracking-wider"
                          [ngClass]="{
                            'bg-amber-100 text-amber-700': r.status === 'Pending',
                            'bg-emerald-100 text-emerald-700': r.status === 'Approved',
                            'bg-red-100 text-red-700': r.status === 'Rejected'
                          }">
                       {{r.status}}
                    </span>
                  </td>
                  <td class="premium-table-cell">
                    <div class="flex items-center gap-2" *ngIf="r.status === 'Pending'">
                      <button (click)="processReturn(r, 'Approved')" class="px-4 py-1.5 rounded-lg bg-emerald-600 text-[10px] font-black text-white hover:bg-emerald-700 transition-all shadow-sm">APPROVE</button>
                      <button (click)="processReturn(r, 'Rejected')" class="px-4 py-1.5 rounded-lg bg-white border border-red-200 text-[10px] font-black text-red-600 hover:bg-red-50 transition-all">REJECT</button>
                    </div>
                    <span *ngIf="r.status !== 'Pending'" class="text-[10px] font-bold text-royal-brown/30 italic">Decision Recorded</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Damages Tab -->
        <div *ngIf="activeTab === 'damages'" class="animate-in fade-in slide-in-from-right-5 duration-500">
          <div class="overflow-x-auto">
            <table class="w-full text-left">
              <thead>
                <tr class="bg-royal-cream/20">
                  <th class="premium-table-header">Damage Date</th>
                  <th class="premium-table-header">Master Product</th>
                  <th class="premium-table-header">Loss Quantity</th>
                  <th class="premium-table-header">Financial Deficit</th>
                  <th class="premium-table-header">Root Cause</th>
                  <th class="premium-table-header">Logged By</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-royal-border/30">
                <tr *ngFor="let d of damages()" class="hover:bg-royal-cream/5 transition-colors group">
                  <td class="premium-table-cell text-royal-brown/60 font-medium">{{d.createdAt | date:'mediumDate'}}</td>
                  <td class="premium-table-cell font-bold text-royal-dark">{{d.productName}}</td>
                  <td class="premium-table-cell font-black text-royal-brown">{{d.quantity}} units</td>
                  <td class="premium-table-cell font-black text-red-600">₹{{d.lossAmount | number}}</td>
                  <td class="premium-table-cell">
                    <span class="text-[11px] font-medium text-royal-brown/60 px-2 py-1 bg-royal-cream rounded-md">{{d.reason}}</span>
                  </td>
                  <td class="premium-table-cell">
                    <div class="flex items-center gap-2">
                         <div class="w-6 h-6 rounded-full bg-royal-dark flex items-center justify-center text-[10px] text-white">{{d.adminName?.charAt(0) || 'A'}}</div>
                         <span class="text-xs font-bold text-royal-dark">{{d.adminName}}</span>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Manual Return Master Modal -->
      <div class="fixed inset-0 z-[100] flex items-center justify-center p-4 sm:p-6" *ngIf="showReturnModal">
        <div class="absolute inset-0 bg-royal-dark/60 backdrop-blur-sm animate-in fade-in duration-300" (click)="showReturnModal = false"></div>
        <div class="relative w-full max-w-xl bg-white rounded-3xl shadow-2xl overflow-hidden animate-in zoom-in-95 duration-300">
          <div class="bg-royal-dark px-8 py-6 flex justify-between items-center">
            <h3 class="text-xl font-serif font-bold text-white">Log Manual Return Master</h3>
            <button class="text-white/60 hover:text-white text-2xl" (click)="showReturnModal = false">×</button>
          </div>
          <div class="p-8 space-y-6">
            <div class="grid grid-cols-2 gap-6">
              <div class="space-y-1.5">
                <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Manifest Order #</label>
                <input type="number" [(ngModel)]="returnMasterData.orderId" class="premium-input-field" placeholder="1045" />
              </div>
              <div class="space-y-1.5">
                <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Select Product Code</label>
                <select [(ngModel)]="returnMasterData.productId" class="premium-input-field">
                  <option *ngFor="let p of products()" [value]="p.productId">{{p.name}}</option>
                </select>
              </div>
            </div>
            <div class="grid grid-cols-2 gap-6">
              <div class="space-y-1.5">
                <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Return Quantity</label>
                <input type="number" [(ngModel)]="returnMasterData.quantity" class="premium-input-field" />
              </div>
              <div class="space-y-1.5">
                <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Refund Credit Value</label>
                <input type="number" [(ngModel)]="returnMasterData.refundAmount" class="premium-input-field" />
              </div>
            </div>
            <div class="space-y-1.5">
              <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Reason for Reverse Shipment</label>
              <textarea [(ngModel)]="returnMasterData.reason" class="premium-input-field" rows="3" placeholder="Reference reasons for return..."></textarea>
            </div>
          </div>
          <div class="p-8 bg-royal-cream/30 border-t border-royal-border/40 flex flex-col gap-3">
            <button class="btn-premium btn-premium-primary w-full px-6 py-4 rounded-2xl shadow-gold font-bold" (click)="confirmManualReturn()">
               Execute Master Return Log
            </button>
            <button class="text-sm font-bold text-royal-brown/40 hover:text-royal-dark transition-colors" (click)="showReturnModal = false">Discard Log Entry</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [``]
})
export class ReturnsDamagesComponent implements OnInit {
  private adminService = inject(AdminService);
  activeTab: 'returns' | 'damages' = 'returns';
  
  returns = signal<any[]>([]);
  damages = signal<any[]>([]);
  products = signal<any[]>([]);

  showReturnModal = false;
  returnMasterData = {
    orderId: 0,
    productId: 0,
    quantity: 1,
    reason: '',
    refundAmount: 0,
    status: 'Approved' // Handled directly if logged by master
  };

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.adminService.getReturns().subscribe((data: any[]) => this.returns.set(data));
    this.adminService.getMovements().subscribe((m: any[]) => {
          this.damages.set(m.filter((x: any) => x.type === 'Damage'));
    });
  }

  processReturn(r: any, status: string) {
    const payload = { ...r, status };
    this.adminService.processReturn(payload).subscribe(() => {
      alert(`Return ${status.toLowerCase()} successfully.`);
      this.loadData();
    });
  }

  openReturnModal() {
    this.adminService.getInventoryMaster().subscribe(data => {
      this.products.set(data);
      this.showReturnModal = true;
    });
  }

  confirmManualReturn() {
    if (this.returnMasterData.productId === 0 || this.returnMasterData.quantity <= 0) {
      alert('Please select a product and valid quantity.');
      return;
    }

    this.adminService.logReturn(this.returnMasterData).subscribe({
      next: () => {
        alert('Manual return processed and inventory updated.');
        this.showReturnModal = false;
        this.loadData();
      },
      error: () => alert('Error processing return. Verify Order ID.')
    });
  }
}
