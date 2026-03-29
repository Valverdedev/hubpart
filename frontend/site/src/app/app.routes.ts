import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'cadastro/perfil',
    pathMatch: 'full',
  },
  {
    path: 'cadastro',
    loadChildren: () =>
      import('./features/cadastro/cadastro.routes').then((m) => m.cadastroRoutes),
  },
  {
    path: '**',
    redirectTo: 'cadastro/perfil',
  },
];
