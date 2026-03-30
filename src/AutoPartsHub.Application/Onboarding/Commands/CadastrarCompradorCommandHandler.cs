using System.Text.Json;
using AutoPartsHub.Application.Common;
using AutoPartsHub.Application.Interfaces;
using AutoPartsHub.Domain.Entidades;
using AutoPartsHub.Domain.Enums;
using AutoPartsHub.Domain.Events;
using AutoPartsHub.Domain.Interfaces;
using AutoPartsHub.Domain.ValueObjects;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoPartsHub.Application.Onboarding.Commands;

public sealed class CadastrarCompradorCommandHandler(
    IOnboardingRascunhoRepository rascunhoRepo,
    ITenantRepository tenantRepo,
    IIdentidadeService identidadeService,
    ILocalizacaoService localizacaoService,
    ICacheService cache,
    IPublisher publisher,
    IDateTimeProvider dateTime,
    ILogger<CadastrarCompradorCommandHandler> logger) : ICommandHandler<CadastrarCompradorCommand, CadastrarCompradorResultadoDto>
{
    public async Task<Result<CadastrarCompradorResultadoDto>> Handle(
        CadastrarCompradorCommand request, CancellationToken cancellationToken)
    {
        var rascunho = await rascunhoRepo.BuscarPorTokenAsync(request.SessionToken, cancellationToken);

        if (rascunho is null)
        {
            var chaveFinalizado = $"onboarding_finalizado:{request.SessionToken}";
            var tenantIdCache = await cache.ObterAsync(chaveFinalizado, cancellationToken);
            if (Guid.TryParse(tenantIdCache, out var tenantJaCadastradoId))
                return Result.Ok(new CadastrarCompradorResultadoDto(tenantJaCadastradoId, "ja_cadastrado"));

            return Result.Fail("sessao_expirada");
        }

        DadosRascunho dados;
        try
        {
            dados = JsonSerializer.Deserialize<DadosRascunho>(rascunho.Dados,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new DadosRascunho();
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Falha ao desserializar dados do rascunho {SessionToken}", request.SessionToken);
            return Result.Fail("dados_invalidos");
        }

        var cnpjNumerico = new string((dados.Cnpj ?? string.Empty).Where(char.IsDigit).ToArray());
        if (cnpjNumerico.Length != 14)
            return Result.Fail("cnpj_obrigatorio");

        var tenantExistente = await tenantRepo.ObterPorCnpjAsync(cnpjNumerico, cancellationToken);
        if (tenantExistente is not null)
        {
            await cache.SetarAsync(
                $"onboarding_finalizado:{request.SessionToken}",
                tenantExistente.Id.ToString(),
                TimeSpan.FromDays(7),
                cancellationToken);

            return Result.Ok(new CadastrarCompradorResultadoDto(
                tenantExistente.Id,
                "empresa_ja_cadastrada"));
        }

        if (string.IsNullOrWhiteSpace(dados.Email))
            return Result.Fail("email_obrigatorio");

        if (string.IsNullOrWhiteSpace(dados.Senha))
            return Result.Fail("senha_obrigatoria");

        var enderecoOnboarding = new EnderecoOnboarding(
            dados.Cep ?? string.Empty,
            dados.Logradouro ?? string.Empty,
            dados.Numero ?? string.Empty,
            dados.Complemento,
            dados.Bairro ?? string.Empty,
            dados.Cidade ?? string.Empty,
            dados.Estado ?? string.Empty);

        var localizacao = await localizacaoService.ResolverAsync(
            dados.Estado,
            dados.Cidade,
            cancellationToken);

        if (!localizacao.CodigoUf.HasValue || !localizacao.CodigoIbge.HasValue)
            return Result.Fail("localizacao_invalida");

        Tenant tenant;
        try
        {
            tenant = Tenant.Criar(
                dados.NomeFantasia ?? string.Empty,
                dados.RazaoSocial ?? string.Empty,
                cnpjNumerico,
                dados.Email,
                rascunho.TipoPerfil,
                request.PlanoEscolhido,
                dados.TelefoneComercial ?? string.Empty,
                dados.InscricaoEstadual,
                dados.ComoNosConheceu,
                dados.DescricaoOutro,
                dados.SegmentoFrota,
                dados.QtdVeiculosEstimada,
                dados.LimiteAprovacaoAdmin,
                localizacao.CodigoUf.Value,
                localizacao.CodigoIbge.Value,
                enderecoOnboarding,
                dateTime);
        }
        catch (ArgumentException ex)
        {
            return Result.Fail(ex.Message);
        }

        var limiteAprovacao = dados.LimiteAprovacaoAdmin ?? 0;
        var resultadoUsuario = await identidadeService.CriarUsuarioAsync(
            dados.NomeCompleto ?? string.Empty,
            dados.Email,
            dados.Senha,
            tenant.Id,
            "Comprador",
            cancellationToken,
            telefone: dados.TelefoneComercial,
            limiteAprovacao: limiteAprovacao);

        if (resultadoUsuario.IsFailed)
            return Result.Fail(resultadoUsuario.Errors);

        await tenantRepo.AdicionarAsync(tenant, cancellationToken);
        await tenantRepo.SalvarAlteracoesAsync(cancellationToken);

        await rascunhoRepo.DeletarAsync(rascunho, cancellationToken);

        await cache.SetarAsync(
            $"onboarding_finalizado:{request.SessionToken}",
            tenant.Id.ToString(),
            TimeSpan.FromDays(7),
            cancellationToken);

        try
        {
            var evento = new CompradorCadastradoEvent(
                tenant.Id,
                dados.Email,
                tenant.NomeFantasia,
                tenant.PlanoAtual,
                tenant.TrialExpiraEmNovo);

            await publisher.Publish(new CompradorCadastradoNotification(evento), cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao publicar CompradorCadastradoEvent para tenant {TenantId}", tenant.Id);
        }

        return Result.Ok(new CadastrarCompradorResultadoDto(tenant.Id, "criado"));
    }
}
