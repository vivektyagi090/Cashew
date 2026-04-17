import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthStore } from '../state/auth.store';

export const authGuard: CanActivateFn = () => {
    const authStore = inject(AuthStore) as InstanceType<typeof AuthStore>;
    const router = inject(Router);

    if (authStore.isAuthenticated()) {
        return true;
    } else {
        // Redirect to login if not authenticated
        return router.parseUrl('/login');
        // router.navigate(['/login']);
        // return false;
    }
};