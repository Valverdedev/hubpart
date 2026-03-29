# Especificação: Cadastro do Comprador — MVP v1.0

## Metadados

| Campo | Valor |
|---|---|
| Versão | 1.0 — MVP |
| Status | Especificação finalizada |
| Escopo | Somente fluxo do comprador |
| Pagamento | Fase 2 — campos visíveis, sem integração |
| Stack | Angular 17+ · ASP.NET Core 8 · PostgreSQL 16 |

---

## 1. Visão Geral

O portal de onboarding (`angular-onboarding`) é um projeto Angular 17+ **separado** do portal operacional (`angular-portal`), hospedado em `cadastro.autopartshub.com.br`. Ele serve compradores e fornecedores com fluxos independentes, sem autenticação prévia.

Este documento especifica exclusivamente o **fluxo do comprador**.

### Por que projeto Angular separado

- **Deploy independente** — `cadastro.autopartshub.com.br` sem impactar o portal operacional.
- **Bundle menor** — sem NgRx, SignalR ou features operacionais. Relevante para taxa de conversão.
- **Ciclo de vida diferente** — marketing altera o onboarding com muito mais frequência que o portal operacional.

---

## 2. Tipos de Comprador

O tipo é escolhido no Step 1 e define variações de campos nos steps seguintes, o segmento de veículos no catálogo de cotação e o plano inicial.

| Tipo | Enum | Plano Inicial | Observação |
|---|---|---|---|
| Oficina mecânica — carros | `OficinaCarro` | Trial 30 dias | Padrão. Catálogo: carros e utilitários. |
| Oficina mecânica — motos | `OficinaMoto` | Trial 30 dias | Catálogo filtrado: somente motos. |
| Logista / revenda de carros | `Logista` | Trial 30 dias | Revisões e preparação de estoque. |
| Frotista | `Frotista` | Trial 30 dias | Campos extras: segmento, qtd. veículos, alçada. |
| Outro | `Outro` | Free direto | Sem trial. Comprador avulso. |

### Tipo "Outro" — regras especiais

- Entra diretamente no plano `Free` (10 cotações/mês, 1 usuário) **sem período de trial**.
- `TrialExpiraEm = null`.
- O campo livre "descreva sua empresa" é salvo em `descricao_outro` no tenant para análise futura.
- Não requer aprovação manual — acesso imediato após verificação de e-mail.

---

## 3. Stepper de Cadastro — 5 Etapas

### Comportamento geral

- Navegação retroativa permitida.
- Ao voltar ao Step 1 e mudar o tipo: campos exclusivos do tipo anterior são limpos, campos comuns são preservados.
- Validações **inline** (ao sair do campo): CNPJ, CEP, e-mail único.
- Demais validações: ao clicar em "Próximo".
- O rascunho é criado no Step 2 ao sair do campo CNPJ (após validação ReceitaWS bem-sucedida).
- O rascunho é atualizado a cada clique em "Próximo".

---

### Step 1 — Tipo de empresa

Seleção única do tipo de comprador. Nenhuma chamada à API neste step.

---

### Step 2 — Dados da empresa

| Campo | Tipo / Máscara | Obrig. | Observação |
|---|---|---|---|
| CNPJ | `00.000.000/0000-00` | Sim | Valida via ReceitaWS ao sair. Preenche razão social e endereço. |
| Razão Social | Texto, máx. 200 | Sim | Preenchido automaticamente. Editável. |
| Nome Fantasia | Texto, máx. 200 | Sim | Exibido na plataforma. Pode diferir da Razão Social. |
| Telefone comercial | `(00) 00000-0000` | Sim | WhatsApp preferencial para notificações críticas. |
| Inscrição Estadual | Número ou "Isento" | Não | Necessária para NF-e dos fornecedores. |
| CEP | `00000-000` | Sim | Consulta ViaCEP. Preenche logradouro, bairro, cidade, estado. |
| Logradouro | Texto | Sim | Preenchido via ViaCEP. Editável. |
| Número | Texto | Sim | Foco automático após preenchimento do CEP. |
| Complemento | Texto | Não | Opcional. |
| Bairro | Texto | Sim | Preenchido via ViaCEP. Editável. |
| Cidade | Texto readonly | Sim | Preenchido via ViaCEP. Não editável. |
| Estado | Texto readonly (2 chars) | Sim | Preenchido via ViaCEP. Não editável. |

