import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StatsCardComponent } from '../../../component/stats-card/stats-card.component';
import { ActivityFeedComponent, Activity } from '../../../component/stats-card/../activity-feed/activity-feed.component';
import { AuthStore } from '../../../core/state/auth.store';
import { AdminService, AdminStats } from '../../../core/services/admin.service';

@Component({
  selector: 'app-admin-overview',
  standalone: true,
  imports: [CommonModule, StatsCardComponent, ActivityFeedComponent],
  template: `
    <div class="space-y-10 animate-fade-in-up">
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-6">
        <div>
          <h1 class="text-4xl font-black text-primary-green leading-tight italic">Welcome to the<br class="sm:hidden" /> Harvest, {{authStore.user()?.fullName || 'Admin'}}! 🌿</h1>
          <p class="text-text-gray mt-2 font-medium flex items-center gap-2">
            <span class="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span>
            Kerala Estate intelligence is synced. High-purity yields flowing.
          </p>
        </div>
        <div class="flex gap-3">
           <div class="flex flex-col items-end">
             <span class="text-[10px] font-black uppercase tracking-widest text-primary-green/40 mb-1">Estate Cycle</span>
             <span class="px-4 py-1.5 rounded-xl bg-primary-green text-white text-xs font-black border border-white/10 shadow-lg uppercase tracking-wider">FY 2026 Season</span>
           </div>
        </div>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6" *ngIf="stats()">
        <app-stats-card icon="shopping_bag" label="Total Orders" [value]="stats()?.totalOrders?.toString() || '0'" change="Syncing with estates"
            type="primary" [isPositive]="true" class="hover:scale-[1.02] transition-all duration-300 shadow-organic">
        </app-stats-card>

        <app-stats-card icon="trending_up" label="Gross Revenue" [value]="'₹' + (stats()?.totalRevenue | number)" change="Yield value delta"
            type="success" [isPositive]="true" class="hover:scale-[1.02] transition-all duration-300 shadow-organic">
        </app-stats-card>

        <app-stats-card icon="warning" label="Stock Alerts" [value]="stats()?.lowStockAlerts?.toString() || '0'" change="Requiring attention"
            type="warning" [isPositive]="false" class="hover:scale-[1.02] transition-all duration-300 shadow-organic">
        </app-stats-card>

        <app-stats-card icon="local_shipping" label="Pending Flow" [value]="stats()?.pendingDeliveries?.toString() || '0'" change="Logistics in transit"
            type="info" [isPositive]="true" class="hover:scale-[1.02] transition-all duration-300 shadow-organic">
        </app-stats-card>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
          <div class="lg:col-span-2 space-y-6">
              <div class="bg-white rounded-[2rem] p-8 border border-organic shadow-organic min-h-[400px] relative overflow-hidden group">
                  <div class="absolute inset-0 bg-gradient-to-br from-accent-honey/5 to-transparent"></div>
                  <div class="flex items-center justify-between mb-8 relative z-10">
                      <h3 class="text-xl font-black text-primary-green uppercase tracking-tight">Recent Harvest Orders</h3>
                      <button class="text-xs font-black text-accent-brown hover:text-primary-green transition-colors uppercase tracking-widest">View All</button>
                  </div>
                  
                  <div class="space-y-4 relative z-10">
                      <div *ngFor="let order of stats()?.recentOrders" class="flex items-center justify-between p-4 rounded-2xl hover:bg-bg-cream transition-colors border border-transparent hover:border-organic">
                          <div class="flex items-center gap-4">
                              <div class="w-10 h-10 rounded-xl bg-primary-green/5 text-primary-green flex items-center justify-center font-black">#{{order.orderId}}</div>
                              <div>
                                  <div class="font-black text-primary-green text-sm">{{order.customerName}}</div>
                                  <div class="text-[10px] text-text-gray font-medium">{{order.date | date:'MMM d, HH:mm'}}</div>
                              </div>
                          </div>
                          <div class="flex items-center gap-6">
                              <span class="px-3 py-1 rounded-full text-[9px] font-black uppercase tracking-widest" 
                                    [ngClass]="order.status === 'Delivered' ? 'bg-emerald-100 text-emerald-700' : 'bg-accent-tan/20 text-accent-brown'">
                                {{order.status}}
                              </span>
                              <span class="font-black text-sm text-primary-green">₹{{order.amount | number}}</span>
                          </div>
                      </div>
                      
                      <div *ngIf="!stats()?.recentOrders?.length" class="flex flex-col items-center justify-center h-48 opacity-50">
                          <div class="text-4xl mb-4">🍂</div>
                          <p class="font-medium text-primary-green">No recent orders found</p>
                      </div>
                  </div>
              </div>
          </div>
          <div class="lg:col-span-1">
             <app-activity-feed [activities]="activities" class="h-full shadow-organic rounded-[2rem] border border-organic overflow-hidden"></app-activity-feed>
          </div>
      </div>
    </div>
  `,
  styles: [`
    @reference "../../../../styles.css";
    :host ::ng-deep app-stats-card .card { @apply rounded-[2rem] border-organic shadow-none bg-white p-6; }
    :host ::ng-deep app-stats-card .icon-container { @apply w-12 h-12 rounded-2xl; }
  `]
})
export class AdminOverviewComponent implements OnInit {
  private adminService = inject(AdminService);
  public authStore = inject(AuthStore);

  stats = signal<AdminStats | null>(null);

  activities: Activity[] = [
    { type: 'create', text: '<strong>Bulk Yield</strong>: W320 Cashews (150kg) verified', time: '5m' },
    { type: 'edit', text: '<strong>Stock Update</strong>: Roasted Salted (+500)', time: '12m' },
    { type: 'user', text: '<strong>Quality Audit</strong>: Section B checked by Admin', time: '1h' },
    { type: 'delete', text: '<strong>Return Logged</strong>: Damaged seal on Item #99', time: '3h' },
  ];

  ngOnInit() {
    this.adminService.getStats().subscribe({
      next: (data) => this.stats.set(data),
      error: (err) => console.error('Estate sync failed:', err)
    });
  }
}
