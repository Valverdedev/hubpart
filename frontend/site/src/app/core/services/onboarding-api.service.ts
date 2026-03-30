import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import {
  BuyerType,
  FinishOnboardingRequest,
  FinishOnboardingResponse,
  LookupCepResponse,
  LookupCnpjResponse,
  OnboardingDraft,
  OnboardingDraftResponse,
  OnboardingSessionResponse,
  StartOnboardingSessionRequest,
  UpdateOnboardingDraftRequest,
} from '../models/onboarding.model';

interface RawOnboardingDraftResponse {
  tipoPerfil: number | string;
  ultimoStep: number;
  dados: unknown;
}

const BUYER_TYPE_TO_API: Record<BuyerType, number> = {
  OficinaCarro: 0,
  OficinaMoto: 1,
  Logista: 2,
  Frotista: 3,
  Outro: 4,
};

const API_TO_BUYER_TYPE: Record<number, BuyerType> = {
  0: 'OficinaCarro',
  1: 'OficinaMoto',
  2: 'Logista',
  3: 'Frotista',
  4: 'Outro',
};

const PLAN_TO_API: Record<FinishOnboardingRequest['planoEscolhido'], number> = {
  Free: 0,
  Basico: 1,
  Profissional: 2,
  Enterprise: 3,
};

@Injectable({ providedIn: 'root' })
export class OnboardingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/onboarding';

  startSession(request: StartOnboardingSessionRequest): Observable<OnboardingSessionResponse> {
    return this.http.post<OnboardingSessionResponse>(`${this.baseUrl}/iniciar`, {
      ...request,
      tipoPerfil: BUYER_TYPE_TO_API[request.tipoPerfil],
    });
  }

  getDraft(sessionToken: string): Observable<OnboardingDraftResponse> {
    return this.http
      .get<RawOnboardingDraftResponse>(`${this.baseUrl}/rascunho/${sessionToken}`)
      .pipe(
        map((response) => ({
          tipoPerfil: this.normalizeBuyerType(response.tipoPerfil),
          ultimoStep: response.ultimoStep,
          dados: this.normalizeDraft(response.dados),
        }))
      );
  }

  updateDraft(sessionToken: string, request: UpdateOnboardingDraftRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/rascunho/${sessionToken}`, request);
  }

  finish(sessionToken: string, request: FinishOnboardingRequest): Observable<FinishOnboardingResponse> {
    return this.http.post<FinishOnboardingResponse>(`${this.baseUrl}/finalizar/${sessionToken}`, {
      planoEscolhido: PLAN_TO_API[request.planoEscolhido],
    });
  }

  lookupCnpj(cnpj: string): Observable<LookupCnpjResponse> {
    const cnpjLimpo = cnpj.replace(/\D/g, '');
    return this.http.get<LookupCnpjResponse>(`${this.baseUrl}/cnpj/${cnpjLimpo}`);
  }

  lookupCep(cep: string): Observable<LookupCepResponse> {
    const cepLimpo = cep.replace(/\D/g, '');
    return this.http.get<LookupCepResponse>(`${this.baseUrl}/cep/${cepLimpo}`);
  }

  private normalizeDraft(rawDraft: unknown): OnboardingDraft {
    if (rawDraft === null || typeof rawDraft !== 'object' || Array.isArray(rawDraft)) {
      return {};
    }

    return rawDraft as OnboardingDraft;
  }

  private normalizeBuyerType(rawBuyerType: number | string): BuyerType {
    if (typeof rawBuyerType === 'number') {
      return API_TO_BUYER_TYPE[rawBuyerType] ?? 'OficinaCarro';
    }

    if (rawBuyerType in BUYER_TYPE_TO_API) {
      return rawBuyerType as BuyerType;
    }

    return 'OficinaCarro';
  }
}
