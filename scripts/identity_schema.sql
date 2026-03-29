-- =============================================================================
-- AutoPartsHub — Tabelas do ASP.NET Identity
-- Schema: public (padrão PostgreSQL)
--
-- Executar uma única vez no banco de dados autopartshub.
-- Compatível com PostgreSQL 16+.
-- Seguro para re-executar (IF NOT EXISTS em todas as instruções).
-- =============================================================================

-- -----------------------------------------------------------------------------
-- usuarios  (IdentityUser<Guid> + campos extras de UsuarioApp)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS usuarios (
    id                     uuid            NOT NULL DEFAULT gen_random_uuid(),
    tenant_id              uuid            NOT NULL,
    nome_completo          varchar(200)    NOT NULL DEFAULT '',
    telefone               varchar(20)     NULL,
    limite_aprovacao       numeric(18, 2)  NOT NULL DEFAULT 0,
    ultimo_login_em        timestamptz     NULL,

    -- Campos herdados de IdentityUser<Guid>
    user_name              varchar(256)    NULL,
    normalized_user_name   varchar(256)    NULL,
    email                  varchar(256)    NULL,
    normalized_email       varchar(256)    NULL,
    email_confirmed        boolean         NOT NULL DEFAULT false,
    password_hash          text            NULL,
    security_stamp         text            NULL,
    concurrency_stamp      text            NULL,
    phone_number           text            NULL,
    phone_number_confirmed boolean         NOT NULL DEFAULT false,
    two_factor_enabled     boolean         NOT NULL DEFAULT false,
    lockout_end            timestamptz     NULL,
    lockout_enabled        boolean         NOT NULL DEFAULT true,
    access_failed_count    integer         NOT NULL DEFAULT 0,

    CONSTRAINT pk_usuarios PRIMARY KEY (id)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_usuarios_normalized_user_name
    ON usuarios (normalized_user_name)
    WHERE normalized_user_name IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_usuarios_normalized_email
    ON usuarios (normalized_email)
    WHERE normalized_email IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_usuarios_tenant_id
    ON usuarios (tenant_id);

-- -----------------------------------------------------------------------------
-- roles  (IdentityRole<Guid>)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS roles (
    id                uuid         NOT NULL DEFAULT gen_random_uuid(),
    name              varchar(256) NULL,
    normalized_name   varchar(256) NULL,
    concurrency_stamp text         NULL,

    CONSTRAINT pk_roles PRIMARY KEY (id)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_roles_normalized_name
    ON roles (normalized_name)
    WHERE normalized_name IS NOT NULL;

-- -----------------------------------------------------------------------------
-- usuario_roles  (IdentityUserRole<Guid>)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS usuario_roles (
    user_id uuid NOT NULL,
    role_id uuid NOT NULL,

    CONSTRAINT pk_usuario_roles PRIMARY KEY (user_id, role_id),
    CONSTRAINT fk_usuario_roles_usuario FOREIGN KEY (user_id)
        REFERENCES usuarios (id) ON DELETE CASCADE,
    CONSTRAINT fk_usuario_roles_role FOREIGN KEY (role_id)
        REFERENCES roles (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_usuario_roles_role_id
    ON usuario_roles (role_id);

-- -----------------------------------------------------------------------------
-- usuario_claims  (IdentityUserClaim<Guid>)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS usuario_claims (
    id          serial NOT NULL,
    user_id     uuid   NOT NULL,
    claim_type  text   NULL,
    claim_value text   NULL,

    CONSTRAINT pk_usuario_claims PRIMARY KEY (id),
    CONSTRAINT fk_usuario_claims_usuario FOREIGN KEY (user_id)
        REFERENCES usuarios (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_usuario_claims_user_id
    ON usuario_claims (user_id);

-- -----------------------------------------------------------------------------
-- usuario_logins  (IdentityUserLogin<Guid>)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS usuario_logins (
    login_provider        varchar(128) NOT NULL,
    provider_key          varchar(128) NOT NULL,
    provider_display_name text         NULL,
    user_id               uuid         NOT NULL,

    CONSTRAINT pk_usuario_logins PRIMARY KEY (login_provider, provider_key),
    CONSTRAINT fk_usuario_logins_usuario FOREIGN KEY (user_id)
        REFERENCES usuarios (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_usuario_logins_user_id
    ON usuario_logins (user_id);

-- -----------------------------------------------------------------------------
-- role_claims  (IdentityRoleClaim<Guid>)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS role_claims (
    id          serial NOT NULL,
    role_id     uuid   NOT NULL,
    claim_type  text   NULL,
    claim_value text   NULL,

    CONSTRAINT pk_role_claims PRIMARY KEY (id),
    CONSTRAINT fk_role_claims_role FOREIGN KEY (role_id)
        REFERENCES roles (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_role_claims_role_id
    ON role_claims (role_id);

-- -----------------------------------------------------------------------------
-- usuario_tokens  (IdentityUserToken<Guid>)
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS usuario_tokens (
    user_id        uuid         NOT NULL,
    login_provider varchar(128) NOT NULL,
    name           varchar(128) NOT NULL,
    value          text         NULL,

    CONSTRAINT pk_usuario_tokens PRIMARY KEY (user_id, login_provider, name),
    CONSTRAINT fk_usuario_tokens_usuario FOREIGN KEY (user_id)
        REFERENCES usuarios (id) ON DELETE CASCADE
);

-- -----------------------------------------------------------------------------
-- refresh_tokens
-- -----------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS refresh_tokens (
    -- EntidadeBase
    id            uuid         NOT NULL DEFAULT gen_random_uuid(),
    tenant_id     uuid         NOT NULL,
    criado_em     timestamptz  NOT NULL DEFAULT now(),
    atualizado_em timestamptz  NULL,
    excluido_em   timestamptz  NULL,

    -- Campos específicos do token
    -- token_hash: SHA-256 (hex lowercase, 64 chars) do valor bruto que trafega apenas no HTTP
    token_hash    varchar(64)  NOT NULL,
    usuario_id    uuid         NOT NULL,
    expira_em     timestamptz  NOT NULL,
    usado_em      timestamptz  NULL,
    revogado      boolean      NOT NULL DEFAULT false,

    CONSTRAINT pk_refresh_tokens PRIMARY KEY (id),
    CONSTRAINT fk_refresh_tokens_usuario FOREIGN KEY (usuario_id)
        REFERENCES usuarios (id) ON DELETE CASCADE
);

-- Apenas o hash é indexado — o valor bruto nunca é persistido
CREATE UNIQUE INDEX IF NOT EXISTS ux_refresh_tokens_token_hash
    ON refresh_tokens (token_hash);

CREATE INDEX IF NOT EXISTS ix_refresh_tokens_usuario_revogado
    ON refresh_tokens (usuario_id, revogado);

-- -----------------------------------------------------------------------------
-- Roles padrão do sistema
-- -----------------------------------------------------------------------------

INSERT INTO roles (id, name, normalized_name, concurrency_stamp)
VALUES
    (gen_random_uuid(), 'Admin',      'ADMIN',      gen_random_uuid()::text),
    (gen_random_uuid(), 'Comprador',  'COMPRADOR',  gen_random_uuid()::text),
    (gen_random_uuid(), 'Fornecedor', 'FORNECEDOR', gen_random_uuid()::text),
    (gen_random_uuid(), 'Aprovador',  'APROVADOR',  gen_random_uuid()::text)
ON CONFLICT DO NOTHING;
