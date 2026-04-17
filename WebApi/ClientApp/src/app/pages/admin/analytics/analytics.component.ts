import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../../core/services/admin.service';

@Component({
  selector: 'app-admin-analytics',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-8 animate-in fade-in slide-in-from-bottom-5 duration-700">
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
           <h1 class="text-3xl font-sans font-black text-white tracking-tighter uppercase italic">Business <span class="text-solar-gold">Insights</span></h1>
           <p class="text-sm text-slate-500 mt-1 font-medium">Real-time revenue monitoring and fiscal performance</p>
        </div>
        <div class="flex items-center gap-3">
          <div class="relative group">
            <span class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500 text-xs">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect width="18" height="18" x="3" y="4" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
            </span>
            <input type="date" class="pl-10 pr-4 py-2 rounded-xl border border-white/5 bg-slate-900/50 text-xs font-bold text-white focus:outline-none focus:border-solar-gold transition-all" />
          </div>
          <button class="w-10 h-10 flex items-center justify-center rounded-xl bg-slate-800 text-white hover:bg-solar-gold hover:text-black transition-all">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
          </button>
        </div>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <div class="rich-card !bg-gradient-to-br from-solar-gold to-solar-amber !border-none shadow-solar group cursor-pointer">
          <div class="flex justify-between items-start">
             <div class="text-[10px] uppercase font-black tracking-[0.2em] text-black/70">Total Revenue</div>
             <span class="text-black/40 group-hover:text-black transition-colors">
               <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
             </span>
          </div>
          <div class="text-3xl font-sans font-black text-black mt-4">₹{{stats()?.totalRevenue | number}}</div>
          <div class="mt-4 flex items-center gap-1.5 text-[10px] font-black uppercase text-black/80 bg-black/10 w-fit px-2 py-1 rounded-md border border-black/5">
             <span class="animate-pulse">↑</span> 12.5% vs last month
          </div>
        </div>

        <div class="rich-card !bg-gradient-to-br from-slate-800 to-slate-950 !border-white/5 group cursor-pointer">
          <div class="flex justify-between items-start">
             <div class="text-[10px] uppercase font-black tracking-[0.2em] text-white/50">Net Operating Profit</div>
             <span class="text-solar-gold/40 group-hover:text-solar-gold transition-colors">
               <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="23 6 13.5 15.5 8.5 10.5 1 18"/><polyline points="17 6 23 6 23 12"/></svg>
             </span>
          </div>
          <div class="text-3xl font-sans font-black text-white mt-4">₹{{stats()?.totalProfit | number}}</div>
          <div class="mt-4 flex items-center gap-1.5 text-[10px] font-black uppercase text-emerald-400 bg-emerald-400/5 w-fit px-2 py-1 rounded-md border border-emerald-400/10">
             ↑ 8.2% healthy margin
          </div>
        </div>

        <div class="rich-card group hover:bg-white/5 transition-all border-white/5">
           <div class="flex justify-between items-start">
             <div class="text-[10px] uppercase font-black tracking-[0.2em] text-slate-500">Pending Shipments</div>
             <span class="text-slate-700 group-hover:text-solar-gold transition-colors">
               <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="1" y="3" width="15" height="13"/><polyline points="16 8 20 8 23 11 23 16 16 16"/><circle cx="5.5" cy="18.5" r="2.5"/><circle cx="18.5" cy="18.5" r="2.5"/></svg>
             </span>
          </div>
          <div class="text-3xl font-sans font-black text-white mt-4">{{stats()?.pendingOrders}}</div>
          <div class="mt-4 text-[10px] font-bold text-red-500 flex items-center gap-1.5 px-2 py-1 bg-red-500/5 rounded-md border border-red-500/10 w-fit">
             <span class="w-1.5 h-1.5 rounded-full bg-red-500 animate-ping"></span> 4 requiring attention
          </div>
        </div>

        <div class="rich-card group hover:bg-white/5 transition-all border-white/5">
           <div class="flex justify-between items-start">
             <div class="text-[10px] uppercase font-black tracking-[0.2em] text-slate-500">Active Inventory</div>
             <span class="text-slate-700 group-hover:text-solar-gold transition-colors">
               <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16Z"/><polyline points="3.27 6.96 12 12.01 20.73 6.96"/><line x1="12" y1="22.08" x2="12" y2="12"/></svg>
             </span>
          </div>
          <div class="text-3xl font-sans font-black text-white mt-4">{{stats()?.totalProducts}} <span class="text-sm font-sans font-medium text-slate-500 uppercase">skus</span></div>
          <div class="mt-4 text-[10px] font-bold text-slate-600 uppercase tracking-tighter">Distributed across 5 Categories</div>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-12 gap-8">
        <!-- Recent Orders -->
        <div class="lg:col-span-8 rich-card !p-0 overflow-hidden border-white/5 flex flex-col">
          <div class="p-6 bg-white/[0.02] border-b border-white/5 flex justify-between items-center">
             <h3 class="font-bold text-white text-sm uppercase tracking-widest flex items-center gap-3">
                <span class="w-8 h-8 rounded-lg bg-slate-800 flex items-center justify-center text-xs text-solar-gold">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
                </span>
                Supply Chain Pipeline
             </h3>
             <button class="text-[10px] font-black text-solar-gold uppercase border-b-2 border-solar-gold/0 hover:border-solar-gold transition-all">View All Flows</button>
          </div>
          <div class="overflow-x-auto">
            <table class="w-full text-left">
              <thead>
                <tr class="bg-white/[0.01]">
                  <th class="premium-table-header">Manifest #</th>
                  <th class="premium-table-header">Client Name</th>
                  <th class="premium-table-header">Invoice Value</th>
                  <th class="premium-table-header">Tracking Status</th>
                  <th class="premium-table-header">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-white/5">
                <tr *ngFor="let o of stats()?.recentOrders" class="hover:bg-white/[0.02] transition-colors group">
                  <td class="premium-table-cell font-mono text-xs text-slate-500">#{{o.orderId}}</td>
                  <td class="premium-table-cell font-bold text-slate-200">{{o.customerName}}</td>
                  <td class="premium-table-cell font-black text-white">₹{{o.amount | number}}</td>
                  <td class="premium-table-cell">
                    <span class="inline-flex items-center gap-1.5 px-3 py-1 rounded-lg text-[9px] font-black uppercase tracking-wider"
                          [ngClass]="getStatusClass(o.status) === 'badge-delivered' ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20' : 'bg-solar-gold/10 text-solar-gold border border-solar-gold/20'">
                       {{o.status}}
                    </span>
                  </td>
                  <td class="premium-table-cell">
                      <button (click)="trackOrder(o.orderId)" class="px-4 py-1.5 rounded-lg bg-slate-800 border border-white/5 text-[10px] font-black text-slate-400 hover:bg-solar-gold hover:text-black transition-all shadow-sm">
                        TRACK FLOW
                      </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Category Analysis -->
        <div class="lg:col-span-4 rich-card border-white/5">
           <h3 class="font-bold text-white text-sm uppercase tracking-widest mb-8">Harvest Segment Yield</h3>
           
           <div class="space-y-10">
                <div *ngFor="let item of [
                    {name: 'Plain Cashews', val: 45, color: '#F59E0B'},
                    {name: 'Salted/Roasted', val: 30, color: '#D97706'},
                    {name: 'Gift Packs', val: 15, color: '#F8FAFC'},
                    {name: 'Spiced/Flavored', val: 10, color: '#1E293B'}
                ]" class="space-y-3">
                    <div class="flex justify-between items-end">
                        <span class="text-xs font-bold text-slate-500">{{item.name}}</span>
                        <span class="text-xs font-black text-white">{{item.val}}%</span>
                    </div>
                    <div class="h-2 w-full bg-slate-800/50 rounded-full overflow-hidden shadow-inner border border-white/5">
                        <div class="h-full rounded-full transition-all duration-1000 ease-out shadow-sm shadow-black/50" [style.width.%]="item.val" [style.background]="item.color"></div>
                    </div>
                </div>
           </div>

           <div class="mt-12 p-5 rounded-[2rem] bg-slate-900 border border-solar-gold/20 shadow-xl relative overflow-hidden group">
               <div class="absolute inset-0 bg-gradient-to-br from-solar-gold/5 to-transparent opacity-0 group-hover:opacity-100 transition-opacity"></div>
               <div class="flex items-center gap-3 mb-4 relative z-10">
                   <span class="w-8 h-8 rounded-lg bg-solar-gold/10 flex items-center justify-center text-solar-gold">
                     <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/><polyline points="7.5 4.21 12 6.81 16.5 4.21"/><polyline points="7.5 19.79 7.5 14.6 3 12"/><polyline points="21 12 16.5 14.6 16.5 19.79"/><polyline points="3.27 6.96 12 12.01 20.73 6.96"/><line x1="12" y1="22.08" x2="12" y2="12"/></svg>
                   </span>
                   <span class="text-[10px] font-black uppercase tracking-widest text-solar-gold">Flow Optimization</span>
               </div>
               <p class="text-[11px] font-medium text-slate-400 leading-relaxed relative z-10">
                   Gift Packs currently provide the highest net margin <strong class="text-white">(+₹340/unit avg)</strong>. Growth in this segment is recommended for Q3.
               </p>
           </div>
        </div>

        <!-- P&L Breakdown -->
        <div class="lg:col-span-12 rich-card border-white/5 overflow-hidden">
            <div class="flex items-center gap-4 mb-10">
                <h3 class="font-bold text-white text-lg uppercase tracking-widest">Financial Flow Index</h3>
                <div class="h-px bg-white/5 flex-1"></div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-4 gap-12">
                <div class="space-y-2">
                    <p class="text-[10px] font-black text-slate-500 uppercase tracking-widest">Gross Sales Flow</p>
                    <p class="text-2xl font-sans font-black text-white">₹{{stats()?.totalRevenue | number}}</p>
                    <div class="h-1 bg-white/5 rounded-full overflow-hidden"><div class="h-full bg-solar-gold w-[85%] rounded-full shadow-solar"></div></div>
                </div>
                <div class="space-y-2">
                    <p class="text-[10px] font-black text-red-500 uppercase tracking-widest opacity-60">Cost Flow (COGS)</p>
                    <p class="text-2xl font-sans font-black text-white">-₹{{(stats()?.totalRevenue - stats()?.totalProfit) | number}}</p>
                    <div class="h-1 bg-white/5 rounded-full overflow-hidden"><div class="h-full bg-red-500 w-[60%] rounded-full"></div></div>
                </div>
                <div class="space-y-2">
                    <p class="text-[10px] font-black text-slate-500 uppercase tracking-widest opacity-60">Operations & Shrinkage</p>
                    <p class="text-2xl font-sans font-black text-white">-₹4,200</p>
                    <div class="h-1 bg-white/5 rounded-full overflow-hidden"><div class="h-full bg-slate-700 w-[5%] rounded-full"></div></div>
                </div>
                <div class="space-y-2 bg-white/[0.02] p-5 rounded-3xl border border-white/5 shadow-inner">
                    <p class="text-[10px] font-black text-solar-gold uppercase tracking-widest">Net Operating Velocity</p>
                    <p class="text-3xl font-sans font-black text-white">₹{{stats()?.totalProfit | number}}</p>
                    <p class="text-[10px] font-bold text-emerald-500 uppercase tracking-tighter">Healthy Cash Velocity</p>
                </div>
            </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    @reference "../../../../styles.css";
    .premium-table-header { @apply px-6 py-4 text-[10px] font-black uppercase tracking-widest text-slate-500; }
    .premium-table-cell { @apply px-6 py-4 text-sm; }
  `]
})
export class AdminAnalyticsComponent implements OnInit {
  private adminService = inject(AdminService);
  stats = signal<any>(null);

  ngOnInit() {
    this.adminService.getStats().subscribe((data: any) => this.stats.set(data));
  }

  getStatusClass(status: string) {
    status = status.toLowerCase();
    if (status.includes('pend')) return 'badge-pending';
    if (status.includes('ship') || status.includes('transit')) return 'badge-shipped';
    if (status.includes('deliv')) return 'badge-delivered';
    return '';
  }

  trackOrder(id: number) {
    alert(`Opening detailed supply chain map for Order #${id}... (Logic implemented in LogisticsRepository)`);
  }
}
