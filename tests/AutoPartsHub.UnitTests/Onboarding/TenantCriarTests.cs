using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Enums;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AutoPartsHub.UnitTests.Onboarding;

public sealed class TenantCriarTests
{
    private static Tenant CriarTenant(TipoComprador tipoComprador, PlanoAssinatura plano)
    {
        var dateTime = Substitute.For<IDateTimeProvider>();
        dateTime.UtcNow.Returns(new DateTime(2026, 3, 29, 0, 0, 0, DateTimeKind.Utc));

        return Tenant.Criar(
            nomeFantasia: "Oficina XPTO",
            razaoSocial: "Oficina XPTO LTDA",
            cnpjNumerico: "12345678000199",
            emailAdmin: "admin@xpto.com",
            tipoComprador: tipoComprador,
            planoEscolhido: plano,
            telefoneComercial: "27999999999",
            inscricaoEstadual: "123456",
            comoNosConheceu: "Google",
            descricaoOutro: "Outro segmento",
            segmentoFrota: "Leve",
            qtdVeiculosEstimada: 12,
            limiteAprovacaoAdmin: 5000,
            codigoUf: 32,
            codigoIbge: 3205309,
            endereco: new EnderecoOnboarding("29140000", "Rua A", "100", null, "Centro", "Vitoria", "ES"),
            dateTime: dateTime);
    }

    [Fact]
    public void Criar_TipoOutro_DefinePlanoEStatusFreeSemTrial()
    {
        var tenant = CriarTenant(TipoComprador.Outro, PlanoAssinatura.Profissional);

        tenant.PlanoAtual.Should().Be(PlanoAssinatura.Free);
        tenant.StatusAssinatura.Should().Be(StatusAssinatura.Free);
        tenant.TrialExpiraEmNovo.Should().BeNull();
    }

    [Fact]
    public void Criar_TipoOficinaCarro_DefineStatusTrialCom30Dias()
    {
        var tenant = CriarTenant(TipoComprador.OficinaCarro, PlanoAssinatura.Basico);

        tenant.StatusAssinatura.Should().Be(StatusAssinatura.Trial);
        tenant.TrialExpiraEmNovo.Should().NotBeNull();
        tenant.TrialExpiraEmNovo!.Value.Should().Be(new DateTime(2026, 4, 28, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Criar_PlanoProfissional_DefineLimitesCorretos()
    {
        var tenant = CriarTenant(TipoComprador.Logista, PlanoAssinatura.Profissional);

        tenant.CotacoesLimiteMes.Should().Be(200);
        tenant.UsuariosLimite.Should().Be(5);
    }

    [Fact]
    public void Criar_PlanoEnterprise_DefineLimitesMaximos()
    {
        var tenant = CriarTenant(TipoComprador.Frotista, PlanoAssinatura.Enterprise);

        tenant.CotacoesLimiteMes.Should().Be(int.MaxValue);
        tenant.UsuariosLimite.Should().Be(int.MaxValue);
    }
}
