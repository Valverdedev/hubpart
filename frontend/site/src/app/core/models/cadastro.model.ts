export type TipoEmpresa = 'OFICINA' | 'FROTA' | 'REVENDA';
export type StatusCadastro = 'EM_ANDAMENTO' | 'CONCLUIDO' | 'ERRO';

export interface SelecaoPerfil {
  tipoPerfil: TipoEmpresa;
}

export interface DadosEmpresa {
  cnpj: string;
  razaoSocial: string;
  nomeFantasia: string;
  telefone: string;
  cep: string;
  logradouro: string;
  numero: string;
  complemento?: string;
  bairro: string;
  cidade: string;
  uf: string;
  // Apenas para Frotista
  quantidadeVeiculos?: number;
  setorAtuacao?: string;
}

export interface ResponsavelAcesso {
  nomeCompleto: string;
  cargo: string;
  email: string;
  telefone: string;
  senha: string;
  confirmacaoSenha: string;
  aceitaTermos: boolean;
}

export type TipoPlano = 'TRIAL' | 'BASICO' | 'PROFISSIONAL' | 'ENTERPRISE';

export interface PlanoSelecionado {
  plano: TipoPlano;
  cicloPagamento: 'MENSAL' | 'ANUAL';
}

export interface CadastroState {
  tipoPerfil: TipoEmpresa | null;
  dadosEmpresa: Partial<DadosEmpresa>;
  responsavel: Partial<ResponsavelAcesso>;
  plano: Partial<PlanoSelecionado>;
  status: StatusCadastro;
  etapaAtual: number;
  erros: string[];
}

export interface InfoPlano {
  tipo: TipoPlano;
  nome: string;
  preco: number | null;
  precoAnual: number | null;
  cotacoesMes: number | string;
  comissao: string;
  recursos: string[];
  destaque: boolean;
  labelBadge?: string;
}
