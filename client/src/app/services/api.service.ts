import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  ExecuteResponse,
  Metadata,
  FreeSearchResponse,
  QueryDefinition,
  SavedQuery,
} from '../models/query.models';

/** Typed HTTP client for the Deep Search backend. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  getMetadata(): Observable<Metadata> {
    return this.http.get<Metadata>(`${this.base}/metadata`);
  }

  execute(definition: QueryDefinition): Observable<ExecuteResponse> {
    return this.http.post<ExecuteResponse>(`${this.base}/queries/execute`, definition);
  }

  phrase(definition: QueryDefinition): Observable<{ question: string }> {
    return this.http.post<{ question: string }>(`${this.base}/queries/phrase`, definition);
  }

  saveQuery(name: string, definition: QueryDefinition): Observable<SavedQuery> {
    return this.http.post<SavedQuery>(`${this.base}/queries/saved`, { name, definition });
  }

  getSavedQueries(): Observable<SavedQuery[]> {
    return this.http.get<SavedQuery[]>(`${this.base}/queries/saved`);
  }

  runSavedQuery(id: string): Observable<ExecuteResponse> {
    return this.http.post<ExecuteResponse>(`${this.base}/queries/saved/${id}/run`, {});
  }

  freeSearch(text: string): Observable<FreeSearchResponse> {
    return this.http.post<FreeSearchResponse>(`${this.base}/free-search`, { text });
  }
}
