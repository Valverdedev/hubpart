import { Routes } from '@angular/router';

export const CADASTRO_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./cadastro-shell/cadastro-shell.component').then((m) => m.CadastroShellComponent),
    children: [
      {
        path: 'perfil',
        loadComponent: () =>
          import('./pages/selecao-perfil/selecao-perfil.component').then(
            (m) => m.SelecaoPerfilComponent
          ),
        title: 'Selecionar Perfil â€” AutoPartsHub',
      },
      {
        path: 'tipo-empresa',
        loadComponent: () =>
          import('./pages/tipo-empresa/tipo-empresa.component').then(
            (m) => m.TipoEmpresaComponent
          ),
        title: 'Tipo de Empresa â€” AutoPartsHub',
      },
      {
        path: 'dados-empresa',
        loadComponent: () =>
          import('./pages/dados-empresa/dados-empresa.component').then(
            (m) => m.DadosEmpresaComponent
          ),
        title: 'Dados da Empresa â€” AutoPartsHub',
      },
      {
        path: 'responsavel',
        loadComponent: () =>
          import('./pages/responsavel-acesso/responsavel-acesso.component').then(
            (m) => m.ResponsavelAcessoComponent
          ),
        title: 'ResponsÃ¡vel e Acesso â€” AutoPartsHub',
      },
      {
        path: 'plano',
        loadComponent: () =>
          import('./pages/escolha-plano/escolha-plano.component').then(
            (m) => m.EscolhaPlanoComponent
          ),
        title: 'Escolha do Plano â€” AutoPartsHub',
      },
      {
        path: 'confirmacao',
        loadComponent: () =>
          import('./pages/confirmacao/confirmacao.component').then(
            (m) => m.ConfirmacaoComponent
          ),
        title: 'Cadastro Confirmado â€” AutoPartsHub',
      },
      {
        path: '',
        redirectTo: 'perfil',
        pathMatch: 'full',
      },
    ],
  },
];

