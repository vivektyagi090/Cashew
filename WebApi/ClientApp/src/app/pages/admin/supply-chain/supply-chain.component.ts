import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../core/services/admin.service';

@Component({
  selector: 'app-supply-chain',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-8 animate-in fade-in slide-in-from-bottom-5 duration-700">
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
           <h1 class="text-3xl font-serif font-black text-royal-dark">Logistics tracking</h1>
           <p class="text-sm text-royal-brown/60 mt-1 font-medium">Real-time supply chain monitoring & manifest management</p>
        </div>
        <div class="flex gap-3">
          <button class="btn-premium px-6 py-2.5 bg-white border border-royal-border text-royal-brown/60 font-bold hover:border-royal-gold hover:text-royal-gold transition-all" (click)="loadLogistics()">🔄 Refresh</button>
          <button class="btn-premium btn-premium-primary px-6" (click)="showManifestModal = true">
            <span>➕ Create Manifest</span>
          </button>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-12 gap-8 h-[calc(100vh-280px)]">
        <!-- List Panel -->
        <div class="lg:col-span-4 flex flex-col rich-card !p-0 overflow-hidden min-h-0 border-royal-border/40">
           <div class="p-5 bg-royal-cream/20 border-b border-royal-border/40">
              <h3 class="font-bold text-royal-dark text-sm uppercase tracking-widest">Active Manifests</h3>
           </div>
           
           <div class="flex-1 overflow-y-auto divide-y divide-royal-border/30 custom-scrollbar">
              <div *ngFor="let item of logistics()" 
                   (click)="selectShipment(item)"
                   class="p-5 cursor-pointer transition-all duration-300 hover:bg-royal-cream/10 relative group"
                   [class.bg-royal-cream/30]="selectedLogistics?.logisticsId === item.logisticsId">
                
                <div *ngIf="selectedLogistics?.logisticsId === item.logisticsId" 
                     class="absolute left-0 top-0 bottom-0 w-1.5 bg-royal-gold"></div>

                <div class="flex justify-between items-start mb-2">
                    <span class="font-mono text-xs font-black text-royal-brown/40 group-hover:text-royal-gold transition-colors">#{{item.trackingNumber || 'TRK-PENDING'}}</span>
                    <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-[9px] font-black uppercase tracking-tighter"
                          [ngClass]="item.status === 'Delivered' ? 'bg-emerald-100 text-emerald-700' : 'bg-royal-gold/10 text-royal-gold'">
                        {{item.status}}
                    </span>
                </div>
                
                <div class="flex items-center gap-3">
                    <div class="w-10 h-10 rounded-xl bg-royal-cream flex items-center justify-center text-lg">🚚</div>
                    <div>
                        <div class="font-bold text-sm text-royal-dark">{{item.carrierName || 'Unassigned Carrier'}}</div>
                        <div class="text-[10px] text-royal-brown/50 font-medium">Updated: {{item.shippedDate | date:'mediumDate'}}</div>
                    </div>
                </div>
              </div>
           </div>
        </div>

        <!-- Detail Panel -->
        <div class="lg:col-span-8 flex flex-col rich-card !p-0 overflow-hidden border-royal-border/40 min-h-0">
           <div *ngIf="selectedLogistics" class="flex flex-col h-full animate-in slide-in-from-right-5 duration-500">
              <div class="p-6 bg-royal-dark flex justify-between items-center">
                  <div>
                      <h3 class="text-white font-serif text-lg font-bold">Manifest Tracking Audit</h3>
                      <p class="text-royal-gold text-[10px] font-black uppercase tracking-[0.2em] mt-0.5">Order UID: {{selectedLogistics.logisticsId}}</p>
                  </div>
                  <div class="px-4 py-1.5 rounded-full bg-white/10 text-white text-[10px] font-bold border border-white/10">
                      LIVE STATUS
                  </div>
              </div>

              <div class="flex-1 overflow-y-auto p-8 custom-scrollbar">
                  <div class="grid grid-cols-1 md:grid-cols-2 gap-8 mb-10">
                      <div class="space-y-6">
                          <div class="space-y-1.5">
                              <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Carrier Details</label>
                              <input type="text" [(ngModel)]="selectedLogistics.carrierName" class="premium-input-field !py-2.5" />
                          </div>
                          <div class="space-y-1.5">
                              <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Tracking Identifier</label>
                              <input type="text" [(ngModel)]="selectedLogistics.trackingNumber" class="premium-input-field !py-2.5" />
                          </div>
                      </div>
                      <div class="space-y-6">
                          <div class="space-y-1.5">
                              <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Shipment Milestone</label>
                              <select [(ngModel)]="selectedLogistics.status" class="premium-input-field !py-2.5">
                                <option value="Packed">Packed for Shipping</option>
                                <option value="Shipped">Shipped from Hub</option>
                                <option value="In-Transit">In-Transit to Customer</option>
                                <option value="Delivered">Delivered (Completed)</option>
                                <option value="Returned">Returned to Origin</option>
                              </select>
                          </div>
                          <div class="space-y-1.5">
                              <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Current Geolocation</label>
                              <input type="text" [(ngModel)]="selectedLogistics.currentLocation" class="premium-input-field !py-2.5" />
                          </div>
                      </div>
                      <div class="md:col-span-2">
                        <button class="btn-premium btn-premium-primary w-full shadow-gold" (click)="saveLogistics()">
                          Commit Status Update
                        </button>
                      </div>
                  </div>

                  <!-- Timeline -->
                  <div class="pt-8 border-t border-royal-border/40">
                      <h4 class="text-xs font-black uppercase tracking-[0.2em] text-royal-gold mb-8 flex items-center gap-2">
                        📡 Tracking Audit Trail <div class="h-px bg-royal-border/40 flex-1"></div>
                      </h4>
                      
                      <div class="space-y-10 pl-4 border-l-2 border-royal-border/30 relative ml-2">
                          <div *ngFor="let step of getTrackingSteps()" class="relative">
                              <div class="absolute -left-6.5 top-0 w-5 h-5 rounded-full border-4 border-white transition-all duration-500"
                                   [class.bg-royal-gold]="step.done" [class.bg-royal-border]="!step.done"
                                   [class.ring-4]="step.done" [class.ring-royal-gold/20]="step.done"></div>
                              
                              <div class="flex justify-between items-start">
                                  <div>
                                      <p class="font-bold text-sm tracking-tight" [class.text-royal-dark]="step.done" [class.text-royal-brown/30]="!step.done">{{step.name}}</p>
                                      <p *ngIf="step.done" class="text-[11px] text-royal-brown/60 font-medium mt-1">Milestone verified at dispatch center</p>
                                  </div>
                                  <div *ngIf="step.done" class="text-[11px] font-bold text-royal-gold">{{selectedLogistics.updatedAt | date:'shortTime'}}</div>
                              </div>
                          </div>
                      </div>
                  </div>
              </div>
           </div>

           <!-- Empty State -->
           <div *ngIf="!selectedLogistics" class="flex-1 flex flex-col items-center justify-center p-12 text-center">
              <div class="w-24 h-24 rounded-full bg-royal-cream flex items-center justify-center text-4xl mb-6 shadow-rich border border-royal-border/40">📡</div>
              <h3 class="text-xl font-serif font-bold text-royal-dark">No Manifest Selected</h3>
              <p class="text-sm text-royal-brown/40 mt-2 max-w-xs mx-auto font-medium">Select a shipment from the left panel to manage tracking milestones and carrier data.</p>
           </div>
        </div>
      </div>

       <!-- Manifest Creator Modal -->
       <div class="fixed inset-0 z-[100] flex items-center justify-center p-4 sm:p-6" *ngIf="showManifestModal">
        <div class="absolute inset-0 bg-royal-dark/60 backdrop-blur-sm animate-in fade-in duration-300" (click)="showManifestModal = false"></div>
        <div class="relative w-full max-w-lg bg-white rounded-3xl shadow-2xl overflow-hidden animate-in zoom-in-95 duration-300">
          <div class="bg-royal-dark px-8 py-6 flex justify-between items-center">
            <h3 class="text-xl font-serif font-bold text-white">Create Tracking Manifest</h3>
            <button class="text-white/60 hover:text-white text-2xl" (click)="showManifestModal = false">×</button>
          </div>
          <div class="p-8 space-y-6">
            <div class="grid grid-cols-2 gap-6">
              <div class="space-y-1.5">
                <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Order ID</label>
                <input type="number" [(ngModel)]="newManifest.orderId" class="premium-input-field" placeholder="0000" />
              </div>
              <div class="space-y-1.5">
                <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Primary Carrier</label>
                <select [(ngModel)]="newManifest.carrierName" class="premium-input-field">
                  <option value="VVKBMS Logistics">VVKBMS Logistics</option>
                  <option value="Delhivery">Delhivery</option>
                  <option value="BlueDart">BlueDart</option>
                  <option value="FedEx">FedEx</option>
                </select>
              </div>
            </div>
            <div class="space-y-1.5">
              <label class="text-[10px] font-black uppercase tracking-widest text-royal-brown/40">Primary Tracking Number</label>
              <input type="text" [(ngModel)]="newManifest.trackingNumber" class="premium-input-field font-mono" placeholder="TRK778844XXXX" />
            </div>
          </div>
          <div class="p-8 bg-royal-cream/30 border-t border-royal-border/40 flex flex-col gap-3">
            <button class="w-full px-6 py-4 bg-royal-gold text-white font-bold rounded-2xl hover:bg-royal-gold/90 transition-all shadow-lg shadow-royal-gold/20" (click)="createManifest()">Generate Manifest & Start Tracking</button>
            <button class="text-sm font-bold text-royal-brown/40 hover:text-royal-dark transition-colors" (click)="showManifestModal = false">Discard Manifest</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .custom-scrollbar::-webkit-scrollbar { width: 6px; }
    .custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
    .custom-scrollbar::-webkit-scrollbar-thumb { background: #e5e7eb; border-radius: 10px; }
    .custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #d1d5db; }
  `]
})
export class SupplyChainComponent implements OnInit {
  private adminService = inject(AdminService);
  logistics = signal<any[]>([]);
  searchTerm = '';
  selectedLogistics: any = null;

  ngOnInit() {
    this.loadLogistics();
  }

  loadLogistics() {
    this.adminService.getLogistics().subscribe((data: any[]) => {
      this.logistics.set(data);
      if (data.length > 0 && !this.selectedLogistics) {
        this.selectedLogistics = { ...data[0] };
      }
    });
  }

  showManifestModal = false;
  newManifest = {
    orderId: 0,
    carrierName: 'VVKBMS Logistics',
    trackingNumber: '',
    status: 'Packed',
    currentLocation: 'Main Warehouse'
  };

  createManifest() {
    if (this.newManifest.orderId === 0) {
      alert('Valid Order ID required.');
      return;
    }
    this.adminService.updateLogistics(this.newManifest).subscribe(() => {
      alert('Manifest created for Order #' + this.newManifest.orderId);
      this.showManifestModal = false;
      this.loadLogistics();
    });
  }

  filteredLogistics() {
    const term = this.searchTerm.toLowerCase();
    return this.logistics().filter(l => 
      l.orderId.toString().includes(term) || 
      l.trackingNumber?.toLowerCase().includes(term)
    );
  }

  selectShipment(item: any) {
    this.selectedLogistics = { ...item };
  }

  saveLogistics() {
    this.adminService.updateLogistics(this.selectedLogistics).subscribe(() => {
      alert('Supply chain status updated for Order #' + this.selectedLogistics.orderId);
      this.loadLogistics();
    });
  }

  isStepDone(step: string) {
    const statuses = ['Packed', 'Shipped', 'In-Transit', 'Delivered'];
    const currentIdx = statuses.indexOf(this.selectedLogistics.status);
    const stepIdx = statuses.indexOf(step);
    return stepIdx != -1 && stepIdx <= currentIdx;
  }

  getTrackingSteps() {
    const steps = ['Packed', 'Shipped', 'In-Transit', 'Delivered'];
    return steps.map(s => ({
      name: s,
      done: this.isStepDone(s)
    }));
  }

  getStatusClass(status: string) {
    if (status === 'Packed') return 'badge-packed';
    if (status === 'Delivered') return 'badge-delivered';
    return 'badge-transit';
  }
}
