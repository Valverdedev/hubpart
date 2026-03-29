using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Infra.Persistencia;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace AutoPartsHub.IntegrationTests.Tenants;

/// <summary>
/// Verifica que o Global Query Filter isola corretamente os dados entre tenants distintos.
/// Usa AppDbContext com InMemory e ITenantContext mockado para simular requests de tenants diferentes.
/// Entidade testada: RefreshToken (herda EntidadeBase — filtro tenant + soft-delete se aplica).
/// </summary>
public sealed class TenantIsolacaoTests
{
    private static AppDbContext CriarContexto(Guid tenantId, string dbName)
    {
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns(tenantId);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var publisher = Substitute.For<MediatR.IPublisher>();
        var dateTime = Substitute.For<IDateTimeProvider>();
        dateTime.UtcNow.Returns(DateTime.UtcNow);

        return new AppDbContext(options, tenantContext, publisher, dateTime);
    }

    [Fact]
    public async Task QueryFilter_TenantA_NaoVeRefreshTokensDeTenantB()
    {
        var dbName = $"isolacao_{Guid.NewGuid()}";
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();
        var dateTime = Substitute.For<IDateTimeProvider>();
        dateTime.UtcNow.Returns(DateTime.UtcNow);

        // Insere RefreshToken do tenant B diretamente no banco compartilhado
        await using var ctxAdmin = CriarContexto(tenantBId, dbName);
        var rtB = RefreshToken.Criar("hash_do_tenantB", Guid.NewGuid(), tenantBId, dateTime);
        ctxAdmin.RefreshTokens.Add(rtB);
        await ctxAdmin.SaveChangesAsync();

        // Tenant A consulta — não deve ver o token de B
        await using var ctxA = CriarContexto(tenantAId, dbName);
        var tokensTenantA = await ctxA.RefreshTokens.ToListAsync();

        tokensTenantA.Should().BeEmpty("tenant A não pode ver tokens de tenant B");
    }

    [Fact]
    public async Task QueryFilter_TenantA_VeApenasSeusPropiosTokens()
    {
        var dbName = $"isolacao_{Guid.NewGuid()}";
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();
        var dateTime = Substitute.For<IDateTimeProvider>();
        dateTime.UtcNow.Returns(DateTime.UtcNow);

        // Insere tokens de ambos os tenants
        await using var ctxA1 = CriarContexto(tenantAId, dbName);
        var rtA = RefreshToken.Criar("hash_do_tenantA", Guid.NewGuid(), tenantAId, dateTime);
        ctxA1.RefreshTokens.Add(rtA);
        await ctxA1.SaveChangesAsync();

        await using var ctxB = CriarContexto(tenantBId, dbName);
        var rtB = RefreshToken.Criar("hash_do_tenantB", Guid.NewGuid(), tenantBId, dateTime);
        ctxB.RefreshTokens.Add(rtB);
        await ctxB.SaveChangesAsync();

        // Tenant A vê apenas os seus
        await using var ctxA2 = CriarContexto(tenantAId, dbName);
        var tokensTenantA = await ctxA2.RefreshTokens.ToListAsync();

        tokensTenantA.Should().HaveCount(1);
        tokensTenantA[0].TokenHash.Should().Be("hash_do_tenantA");
    }

    [Fact]
    public async Task QueryFilter_SoftDelete_TokenExcluidoNaoAparece()
    {
        var dbName = $"isolacao_{Guid.NewGuid()}";
        var tenantId = Guid.NewGuid();
        var dateTime = Substitute.For<IDateTimeProvider>();
        dateTime.UtcNow.Returns(DateTime.UtcNow);

        await using var ctx1 = CriarContexto(tenantId, dbName);
        var rt = RefreshToken.Criar("hash_ativo", Guid.NewGuid(), tenantId, dateTime);
        ctx1.RefreshTokens.Add(rt);
        await ctx1.SaveChangesAsync();

        // Soft delete
        await using var ctx2 = CriarContexto(tenantId, dbName);
        var tokenParaDeletar = await ctx2.RefreshTokens.FirstAsync();
        tokenParaDeletar.ExcluirLogico(dateTime);
        await ctx2.SaveChangesAsync();

        // Deve sumir da consulta normal
        await using var ctx3 = CriarContexto(tenantId, dbName);
        var tokens = await ctx3.RefreshTokens.ToListAsync();

        tokens.Should().BeEmpty("token excluído logicamente não deve aparecer");
    }
}
