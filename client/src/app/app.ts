import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { ButtonModule } from 'primeng/button';

const DARK_CLASS = 'app-dark';
const STORAGE_KEY = 'lamas-theme';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ButtonModule],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly title = signal('הלשכה המרכזית לסטטיסטיקה');
  protected readonly dark = signal(this.initialDark());
  /** Falls back to the text brand if the logo asset is missing. */
  protected readonly logoError = signal(false);

  constructor() {
    this.applyTheme(this.dark());
  }

  /** Flip the theme: toggles `.app-dark` on <html>, which Aura's darkModeSelector targets. */
  toggleTheme(): void {
    const next = !this.dark();
    this.dark.set(next);
    this.applyTheme(next);
    localStorage.setItem(STORAGE_KEY, next ? 'dark' : 'light');
  }

  private initialDark(): boolean {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) return saved === 'dark';
    // Fall back to the OS preference on first visit.
    return window.matchMedia?.('(prefers-color-scheme: dark)').matches ?? false;
  }

  private applyTheme(dark: boolean): void {
    document.documentElement.classList.toggle(DARK_CLASS, dark);
  }
}
