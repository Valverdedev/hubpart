import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import {
  BuyerType,
  FinishOnboardingStatus,
  SubscriptionPlan,
} from '../../../../core/models/onboarding.model';
import { OnboardingStateService } from '../../../../core/services/onboarding-state.service';

interface ProximaEtapa {
  titulo: string;
  descricao: string;
}

@Component({
  selector: 'app-confirmacao',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './confirmacao.component.html',
  styleUrl: './confirmacao.component.scss',
})
export class ConfirmacaoComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly onboardingState = inject(OnboardingStateService);

  readonly redirectTo = signal('/login');
  readonly finishResult = this.onboardingState.finishResult;
  readonly finalizadoComSucesso = computed(() => this.finishResult() !== null);

  readonly dadosEmpresa = computed(() => this.onboardingState.draft());
  readonly responsavel = computed(() => this.onboardingState.draft());
  readonly plano = computed(() => this.onboardingState.draft());
  readonly tipoPerfil = computed(
    () => this.onboardingState.draft().tipoComprador ?? this.onboardingState.buyerType()
  );
  readonly erro = this.onboardingState.error;
  readonly finalizando = computed(() => this.onboardingState.isStepLoading(5));

  readonly labelPerfil = computed(() => {
    const mapa: Record<BuyerType, string> = {
      OficinaCarro: 'Oficina Mecanica',
      OficinaMoto: 'Oficina de Motos',
      Logista: 'Revenda / Distribuidora',
      Frotista: 'Gestor de Frotas',
      Outro: 'Outro',
    };

    const tipo = this.tipoPerfil();
    return tipo ? mapa[tipo] : '-';
  });

  readonly labelPlano = computed(() => {
    const mapa: Record<SubscriptionPlan, string> = {
      Free: 'Trial 30d',
      Basico: 'Basico',
      Profissional: 'Profissional',
      Enterprise: 'Enterprise',
    };

    const plano = this.plano().planoEscolhido;
    return plano ? mapa[plano] : '-';
  });

  readonly labelCiclo = computed(() =>
    this.plano().cicloPagamento === 'ANUAL' ? '(anual)' : '(mensal)'
  );

  readonly mensagemResultado = computed(() => {
    const status = this.finishResult()?.status;
    const mensagens: Record<FinishOnboardingStatus, string> = {
      criado: 'Tenant criado com sucesso. Sua conta esta pronta para acesso.',
      ja_cadastrado: 'Cadastro ja finalizado anteriormente para esta sessao.',
      empresa_ja_cadastrada: 'Este CNPJ ja possui tenant. Redirecione para o login.',
    };

    return status ? mensagens[status] : '';
  });

  readonly proximasEtapas: ProximaEtapa[] = [
    {
      titulo: 'Verifique seu e-mail',
      descricao: 'Enviamos um link de verificacao para o e-mail cadastrado.',
    },
    {
      titulo: 'Complete seu perfil',
      descricao: 'Adicione logo, horarios e outras informacoes do seu negocio.',
    },
    {
      titulo: 'Faca sua primeira cotacao',
      descricao: 'Solicite precos de pecas em segundos para dezenas de fornecedores.',
    },
  ];

  ngOnInit(): void {
    void this.finalizarOnboarding();
  }

  async irAoDashboard(): Promise<void> {
    if (!this.finalizadoComSucesso()) {
      return;
    }

    await this.router.navigateByUrl(this.redirectTo());
  }

  async novoCadastro(): Promise<void> {
    this.onboardingState.clearSession({ clearDraft: true });
    await this.router.navigate(['/cadastro/perfil']);
  }

  async tentarNovamente(): Promise<void> {
    await this.finalizarOnboarding();
  }

  private async finalizarOnboarding(): Promise<void> {
    if (this.finalizadoComSucesso()) {
      return;
    }

    if (!this.onboardingState.sessionToken()) {
      this.onboardingState.setError('Sessao de onboarding nao encontrada para finalizar.');
      return;
    }

    const resultado = await this.onboardingState.finish();
    if (resultado?.redirectTo) {
      this.redirectTo.set(resultado.redirectTo);
    }
  }
}
