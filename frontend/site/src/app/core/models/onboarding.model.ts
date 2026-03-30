export type BuyerType = 'OficinaCarro' | 'OficinaMoto' | 'Logista' | 'Frotista' | 'Outro';

export type SubscriptionPlan = 'Free' | 'Basico' | 'Profissional' | 'Enterprise';

export interface OnboardingSessionResponse {
  sessionToken: string;
}

export interface StartOnboardingSessionRequest {
  tipoPerfil: BuyerType;
  ipOrigem?: string | null;
  userAgent?: string | null;
}

export interface OnboardingDraft {
  tipoComprador?: BuyerType;
  cnpj?: string;
  razaoSocial?: string;
  nomeFantasia?: string;
  telefoneComercial?: string;
  inscricaoEstadual?: string;
  cep?: string;
  logradouro?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  cidade?: string;
  estado?: string;
  nomeCompleto?: string;
  cargo?: string;
  email?: string;
  responsavelTelefone?: string;
  senha?: string;
  aceitaTermos?: boolean;
  comoNosConheceu?: string;
  descricaoOutro?: string;
  segmentoFrota?: string;
  qtdVeiculosEstimada?: number;
  limiteAprovacaoAdmin?: number;
  planoEscolhido?: SubscriptionPlan;
  cicloPagamento?: 'MENSAL' | 'ANUAL';
}

export interface UpdateOnboardingDraftRequest {
  step: number;
  dados: OnboardingDraft;
  email?: string | null;
}

export interface OnboardingDraftResponse {
  tipoPerfil: BuyerType;
  ultimoStep: number;
  dados: OnboardingDraft;
}

export interface LookupCnpjResponse {
  razaoSocial: string;
  nomeFantasia?: string | null;
  situacao: string;
  logradouro?: string | null;
  numero?: string | null;
  complemento?: string | null;
  bairro?: string | null;
  cidade?: string | null;
  estado?: string | null;
  cep?: string | null;
}

export interface LookupCepResponse {
  cep: string;
  logradouro: string;
  complemento?: string | null;
  bairro: string;
  cidade: string;
  estado: string;
}

export interface FinishOnboardingRequest {
  planoEscolhido: SubscriptionPlan;
}

export type FinishOnboardingStatus = 'criado' | 'ja_cadastrado' | 'empresa_ja_cadastrada';

export interface FinishOnboardingResponse {
  tenantId: string;
  status: FinishOnboardingStatus;
  redirectTo: string;
}
