import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { DadosEmpresa, ResponsavelAcesso, PlanoSelecionado } from '../models/cadastro.model';

export interface RespostaCadastro {
  sucesso: boolean;
  tenantId?: string;
  mensagem?: string;
}

export interface ConsultaCnpj {
  cnpj: string;
  razaoSocial: string;
  nomeFantasia: string;
  logradouro: string;
  numero: string;
  complemento: string;
  bairro: string;
  municipio: string;
  uf: string;
  cep: string;
  telefone: string;
  situacao: string;
}

@Injectable({ providedIn: 'root' })
export class CadastroApiService {
  private readonly http = inject(HttpClient);

  // TODO: Substituir pela URL real da API quando o backend estiver disponível
  private readonly baseUrl = '/api/v1/cadastro';

  consultarCnpj(cnpj: string): Observable<ConsultaCnpj> {
    const cnpjLimpo = cnpj.replace(/\D/g, '');
    // Futuramente: return this.http.get<ConsultaCnpj>(`${this.baseUrl}/cnpj/${cnpjLimpo}`);
    return this.http.get<ConsultaCnpj>(`https://brasilapi.com.br/api/cnpj/v1/${cnpjLimpo}`).pipe(
      map((res: any) => ({
        cnpj: res.cnpj,
        razaoSocial: res.razao_social,
        nomeFantasia: res.nome_fantasia || res.razao_social,
        logradouro: res.logradouro,
        numero: res.numero,
        complemento: res.complemento,
        bairro: res.bairro,
        municipio: res.municipio,
        uf: res.uf,
        cep: res.cep,
        telefone: res.ddd_telefone_1,
        situacao: res.descricao_situacao_cadastral,
      })),
      catchError(() => throwError(() => new Error('CNPJ não encontrado ou inválido.')))
    );
  }

  efetuarCadastro(
    dadosEmpresa: Partial<DadosEmpresa>,
    responsavel: Partial<ResponsavelAcesso>,
    plano: Partial<PlanoSelecionado>
  ): Observable<RespostaCadastro> {
    // TODO: Implementar chamada real ao backend
    return this.http.post<RespostaCadastro>(`${this.baseUrl}/novo`, {
      empresa: dadosEmpresa,
      responsavel,
      plano,
    });
  }
}
