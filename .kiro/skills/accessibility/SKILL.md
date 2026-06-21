---
name: accessibility
description: Web accessibility (a11y) best practices for this Angular 21 + PrimeNG app, covering WCAG 2.2 AA, semantic HTML, ARIA, keyboard navigation, focus management, RTL/Hebrew direction, color contrast, and accessible forms. Use when building or reviewing any UI under client/ — components, templates, forms, dialogs, tables, or styles.
---

# Accessibility (a11y)

Practices for building accessible UI in the `client/` Angular 21 app. Target **WCAG 2.2 level AA**. The UI is **RTL / Hebrew**, so direction and bidi handling matter throughout.

## Guiding rules

1. **Semantic HTML first.** Use the right element (`<button>`, `<nav>`, `<main>`, `<table>`, `<label>`) before reaching for ARIA. ARIA supplements semantics; it never replaces them.
2. **Everything reachable by keyboard.** Every interactive control must be focusable and operable with Tab / Shift+Tab / Enter / Space / arrow keys. No `(click)` on a `<div>` — use `<button>` (or `<p-button>`).
3. **Visible focus.** Never remove focus outlines without a clearly visible replacement.
4. **Name, role, value.** Every control exposes an accessible name (visible label, `aria-label`, or `aria-labelledby`).
5. **Don't rely on color alone** to convey meaning (e.g. errors need text/icon too).

## RTL / Hebrew

- The document/root should set `dir="rtl"` and `lang="he"` (set `lang` per-language if mixing English/Hebrew).
- Use CSS **logical properties** (`margin-inline-start`, `padding-inline-end`, `inset-inline-start`, `text-align: start`) instead of physical `left`/`right` so layout mirrors correctly.
- For mixed LTR content inside RTL (numbers, codes, English terms) use `dir="auto"` or `<bdi>` to prevent bidi reordering bugs.
- Keyboard arrow semantics flip in RTL — rely on PrimeNG's built-in RTL support rather than hardcoding left/right key handling.

## Semantic structure

- One `<h1>` per page; headings in order (don't skip levels) to form a logical outline.
- Landmarks: `<header>`, `<nav>`, `<main>`, `<footer>`. One `<main>` per page.
- Provide a "skip to content" link as the first focusable element on the page.
- Lists use `<ul>`/`<ol>`/`<li>`; tabular data uses `<table>` with `<th scope="col|row">` and a `<caption>`.

## Forms (critical for this app's query builder & search)

```html
<label for="q">חיפוש חופשי</label>
<input id="q" type="search" [attr.aria-invalid]="error() ? 'true' : null"
       aria-describedby="q-help q-err" />
<small id="q-help">הקלד שאלה בשפה חופשית</small>
@if (error()) {
  <small id="q-err" role="alert">{{ error() }}</small>
}
```

- Every input has an associated `<label for>` (or `aria-label` if visually label-less, e.g. an icon search box).
- Group related controls with `<fieldset>` + `<legend>`.
- Mark required fields with the `required` attribute (not just an asterisk).
- Associate hints and errors via `aria-describedby`; set `aria-invalid` on invalid fields.
- Announce validation errors with `role="alert"` (or an `aria-live` region) so screen readers read them.

## PrimeNG components

- PrimeNG ships ARIA support and keyboard interaction; prefer its components over custom widgets.
- Still **provide accessible names**: set `ariaLabel` / `[ariaLabelledBy]` on icon-only `<p-button>`, paginators, etc.
- `<p-table>`: provide a caption or `aria-label`, sortable headers expose sort state automatically — keep `<th>` semantics.
- `<p-dialog>` is modal: it traps focus and restores it on close. Give it `[header]` or `ariaLabelledBy`, and ensure it returns focus to the trigger.
- Override PrimeNG's default `ariaLabel` strings with Hebrew where the visible UI is Hebrew (e.g. close button, pagination labels) via `providePrimeNG({ translation: { ... } })`.

## Keyboard & focus management (Angular)

- Use the Angular CDK a11y utilities when you need custom widgets: `FocusTrap`, `LiveAnnouncer`, `FocusMonitor` from `@angular/cdk/a11y`.
- After async actions (search results loaded, error shown), announce via `LiveAnnouncer` or an `aria-live="polite"` region so non-visual users get feedback. This app sets `loading`/`error`/`result` signals — surface them in a live region.
- On route change, move focus to the page heading or main landmark.
- For dynamically inserted dialogs/menus, trap focus while open and restore it on close (PrimeNG handles this for its own overlays).

```ts
import { inject } from '@angular/core';
import { LiveAnnouncer } from '@angular/cdk/a11y';

private readonly announcer = inject(LiveAnnouncer);
// after results arrive:
this.announcer.announce(`נמצאו ${count} תוצאות`, 'polite');
```

## Live regions for async state

```html
<div aria-live="polite" class="sr-only">
  @if (state.loading()) { טוען… }
  @else if (state.error()) { {{ state.error() }} }
  @else if (state.hasResult()) { התקבלו תוצאות }
</div>
```

Provide a `.sr-only` (visually-hidden but screen-reader-available) utility in `styles.scss`:

```scss
.sr-only {
  position: absolute;
  width: 1px; height: 1px;
  padding: 0; margin: -1px;
  overflow: hidden; clip: rect(0, 0, 0, 0);
  white-space: nowrap; border: 0;
}
```

## Color & contrast

- Text contrast ≥ 4.5:1 (≥ 3:1 for large text ≥ 24px or 19px bold).
- Non-text UI (icons, input borders, focus rings) ≥ 3:1 against adjacent colors.
- Verify both the Aura light theme and `.app-dark` dark mode.
- Don't convey state by color only — pair with text/icon (e.g. error message, not just red border).

## Images & media

- Informative images need meaningful `alt`; decorative images use `alt=""` (empty).
- Icon-only buttons need an accessible name even though the icon is decorative.
- Charts (chart.js is in this project): provide a text alternative — a caption, summary, or an accessible data table — since canvas is opaque to assistive tech.

## Reduced motion

Respect user preferences for animations (PrimeNG animations included):

```scss
@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after {
    animation-duration: 0.01ms !important;
    transition-duration: 0.01ms !important;
  }
}
```

## Verification checklist

- [ ] Navigate the whole flow with keyboard only — focus order is logical, focus is always visible.
- [ ] Every control has a visible label or accessible name.
- [ ] Forms: labels, `aria-describedby` hints, `role="alert"` errors, `aria-invalid`.
- [ ] Async state changes announced via live region / `LiveAnnouncer`.
- [ ] `dir="rtl"`/`lang` set; layout uses logical CSS properties; bidi content handled.
- [ ] Contrast passes in both light and dark themes.
- [ ] Charts have a text/table alternative.
- [ ] Test with a screen reader (NVDA on Windows, VoiceOver on macOS) and check with axe DevTools / Lighthouse.

## Anti-patterns to avoid

- ❌ `(click)` on non-interactive elements (`<div>`, `<span>`, `<i>`).
- ❌ Removing `:focus` outlines with no replacement.
- ❌ `aria-label` on elements that already have a visible label (creates duplication/conflict).
- ❌ Positive `tabindex` values (> 0) — they break natural focus order.
- ❌ Placeholder text used as the only label.
- ❌ Physical `left`/`right` CSS that breaks RTL mirroring.
- ❌ Color-only status indicators.
