using System.Text.Json;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Application.Onboarding.Commands;
using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Enums;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Domain.ValueObjects;
using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AutoPartsHub.UnitTests.Onboarding;

public sealed class CadastrarCompradorCommandHandlerTests
{
    private readonly IOnboardingRascunhoRepository _rascunhoRepo = Substitute.For<IOnboardingRascunhoRepository>();
    private readonly ITenantRepository _tenantRepo = Substitute.For<ITenantRepository>();
    private readonly IIdentidadeService _identidade = Substitute.For<IIdentidadeService>();
    private readonly ILocalizacaoService _localizacao = Substitute.For<ILocalizacaoService>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
    private readonly ILogger<CadastrarCompradorCommandHandler> _logger = Substitute.For<ILogger<CadastrarCompradorCommandHandler>>();

    private CadastrarCompradorCommandHandler CriarHandler()
    {
        _publisher.Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _tenantRepo.AdicionarAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _tenantRepo.SalvarAlteracoesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _rascunhoRepo.DeletarAsync(Arg.Any<OnboardingRascunho>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _cache.SetarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _localizacao.ResolverAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new LocalizacaoResolvidaDto(32, 3205309));

        _dateTime.UtcNow.Returns(new DateTime(2026, 3, 29, 0, 0, 0, DateTimeKind.Utc));

        return new CadastrarCompradorCommandHandler(
            _rascunhoRepo,
            _tenantRepo,
            _identidade,
            _localizacao,
            _cache,
            _publisher,
            _dateTime,
            _logger);
    }

    [Fact]
    public async Task Handle_CnpjJaCadastrado_RetornaStatusEmpresaJaCadastrada()
    {
        var rascunho = CriarRascunho();
        _rascunhoRepo.BuscarPorTokenAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(rascunho);

        var tenantExistente = CriarTenantExistente();
        _tenantRepo.ObterPorCnpjAsync("12345678000199", Arg.Any<CancellationToken>()).Returns(tenantExistente);

        var resultado = await CriarHandler().Handle(
            new CadastrarCompradorCommand(rascunho.SessionToken, PlanoAssinatura.Basico),
            CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Status.Should().Be("empresa_ja_cadastrada");
        resultado.Value.TenantId.Should().Be(tenantExistente.Id);
    }

    [Fact]
    public async Task Handle_RascunhoNaoEncontradoSemCache_RetornaSessaoExpirada()
    {
        _rascunhoRepo.BuscarPorTokenAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((OnboardingRascunho?)null);
        _cache.ObterAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((string?)null);

        var resultado = await CriarHandler().Handle(
            new CadastrarCompradorCommand(Guid.NewGuid(), PlanoAssinatura.Basico),
            CancellationToken.None);

        resultado.IsFailed.Should().BeTrue();
        resultado.Errors[0].Message.Should().Be("sessao_expirada");
    }

    [Fact]
    public async Task Handle_FluxoFeliz_CriaTenantDeletaRascunhoEPublicaEvento()
    {
        var rascunho = CriarRascunho();
        _rascunhoRepo.BuscarPorTokenAsync(rascunho.SessionToken, Arg.Any<CancellationToken>()).Returns(rascunho);
        _tenantRepo.ObterPorCnpjAsync("12345678000199", Arg.Any<CancellationToken>()).Returns((Tenant?)null);
        _identidade.CriarUsuarioAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<string?>(),
                Arg.Any<decimal>())
            .Returns(Result.Ok(Guid.NewGuid()));

        var resultado = await CriarHandler().Handle(
            new CadastrarCompradorCommand(rascunho.SessionToken, PlanoAssinatura.Profissional),
            CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Status.Should().Be("criado");

        await _tenantRepo.Received(1).AdicionarAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>());
        await _rascunhoRepo.Received(1).DeletarAsync(rascunho, Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(
            Arg.Is<CompradorCadastradoNotification>(_ => true),
            Arg.Any<CancellationToken>());
    }

    private OnboardingRascunho CriarRascunho()
    {
        var dados = new
        {
            cnpj = "12.345.678/0001-99",
            razaoSocial = "Oficina XPTO LTDA",
            nomeFantasia = "Oficina XPTO",
            telefoneComercial = "27999999999",
            cep = "29140000",
            logradouro = "Rua A",
            numero = "100",
            bairro = "Centro",
            cidade = "Vitoria",
            estado = "ES",
            nomeCompleto = "Admin XPTO",
            email = "admin@xpto.com",
            senha = "Senha123",
            limiteAprovacaoAdmin = 2500m
        };

        var rascunho = OnboardingRascunho.Criar(TipoComprador.OficinaCarro, "127.0.0.1", "agent", _dateTime);
        rascunho.Atualizar(3, JsonSerializer.Serialize(dados), "admin@xpto.com", _dateTime);
        return rascunho;
    }

    private Tenant CriarTenantExistente()
    {
        var cnpj = Cnpj.Criar("12345678000199").Value;
        var email = Email.Criar("existente@xpto.com").Value;
        var telefone = Telefone.Criar("27999999999").Value;
        var endereco = Endereco.Criar("29140000", "Rua A", "100", null, "Centro", 3205309, 32);

        return Tenant.CriarComprador(
            "Oficina XPTO LTDA",
            "Oficina XPTO",
            cnpj,
            email,
            [telefone],
            TipoTenant.Oficina,
            endereco,
            _dateTime);
    }
}
