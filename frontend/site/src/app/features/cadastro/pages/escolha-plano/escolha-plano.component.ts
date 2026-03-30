import { Component, effect, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { SubscriptionPlan } from '../../../../core/models/onboarding.model';
import { OnboardingStateService } from '../../../../core/services/onboarding-state.service';

interface PlanoInfo {
  tipo: SubscriptionPlan;
  nome: string;
  preco: number | null;
  precoAnual: number | null;
  cotacoesMes: number | 'Ilimitado';
  comissao: string;
  recursos: string[];
  destaque: boolean;
  labelBadge?: string;
}

@Component({
  selector: 'app-escolha-plano',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatRippleModule, MatButtonToggleModule],
  templateUrl: './escolha-plano.component.html',
  styleUrl: './escolha-plano.component.scss',
})
export class EscolhaPlanoComponent {
  private readonly router = inject(Router);
  private readonly onboardingState = inject(OnboardingStateService);

  readonly planoSelecionado = signal<SubscriptionPlan | null>(null);
  cicloSelecionado: 'MENSAL' | 'ANUAL' = 'MENSAL';

  readonly planos: PlanoInfo[] = [
    {
      tipo: 'Free',
      nome: 'Trial Gratuito',
      preco: null,
      precoAnual: null,
      cotacoesMes: 10,
      comissao: 'Sem comissao',
      destaque: false,
      recursos: ['Acesso por 30 dias', 'Suporte por e-mail', 'Dashboard basico', 'Sem cartao de credito'],
    },
    {
      tipo: 'Basico',
      nome: 'Basico',
      preco: 149,
      precoAnual: 119,
      cotacoesMes: 50,
      comissao: '3%',
      destaque: false,
      recursos: ['Historico 90 dias', 'Suporte via chat', 'Relatorios basicos', 'App mobile (PWA)'],
    },
    {
      tipo: 'Profissional',
      nome: 'Profissional',
      preco: 349,
      precoAnual: 279,
      cotacoesMes: 200,
      comissao: '2%',
      destaque: true,
      labelBadge: 'Mais popular',
      recursos: [
        'Historico ilimitado',
        'Suporte prioritario',
        'Relatorios avancados',
        'Multiplos usuarios (5)',
        'Integracao ERP basica',
      ],
    },
    {
      tipo: 'Enterprise',
      nome: 'Enterprise',
      preco: 799,
      precoAnual: 639,
      cotacoesMes: 'Ilimitado',
      comissao: '1,5%',
      destaque: false,
      recursos: [
        'Usuarios ilimitados',
        'Gerente de conta dedicado',
        'Integracao ERP avancada',
        'SLA 99,9%',
        'API completa',
      ],
    },
  ];

  constructor() {
    effect(() => {
      const draft = this.onboardingState.draft();

      if (!this.planoSelecionado() && draft.planoEscolhido) {
        this.planoSelecionado.set(draft.planoEscolhido);
      }

      if (draft.cicloPagamento) {
        this.cicloSelecionado = draft.cicloPagamento;
      }
    });
  }

  precoExibido(plano: PlanoInfo): number {
    if (this.cicloSelecionado === 'ANUAL' && plano.precoAnual !== null) {
      return plano.precoAnual;
    }

    return plano.preco ?? 0;
  }

  selecionarPlano(tipo: SubscriptionPlan): void {
    this.planoSelecionado.set(tipo);
  }

  async voltar(): Promise<void> {
    await this.router.navigate(['/cadastro/responsavel']);
  }

  async continuar(): Promise<void> {
    const plano = this.planoSelecionado();
    if (!plano) {
      return;
    }

    this.onboardingState.patchDraft({
      planoEscolhido: plano,
      cicloPagamento: this.cicloSelecionado,
    });

    const saved = await this.onboardingState.saveStep(4);
    if (!saved) {
      return;
    }

    await this.router.navigate(['/cadastro/confirmacao']);
  }
}
