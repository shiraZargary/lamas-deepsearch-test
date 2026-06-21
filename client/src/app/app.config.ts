import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import { definePreset } from '@primeng/themes';

import { routes } from './app.routes';

/**
 * Aura with the Israeli government blue palette (gov.il).
 * Anchored on the flag/government blue #0038B8 at 500, deepening to navy.
 */
const GovBlue = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#eaf0fb',
      100: '#cbd9f5',
      200: '#a3bdee',
      300: '#7098e4',
      400: '#3a6cd9',
      500: '#0038b8',
      600: '#002f9c',
      700: '#00257d',
      800: '#001d63',
      900: '#00164d',
      950: '#000f33',
    },
  },
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withFetch()),
    provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: GovBlue,
        options: {
          darkModeSelector: '.app-dark'
        }
      }
    })
  ]
};
