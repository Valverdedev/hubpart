import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatButtonModule } from '@angular/material/button';
import { CadastroStateService } from '../../../../core/services/cadastro-state.service';

// Validator senha forte
function senhaForteValidator(control: AbstractControl): ValidationErrors | null {
  const v = control.value ?? '';
  if (v.length < 8)   return { senhaFraca: 'Mínimo 8 caracteres' };
  if (!/[A-Z]/.test(v)) return { senhaFraca: '1 letra maiúscula' };
  if (!/[0-9]/.test(v)) return { senhaFraca: '1 número' };
  return null;
}

// Validator confirmar senha
function confirmarSenhaValidator(control: AbstractControl): ValidationErrors | null {
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
export class ResponsavelAcessoComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly cadastroState = inject(CadastroStateService);

  readonly mostrarSenha = signal(false);
  readonly forcaSenha = signal(0);
  readonly forcaSenhaTexto = signal('');

  form = this.fb.group({
    nomeCompleto:     ['', Validators.required],
    cargo:            ['', Validators.required],
    email:            ['', [Validators.required, Validators.email]],
    telefone:         [''],
    senha:            ['', [Validators.required, senhaForteValidator]],
    confirmacaoSenha: ['', [Validators.required, confirmarSenhaValidator]],
    aceitaTermos:     [false, Validators.requiredTrue],
  });

  ngOnInit(): void {
    const dados = this.cadastroState.responsavel();
    if (dados && Object.keys(dados).length > 0) {
      this.form.patchValue(dados as any);
    }

    this.form.get('senha')?.valueChanges.subscribe((senha) => {
      this.calcularForcaSenha(senha ?? '');
    });
  }

  private calcularForcaSenha(senha: string): void {
    let forca = 0;
    if (senha.length >= 8)        forca++;
    if (/[A-Z]/.test(senha))      forca++;
    if (/[0-9]/.test(senha))      forca++;
    if (/[^A-Za-z0-9]/.test(senha)) forca++;
    this.forcaSenha.set(forca);
    this.forcaSenhaTexto.set(['', 'Fraca', 'Regular', 'Boa', 'Forte'][forca]);
  }

  toggleSenha(): void {
    this.mostrarSenha.update((v) => !v);
  }

  voltar(): void {
    this.cadastroState.voltarEtapa();
    this.router.navigate(['/cadastro/dados-empresa']);
  }

  continuar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.cadastroState.salvarResponsavel(this.form.value as any);
    this.router.navigate(['/cadastro/plano']);
  }
}
