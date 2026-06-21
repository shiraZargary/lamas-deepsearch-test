import { Injectable, computed, inject, signal } from '@angular/core';
import { ApiService } from './api.service';
import {
  ExecuteResponse,
  Metadata,
  QueryDefinition,
  QueryResult,
  SavedQuery,
  emptyDefinition,
} from '../models/query.models';

/**
 * Central UI state held in signals. Components read the signals and call the
 * action methods; the service performs the API calls and updates state.
 */
@Injectable({ providedIn: 'root' })
export class QueryStateService {
  private readonly api = inject(ApiService);

  // --- state ---
  readonly metadata = signal<Metadata | null>(null);
  readonly definition = signal<QueryDefinition>(emptyDefinition());
  readonly question = signal<string>('');
  readonly result = signal<QueryResult | null>(null);
  readonly notes = signal<string[]>([]);
  readonly recognized = signal<boolean>(true);
  readonly saved = signal<SavedQuery[]>([]);
  readonly loading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  readonly hasResult = computed(() => (this.result()?.rows.length ?? 0) > 0);

  // --- actions ---
  loadMetadata(): void {
    this.api.getMetadata().subscribe({
      next: (m) => this.metadata.set(m),
      error: () => this.error.set('טעינת המטא-דאטה נכשלה'),
    });
  }

  setDefinition(def: QueryDefinition): void {
    this.definition.set(def);
    this.refreshPhrase();
  }

  patchDefinition(patch: Partial<QueryDefinition>): void {
    this.setDefinition({ ...this.definition(), ...patch });
  }

  refreshPhrase(): void {
    this.api.phrase(this.definition()).subscribe({
      next: (r) => this.question.set(r.question),
      error: () => {},
    });
  }

  execute(): void {
    this.runRequest(this.api.execute(this.definition()));
  }

  save(name: string): void {
    this.loading.set(true);
    this.api.saveQuery(name, this.definition()).subscribe({
      next: () => {
        this.loading.set(false);
        this.loadSaved();
      },
      error: () => this.fail('שמירת השאילתה נכשלה'),
    });
  }

  loadSaved(): void {
    this.api.getSavedQueries().subscribe({
      next: (list) => this.saved.set(list),
      error: () => this.error.set('טעינת השאילתות השמורות נכשלה'),
    });
  }

  runSaved(item: SavedQuery): void {
    this.definition.set(item.definition);
    this.runRequest(this.api.runSavedQuery(item.id));
  }

  freeSearch(text: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.freeSearch(text).subscribe({
      next: (r) => {
        this.definition.set(r.definition);
        this.question.set(r.question);
        this.notes.set(r.notes);
        this.recognized.set(r.recognized ?? true);
        this.result.set(null);
        this.loading.set(false);
      },
      error: () => this.fail('פירוש השאלה נכשל'),
    });
  }

  /** Clear the current question, interpretation and results to start fresh. */
  reset(): void {
    this.definition.set(emptyDefinition());
    this.question.set('');
    this.notes.set([]);
    this.recognized.set(true);
    this.result.set(null);
    this.error.set(null);
    this.loading.set(false);
  }

  private runRequest(obs: ReturnType<ApiService['execute']>): void {
    this.loading.set(true);
    this.error.set(null);
    obs.subscribe({
      next: (r: ExecuteResponse) => {
        this.question.set(r.question);
        this.result.set(r.result);
        this.loading.set(false);
      },
      error: () => this.fail('הרצת השאילתה נכשלה'),
    });
  }

  private fail(message: string): void {
    this.error.set(message);
    this.loading.set(false);
  }
}
