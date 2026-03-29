import { Component, inject, computed } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { CadastroStateService } from '../../../core/services/cadastro-state.service';

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
export class CadastroShellComponent {
  private readonly cadastroState = inject(CadastroStateService);

  readonly etapaAtual = this.cadastroState.etapaAtual;
  readonly progressoPercent = this.cadastroState.progressoPercent;

  readonly etapas: Etapa[] = [
    { rotulo: 'Seleção de Perfil',    icone: 'person',             rota: 'perfil' },
    { rotulo: 'Tipo de Empresa',      icone: 'business',           rota: 'tipo-empresa' },
    { rotulo: 'Dados da Empresa',     icone: 'domain',             rota: 'dados-empresa' },
    { rotulo: 'Responsável e Acesso', icone: 'manage_accounts',    rota: 'responsavel' },
    { rotulo: 'Escolha do Plano',     icone: 'workspace_premium',  rota: 'plano' },
    { rotulo: 'Confirmação',          icone: 'task_alt',           rota: 'confirmacao' },
  ];
}
