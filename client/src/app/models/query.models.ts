// Mirrors the backend contract (DeepSearch.Domain.Queries + Application DTOs).

export type MetricType = 'Average' | 'Count' | 'Sum';
export type PeriodKind = 'SingleYear' | 'Range';
export type BreakdownDimension = 'Year' | 'Gender' | 'City' | 'AgeGroup' | 'Sector';

export interface Population {
  gender?: string | null;
  ageGroup?: string | null;
  city?: string | null;
  sector?: string | null;
}

export interface Metric {
  type: MetricType;
  field?: string | null;
}

export interface Period {
  kind: PeriodKind;
  fromYear: number;
  toYear: number;
}

export interface QueryDefinition {
  population: Population;
  metric: Metric;
  period: Period;
  breakdowns: BreakdownDimension[];
}

export interface MetricInfo {
  code: string;
  type: MetricType;
  field?: string | null;
  label: string;
}

export interface DimensionInfo {
  code: string;
  label: string;
  values: string[];
}

export interface Metadata {
  metrics: MetricInfo[];
  dimensions: DimensionInfo[];
}

export interface QueryResult {
  columns: string[];
  rows: Array<Record<string, unknown>>;
}

export interface ExecuteResponse {
  question: string;
  result: QueryResult;
}

export interface SavedQuery {
  id: string;
  name: string;
  definition: QueryDefinition;
  createdAt: string;
}

export interface FreeSearchResponse {
  definition: QueryDefinition;
  question: string;
  notes: string[];
  recognized: boolean;
}

/** A sensible empty definition for initialising the builder. */
export function emptyDefinition(): QueryDefinition {
  return {
    population: {},
    metric: { type: 'Average', field: 'income' },
    period: { kind: 'Range', fromYear: 2021, toYear: 2024 },
    breakdowns: ['Year'],
  };
}
