import { ApplicationConfig, provideBrowserGlobalErrorListeners, APP_INITIALIZER } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http'; // 👈 Ensure this is here
import { AuthStore } from './core/state/auth.store';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(), // 👈 Required for your AuthService to work
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
