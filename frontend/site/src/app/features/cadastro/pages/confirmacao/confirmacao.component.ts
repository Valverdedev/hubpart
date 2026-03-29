import { Component, inject, computed, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { CadastroStateService } from '../../../../core/services/cadastro-state.service';

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
  private readonly cadastroState = inject(CadastroStateService);

  readonly dadosEmpresa = this.cadastroState.dadosEmpresa;
  readonly responsavel  = this.cadastroState.responsavel;
  readonly plano        = this.cadastroState.plano;
  readonly tipoPerfil   = this.cadastroState.tipoPerfil;

  readonly labelPerfil = computed(() => {
    const mapa: Record<string, string> = {
      OFICINA:  'Oficina Mecânica',
      FROTA:    'Gestor de Frotas',
      REVENDA:  'Revenda / Distribuidora',
    };
    return mapa[this.tipoPerfil() ?? ''] ?? '—';
  });

  readonly labelPlano = computed(() => {
    const mapa: Record<string, string> = {
      TRIAL:         'Trial 30d',
      BASICO:        'Básico',
      PROFISSIONAL:  'Profissional',
      ENTERPRISE:    'Enterprise',
    };
    return mapa[this.plano().plano ?? ''] ?? '—';
  });

  readonly labelCiclo = computed(() =>
    this.plano().cicloPagamento === 'ANUAL' ? '(anual)' : '(mensal)'
  );

  readonly proximasEtapas: ProximaEtapa[] = [
    {
      titulo: 'Verifique seu e-mail',
      descricao: 'Enviamos um link de verificação para o e-mail cadastrado.',
    },
    {
      titulo: 'Complete seu perfil',
      descricao: 'Adicione logo, horários e outras informações do seu negócio.',
    },
    {
      titulo: 'Faça sua primeira cotação',
      descricao: 'Solicite preços de peças em segundos para dezenas de fornecedores.',
    },
  ];

  ngOnInit(): void {
    this.cadastroState.concluirCadastro();
  }

  irAoDashboard(): void {
    // TODO: Redirecionar para o dashboard real após integração com o backend
    window.location.href = '/app/dashboard';
  }

  novoCadastro(): void {
    this.cadastroState.resetar();
    this.router.navigate(['/cadastro/perfil']);
  }
}
