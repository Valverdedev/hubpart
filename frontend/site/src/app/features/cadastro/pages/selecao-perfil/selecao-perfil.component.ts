import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { CadastroStateService } from '../../../../core/services/cadastro-state.service';
import { TipoEmpresa } from '../../../../core/models/cadastro.model';

interface OpcaoPerfil {
  tipo: TipoEmpresa;
  icone: string;
  titulo: string;
  descricao: string;
  detalhes: string[];
  cor: string;
}

@Component({
  selector: 'app-selecao-perfil',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatRippleModule],
  templateUrl: './selecao-perfil.component.html',
  styleUrl: './selecao-perfil.component.scss',
})
export class SelecaoPerfilComponent {
  private readonly router = inject(Router);
  private readonly cadastroState = inject(CadastroStateService);

  perfilSelecionado: TipoEmpresa | null = null;

  readonly opcoes: OpcaoPerfil[] = [
    {
      tipo: 'OFICINA',
      icone: 'build',
      titulo: 'Oficina Mecânica',
      descricao: 'Compre peças de fornecedores locais com agilidade e preços competitivos.',
      detalhes: ['Cotação em tempo real', 'Histórico de pedidos', 'Comparação de preços'],
      cor: '#FF6B2B',
    },
    {
      tipo: 'FROTA',
      icone: 'local_shipping',
      titulo: 'Gestor de Frotas',
      descricao: 'Gerencie a manutenção de toda sua frota numa plataforma integrada.',
      detalhes: ['Múltiplos veículos', 'Controle por placa', 'Relatórios de gastos'],
      cor: '#1A2D3E',
    },
    {
      tipo: 'REVENDA',
      icone: 'storefront',
      titulo: 'Revenda / Distribuidora',
      descricao: 'Amplie seus canais de venda e alcance mais clientes na sua região.',
      detalhes: ['Catálogo digital', 'Receba cotações', 'Gestão de estoque'],
      cor: '#10B981',
    },
  ];

  selecionarPerfil(tipo: TipoEmpresa): void {
    this.perfilSelecionado = tipo;
  }

  continuar(): void {
    if (!this.perfilSelecionado) return;
    this.cadastroState.definirTipoPerfil(this.perfilSelecionado);
    this.router.navigate(['/cadastro/tipo-empresa']);
  }
}