**Variação Frotista** — campos adicionais após Telefone:

| Campo | Tipo | Observação |
|---|---|---|
| Segmento da frota | Select | Transporte · Logística · Construção · Saúde · Outro |
| Quantidade estimada de veículos | Número inteiro | Informativo — usado para sugerir plano |

---

### Step 3 — Responsável e acesso

| Campo | Tipo / Máscara | Obrig. | Observação |
|---|---|---|---|
| Nome completo | Texto, máx. 200 | Sim | Administrador do tenant. |
| CPF | `000.000.000-00` | Sim | Validação de dígito verificador no frontend. |
| E-mail | E-mail válido | Sim | Único na plataforma. Verificado por link após cadastro. |
| Cargo | Select | Sim | Proprietário · Gerente · Comprador · Outro |
| Celular / WhatsApp | `(00) 00000-0000` | Sim | Fallback SMS via Twilio para alertas críticos. |
| Como nos conheceu? | Select | Não | Google · Indicação · Redes sociais · Evento · Outro |
| Senha | Password | Sim | Mín. 8 chars, 1 número, 1 minúscula. FluentValidation. |
| Confirmar senha | Password | Sim | Deve ser idêntica à senha. |
| Aceites | Checkboxes | Sim | Termos de uso + Política de privacidade. Registrado com timestamp e IP. |

**Variação Frotista** — campo adicional:

| Campo | Tipo | Observação |
|---|---|---|
| Limite de aprovação individual (R$) | Decimal | Valor máximo sem segundo aprovador. Vazio = ilimitado. |

---

### Step 4 — Plano

O usuário escolhe o plano desejado. O pagamento está previsto para a **Fase 2**.

#### Tabela de planos

| Plano | Preço/mês | Cotações/mês | Usuários | Comissão (Fase 2) |
|---|---|---|---|---|
| Free / Trial | R$ 0 (30 dias) | 10 | 1 | — |
| Básico | R$ 149 | 50 | 2 | 3% |
| Profissional | R$ 349 | 200 | 5 | 2% |
| Enterprise | R$ 799+ | Ilimitado | Ilimitado | 1,5% |

#### Comportamento dos campos de cartão no MVP

- Campos exibidos com `opacity` reduzida e `pointer-events: none`.
- Label "Em breve" — não "desabilitado".
- Nenhum dado de pagamento coletado ou validado nesta fase.
- Para plano Trial: seção de pagamento colapsa com mensagem "Nenhum cartão necessário".
- Integração `pagarme.js` adicionada na Fase 2 sem redesenho da tela.

---

### Step 5 — Confirmação

Tela final após `POST /onboarding/finalizar` retornar sucesso.

- Link de verificação enviado via AWS SES imediatamente após criação do tenant.
- Trial começa a contar a partir do **submit** (não da verificação do e-mail).
- Botão "Acessar o portal" redireciona para o `angular-portal` (login).
- Botão "Convidar usuários" redireciona para o painel de usuários após login.

---

## 4. Rascunho Server-Side (Opção B)

O rascunho preserva o progresso entre sessões e dispositivos. A tabela `onboarding_rascunho` é completamente isolada do multi-tenancy — **sem `tenant_id`, sem Global Query Filter**.

### Schema da tabela `onboarding_rascunho`

