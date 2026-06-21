import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { QueryStateService } from '../../services/query-state.service';
import { SavedQuery } from '../../models/query.models';
import { ResultsComponent } from '../results/results';

/** Lists saved queries and lets the user re-run them (Req #4). */
@Component({
  selector: 'app-saved-queries',
  imports: [DatePipe, CardModule, TableModule, ButtonModule, ResultsComponent],
  template: `
    <p-card header="שאילתות שמורות">
      <p-table [value]="state.saved()" styleClass="p-datatable-sm">
        <ng-template #header>
          <tr><th>שם</th><th>נוצר</th><th></th></tr>
        </ng-template>
        <ng-template #body let-item>
          <tr>
            <td>{{ item.name }}</td>
            <td>{{ item.createdAt | date: 'short' }}</td>
            <td>
              <p-button label="הרץ" icon="pi pi-play" size="small" (onClick)="run(item)" />
            </td>
          </tr>
        </ng-template>
        <ng-template #emptymessage>
          <tr><td colspan="3">אין שאילתות שמורות עדיין.</td></tr>
        </ng-template>
      </p-table>
    </p-card>

    <div style="margin-top: 1.5rem;">
      <app-results />
    </div>
  `,
})
export class SavedQueriesComponent implements OnInit {
  protected readonly state = inject(QueryStateService);

  ngOnInit(): void {
    this.state.loadSaved();
  }

  protected run(item: SavedQuery): void {
    this.state.runSaved(item);
  }
}
