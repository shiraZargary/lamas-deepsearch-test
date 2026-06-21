import { Routes } from '@angular/router';
import { BuilderPageComponent } from './pages/builder-page';
import { FreeSearchComponent } from './components/free-search/free-search';
import { SavedQueriesComponent } from './components/saved-queries/saved-queries';

export const routes: Routes = [
  { path: '', redirectTo: 'free-search', pathMatch: 'full' },
  { path: 'free-search', component: FreeSearchComponent, title: 'חיפוש חופשי' },
  { path: 'builder', component: BuilderPageComponent, title: 'בניית שאילתה' },
  { path: 'saved', component: SavedQueriesComponent, title: 'שאילתות שמורות' },
  { path: '**', redirectTo: 'free-search' },
];