| Campo | Tipo | Obrig. | Observação |
|---|---|---|---|
| `id` | `uuid PK` | Sim | `gen_random_uuid()` |
| `session_token` | `uuid UNIQUE` | Sim | Enviado ao browser. Cookie httpOnly + localStorage. |
| `tipo_perfil` | `varchar(40)` | Sim | Enum: `OficinaCarro \| OficinaMoto \| Logista \| Frotista \| Outro` |
| `ultimo_step` | `smallint DEFAULT 0` | Sim | Permite retomar do ponto exato de abandono. |
| `dados` | `jsonb DEFAULT '{}'` | Sim | Acumula campos de cada step. Flexível a mudanças. |
| `email` | `varchar(256) NULL` | Não | Preenchido no Step 3. Usado para e-mail de retomada. |
| `ip_origem` | `inet NULL` | Não | Registro para análise de fraude. |
| `user_agent` | `text NULL` | Não | Registro para análise de fraude. |
| `criado_em` | `timestamptz DEFAULT now()` | Sim | Referência para job de limpeza. |
| `atualizado_em` | `timestamptz DEFAULT now()` | Sim | Atualizado a cada `PUT /rascunho/{token}`. |

Índices:
- `UNIQUE` em `session_token`
- Parcial em `email WHERE email IS NOT NULL`
- Em `criado_em` (para o job de limpeza)

### Quando o rascunho é salvo

- **Criado** (`POST /onboarding/iniciar`): ao sair do campo CNPJ no Step 2, após validação ReceitaWS bem-sucedida.
- **Atualizado** (`PUT /onboarding/rascunho/{token}`): a cada clique em "Próximo".
- **Deletado**: imediatamente após `POST /onboarding/finalizar` criar o tenant com sucesso.

### Retomada por link

- URL do e-mail: `cadastro.autopartshub.com.br?token={sessionToken}`
- Angular lê o token da query string, chama `GET /onboarding/rascunho/{token}` e reposiciona o stepper no `ultimo_step`.
- Token não encontrado (expirado ou finalizado): exibe mensagem amigável e inicia novo cadastro.

### Job de limpeza

- Hangfire — diário às 03h00 UTC.
- Deleta rascunhos com `criado_em < now() - 7 days`.
- Antes de deletar: se `email` preenchido e e-mail de retomada não enviado (verifica flag Redis `retomada_enviada:{sessionToken}`), dispara e-mail antes da exclusão.
- Registra contagem removida no CloudWatch.

### E-mails de retomada

- **D+2**: "você começou seu cadastro, continue de onde parou".
- **D+5**: segundo lembrete antes da expiração do rascunho.
- Condição: `email != null` e rascunho ainda existente.

---

## 5. Planos e Trial

### Máquina de estados do tenant

| Status | Plano | Limites | Quando ocorre |
|---|---|---|---|
| `Trial` | Plano escolhido no cadastro | Limites do plano escolhido | Cadastro finalizado (exceto tipo `Outro`). |
| `Free` permanente | `Free` | 10 cotações/mês · 1 usuário | Trial expirado sem pagamento configurado. |
| `Ativo` | Plano contratado | Conforme plano | Assinatura paga via Pagar.me (Fase 2). |
| `Bloqueado` | Qualquer | Sem novas cotações | Inadimplência (Fase 2). |

### Campos no tenant relacionados a planos

```
plano_atual            PlanoAssinatura  — Free | Basico | Profissional | Enterprise
assinatura_status      StatusAssinatura — Trial | Free | Ativo | Bloqueado
trial_expira_em        timestamptz?     — null para tipo Outro; now() + 30 days para demais
cotacoes_limite_mes    smallint         — desnormalizado do plano para consulta rápida
usuarios_limite        smallint         — desnormalizado do plano para consulta rápida
```

Valores desnormalizados por plano:

| Plano | `cotacoes_limite_mes` | `usuarios_limite` |
|---|---|---|
| Free | 10 | 1 |
| Basico | 50 | 2 |
| Profissional | 200 | 5 |
| Enterprise | `int.MaxValue` | `int.MaxValue` |

### Job `ExpirarTrialJob` — Hangfire, diário às 02h00 UTC

Três queries independentes no mesmo job:

**D-7:** `TrialExpiraEm BETWEEN now() AND now() + 7 days AND StatusAssinatura = Trial`
→ Publica `AlertaTrialEvent` com tipo `"D7"` → e-mail SES "trial termina em 7 dias".

**D-1:** `TrialExpiraEm BETWEEN now() AND now() + 1 day AND StatusAssinatura = Trial`
→ Publica `AlertaTrialEvent` com tipo `"D1"` → e-mail SES "trial termina amanhã".

