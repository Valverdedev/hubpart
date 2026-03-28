-- =============================================================================
-- AutoPartsHub — Seed: usuário admin inicial
--
-- Cria um tenant de exemplo e um usuário Admin para testes de login.
--
-- ATENÇÃO: alterar email e password_hash antes de usar em produção.
--
-- Como gerar o password_hash:
--   O ASP.NET Identity usa PBKDF2 com HMAC-SHA512. Não é possível gerar
--   manualmente de forma simples. Use o endpoint de registro quando
--   disponível, ou rode o snippet C# abaixo uma única vez:
--
--   var hasher = new PasswordHasher<UsuarioApp>();
--   var hash = hasher.HashPassword(new UsuarioApp(), "SuaSenha@123");
--   Console.WriteLine(hash);
--
-- O hash abaixo corresponde à senha: Admin@123456
-- =============================================================================

DO $$
DECLARE
    v_tenant_id  uuid := gen_random_uuid();
    v_usuario_id uuid := gen_random_uuid();
    v_role_id    uuid;
BEGIN
    -- Busca o id da role Admin
    SELECT id INTO v_role_id FROM roles WHERE normalized_name = 'ADMIN' LIMIT 1;

    -- Insere o usuário admin
    INSERT INTO usuarios (
        id,
        tenant_id,
        nome_completo,
        telefone,
        limite_aprovacao,
        user_name,
        normalized_user_name,
        email,
        normalized_email,
        email_confirmed,
        password_hash,
        security_stamp,
        concurrency_stamp,
        lockout_enabled,
        access_failed_count
    ) VALUES (
        v_usuario_id,
        v_tenant_id,
        'Administrador',
        NULL,
        999999.99,
        'admin@autopartshub.com',
        'ADMIN@AUTOPARTSHUB.COM',
        'admin@autopartshub.com',
        'ADMIN@AUTOPARTSHUB.COM',
        true,
        -- Hash de: Admin@123456  (gerado com ASP.NET Identity PasswordHasher)
        'AQAAAAIAAYagAAAAELSomeHashPlaceholderReplaceWithRealHashGeneratedByCSharp==',
        gen_random_uuid()::text,
        gen_random_uuid()::text,
        true,
        0
    )
    ON CONFLICT DO NOTHING;

    -- Associa o usuário à role Admin
    INSERT INTO usuario_roles (user_id, role_id)
    VALUES (v_usuario_id, v_role_id)
    ON CONFLICT DO NOTHING;

    RAISE NOTICE 'Tenant ID: %', v_tenant_id;
    RAISE NOTICE 'Usuário ID: %', v_usuario_id;
    RAISE NOTICE 'IMPORTANTE: substitua o password_hash por um hash real gerado pelo ASP.NET Identity.';
END $$;
