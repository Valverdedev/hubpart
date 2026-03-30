import { Component, computed, inject, OnInit } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { OnboardingStateService } from '../../../core/services/onboarding-state.service';

interface Etapa {
  rotulo: string;
  icone: string;
  rota: string;
}

@Component({
  selector: 'app-cadastro-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, MatIconModule],
  templateUrl: './cadastro-shell.component.html',
  styleUrl: './cadastro-shell.component.scss',
})
export class CadastroShellComponent implements OnInit {
  private readonly onboardingState = inject(OnboardingStateService);

  readonly etapaAtual = this.onboardingState.currentStep;
  readonly progressoPercent = computed(() => {
    const totalEtapas = 5;
    return Math.round((Math.min(this.etapaAtual(), totalEtapas) / totalEtapas) * 100);
  });

  readonly etapas: Etapa[] = [
    { rotulo: 'Selecao de Perfil', icone: 'person', rota: 'perfil' },
    { rotulo: 'Tipo de Empresa', icone: 'business', rota: 'tipo-empresa' },
    { rotulo: 'Dados da Empresa', icone: 'domain', rota: 'dados-empresa' },
    { rotulo: 'Responsavel e Acesso', icone: 'manage_accounts', rota: 'responsavel' },
    { rotulo: 'Escolha do Plano', icone: 'workspace_premium', rota: 'plano' },
    { rotulo: 'Confirmacao', icone: 'task_alt', rota: 'confirmacao' },
  ];

  ngOnInit(): void {
    void this.bootstrapSession();
  }

  private async bootstrapSession(): Promise<void> {
    if (this.onboardingState.sessionToken()) {
      const restored = await this.onboardingState.restoreSession();
      if (!restored) {
        await this.onboardingState.initSession(undefined, { preserveDraft: false });
      }
      return;
    }

    await this.onboardingState.initSession(undefined, { preserveDraft: false });
  }
}
