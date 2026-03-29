import { Routes } from '@angular/router';

export const cadastroRoutes: Routes = [
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
        title: 'Selecionar Perfil — AutoPartsHub',
      },
      {
        path: 'tipo-empresa',
        loadComponent: () =>
          import('./pages/tipo-empresa/tipo-empresa.component').then(
            (m) => m.TipoEmpresaComponent
          ),
        title: 'Tipo de Empresa — AutoPartsHub',
      },
      {
        path: 'dados-empresa',
        loadComponent: () =>
          import('./pages/dados-empresa/dados-empresa.component').then(
            (m) => m.DadosEmpresaComponent
          ),
        title: 'Dados da Empresa — AutoPartsHub',
      },
      {
        path: 'responsavel',
        loadComponent: () =>
          import('./pages/responsavel-acesso/responsavel-acesso.component').then(
            (m) => m.ResponsavelAcessoComponent
          ),
        title: 'Responsável e Acesso — AutoPartsHub',
      },
      {
        path: 'plano',
        loadComponent: () =>
          import('./pages/escolha-plano/escolha-plano.component').then(
            (m) => m.EscolhaPlanoComponent
          ),
        title: 'Escolha do Plano — AutoPartsHub',
      },
      {
        path: 'confirmacao',
        loadComponent: () =>
          import('./pages/confirmacao/confirmacao.component').then(
            (m) => m.ConfirmacaoComponent
          ),
        title: 'Cadastro Confirmado — AutoPartsHub',
      },
      {
        path: '',
        redirectTo: 'perfil',
        pathMatch: 'full',
      },
    ],
  },
];
