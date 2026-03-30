import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () =>
      import('./features/home/home.routes').then((m) => m.HOME_ROUTES),
  },
  {
    path: 'cadastro',
    loadChildren: () =>
      import('./features/cadastro/cadastro.routes').then((m) => m.CADASTRO_ROUTES),
  },
  {
    path: '**',
    redirectTo: '',
  },
];