**Expirados:** `StatusAssinatura = Trial AND TrialExpiraEm < now()`
→ Para cada tenant:
1. `UPDATE`: `StatusAssinatura = Free`, `PlanoAtual = Free`, `CotacoesLimiteMes = 10`, `UsuariosLimite = 1`.
2. Invalida cache Redis: `DEL tenant:{tenantId}`.
3. Publica `TrialExpiradoEvent` → e-mail SES "trial expirou".
4. Registra contagem no CloudWatch.

### Usuários acima do limite após rebaixamento

- Usuários existentes **não são desativados**.
- Novos convites bloqueados até upgrade de plano.
- Painel exibe aviso: "Você atingiu o limite de usuários do plano Free".

---

## 6. Enforcement de Cotas

### Schema da tabela `cotacao_uso_mensal`

| Campo | Tipo | Observação |
|---|---|---|
| `tenant_id` | `uuid` | Parte da PK composta. FK para `tenants`. |
| `ano_mes` | `char(7)` | Formato `YYYY-MM`. Parte da PK composta. |
| `total_cotacoes` | `integer DEFAULT 0` | Incrementado atomicamente. |
| `atualizado_em` | `timestamptz` | Última atualização. |

`PRIMARY KEY (tenant_id, ano_mes)` — sem auto-increment, sem sequence.

### Fluxo de verificação ao criar cotação

1. `ITenantContext` carrega `cotacoes_limite_mes` via JWT + cache Redis (TTL 5 min).
2. `CriarCotacaoCommandHandler` consulta `cotacao_uso_mensal` para o mês atual.
3. Se `total_cotacoes >= cotacoes_limite_mes` → `Result.Fail("cota_mensal_atingida")`.
4. Cotação criada → upsert atômico:

```sql
INSERT INTO cotacao_uso_mensal (tenant_id, ano_mes, total_cotacoes, atualizado_em)
VALUES (@tenantId, @anoMes, 1, now())
ON CONFLICT (tenant_id, ano_mes)
DO UPDATE SET
  total_cotacoes = cotacao_uso_mensal.total_cotacoes + 1,
  atualizado_em  = now();
```

5. Cache Redis do contador invalidado: `DEL cotacao_uso:{tenantId}:{anoMes}`.

### Enforcement de usuários convidados

- `SELECT COUNT(*) FROM usuarios WHERE tenant_id = X`.
- Se `count >= usuarios_limite` → `Result.Fail("limite_usuarios_atingido")`.
- Usuários existentes nunca removidos por rebaixamento.

---

## 7. Endpoints da API de Onboarding

Todos os endpoints são **públicos** (sem `[Authorize]`). Rate limiting por IP.

| Método | Endpoint | Rate limit | Descrição |
|---|---|---|---|
| `GET` | `/api/v1/onboarding/cnpj/{cnpj}` | 30 req/min por IP | Consulta ReceitaWS. Retorna dados da empresa. |
| `GET` | `/api/v1/onboarding/cep/{cep}` | 60 req/min por IP | Consulta ViaCEP. Retorna dados de endereço. |
| `POST` | `/api/v1/onboarding/iniciar` | 10 req/min por IP | Cria rascunho. Retorna `sessionToken`. |
| `PUT` | `/api/v1/onboarding/rascunho/{token}` | — | Atualiza dados parciais do step. |
| `GET` | `/api/v1/onboarding/rascunho/{token}` | — | Retorna rascunho para retomada. |
| `POST` | `/api/v1/onboarding/finalizar/{token}` | 5 req/min por IP | Cria tenant + usuário. Idempotente. |
| `POST` | `/api/v1/onboarding/reenviar-verificacao` | 3 req/hora por e-mail | Reenvia link de verificação de e-mail. |

### Idempotência do `/finalizar`

Sempre retorna `HTTP 200`. Nunca `409`.

