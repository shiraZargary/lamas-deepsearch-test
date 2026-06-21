import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { routes } from './app.routes';

describe('App', () => {
  beforeEach(async () => {
    localStorage.clear();
    document.documentElement.classList.remove('app-dark');
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter(routes)],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render the brand title', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.brand')?.textContent).toContain('Deep Search');
  });

  it('should toggle dark mode on and off', async () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    await fixture.whenStable();

    const initial = app['dark']();
    app.toggleTheme();
    expect(app['dark']()).toBe(!initial);
    expect(document.documentElement.classList.contains('app-dark')).toBe(!initial);

    app.toggleTheme();
    expect(app['dark']()).toBe(initial);
    expect(document.documentElement.classList.contains('app-dark')).toBe(initial);
  });
});
