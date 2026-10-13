using Kerma.Application.DTOs.Request;
using Kerma.Application.DTOs.Response;
using Kerma.Domain.Entities;
using Kerma.Domain.Enums;
using Kerma.Domain.Interfaces;

namespace Kerma.Application.Services;

public interface ICaixaService
{
    Task<CaixaResponse> AbrirAsync(AbrirCaixaRequest dto, int operadorId, CancellationToken ct = default);
    Task<CaixaResponse> FecharAsync(int id, FecharCaixaRequest dto, CancellationToken ct = default);
    Task<CaixaResponse?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<CaixaResponse>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default);
    Task<CaixaResponse?> ObterCaixaAbertoDoOperadorAsync(int operadorId, int eventoId, CancellationToken ct = default);
}

public class CaixaService(
    ICaixaRepositorio caixaRepo,
    IEventoRepositorio eventoRepo) : ICaixaService
{
    public async Task<CaixaResponse> AbrirAsync(AbrirCaixaRequest dto, int operadorId, CancellationToken ct = default)
    {
        // Verificar ATIVIDADE
        var evento = await eventoRepo.ObterPorIdAsync(dto.EventoId, ct)
            ?? throw new KeyNotFoundException($"Evento {dto.EventoId} não encontrado.");

        if (evento.Status != StatusEvento.Ativo)
            throw new InvalidOperationException("Só é possível abrir caixa em eventos com status Ativo.");

        // Verificar se o operador em caixa aberto neste evento
        var caixaExistente = await caixaRepo.ObterCaixaAbertoPorOperadorAsync(operadorId, dto.EventoId, ct);
        if (caixaExistente is not null)
            throw new InvalidOperationException("Você já possui um caixa aberto neste evento.");

        var caixa = new Caixa
        {
            EventoId = dto.EventoId,
            OperadorId = operadorId,
            SaldoInicial = dto.SaldoInicial,
            AbertoEm = DateTime.UtcNow,
            Status = StatusCaixa.Aberto
        };

        await caixaRepo.AdicionarAsync(caixa, ct);
        await caixaRepo.SalvarAsync(ct);
        return ToResponse(caixa);
    }

    public async Task<CaixaResponse> FecharAsync(int id, FecharCaixaRequest dto, CancellationToken ct = default)
    {
        var caixa = await caixaRepo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Caixa {id} não encontrado.");

        if (caixa.Status == StatusCaixa.Fechado)
            throw new InvalidOperationException("Este caixa já está fechado.");

        caixa.SaldoFinal = dto.SaldoFinal;
        caixa.Observacao = dto.Observacao;
        caixa.FechadoEm = DateTime.UtcNow;
        caixa.Status = StatusCaixa.Fechado;
        caixa.AtualizadoEm = DateTime.UtcNow;

        await caixaRepo.AtualizarAsync(caixa, ct);
        await caixaRepo.SalvarAsync(ct);
        return ToResponse(caixa);
    }

    public async Task<CaixaResponse?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var c = await caixaRepo.ObterPorIdAsync(id, ct);
        return c is null ? null : ToResponse(c);
    }

    public async Task<IEnumerable<CaixaResponse>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default)
    {
        var lista = await caixaRepo.ListarPorEventoAsync(eventoId, ct);
        return lista.Select(ToResponse);
    }

    public async Task<CaixaResponse?> ObterCaixaAbertoDoOperadorAsync(int operadorId, int eventoId, CancellationToken ct = default)
    {
        var c = await caixaRepo.ObterCaixaAbertoPorOperadorAsync(operadorId, eventoId, ct);
        return c is null ? null : ToResponse(c);
    }

    private static CaixaResponse ToResponse(Caixa c) => new(
        c.Id, c.EventoId, c.OperadorId,
        c.SaldoInicial, c.SaldoFinal,
        c.AbertoEm, c.FechadoEm,
        c.Status.ToString(), c.Observacao);
}