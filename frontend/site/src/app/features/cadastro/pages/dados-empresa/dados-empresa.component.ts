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
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { CadastroStateService } from '../../../../core/services/cadastro-state.service';
import { CadastroApiService } from '../../../../core/services/cadastro-api.service';

// Validator CNPJ
function cnpjValidator(control: AbstractControl): ValidationErrors | null {
  const valor = control.value?.replace(/\D/g, '');
  if (!valor || valor.length !== 14) return { cnpjInvalido: true };
  if (/^(\d)\1+$/.test(valor)) return { cnpjInvalido: true };
  return null;
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
export class DadosEmpresaComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly cadastroState = inject(CadastroStateService);
  private readonly cadastroApi = inject(CadastroApiService);

  readonly ehFrotista = this.cadastroState.ehFrotista;
  readonly consultandoCnpj = signal(false);
  readonly erroCnpj = signal<string | null>(null);

  form = this.fb.group({
    cnpj:               ['', [Validators.required, cnpjValidator]],
    razaoSocial:        ['', Validators.required],
    nomeFantasia:       [''],
    telefone:           [''],
    cep:                ['', Validators.required],
    logradouro:         ['', Validators.required],
    numero:             ['', Validators.required],
    complemento:        [''],
    bairro:             ['', Validators.required],
    cidade:             ['', Validators.required],
    uf:                 ['', [Validators.required, Validators.minLength(2)]],
    quantidadeVeiculos: [null as number | null],
    setorAtuacao:       [''],
  });

  ngOnInit(): void {
    const dados = this.cadastroState.dadosEmpresa();
    if (dados && Object.keys(dados).length > 0) {
      this.form.patchValue(dados as any);
    }
  }

  consultarCnpj(): void {
    const cnpjControl = this.form.get('cnpj');
    if (!cnpjControl?.valid) return;

    this.consultandoCnpj.set(true);
    this.erroCnpj.set(null);

    this.cadastroApi.consultarCnpj(cnpjControl.value!).subscribe({
      next: (dados) => {
        this.form.patchValue({
          razaoSocial: dados.razaoSocial,
          nomeFantasia: dados.nomeFantasia,
          logradouro:  dados.logradouro,
          numero:      dados.numero,
          complemento: dados.complemento,
          bairro:      dados.bairro,
          cidade:      dados.municipio,
          uf:          dados.uf,
          cep:         dados.cep,
          telefone:    dados.telefone,
        });
        this.consultandoCnpj.set(false);
      },
      error: (err) => {
        this.erroCnpj.set(err.message ?? 'Erro ao consultar CNPJ. Preencha manualmente.');
        this.consultandoCnpj.set(false);
      },
    });
  }

  voltar(): void {
    this.cadastroState.voltarEtapa();
    this.router.navigate(['/cadastro/tipo-empresa']);
  }

  continuar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.cadastroState.salvarDadosEmpresa(this.form.value as any);
    this.router.navigate(['/cadastro/responsavel']);
  }
}
