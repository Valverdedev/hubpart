import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';

type TipoEmpresaDetalhe = 'MEI' | 'ME' | 'EPP' | 'LTDA' | 'SA' | 'SLU';

interface OpcaoTipoEmpresa {
  tipo: TipoEmpresaDetalhe;
  nome: string;
  descricao: string;
  limiteReceita: string;
  icone: string;
}

@Component({
  selector: 'app-tipo-empresa',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatRippleModule],
  templateUrl: './tipo-empresa.component.html',
  styleUrl: './tipo-empresa.component.scss',
})
export class TipoEmpresaComponent {
  private readonly router = inject(Router);

  readonly tipoSelecionado = signal<TipoEmpresaDetalhe | null>(null);

  readonly opcoes: OpcaoTipoEmpresa[] = [
    {
      tipo: 'MEI',
      nome: 'Microempreendedor Individual',
      descricao: 'Empreendedor individual com faturamento anual de até R$ 81 mil.',
      limiteReceita: 'Até R$ 81.000/ano',
      icone: 'person',
    },
    {
      tipo: 'ME',
      nome: 'Microempresa',
      descricao: 'Empresa com receita bruta anual de até R$ 360 mil.',
      limiteReceita: 'Até R$ 360.000/ano',
      icone: 'store',
    },
    {
      tipo: 'EPP',
      nome: 'Empresa de Pequeno Porte',
      descricao: 'Empresa com faturamento entre R$ 360 mil e R$ 4,8 milhões.',
      limiteReceita: 'Até R$ 4,8 milhões/ano',
      icone: 'business',
    },
    {
      tipo: 'LTDA',
      nome: 'Sociedade Limitada',
      descricao: 'Sociedade empresarial com responsabilidade limitada ao capital social.',
      limiteReceita: 'Sem limite de receita',
      icone: 'corporate_fare',
    },
    {
      tipo: 'SA',
      nome: 'Sociedade Anônima',
      descricao: 'Empresa de capital aberto ou fechado com ações.',
      limiteReceita: 'Sem limite de receita',
      icone: 'account_balance',
    },
    {
      tipo: 'SLU',
      nome: 'Sociedade Limitada Unipessoal',
      descricao: 'Empresa individual com proteção do patrimônio pessoal.',
      limiteReceita: 'Sem limite de receita',
      icone: 'person_pin',
    },
  ];

  selecionar(tipo: TipoEmpresaDetalhe): void {
    this.tipoSelecionado.set(tipo);
  }

  voltar(): void {
    this.router.navigate(['/cadastro/perfil']);
  }

  continuar(): void {
    if (!this.tipoSelecionado()) return;
    this.router.navigate(['/cadastro/dados-empresa']);
  }
}
