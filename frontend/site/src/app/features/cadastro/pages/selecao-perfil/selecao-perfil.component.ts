import { Component, computed, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { BuyerType } from '../../../../core/models/onboarding.model';
import { OnboardingStateService } from '../../../../core/services/onboarding-state.service';

interface OpcaoPerfil {
  tipo: BuyerType;
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
export class SelecaoPerfilComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly onboardingState = inject(OnboardingStateService);

  perfilSelecionado: BuyerType | null = null;
  readonly carregando = computed(
    () => this.onboardingState.isStepLoading(0) || this.onboardingState.isStepLoading(1)
  );
  readonly erro = this.onboardingState.error;

  readonly opcoes: OpcaoPerfil[] = [
    {
      tipo: 'OficinaCarro',
      icone: 'build',
      titulo: 'Oficina Mecanica',
      descricao: 'Compre pecas de fornecedores locais com agilidade e precos competitivos.',
      detalhes: ['Cotacao em tempo real', 'Historico de pedidos', 'Comparacao de precos'],
      cor: '#FF6B2B',
    },
    {
      tipo: 'Frotista',
      icone: 'local_shipping',
      titulo: 'Gestor de Frotas',
      descricao: 'Gerencie a manutencao de toda sua frota numa plataforma integrada.',
      detalhes: ['Multiplos veiculos', 'Controle por placa', 'Relatorios de gastos'],
      cor: '#1A2D3E',
    },
    {
      tipo: 'Logista',
      icone: 'storefront',
      titulo: 'Revenda / Distribuidora',
      descricao: 'Amplie seus canais de venda e alcance mais clientes na sua regiao.',
      detalhes: ['Catalogo digital', 'Receba cotacoes', 'Gestao de estoque'],
      cor: '#10B981',
    },
  ];

  ngOnInit(): void {
    this.perfilSelecionado =
      this.onboardingState.draft().tipoComprador ?? this.onboardingState.buyerType() ?? null;
  }

  selecionarPerfil(tipo: BuyerType): void {
    this.perfilSelecionado = tipo;
  }

  async continuar(): Promise<void> {
    if (!this.perfilSelecionado || this.carregando()) {
      return;
    }

    const selectedBuyerType = this.perfilSelecionado;
    this.onboardingState.patchDraft({
      tipoComprador: selectedBuyerType,
      segmentoFrota: selectedBuyerType === 'Frotista' ? this.onboardingState.draft().segmentoFrota : undefined,
      qtdVeiculosEstimada:
        selectedBuyerType === 'Frotista' ? this.onboardingState.draft().qtdVeiculosEstimada : undefined,
      limiteAprovacaoAdmin:
        selectedBuyerType === 'Frotista' ? this.onboardingState.draft().limiteAprovacaoAdmin : undefined,
      descricaoOutro: selectedBuyerType === 'Outro' ? this.onboardingState.draft().descricaoOutro : undefined,
    });

    const shouldRecreateSession =
      this.onboardingState.buyerType() !== null && this.onboardingState.buyerType() !== selectedBuyerType;

    const token = await this.onboardingState.initSession(selectedBuyerType, {
      force: shouldRecreateSession,
      preserveDraft: true,
    });

    if (!token) {
      return;
    }

    const saved = await this.onboardingState.saveStep(1);
    if (!saved) {
      return;
    }

    await this.router.navigate(['/cadastro/tipo-empresa']);
  }
}
