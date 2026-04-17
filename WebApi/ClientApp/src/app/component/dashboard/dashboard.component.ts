import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { AuthStore } from '../../core/state/auth.store';
import { Router, RouterOutlet } from '@angular/router';
import { SidebarComponent, MenuItem } from '../sidebar/sidebar.component';
import { ChatbotComponent } from '../chatbot/chatbot.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    SidebarComponent,
    ChatbotComponent,
    RouterOutlet
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent {
  public authStore = inject(AuthStore);
  private router = inject(Router);

  // Sidebar state
  sidebarOpen = signal(true);
  mobileMenuOpen = signal(false);

  // Menu items calculated based on role
  menuItems: MenuItem[] = this.getMenuItems();

  private getMenuItems(): MenuItem[] {
    const userRole = this.authStore.user()?.role;
    const isAdmin = userRole === 'Admin';
    
    if (isAdmin) {
      return [
        { icon: 'dashboard', label: 'Overview', route: '/dashboard' },
        { icon: 'inventory_2', label: 'Stock Master', route: '/dashboard/inventory' },
        { icon: 'tune', label: 'Master Records', route: '/dashboard/master' },
        { icon: 'analytics', label: 'Business Insights', route: '/dashboard/analytics' },
        { icon: 'local_shipping', label: 'Supply Chain', route: '/dashboard/logistics' },
        { icon: 'assignment_returned', label: 'Returns & Losses', route: '/dashboard/returns' },
        { icon: 'settings', label: 'Settings', route: '/dashboard/settings' },
      ];
    }
    return [
      { icon: 'dashboard', label: 'My Dashboard', route: '/dashboard' },
      { icon: 'shopping_bag', label: 'Recent Orders', route: '/orders' },
      { icon: 'settings', label: 'Settings', route: '/settings' },
    ];
  }

  toggleSidebar() {
    this.sidebarOpen.update(val => !val);
  }

  toggleMobileMenu() {
    this.mobileMenuOpen.update(val => !val);
  }

  closeMobileMenu() {
    this.mobileMenuOpen.set(false);
  }

  logout() {
    this.authStore.logout();
    this.router.navigateByUrl('/login');
  }
}
