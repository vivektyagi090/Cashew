import { ApplicationConfig, provideBrowserGlobalErrorListeners, APP_INITIALIZER, provideZoneChangeDetection } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http'; // 👈 Ensure this is here
import { AuthStore } from './core/state/auth.store';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(),
    provideAnimationsAsync(),
    {
      provide: APP_INITIALIZER,
      useFactory: (store: InstanceType<typeof AuthStore>) => () => {
        // This runs before the app starts
        store.autoLogin();
        return Promise.resolve();
      },
      deps: [AuthStore],
      multi: true
    }
  ]
};
