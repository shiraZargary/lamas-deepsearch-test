---
name: modern-angular
description: Conventions and best practices for modern Angular (v17–v21) in this repo — standalone components, signals, the inject() function, the new @if/@for/@switch control flow, the @angular/build system, PrimeNG/Aura, and vitest. Use when creating or editing any Angular code under client/, including components, services, routing, state, templates, styles, or tests.
---

# Modern Angular (v17–v21)

Guidance for writing Angular code in the `client/` app. This project runs **Angular 21** with the `@angular/build` application builder, standalone APIs, signals, PrimeNG 21 (Aura theme), SCSS, and **vitest** for unit tests. The UI is Hebrew / RTL.

## Core principles

1. **Standalone by default.** No `NgModule`. Components, directives, and pipes are standalone and declare their own `imports`.
2. **Signals for state.** Use `signal()`, `computed()`, and `effect()` instead of mutable fields or `BehaviorSubject` for view state. Keep RxJS for async streams (HTTP, events).
3. **`inject()` over constructor injection.** Use the `inject()` function for DI in services and components.
4. **New control flow in templates.** Use `@if`, `@for`, `@switch` — never `*ngIf`, `*ngFor`, `*ngSwitch`.
5. **Provider functions, not modules.** Configure the app with `provide*` functions in `app.config.ts`.

## File & naming conventions (match this repo)

- App-shell files drop the type suffix: `app.ts`, `app.html`, `app.scss`, `app.config.ts`, `app.routes.ts`.
- Services keep the suffix: `*.service.ts` (e.g. `api.service.ts`, `query-state.service.ts`).
- Models live in `*.models.ts` under `src/app/models/`.
- Components live under `src/app/components/<feature>/` and pages under `src/app/pages/`.
- Use `templateUrl` / `styleUrl` (singular) — not `styleUrls`.
- Component selector prefix is `app-`. Styles are SCSS.

## Component pattern

```ts
import { Component, signal, computed, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-example',
  imports: [ButtonModule],
  templateUrl: './example.html',
  styleUrl: './example.scss',
})
export class Example {
  private readonly state = inject(QueryStateService);

  // local UI state as signals
  protected readonly open = signal(false);
  protected readonly canSubmit = computed(() => !this.state.loading());

  toggle(): void {
    this.open.update((v) => !v);
  }
}
```

Notes:
- Prefer `readonly` signals exposed from services; components read them directly in templates (`state.loading()`).
- Mark view-only members `protected readonly` when only the template needs them.
- Use `input()` / `output()` signal APIs for component I/O (not `@Input()`/`@Output()` decorators) on new code.
- Use `model()` for two-way bindable signals when needed.

## Service & state pattern

Hold shared UI state in a `providedIn: 'root'` service using signals; components call action methods that perform the API call and update the signals (see `query-state.service.ts`).

```ts
@Injectable({ providedIn: 'root' })
export class ExampleService {
  private readonly api = inject(ApiService);

  readonly items = signal<Item[]>([]);
  readonly loading = signal(false);
  readonly hasItems = computed(() => this.items().length > 0);

  load(): void {
    this.loading.set(true);
    this.api.getItems().subscribe({
      next: (list) => { this.items.set(list); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}
```

## Templates (new control flow)

```html
@if (state.loading()) {
  <p-progressSpinner />
} @else if (state.hasResult()) {
  <app-results [data]="state.result()" />
} @else {
  <p>אין תוצאות</p>
}

@for (row of state.result()?.rows ?? []; track row.id) {
  <tr>{{ row.label }}</tr>
} @empty {
  <tr><td>אין נתונים</td></tr>
}
```

- `@for` **requires** a `track` expression.
- Keep RTL/Hebrew strings inline as they already are; don't introduce i18n machinery unless asked.

## App bootstrap & providers

Configure in `app.config.ts` with provider functions:

```ts
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withFetch()),
    provideAnimationsAsync(),
    providePrimeNG({ theme: { preset: Aura, options: { darkModeSelector: '.app-dark' } } }),
  ],
};
```

- HTTP: always `provideHttpClient(withFetch())`. Use `HttpClient` via `inject(HttpClient)`.
- Routing: lazy-load routes with `loadComponent: () => import('...').then(m => m.X)` where practical.

## PrimeNG

- Import only the specific PrimeNG modules a component uses (e.g. `ButtonModule`, `ToolbarModule`) in that component's `imports`.
- Theme is Aura via `@primeng/themes/aura`; dark mode toggled by the `.app-dark` selector.
- Use PrimeNG components/tags (e.g. `<p-button>`, `<p-table>`) rather than re-implementing UI.

## Testing (vitest)

Tests use `@angular/build:unit-test` (vitest) with `TestBed`. Pattern from `app.spec.ts`:

```ts
import { TestBed } from '@angular/core/testing';
import { Example } from './example';

describe('Example', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Example] }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(Example);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
```

- Standalone components go in `imports`, not `declarations`.
- Use `await fixture.whenStable()` before asserting on rendered DOM (zoneless-friendly).
- Run with `npm test` (`ng test`).

## Build & dev commands (run in `client/`)

- `npm start` — dev server (`ng serve`).
- `npm run build` — production build (`@angular/build:application`).
- `npm test` — unit tests (vitest).
- Production budgets: initial ≤ 3MB error / 2MB warning; component styles ≤ 8kB error / 4kB warning. Keep bundles lean.

## Anti-patterns to avoid

- ❌ `NgModule`, `*ngIf`/`*ngFor`, `styleUrls`, constructor DI on new code.
- ❌ `@Input()`/`@Output()` decorators on new components (prefer `input()`/`output()`).
- ❌ Manual subscription state when a `signal`/`computed` fits.
- ❌ Importing the whole of PrimeNG; import only what's used.
- ❌ Storing derived values in fields — use `computed()`.
