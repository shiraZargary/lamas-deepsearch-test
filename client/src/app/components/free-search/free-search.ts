import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { QueryStateService } from '../../services/query-state.service';
import { QuestionPreviewComponent } from '../question-preview/question-preview';
import { ResultsComponent } from '../results/results';

/** Free-text querying: interpret → show interpretation → run (Req #5). */
@Component({
  selector: 'app-free-search',
  imports: [
    FormsModule, CardModule, ButtonModule, TextareaModule,
    QuestionPreviewComponent, ResultsComponent,
  ],
  template: `
    <p-card header="חיפוש חופשי">
      <div class="free-search">
        <textarea pTextarea [(ngModel)]="text" rows="2"
                  placeholder="לדוגמה: הצג את השכר הממוצע של נשים בירושלים בשנים 2021-2024 לפי שנה"></textarea>
        <div class="row">
          <p-button label="נתח את השאלה" icon="pi pi-sparkles" (onClick)="search()" [loading]="state.loading()" />
          @if (state.notes().length && state.recognized()) {
            <p-button label="הרץ" icon="pi pi-play" severity="secondary" (onClick)="run()" />
          }
          @if (state.hasResult()) {
            <p-button label="שאלה חדשה" icon="pi pi-refresh" severity="secondary" [text]="true" (onClick)="reset()" />
          }
        </div>

        @if (state.notes().length) {
          @if (state.recognized()) {
            <div class="interpretation">
              <strong>כך הבנתי את השאלה:</strong>
              <ul>
                @for (note of state.notes(); track note) { <li>{{ note }}</li> }
              </ul>
            </div>
          } @else {
            <div class="not-understood">
              <i class="pi pi-exclamation-triangle"></i>
              <div>
                @for (note of state.notes(); track note) { <p>{{ note }}</p> }
              </div>
            </div>
          }
        }

        @if (state.error()) {
          <div style="color: var(--p-red-500);">{{ state.error() }}</div>
        }
      </div>
    </p-card>

    @if (state.notes().length && state.recognized()) {
      <div style="margin-top: 1rem;"><app-question-preview /></div>
    }
    <div style="margin-top: 1rem;"><app-results /></div>
  `,
  styles: [`
    .free-search { display: flex; flex-direction: column; gap: 1rem; }
    .row { display: flex; gap: .75rem; }
    textarea {
      width: 100%;
      box-sizing: border-box;
      min-height: 90px;
      direction: rtl;
      text-align: right;
    }
    .interpretation { text-align: right; }
    .interpretation ul { margin: .25rem 0; padding-inline-start: 1.25rem; }
    .not-understood {
      display: flex;
      align-items: flex-start;
      gap: .6rem;
      text-align: right;
      padding: .85rem 1rem;
      border-radius: 10px;
      color: var(--p-yellow-700, #8a6d00);
      background: var(--p-yellow-50, #fff8e1);
      border: 1px solid var(--p-yellow-200, #ffe082);
    }
    .not-understood i { font-size: 1.2rem; margin-top: .1rem; }
    .not-understood p { margin: 0; }
  `],
})
export class FreeSearchComponent {
  protected readonly state = inject(QueryStateService);
  protected text = '';

  protected search(): void {
    const t = this.text.trim();
    if (t) this.state.freeSearch(t);
  }

  protected run(): void {
    this.state.execute();
  }

  protected reset(): void {
    this.text = '';
    this.state.reset();
  }
}
