import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { tap } from 'rxjs';
interface AuthState {
    token: string | null;
    isAuthenticated: boolean;
    user: any | null;
    loading: boolean;
    error: string | null;
}

const initialState: AuthState = {
    token: null,
    isAuthenticated: false,
    user: null,
    loading: false,
    error: null
};
export const AuthStore = signalStore(
    { providedIn: 'root' },

    withState(initialState),

    withMethods((store: any) => {
        const authService = inject(AuthService);

        return {
            login(credentials: any) {
                debugger;
                patchState(store, { loading: true, error: null });

                authService.login(credentials).pipe(
                    tap({
                        next: (res: any) => {
                            if (res.success) {
                                localStorage.setItem('token', res.token);
                                localStorage.setItem('user', JSON.stringify(res.user));
                                patchState(store, {
                                    token: res.token,
                                    user: res.user,
                                    isAuthenticated: true,
                                    loading: false
                                });
                            } else {
                                patchState(store, { error: res.message, loading: false });
                            }
                        },
                        error: () => {
                            patchState(store, {
                                error: 'Invalid email or password',
                                loading: false
                            });
                        }
                    })
                ).subscribe();
            },

            logout() {
                localStorage.clear();
                patchState(store, initialState);
            },

            autoLogin() {
                const token = localStorage.getItem('token');
                const userJson = localStorage.getItem('user');
                if (token) {
                    patchState(store, {
                        token,
                        user: userJson ? JSON.parse(userJson) : null,
                        isAuthenticated: true
                    });
                }
            }
        };
    })
);