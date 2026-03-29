import { Injectable, signal, computed } from '@angular/core';
import {
  CadastroState,
  TipoEmpresa,
  DadosEmpresa,
  ResponsavelAcesso,
  PlanoSelecionado,
} from '../models/cadastro.model';

const ESTADO_INICIAL: CadastroState = {
  tipoPerfil: null,
  dadosEmpresa: {},
  responsavel: {},
  plano: {},
  status: 'EM_ANDAMENTO',
  etapaAtual: 0,
  erros: [],
};

@Injectable({ providedIn: 'root' })
export class CadastroStateService {
  // ─── State privado usando Signals ──────────────────────────────────────────
  private readonly _state = signal<CadastroState>(ESTADO_INICIAL);

  // ─── Selectors públicos ────────────────────────────────────────────────────
  readonly estado = this._state.asReadonly();

  readonly tipoPerfil = computed(() => this._state().tipoPerfil);
  readonly dadosEmpresa = computed(() => this._state().dadosEmpresa);
  readonly responsavel = computed(() => this._state().responsavel);
  readonly plano = computed(() => this._state().plano);
  readonly etapaAtual = computed(() => this._state().etapaAtual);
  readonly statusCadastro = computed(() => this._state().status);
  readonly erros = computed(() => this._state().erros);

  readonly ehFrotista = computed(() => this._state().tipoPerfil === 'FROTA');
  readonly progressoPercent = computed(() => {
    const totalEtapas = 5;
    return Math.round((this._state().etapaAtual / totalEtapas) * 100);
  });

  // ─── Ações ─────────────────────────────────────────────────────────────────
  definirTipoPerfil(tipo: TipoEmpresa): void {
    this._state.update((s) => ({ ...s, tipoPerfil: tipo, etapaAtual: 1 }));
  }

  salvarDadosEmpresa(dados: Partial<DadosEmpresa>): void {
    this._state.update((s) => ({
      ...s,
      dadosEmpresa: { ...s.dadosEmpresa, ...dados },
      etapaAtual: 2,
    }));
  }

  salvarResponsavel(responsavel: Partial<ResponsavelAcesso>): void {
    this._state.update((s) => ({
      ...s,
      responsavel: { ...s.responsavel, ...responsavel },
      etapaAtual: 3,
    }));
  }

  salvarPlano(plano: Partial<PlanoSelecionado>): void {
    this._state.update((s) => ({
      ...s,
      plano: { ...s.plano, ...plano },
      etapaAtual: 4,
    }));
  }

  concluirCadastro(): void {
    this._state.update((s) => ({ ...s, status: 'CONCLUIDO', etapaAtual: 5 }));
  }

  voltarEtapa(): void {
    this._state.update((s) => ({
      ...s,
      etapaAtual: Math.max(0, s.etapaAtual - 1),
    }));
  }

  registrarErro(mensagem: string): void {
    this._state.update((s) => ({
      ...s,
      status: 'ERRO',
      erros: [...s.erros, mensagem],
    }));
  }

  resetar(): void {
    this._state.set(ESTADO_INICIAL);
  }
}
