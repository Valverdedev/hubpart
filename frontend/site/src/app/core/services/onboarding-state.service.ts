import { Injectable, signal, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {
  BuyerType,
  FinishOnboardingResponse,
  OnboardingDraft,
  SubscriptionPlan,
} from '../models/onboarding.model';
import { OnboardingApiService } from './onboarding-api.service';

const SESSION_TOKEN_STORAGE_KEY = 'onboarding.sessionToken';
const DEFAULT_BUYER_TYPE: BuyerType = 'OficinaCarro';

type LoadingByStep = Record<number, boolean>;

@Injectable({ providedIn: 'root' })
export class OnboardingStateService {
  private readonly onboardingApi = inject(OnboardingApiService);

  readonly sessionToken = signal<string | null>(this.readTokenFromStorage());
  readonly draft = signal<OnboardingDraft>({});
  readonly currentStep = signal(0);
  readonly loading = signal<LoadingByStep>({});
  readonly error = signal<string | null>(null);
  readonly buyerType = signal<BuyerType | null>(null);
  readonly finishResult = signal<FinishOnboardingResponse | null>(null);

  async initSession(
    tipoPerfil?: BuyerType,
    options?: { force?: boolean; preserveDraft?: boolean }
  ): Promise<string | null> {
    const resolvedBuyerType = tipoPerfil ?? this.resolveBuyerType();
    const force = options?.force ?? false;
    const requestedPreserveDraft = options?.preserveDraft ?? false;
    const preserveDraft = requestedPreserveDraft && this.finishResult() === null;
    const existingToken = this.sessionToken();

    if (!force && existingToken && this.buyerType() === resolvedBuyerType) {
      return existingToken;
    }

    this.setStepLoading(0, true);
    this.error.set(null);

    try {
      const response = await firstValueFrom(
        this.onboardingApi.startSession({
          tipoPerfil: resolvedBuyerType,
          ipOrigem: null,
          userAgent: this.getUserAgent(),
        })
      );

      this.sessionToken.set(response.sessionToken);
      this.buyerType.set(resolvedBuyerType);
      this.currentStep.set(0);
      this.finishResult.set(null);
      this.writeTokenToStorage(response.sessionToken);

      if (!preserveDraft) {
        this.draft.set({ tipoComprador: resolvedBuyerType });
      } else {
        this.patchDraft({ tipoComprador: resolvedBuyerType });
      }

      return response.sessionToken;
    } catch (error) {
      this.setErrorFromHttp(error, 'Nao foi possivel iniciar a sessao de onboarding.');
      return null;
    } finally {
      this.setStepLoading(0, false);
    }
  }

  async restoreSession(): Promise<boolean> {
    const token = this.sessionToken() ?? this.readTokenFromStorage();

    if (!token) {
      return false;
    }

    this.setStepLoading(0, true);
    this.error.set(null);

    try {
      const response = await firstValueFrom(this.onboardingApi.getDraft(token));

      this.sessionToken.set(token);
      this.buyerType.set(response.tipoPerfil);
      this.currentStep.set(response.ultimoStep);
      this.draft.set({
        ...response.dados,
        tipoComprador: response.tipoPerfil,
      });

      this.writeTokenToStorage(token);
      return true;
    } catch (error) {
      if (this.isNotFoundError(error)) {
        this.error.set('Sessao invalida ou expirada. Inicie um novo cadastro.');
        this.clearSession({ clearDraft: false });
        return false;
      }

      this.setErrorFromHttp(error, 'Nao foi possivel restaurar a sessao de onboarding.');
      return false;
    } finally {
      this.setStepLoading(0, false);
    }
  }

  patchDraft(patch: Partial<OnboardingDraft>): void {
    this.draft.update((currentDraft) => ({ ...currentDraft, ...patch }));
  }

  async saveStep(step: number): Promise<boolean> {
    this.setStepLoading(step, true);
    this.error.set(null);

    const token = await this.ensureSessionToken();
    if (!token) {
      this.setStepLoading(step, false);
      return false;
    }

    const consolidatedDraft = { ...this.draft() };

    try {
      await firstValueFrom(
        this.onboardingApi.updateDraft(token, {
          step,
          dados: consolidatedDraft,
          email: consolidatedDraft.email ?? null,
        })
      );

      this.currentStep.set(Math.max(this.currentStep(), step));
      return true;
    } catch (error) {
      if (this.isNotFoundError(error)) {
        return this.recoverSessionAndRetry(step, consolidatedDraft);
      }

      this.setErrorFromHttp(error, `Nao foi possivel salvar a etapa ${step}.`);
      return false;
    } finally {
      this.setStepLoading(step, false);
    }
  }

  async finish(): Promise<FinishOnboardingResponse | null> {
    const token = this.sessionToken();
    if (!token) {
      this.error.set('Sessao de onboarding nao encontrada.');
      return null;
    }

    this.setStepLoading(5, true);
    this.error.set(null);

    try {
      const response = await firstValueFrom(
        this.onboardingApi.finish(token, {
          planoEscolhido: this.resolvePlan(),
        })
      );

      this.finishResult.set(response);
      this.clearSession({ clearDraft: false });
      this.currentStep.set(5);

      return response;
    } catch (error) {
      if (error instanceof HttpErrorResponse && error.status === 400) {
        const backendMessage = this.readBackendErrorMessage(error);
        this.error.set(backendMessage ?? 'Sua sessao expirou. Inicie um novo cadastro.');
      } else {
        this.setErrorFromHttp(error, 'Nao foi possivel finalizar o onboarding.');
      }

      return null;
    } finally {
      this.setStepLoading(5, false);
    }
  }

  clearSession(options?: { clearDraft?: boolean }): void {
    const clearDraft = options?.clearDraft ?? true;

    this.sessionToken.set(null);
    this.removeTokenFromStorage();

    if (clearDraft) {
      this.finishResult.set(null);
      this.draft.set({});
      this.currentStep.set(0);
      this.error.set(null);
      this.buyerType.set(null);
      this.loading.set({});
    }
  }

  isStepLoading(step: number): boolean {
    return Boolean(this.loading()[step]);
  }

  setLoading(step: number, loading: boolean): void {
    this.setStepLoading(step, loading);
  }

  setError(message: string | null): void {
    this.error.set(message);
  }

  private async recoverSessionAndRetry(step: number, consolidatedDraft: OnboardingDraft): Promise<boolean> {
    const recoveredToken = await this.initSession(this.resolveBuyerType(consolidatedDraft), {
      force: true,
      preserveDraft: true,
    });

    if (!recoveredToken) {
      this.error.set('Sessao invalida. Nao foi possivel recuperar o cadastro.');
      return false;
    }

    try {
      await firstValueFrom(
        this.onboardingApi.updateDraft(recoveredToken, {
          step,
          dados: consolidatedDraft,
          email: consolidatedDraft.email ?? null,
        })
      );

      this.currentStep.set(Math.max(this.currentStep(), step));
      return true;
    } catch (error) {
      this.setErrorFromHttp(error, 'Falha ao recuperar sessao expirada.');
      return false;
    }
  }

  private async ensureSessionToken(): Promise<string | null> {
    const existingToken = this.sessionToken();
    if (existingToken) {
      return existingToken;
    }

    return this.initSession(this.resolveBuyerType(), { preserveDraft: true });
  }

  private resolveBuyerType(draft: OnboardingDraft = this.draft()): BuyerType {
    return draft.tipoComprador ?? this.buyerType() ?? DEFAULT_BUYER_TYPE;
  }

  private resolvePlan(): SubscriptionPlan {
    return this.draft().planoEscolhido ?? 'Free';
  }

  private setStepLoading(step: number, loading: boolean): void {
    this.loading.update((state) => {
      if (loading) {
        return { ...state, [step]: true };
      }

      const { [step]: _discardedStep, ...remainingState } = state;
      return remainingState;
    });
  }

  private setErrorFromHttp(error: unknown, fallbackMessage: string): void {
    if (error instanceof HttpErrorResponse) {
      const backendMessage = this.readBackendErrorMessage(error);
      this.error.set(backendMessage ?? fallbackMessage);
      return;
    }

    this.error.set(fallbackMessage);
  }

  private isNotFoundError(error: unknown): boolean {
    return error instanceof HttpErrorResponse && error.status === 404;
  }

  private readBackendErrorMessage(error: HttpErrorResponse): string | null {
    const payload = error.error;

    if (payload === null || typeof payload !== 'object') {
      return null;
    }

    const maybeMensagem = Reflect.get(payload, 'mensagem');
    if (typeof maybeMensagem === 'string' && maybeMensagem.trim().length > 0) {
      return maybeMensagem;
    }

    const maybeErros = Reflect.get(payload, 'erros');
    if (Array.isArray(maybeErros)) {
      const firstMessage = maybeErros.find((value): value is string => typeof value === 'string');
      if (firstMessage) {
        return firstMessage;
      }
    }

    return null;
  }

  private readTokenFromStorage(): string | null {
    try {
      return sessionStorage.getItem(SESSION_TOKEN_STORAGE_KEY);
    } catch {
      return null;
    }
  }

  private writeTokenToStorage(token: string): void {
    try {
      sessionStorage.setItem(SESSION_TOKEN_STORAGE_KEY, token);
    } catch {
      // Ignora erro de storage e segue com estado em memoria.
    }
  }

  private removeTokenFromStorage(): void {
    try {
      sessionStorage.removeItem(SESSION_TOKEN_STORAGE_KEY);
    } catch {
      // Ignora erro de storage.
    }
  }

  private getUserAgent(): string | null {
    if (typeof navigator === 'undefined') {
      return null;
    }

    return navigator.userAgent;
  }
}
