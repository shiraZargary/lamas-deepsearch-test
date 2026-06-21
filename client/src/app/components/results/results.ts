import { Component, computed, inject } from '@angular/core';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { ChartModule } from 'primeng/chart';
import { QueryStateService } from '../../services/query-state.service';

/** Renders the query result as a PrimeNG table and a bar chart (Req #3). */
@Component({
  selector: 'app-results',
  imports: [CardModule, TableModule, ChartModule],
  template: `
    @if (state.result(); as result) {
      <p-card>
        <div class="results">
          <p-table [value]="result.rows" [columns]="result.columns" [paginator]="result.rows.length > 10"
                   [rows]="10" styleClass="p-datatable-sm">
            <ng-template #header let-columns>
              <tr>
                @for (col of columns; track col) {
                  <th>{{ label(col) }}</th>
                }
              </tr>
            </ng-template>
            <ng-template #body let-row let-columns="columns">
              <tr>
                @for (col of columns; track col) {
                  <td>{{ row[col] }}</td>
                }
              </tr>
            </ng-template>
          </p-table>

          <p-chart type="bar" [data]="chartData()" [options]="chartOptions" />
        </div>
      </p-card>
    }
  `,
  styles: [`
    .results { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; align-items: start; }
    @media (max-width: 900px) { .results { grid-template-columns: 1fr; } }
  `],
})
export class ResultsComponent {
  protected readonly state = inject(QueryStateService);

  protected readonly chartOptions = {
    responsive: true,
    plugins: { legend: { display: false } },
  };

  /** Builds a single-series bar chart: x = first group column (or "סה""כ"), y = value. */
  protected readonly chartData = computed(() => {
    const result = this.state.result();
    if (!result) return { labels: [], datasets: [] };

    const valueCol = 'value';
    const labelCol = result.columns.find((c) => c !== valueCol);

    const labels = result.rows.map((r) => (labelCol ? String(r[labelCol]) : 'סה״כ'));
    const data = result.rows.map((r) => Number(r[valueCol] ?? 0));

    return {
      labels,
      datasets: [
        {
          label: 'ערך',
          data,
          backgroundColor: '#6366f1',
          borderRadius: 4,
        },
      ],
    };
  });

  private static readonly LABELS: Record<string, string> = {
    value: 'ערך',
    year: 'שנה',
    gender: 'מגדר',
    city: 'עיר',
    ageGroup: 'קבוצת גיל',
    sector: 'מגזר',
  };

  protected label(col: string): string {
    return ResultsComponent.LABELS[col] ?? col;
  }
}
