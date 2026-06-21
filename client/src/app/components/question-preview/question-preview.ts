import { Component, inject } from '@angular/core';
import { CardModule } from 'primeng/card';
import { QueryStateService } from '../../services/query-state.service';

/** Shows the live, human-readable Hebrew phrasing of the current definition (Req #2). */
@Component({
  selector: 'app-question-preview',
  imports: [CardModule],
  template: `
    <p-card>
      <div class="preview">
        <i class="pi pi-comment"></i>
        <span class="text">{{ state.question() || 'בנה שאילתה כדי לראות את הניסוח...' }}</span>
      </div>
    </p-card>
  `,
  styles: [`
    .preview { display: flex; align-items: center; gap: .5rem; font-size: 1.05rem; }
    .text { font-weight: 600; }
    .pi { color: var(--p-primary-color); }
  `],
})
export class QuestionPreviewComponent {
  protected readonly state = inject(QueryStateService);
}
