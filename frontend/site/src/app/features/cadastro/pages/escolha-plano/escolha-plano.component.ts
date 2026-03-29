import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { CadastroStateService } from '../../../../core/services/cadastro-state.service';
import { InfoPlano, TipoPlano } from '../../../../core/models/cadastro.model';

@Component({
  selector: 'app-escolha-plano',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatRippleModule, MatButtonToggleModule],
  templateUrl: './escolha-plano.component.html',
  styleUrl: './escolha-plano.component.scss',
})
export class EscolhaPlanoComponent {
  private readonly router = inject(Router);
  private readonly cadastroState = inject(CadastroStateService);

  readonly planoSelecionado = signal<TipoPlano | null>(null);
  cicloSelecionado: 'MENSAL' | 'ANUAL' = 'MENSAL';

  readonly planos: InfoPlano[] = [
    {
      tipo: 'TRIAL',
      nome: 'Trial Gratuito',
      preco: null,
      precoAnual: null,
      cotacoesMes: 10,
      comissao: 'Sem comissão',
      destaque: false,
      recursos: [
        'Acesso por 30 dias',
        'Suporte por e-mail',
        'Dashboard básico',
        'Sem cartão de crédito',
      ],
    },
    {
      tipo: 'BASICO',
      nome: 'Básico',
      preco: 149,
      precoAnual: 119,
      cotacoesMes: 50,
      comissao: '3%',
      destaque: false,
      recursos: [
        'Histórico 90 dias',
        'Suporte via chat',
        'Relatórios básicos',
        'App mobile (PWA)',
      ],
    },
    {
      tipo: 'PROFISSIONAL',
      nome: 'Profissional',
      preco: 349,
      precoAnual: 279,
      cotacoesMes: 200,
      comissao: '2%',
      destaque: true,
      labelBadge: 'Mais popular',
      recursos: [
        'Histórico ilimitado',
        'Suporte prioritário',
        'Relatórios avançados',
        'Múltiplos usuários (5)',
        'Integração ERP básica',
      ],
    },
    {
      tipo: 'ENTERPRISE',
      nome: 'Enterprise',
      preco: 799,
      precoAnual: 639,
      cotacoesMes: 'Ilimitado',
      comissao: '1,5%',
      destaque: false,
      recursos: [
        'Usuários ilimitados',
        'Gerente de conta dedicado',
        'Integração ERP avançada',
        'SLA 99,9%',
        'API completa',
      ],
    },
  ];

  precoExibido(plano: InfoPlano): number {
    if (this.cicloSelecionado === 'ANUAL' && plano.precoAnual !== null) {
      return plano.precoAnual;
    }
    return plano.preco!;
  }

  selecionarPlano(tipo: TipoPlano): void {
    this.planoSelecionado.set(tipo);
  }

  voltar(): void {
    this.cadastroState.voltarEtapa();
    this.router.navigate(['/cadastro/responsavel']);
  }

  continuar(): void {
    if (!this.planoSelecionado()) return;
    this.cadastroState.salvarPlano({
      plano: this.planoSelecionado()!,
      cicloPagamento: this.cicloSelecionado,
    });
    this.router.navigate(['/cadastro/confirmacao']);
  }
}
