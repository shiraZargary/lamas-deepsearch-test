import { Component, OnInit, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { SelectModule } from 'primeng/select';
import { MultiSelectModule } from 'primeng/multiselect';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { QueryStateService } from '../../services/query-state.service';
import {
  BreakdownDimension,
  MetricType,
  PeriodKind,
  QueryDefinition,
} from '../../models/query.models';

@Component({
  selector: 'app-query-builder',
  imports: [
    FormsModule, CardModule, SelectModule, MultiSelectModule, ButtonModule, InputTextModule,
  ],
  templateUrl: './query-builder.html',
  styles: [`
    .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 1rem; }
    .field { display: flex; flex-direction: column; gap: .35rem; }
    .field label { font-weight: 600; font-size: .9rem; }
    .actions { display: flex; gap: .75rem; align-items: end; flex-wrap: wrap; margin-top: 1rem; }
    .save-box { display: flex; gap: .5rem; align-items: end; margin-inline-start: auto; }
    :host ::ng-deep .p-select, :host ::ng-deep .p-multiselect { width: 100%; }
  `],
})
export class QueryBuilderComponent implements OnInit {
  protected readonly state = inject(QueryStateService);

  // local form model
  protected metricCode = 'avg_income';
  protected gender: string | null = null;
  protected city: string | null = null;
  protected sector: string | null = null;
  protected ageGroup: string | null = null;
  protected periodKind: PeriodKind = 'Range';
  protected fromYear = 2021;
  protected toYear = 2024;
  protected breakdowns: BreakdownDimension[] = ['Year'];
  protected saveName = signal('');

  protected readonly periodKinds = [
    { label: 'טווח שנים', value: 'Range' as PeriodKind },
    { label: 'שנה בודדת', value: 'SingleYear' as PeriodKind },
  ];

  protected readonly breakdownOptions = [
    { label: 'שנה', value: 'Year' as BreakdownDimension },
    { label: 'מגדר', value: 'Gender' as BreakdownDimension },
    { label: 'עיר', value: 'City' as BreakdownDimension },
    { label: 'קבוצת גיל', value: 'AgeGroup' as BreakdownDimension },
    { label: 'מגזר', value: 'Sector' as BreakdownDimension },
  ];

  constructor() {
    // Reflect external changes (NL parse / saved query) back into the form.
    effect(() => this.syncFromDefinition(this.state.definition()));
  }

  ngOnInit(): void {
    this.state.loadMetadata();
    this.state.refreshPhrase();
  }

  protected dimValues(code: string): string[] {
    return this.state.metadata()?.dimensions.find((d) => d.code === code)?.values ?? [];
  }

  protected get yearOptions(): number[] {
    const years = this.dimValues('year').map(Number);
    return years.length ? years : [2020, 2021, 2022, 2023, 2024];
  }

  /** Builds a QueryDefinition from the form and pushes it to shared state. */
  protected onChange(): void {
    const metric = this.state.metadata()?.metrics.find((m) => m.code === this.metricCode);
    const def: QueryDefinition = {
      population: {
        gender: this.gender,
        city: this.city,
        sector: this.sector,
        ageGroup: this.ageGroup,
      },
      metric: {
        type: (metric?.type ?? 'Average') as MetricType,
        field: metric?.field ?? null,
      },
      period: {
        kind: this.periodKind,
        fromYear: this.fromYear,
        toYear: this.periodKind === 'SingleYear' ? this.fromYear : this.toYear,
      },
      breakdowns: this.breakdowns,
    };
    this.state.setDefinition(def);
  }

  protected execute(): void {
    this.onChange();
    this.state.execute();
  }

  protected save(): void {
    const name = this.saveName().trim();
    if (name) {
      this.state.save(name);
      this.saveName.set('');
    }
  }

  private syncFromDefinition(def: QueryDefinition): void {
    this.gender = def.population.gender ?? null;
    this.city = def.population.city ?? null;
    this.sector = def.population.sector ?? null;
    this.ageGroup = def.population.ageGroup ?? null;
    this.periodKind = def.period.kind;
    this.fromYear = def.period.fromYear;
    this.toYear = def.period.toYear;
    this.breakdowns = [...def.breakdowns];
    const metric = this.state.metadata()?.metrics.find(
      (m) => m.type === def.metric.type && (m.field ?? null) === (def.metric.field ?? null));
    if (metric) this.metricCode = metric.code;
  }
}
