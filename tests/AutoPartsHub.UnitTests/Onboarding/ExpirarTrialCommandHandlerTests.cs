using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Application.Onboarding.Commands;
using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Enums;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AutoPartsHub.UnitTests.Onboarding;

public sealed class ExpirarTrialCommandHandlerTests
{
    private readonly ITenantRepository _tenantRepo = Substitute.For<ITenantRepository>();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly ILogger<ExpirarTrialCommandHandler> _logger = Substitute.For<ILogger<ExpirarTrialCommandHandler>>();

    private ExpirarTrialCommandHandler CriarHandler()
    {
        _publisher.Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _cache.RemoverAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _tenantRepo.SalvarAlteracoesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _dateTime.UtcNow.Returns(new DateTime(2026, 3, 29, 0, 0, 0, DateTimeKind.Utc));

        return new ExpirarTrialCommandHandler(_tenantRepo, _publisher, _dateTime, _cache, _logger);
    }

    [Fact]
    public async Task Handle_TenantExpirado_RebaixaParaFreeEInvalidaCache()
    {
        var tenantExpirado = CriarTenantEmTrial();

        _tenantRepo.ListarTrialComExpiracaoEmAsync(7, Arg.Any<CancellationToken>()).Returns(new List<Tenant>());
        _tenantRepo.ListarTrialComExpiracaoEmAsync(1, Arg.Any<CancellationToken>()).Returns(new List<Tenant>());
        _tenantRepo.ListarTrialExpiradosAsync(Arg.Any<CancellationToken>()).Returns(new List<Tenant> { tenantExpirado });

        var resultado = await CriarHandler().Handle(new ExpirarTrialCommand(), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        tenantExpirado.StatusAssinatura.Should().Be(StatusAssinatura.Free);
        tenantExpirado.PlanoAtual.Should().Be(PlanoAssinatura.Free);
        tenantExpirado.CotacoesLimiteMes.Should().Be(10);
        tenantExpirado.UsuariosLimite.Should().Be(1);

        await _cache.Received(1).RemoverAsync($"tenant:{tenantExpirado.Id}", Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(
            Arg.Is<TrialExpiradoNotification>(n => n.Evento.TenantId == tenantExpirado.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TenantD7_PublicaAlertaTrialD7()
    {
        var tenantD7 = CriarTenantEmTrial();

        _tenantRepo.ListarTrialComExpiracaoEmAsync(7, Arg.Any<CancellationToken>()).Returns(new List<Tenant> { tenantD7 });
        _tenantRepo.ListarTrialComExpiracaoEmAsync(1, Arg.Any<CancellationToken>()).Returns(new List<Tenant>());
        _tenantRepo.ListarTrialExpiradosAsync(Arg.Any<CancellationToken>()).Returns(new List<Tenant>());

        var resultado = await CriarHandler().Handle(new ExpirarTrialCommand(), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();

        await _publisher.Received(1).Publish(
            Arg.Is<AlertaTrialNotification>(n => n.Evento.Tipo == "D7"),
            Arg.Any<CancellationToken>());
    }

    private Tenant CriarTenantEmTrial()
    {
        return Tenant.Criar(
            nomeFantasia: "Oficina XPTO",
            razaoSocial: "Oficina XPTO LTDA",
            cnpjNumerico: "12345678000199",
            emailAdmin: "admin@xpto.com",
            tipoComprador: TipoComprador.OficinaCarro,
            planoEscolhido: PlanoAssinatura.Basico,
            telefoneComercial: "27999999999",
            inscricaoEstadual: null,
            comoNosConheceu: null,
            descricaoOutro: null,
            segmentoFrota: null,
            qtdVeiculosEstimada: null,
            limiteAprovacaoAdmin: null,
            codigoUf: 32,
            codigoIbge: 3205309,
            endereco: new EnderecoOnboarding("29140000", "Rua A", "100", null, "Centro", "Vitoria", "ES"),
            dateTime: _dateTime);
    }
}