| Situação | `status` | `redirectTo` |
|---|---|---|
| Tenant criado com sucesso | `"criado"` | `"/login"` |
| Token não existe, CNPJ já é tenant | `"ja_cadastrado"` | `"/login"` |
| CNPJ já é tenant (outro usuário) | `"empresa_ja_cadastrada"` | `"/login"` |
| Token não existe e sem cache | `HTTP 400` `"sessao_expirada"` | — |

### Verificação de e-mail

- Link enviado via AWS SES após `CadastrarCompradorCommand` concluir.
- Link expira em **24 horas**.
- Formato: `portal.autopartshub.com.br/verificar?token={emailToken}`.
- Antes de verificar: usuário consegue logar, mas vê banner de aviso. Acesso ao portal permitido.
- Trial começa no **submit do cadastro**, não na verificação.
- Reenvio: máximo **3 tentativas/hora** por e-mail (Redis: `reenvio_email:{email}`).

---

## 8. Schema do Tenant — Campos Novos

Campos adicionados à tabela `tenants` para suportar o onboarding do comprador:

| Campo | Tipo | Obrig. | Observação |
|---|---|---|---|
| `tipo_comprador` | `varchar(40)` | Sim | Enum persistido como string. |
| `plano_atual` | `varchar(20)` | Sim | Enum persistido como string. |
| `assinatura_status` | `varchar(20)` | Sim | Enum persistido como string. |
| `trial_expira_em` | `timestamptz NULL` | Não | `null` para tipo `Outro`. |
| `cotacoes_limite_mes` | `smallint` | Sim | Desnormalizado. Atualizado pelo job. |
| `usuarios_limite` | `smallint` | Sim | Desnormalizado. Atualizado pelo job. |
| `segmento_frota` | `varchar(40) NULL` | Não | Somente `Frotista`. |
| `qtd_veiculos_estimada` | `smallint NULL` | Não | Somente `Frotista`. Informativo. |
| `limite_aprovacao_admin` | `numeric(18,2) NULL` | Não | Somente `Frotista`. Alçada do admin. |
| `descricao_outro` | `text NULL` | Não | Somente `Outro`. Campo livre. |
| `como_nos_conheceu` | `varchar(40) NULL` | Não | Atribuição de marketing. |

---

## 9. Decisões Registradas

| Tema | Decisão | Justificativa |
|---|---|---|
| Rascunho | Opção B — server-side | Tabela separada. Rascunho ≠ tenant. Zero impacto no multi-tenancy. |
| Pagamento | Fase 2 | Campos visíveis e desabilitados. Sem integração Pagar.me no MVP. |
| CNPJ duplicado | Redireciona para login | `HTTP 200` com `redirectTo: /login`. Sem revelar detalhes do tenant. |
| Início do trial | No submit do cadastro | `trial_expira_em = now() + 30 days` no command. Simples. |
| Trial expirado | Rebaixa para Free | Sem bloquear acesso. Usuários existentes mantidos. |
| Tipo Outro | Free direto, sem trial | Comprador avulso de baixo volume. Campo livre para análise futura. |
| Alertas trial | D-7 e D-1 no `ExpirarTrialJob` | Mesmo job, três queries, três templates SES. |
| Double submit | Idempotente | Busca tenant por CNPJ. Retorna `200` com `redirectTo`. |
| Acesso pré-verificação | Permitido com banner | Trial começa no submit. Sem bloquear acesso antes da verificação. |
| Fornecedor pré-aprovação | Cadastra estoque, sem cotações | Pode preparar catálogo. Não recebe cotações até aprovação admin. |

---

## 10. Fora do Escopo — MVP

- Integração Pagar.me — pagamento de assinatura e split de comissão (Fase 2).
- Upgrade/downgrade de plano via portal (Fase 2, após Pagar.me).
- Fluxo de inadimplência e estado `Bloqueado` (Fase 2).
- Rate limiting nos endpoints públicos — implementar antes de ir para produção.
- Cadastro do fornecedor — especificação separada.
- Convite de usuários adicionais — fluxo e-mail → aceite → vinculação ao tenant.
- `SesEmailService` real — usar `MockEmailService` no MVP.
- App MAUI para fornecedores (Fase 3).
