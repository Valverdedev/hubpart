-- =============================================================================
-- AutoPartsHub — Script DDL
-- Gerado a partir dos mappings EF Core
-- PostgreSQL 16
-- =============================================================================

-- -----------------------------------------------------------------------------
-- Tabelas de referência (dados externos, sem multi-tenancy)
-- Ordem: estados antes de municipios (FK)
-- -----------------------------------------------------------------------------

CREATE TABLE estados (
    codigo_uf   integer                 NOT NULL,
    uf          character varying(2)    NOT NULL,
    nome        character varying(100)  NOT NULL,
    latitude    float(8)                NOT NULL,
    longitude   float(8)                NOT NULL,
    regiao      character varying(12)   NOT NULL,

    CONSTRAINT pk_estados PRIMARY KEY (codigo_uf)
);

CREATE TABLE municipios (
    codigo_ibge integer                 NOT NULL,
    nome        character varying(100)  NOT NULL,
    latitude    float(8)                NOT NULL,
    longitude   float(8)                NOT NULL,
    capital     boolean                 NOT NULL,
    codigo_uf   integer                 NOT NULL,
    siafi_id    character varying(4)    NOT NULL,
    ddd         integer                 NOT NULL,
    fuso_horario character varying(32)  NOT NULL,

    CONSTRAINT pk_municipios        PRIMARY KEY (codigo_ibge),
    CONSTRAINT uq_municipios_siafi  UNIQUE (siafi_id),
    CONSTRAINT fk_municipios_estado FOREIGN KEY (codigo_uf) REFERENCES estados (codigo_uf)
);

-- -----------------------------------------------------------------------------
-- Tenants (aggregate raiz — TenantId = Id)
-- -----------------------------------------------------------------------------

CREATE TABLE tenants (
    -- EntidadeBase
    id                          uuid                        NOT NULL DEFAULT gen_random_uuid(),
    tenant_id                   uuid                        NOT NULL,
    criado_em                   timestamp with time zone    NOT NULL DEFAULT now(),
    atualizado_em               timestamp with time zone,
    excluido_em                 timestamp with time zone,

    -- Dados cadastrais
    razao_social                character varying(200)      NOT NULL,
    nome_fantasia               character varying(200)      NOT NULL,

    -- VO: Cnpj (OwnsOne → coluna inline)
    cnpj                        character varying(14)       NOT NULL,

    -- VO: Email (OwnsOne → coluna inline)
    email                       character varying(256)      NOT NULL,

    -- Enums (armazenados como string)
    tipo                        character varying(20)       NOT NULL,    -- Oficina | Frota | Revenda | Fornecedor
    status                      character varying(30)       NOT NULL,    -- AguardandoAprovacao | Ativo | Suspenso | Cancelado
    plano                       character varying(20)       NOT NULL,    -- Free | Basico | Profissional | Enterprise

    -- Assinatura / trial
    trial_expira_em             timestamp with time zone    NOT NULL,
    assinatura_renova_em        timestamp with time zone,
    cotacoes_usadas_no_ciclo    integer                     NOT NULL DEFAULT 0,

    -- Localização (opcional — fornecedores sem lat/lng não recebem notificações)
    latitude                    double precision,
    longitude                   double precision,

    -- VO: Endereco (OwnsOne → colunas com prefixo end_)
    end_cep                     character varying(8),
    end_logradouro              character varying(200),
    end_numero                  character varying(20),
    end_complemento             character varying(100),
    end_bairro                  character varying(100),
    end_codigo_ibge             integer,
    end_codigo_uf               integer,

    CONSTRAINT pk_tenants           PRIMARY KEY (id),
    CONSTRAINT fk_tenants_municipio FOREIGN KEY (end_codigo_ibge) REFERENCES municipios (codigo_ibge),
    CONSTRAINT fk_tenants_estado    FOREIGN KEY (end_codigo_uf)   REFERENCES estados    (codigo_uf)
);

-- CNPJ único na plataforma inteira — sem filtro de tenant
CREATE UNIQUE INDEX ix_tenants_cnpj      ON tenants (cnpj);

-- Índice de suporte para o Global Query Filter (tenant_id + soft delete)
CREATE INDEX ix_tenants_tenant_id        ON tenants (tenant_id) WHERE excluido_em IS NULL;

-- -----------------------------------------------------------------------------
-- Telefones do tenant (OwnsMany → tabela separada)
-- -----------------------------------------------------------------------------

CREATE TABLE tenant_telefones (
    id          serial                  NOT NULL,
    tenant_id   uuid                    NOT NULL,
    valor       character varying(11)   NOT NULL,   -- apenas dígitos (10 ou 11)
    ddd         character varying(2)    NOT NULL,
    tipo        character varying(10)   NOT NULL,   -- Fixo | Celular

    CONSTRAINT pk_tenant_telefones      PRIMARY KEY (id),
    CONSTRAINT fk_tenant_telefones_tenant
        FOREIGN KEY (tenant_id) REFERENCES tenants (id) ON DELETE CASCADE
);

CREATE INDEX ix_tenant_telefones_tenant_id ON tenant_telefones (tenant_id);
