-- Onboarding comprador (API)
-- Script incremental e idempotente, sem EF migrations.
-- Data: 2026-03-29

BEGIN;

-- ------------------------------------------------------------------
-- Tabela publica de rascunho (sem tenant_id)
-- ------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS onboarding_rascunho (
    id uuid NOT NULL,
    session_token uuid NOT NULL,
    tipo_perfil varchar(40) NOT NULL,
    ultimo_step integer NOT NULL DEFAULT 0,
    dados jsonb NOT NULL DEFAULT '{}'::jsonb,
    email varchar(256),
    ip_origem text,
    user_agent text,
    criado_em timestamptz NOT NULL DEFAULT now(),
    atualizado_em timestamptz NOT NULL,
    CONSTRAINT pk_onboarding_rascunho PRIMARY KEY (id)
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_onboarding_rascunho_session_token
    ON onboarding_rascunho (session_token);

CREATE INDEX IF NOT EXISTS ix_onboarding_rascunho_email
    ON onboarding_rascunho (email)
    WHERE email IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_onboarding_rascunho_criado_em
    ON onboarding_rascunho (criado_em);

-- ------------------------------------------------------------------
-- Tabela de uso mensal de cotacoes (sem tenant_id de isolamento)
-- ------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS cotacao_uso_mensal (
    tenant_id uuid NOT NULL,
    ano_mes char(7) NOT NULL,
    total_cotacoes integer NOT NULL DEFAULT 0,
    atualizado_em timestamptz NOT NULL,
    CONSTRAINT pk_cotacao_uso_mensal PRIMARY KEY (tenant_id, ano_mes)
);

-- ------------------------------------------------------------------
-- Evolucao incremental da tabela tenants
-- ------------------------------------------------------------------
ALTER TABLE tenants
    ADD COLUMN IF NOT EXISTS tipo_comprador varchar(40),
    ADD COLUMN IF NOT EXISTS plano_atual varchar(20) NOT NULL DEFAULT 'Free',
    ADD COLUMN IF NOT EXISTS assinatura_status varchar(20) NOT NULL DEFAULT 'Free',
    ADD COLUMN IF NOT EXISTS trial_expira_em_novo timestamptz,
    ADD COLUMN IF NOT EXISTS cotacoes_limite_mes integer NOT NULL DEFAULT 10,
    ADD COLUMN IF NOT EXISTS usuarios_limite integer NOT NULL DEFAULT 1,
    ADD COLUMN IF NOT EXISTS inscricao_estadual varchar(30),
    ADD COLUMN IF NOT EXISTS telefone_comercial varchar(20) NOT NULL DEFAULT '',
    ADD COLUMN IF NOT EXISTS como_nos_conheceu varchar(40),
    ADD COLUMN IF NOT EXISTS descricao_outro text,
    ADD COLUMN IF NOT EXISTS segmento_frota varchar(40),
    ADD COLUMN IF NOT EXISTS qtd_veiculos_estimada integer,
    ADD COLUMN IF NOT EXISTS limite_aprovacao_admin numeric(18,2),
    ADD COLUMN IF NOT EXISTS endereco_cep varchar(9),
    ADD COLUMN IF NOT EXISTS endereco_logradouro varchar(200),
    ADD COLUMN IF NOT EXISTS endereco_numero varchar(20),
    ADD COLUMN IF NOT EXISTS endereco_complemento varchar(100),
    ADD COLUMN IF NOT EXISTS endereco_bairro varchar(100),
    ADD COLUMN IF NOT EXISTS endereco_cidade varchar(100),
    ADD COLUMN IF NOT EXISTS endereco_estado varchar(2);

CREATE UNIQUE INDEX IF NOT EXISTS ix_tenants_cnpj
    ON tenants (cnpj);

COMMIT;
