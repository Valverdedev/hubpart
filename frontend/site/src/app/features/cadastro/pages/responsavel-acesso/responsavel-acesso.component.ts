import { Component, DestroyRef, effect, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { OnboardingDraft } from '../../../../core/models/onboarding.model';
import { OnboardingStateService } from '../../../../core/services/onboarding-state.service';

function senhaForteValidator(control: AbstractControl<string>): ValidationErrors | null {
  const valor = control.value;
  if (valor.length < 8) {
    return { senhaFraca: 'Minimo 8 caracteres' };
  }

  if (!/[A-Z]/.test(valor)) {
    return { senhaFraca: '1 letra maiuscula' };
  }

  if (!/[0-9]/.test(valor)) {
    return { senhaFraca: '1 numero' };
  }

  return null;
}

function confirmarSenhaValidator(control: AbstractControl<string>): ValidationErrors | null {
  const senha = control.parent?.get('senha')?.value;
  return control.value !== senha ? { senhasNaoConferem: true } : null;
}

@Component({
  selector: 'app-responsavel-acesso',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatCheckboxModule,
    MatDividerModule,
    MatButtonModule,
  ],
  templateUrl: './responsavel-acesso.component.html',
  styleUrl: './responsavel-acesso.component.scss',
})
export class ResponsavelAcessoComponent {
  private readonly router = inject(Router);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly onboardingState = inject(OnboardingStateService);

  private draftHydrated = false;

  readonly mostrarSenha = signal(false);
  readonly forcaSenha = signal(0);
  readonly forcaSenhaTexto = signal('');

  readonly form = this.fb.group({
    nomeCompleto: this.fb.control('', { validators: [Validators.required] }),
    cargo: this.fb.control('', { validators: [Validators.required] }),
    email: this.fb.control('', { validators: [Validators.required, Validators.email] }),
    telefone: this.fb.control(''),
    senha: this.fb.control('', { validators: [Validators.required, senhaForteValidator] }),
    confirmacaoSenha: this.fb.control('', { validators: [Validators.required, confirmarSenhaValidator] }),
    aceitaTermos: this.fb.control(false, { validators: [Validators.requiredTrue] }),
  });

  constructor() {
    effect(() => {
      const draft = this.onboardingState.draft();
      if (this.draftHydrated || Object.keys(draft).length === 0) {
        return;
      }

      this.form.patchValue({
        nomeCompleto: draft.nomeCompleto ?? '',
        cargo: draft.cargo ?? '',
        email: draft.email ?? '',
        telefone: draft.responsavelTelefone ?? '',
        senha: draft.senha ?? '',
        aceitaTermos: draft.aceitaTermos ?? false,
      });

      this.draftHydrated = true;
    });

    this.form.controls.senha.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((senha) => {
        this.calcularForcaSenha(senha ?? '');
      });
  }

  private calcularForcaSenha(senha: string): void {
    let forca = 0;

    if (senha.length >= 8) {
      forca += 1;
    }

    if (/[A-Z]/.test(senha)) {
      forca += 1;
    }

    if (/[0-9]/.test(senha)) {
      forca += 1;
    }

    if (/[^A-Za-z0-9]/.test(senha)) {
      forca += 1;
    }

    this.forcaSenha.set(forca);
    this.forcaSenhaTexto.set(['', 'Fraca', 'Regular', 'Boa', 'Forte'][forca]);
  }

  toggleSenha(): void {
    this.mostrarSenha.update((visible) => !visible);
  }

  async voltar(): Promise<void> {
    await this.router.navigate(['/cadastro/dados-empresa']);
  }

  async continuar(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.onboardingState.patchDraft(this.mapFormToDraft());

    const saved = await this.onboardingState.saveStep(3);
    if (!saved) {
      return;
    }

    await this.router.navigate(['/cadastro/plano']);
  }

  private mapFormToDraft(): Partial<OnboardingDraft> {
    const formValue = this.form.getRawValue();

    return {
      nomeCompleto: formValue.nomeCompleto,
      cargo: formValue.cargo,
      email: formValue.email,
      responsavelTelefone: formValue.telefone,
      senha: formValue.senha,
      aceitaTermos: formValue.aceitaTermos,
    };
  }
}
