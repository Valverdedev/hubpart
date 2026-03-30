import { Component, computed, effect, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { firstValueFrom } from 'rxjs';
import { OnboardingDraft } from '../../../../core/models/onboarding.model';
import { OnboardingApiService } from '../../../../core/services/onboarding-api.service';
import { OnboardingStateService } from '../../../../core/services/onboarding-state.service';

function cnpjValidator(control: AbstractControl<string>): ValidationErrors | null {
  const valor = control.value.replace(/\D/g, '');
  if (!valor || valor.length !== 14) {
    return { cnpjInvalido: true };
  }

  if (/^(\d)\1+$/.test(valor)) {
    return { cnpjInvalido: true };
  }

  return null;
}

interface DadosEmpresaFormValue {
  cnpj: string;
  razaoSocial: string;
  nomeFantasia: string;
  telefone: string;
  inscricaoEstadual: string;
  cep: string;
  logradouro: string;
  numero: string;
  complemento: string;
  bairro: string;
  cidade: string;
  uf: string;
  quantidadeVeiculos: number | null;
  setorAtuacao: string;
}

@Component({
  selector: 'app-dados-empresa',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
  ],
  templateUrl: './dados-empresa.component.html',
  styleUrl: './dados-empresa.component.scss',
})
export class DadosEmpresaComponent {
  private readonly router = inject(Router);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly onboardingState = inject(OnboardingStateService);
  private readonly onboardingApi = inject(OnboardingApiService);

  private draftHydrated = false;

  readonly ehFrotista = computed(
    () =>
      this.onboardingState.draft().tipoComprador === 'Frotista' ||
      this.onboardingState.buyerType() === 'Frotista'
  );

  readonly consultandoCnpj = computed(() => this.onboardingState.isStepLoading(21));
  readonly consultandoCep = computed(() => this.onboardingState.isStepLoading(22));
  readonly erroCnpj = computed(() => this.onboardingState.error());

  readonly form = this.fb.group({
    cnpj: this.fb.control('', { validators: [Validators.required, cnpjValidator] }),
    razaoSocial: this.fb.control('', { validators: [Validators.required] }),
    nomeFantasia: this.fb.control(''),
    telefone: this.fb.control('', { validators: [Validators.required] }),
    inscricaoEstadual: this.fb.control(''),
    cep: this.fb.control('', { validators: [Validators.required] }),
    logradouro: this.fb.control('', { validators: [Validators.required] }),
    numero: this.fb.control('', { validators: [Validators.required] }),
    complemento: this.fb.control(''),
    bairro: this.fb.control('', { validators: [Validators.required] }),
    cidade: this.fb.control('', { validators: [Validators.required] }),
    uf: this.fb.control('', { validators: [Validators.required, Validators.minLength(2)] }),
    quantidadeVeiculos: new FormControl<number | null>(null),
    setorAtuacao: this.fb.control(''),
  });

  constructor() {
    effect(() => {
      const draft = this.onboardingState.draft();
      if (this.draftHydrated || Object.keys(draft).length === 0) {
        return;
      }

      this.form.patchValue(this.mapDraftToForm(draft));
      this.draftHydrated = true;
    });
  }

  async consultarCnpj(): Promise<void> {
    const cnpjControl = this.form.controls.cnpj;
    if (cnpjControl.invalid) {
      return;
    }

    this.setLookupLoading(21, true);
    this.onboardingState.setError(null);

    try {
      const dados = await firstValueFrom(this.onboardingApi.lookupCnpj(cnpjControl.value));

      this.form.patchValue({
        razaoSocial: dados.razaoSocial,
        nomeFantasia: dados.nomeFantasia ?? dados.razaoSocial,
        logradouro: dados.logradouro ?? '',
        numero: dados.numero ?? '',
        complemento: dados.complemento ?? '',
        bairro: dados.bairro ?? '',
        cidade: dados.cidade ?? '',
        uf: dados.estado ?? '',
        cep: dados.cep ?? this.form.controls.cep.value,
      });
    } catch {
      this.onboardingState.setError('Erro ao consultar CNPJ. Preencha os dados manualmente.');
    } finally {
      this.setLookupLoading(21, false);
    }
  }

  async consultarCep(): Promise<void> {
    const cepControl = this.form.controls.cep;
    if (!cepControl.value || cepControl.value.replace(/\D/g, '').length < 8) {
      return;
    }

    this.setLookupLoading(22, true);
    this.onboardingState.setError(null);

    try {
      const dados = await firstValueFrom(this.onboardingApi.lookupCep(cepControl.value));

      this.form.patchValue({
        cep: dados.cep,
        logradouro: dados.logradouro,
        complemento: dados.complemento ?? this.form.controls.complemento.value,
        bairro: dados.bairro,
        cidade: dados.cidade,
        uf: dados.estado,
      });
    } catch {
      this.onboardingState.setError('Erro ao consultar CEP.');
    } finally {
      this.setLookupLoading(22, false);
    }
  }

  async voltar(): Promise<void> {
    await this.router.navigate(['/cadastro/tipo-empresa']);
  }

  async continuar(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.onboardingState.patchDraft(this.mapFormToDraft(this.form.getRawValue()));

    const saved = await this.onboardingState.saveStep(2);
    if (!saved) {
      return;
    }

    await this.router.navigate(['/cadastro/responsavel']);
  }

  private mapDraftToForm(draft: OnboardingDraft): Partial<DadosEmpresaFormValue> {
    return {
      cnpj: draft.cnpj ?? '',
      razaoSocial: draft.razaoSocial ?? '',
      nomeFantasia: draft.nomeFantasia ?? '',
      telefone: draft.telefoneComercial ?? '',
      inscricaoEstadual: draft.inscricaoEstadual ?? '',
      cep: draft.cep ?? '',
      logradouro: draft.logradouro ?? '',
      numero: draft.numero ?? '',
      complemento: draft.complemento ?? '',
      bairro: draft.bairro ?? '',
      cidade: draft.cidade ?? '',
      uf: draft.estado ?? '',
      quantidadeVeiculos: draft.qtdVeiculosEstimada ?? null,
      setorAtuacao: draft.segmentoFrota ?? '',
    };
  }

  private mapFormToDraft(form: DadosEmpresaFormValue): Partial<OnboardingDraft> {
    const cepNumerico = form.cep.replace(/\D/g, '').slice(0, 8);

    const draft: Partial<OnboardingDraft> = {
      cnpj: form.cnpj,
      razaoSocial: form.razaoSocial,
      nomeFantasia: form.nomeFantasia,
      telefoneComercial: form.telefone,
      inscricaoEstadual: form.inscricaoEstadual,
      cep: cepNumerico,
      logradouro: form.logradouro,
      numero: form.numero,
      complemento: form.complemento || undefined,
      bairro: form.bairro,
      cidade: form.cidade,
      estado: form.uf,
    };

    if (this.ehFrotista()) {
      draft.qtdVeiculosEstimada = form.quantidadeVeiculos ?? undefined;
      draft.segmentoFrota = form.setorAtuacao || undefined;
    } else {
      draft.qtdVeiculosEstimada = undefined;
      draft.segmentoFrota = undefined;
    }

    return draft;
  }

  private setLookupLoading(step: number, loading: boolean): void {
    this.onboardingState.setLoading(step, loading);
  }
}
