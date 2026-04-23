import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: 'login',    loadComponent: () => import('./auth/login/login').then(m => m.Login) },
  { path: 'register', loadComponent: () => import('./auth/register/register').then(m => m.Register) },
  { path: 'home',     loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent) },
  { path: '',         loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent) },
  { path: 'products', loadComponent: () => import('./pages/products/products.component').then(m => m.ProductsComponent) },
  {
    path: 'cart',
    loadComponent: () => import('./pages/cart/cart.component').then(m => m.CartComponent),
    canActivate: [authGuard]
  },
  {
    path: 'checkout',
    loadComponent: () => import('./pages/checkout/checkout.component').then(m => m.CheckoutComponent),
    canActivate: [authGuard]
  },
  {
    path: 'orders',
    loadComponent: () => import('./pages/orders/orders.component').then(m => m.OrdersComponent),
    canActivate: [authGuard]
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./component/dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [authGuard],
    children: [
      { 
        path: '', 
        loadComponent: () => import('./pages/admin/overview/overview.component').then(m => m.AdminOverviewComponent) 
      },
      { 
        path: 'inventory', 
        loadComponent: () => import('./pages/admin/inventory-master/inventory-master.component').then(m => m.InventoryMasterComponent) 
      },
      { 
        path: 'analytics', 
        loadComponent: () => import('./pages/admin/analytics/analytics.component').then(m => m.AdminAnalyticsComponent) 
      },
      { 
        path: 'logistics', 
        loadComponent: () => import('./pages/admin/supply-chain/supply-chain.component').then(m => m.SupplyChainComponent) 
      },
      { 
        path: 'returns', 
        loadComponent: () => import('./pages/admin/returns-damages/returns-damages.component').then(m => m.ReturnsDamagesComponent) 
      },
      { 
        path: 'master', 
        loadComponent: () => import('./pages/admin/master-records/master-records.component').then(m => m.MasterRecordsComponent) 
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
