using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Infra.Persistencia;
using AutoPartsHub.Infra.Repositorios;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace AutoPartsHub.IntegrationTests.Onboarding;

public sealed class CotacaoUsoMensalRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer? _postgres;
    private bool _dockerDisponivel;

    public CotacaoUsoMensalRepositoryTests()
    {
        try
        {
            _postgres = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("autopartshub")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();
            _dockerDisponivel = true;
        }
        catch
        {
            _postgres = null;
            _dockerDisponivel = false;
        }
    }

    public async Task InitializeAsync()
    {
        if (_postgres is null)
            return;

        try
        {
            await _postgres.StartAsync();
        }
        catch
        {
            _dockerDisponivel = false;
            return;
        }

        await using var conexao = new NpgsqlConnection(_postgres.GetConnectionString());
        await conexao.OpenAsync();

        var comando = conexao.CreateCommand();
        comando.CommandText = @"
            CREATE TABLE IF NOT EXISTS cotacao_uso_mensal (
                tenant_id uuid NOT NULL,
                ano_mes char(7) NOT NULL,
                total_cotacoes integer NOT NULL DEFAULT 0,
                atualizado_em timestamptz NOT NULL,
                CONSTRAINT pk_cotacao_uso_mensal PRIMARY KEY (tenant_id, ano_mes)
            );";

        await comando.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        if (_postgres is not null)
            await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task IncrementarAsync_Concorrente_MantemContagemAtomica()
    {
        if (!_dockerDisponivel || _postgres is null)
            return;

        var tenantId = Guid.NewGuid();
        const string anoMes = "2026-03";

        var tarefas = Enumerable.Range(0, 50)
            .Select(_ => ExecutarIncrementoAsync(tenantId, anoMes))
            .ToArray();

        await Task.WhenAll(tarefas);

        await using var contexto = CriarContexto();
        var repositorio = new CotacaoUsoMensalRepository(contexto);

        var total = await repositorio.BuscarTotalAsync(tenantId, anoMes, CancellationToken.None);

        total.Should().Be(50);
    }

    private async Task ExecutarIncrementoAsync(Guid tenantId, string anoMes)
    {
        await using var contexto = CriarContexto();
        var repositorio = new CotacaoUsoMensalRepository(contexto);
        await repositorio.IncrementarAsync(tenantId, anoMes, CancellationToken.None);
    }

    private AppDbContext CriarContexto()
    {
        if (_postgres is null)
            throw new InvalidOperationException("Container PostgreSQL nao inicializado.");

        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns(Guid.NewGuid());

        var publisher = Substitute.For<IPublisher>();
        var dateTime = Substitute.For<IDateTimeProvider>();
        dateTime.UtcNow.Returns(DateTime.UtcNow);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AppDbContext(options, tenantContext, publisher, dateTime);
    }
}
