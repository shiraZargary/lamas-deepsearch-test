import { Component } from '@angular/core';
import { QueryBuilderComponent } from '../components/query-builder/query-builder';
import { QuestionPreviewComponent } from '../components/question-preview/question-preview';
import { ResultsComponent } from '../components/results/results';

/** Structured query flow: build → preview → run → results. */
@Component({
  selector: 'app-builder-page',
  imports: [QueryBuilderComponent, QuestionPreviewComponent, ResultsComponent],
  template: `
    <div class="stack">
      <app-query-builder />
      <app-question-preview />
      <app-results />
    </div>
  `,
  styles: [`.stack { display: flex; flex-direction: column; gap: 1.25rem; }`],
})
export class BuilderPageComponent {}
