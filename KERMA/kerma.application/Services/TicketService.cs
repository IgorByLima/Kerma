using Kerma.Application.DTOs.Request;
using Kerma.Application.DTOs.Response;
using Kerma.Domain.Entities;
using Kerma.Domain.Enums;
using Kerma.Domain.Interfaces;

namespace Kerma.Application.Services;

public interface ITicketService
{
    Task<TicketResponse?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IEnumerable<TicketResponse>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default);
    Task<IEnumerable<TicketResponse>> ListarFilaCozinhaAsync(int eventoId, CancellationToken ct = default);
    Task<IEnumerable<TicketResponse>> ListarFilaCozinhaPorCozinhaAsync(int eventoId, int cozinhaEventoId, CancellationToken ct = default);
    Task<IEnumerable<TicketResponse>> ListarProntosPorEventoAsync(int eventoId, CancellationToken ct = default);
    Task<IEnumerable<TicketResponse>> EnviarParaFilaAsync(EnviarFilaRequest dto, CancellationToken ct = default);
    Task<TicketResponse> IniciarPreparoAsync(int ticketId, CancellationToken ct = default);
    Task<TicketResponse> MarcarProntoAsync(int ticketId, CancellationToken ct = default);
    Task<IEnumerable<TicketResponse>> ConfirmarRetiradaAsync(ConfirmarRetiradaRequest dto, CancellationToken ct = default);
}

public class TicketService(ITicketRepositorio ticketRepo) : ITicketService
{
    public async Task<TicketResponse?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default)
    {
        var ticket = await ticketRepo.ObterPorCodigoAsync(codigo, ct);
        return ticket is null ? null : ToResponse(ticket);
    }

    public async Task<IEnumerable<TicketResponse>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default)
    {
        var tickets = await ticketRepo.ListarPorEventoAsync(eventoId, ct);
        return tickets.Select(ToResponse);
    }

    public async Task<IEnumerable<TicketResponse>> ListarFilaCozinhaAsync(int eventoId, CancellationToken ct = default)
    {
        var tickets = await ticketRepo.ListarFilaCozinhaAsync(eventoId, ct);
        return tickets.Select(ToResponse);
    }

    public async Task<IEnumerable<TicketResponse>> ListarFilaCozinhaPorCozinhaAsync(int eventoId, int cozinhaEventoId, CancellationToken ct = default)
    {
        var tickets = await ticketRepo.ListarFilaCozinhaPorCozinhaAsync(eventoId, cozinhaEventoId, ct);
        return tickets.Select(ToResponse);
    }

    public async Task<IEnumerable<TicketResponse>> ListarProntosPorEventoAsync(int eventoId, CancellationToken ct = default)
    {
        var tickets = await ticketRepo.ListarProntosPorEventoAsync(eventoId, ct);
        return tickets.Select(ToResponse);
    }

    public async Task<IEnumerable<TicketResponse>> EnviarParaFilaAsync(EnviarFilaRequest dto, CancellationToken ct = default)
    {
        var resultados = new List<TicketResponse>();
        foreach (var codigo in dto.Codigos)
        {
            var ticket = await ticketRepo.ObterPorCodigoAsync(codigo, ct);
            if (ticket is null || ticket.Status != StatusTicket.Pendente) continue;

            await ticketRepo.AtualizarStatusAsync(
                ticket.Id, StatusTicket.EmFila,
                dto.NomeCliente, DateTime.UtcNow,
                null, null, null, ct);

            ticket.Status = StatusTicket.EmFila;
            ticket.NomeCliente = dto.NomeCliente;
            ticket.EnviadoFilaEm = DateTime.UtcNow;
            resultados.Add(ToResponse(ticket));
        }
        return resultados;
    }

    public async Task<TicketResponse> IniciarPreparoAsync(int ticketId, CancellationToken ct = default)
    {
        var ticket = await ticketRepo.ObterPorIdAsync(ticketId, ct)
            ?? throw new KeyNotFoundException($"Ticket {ticketId} não encontrado.");

        if (ticket.Status != StatusTicket.EmFila)
            throw new InvalidOperationException("Ticket não está na fila.");

        await ticketRepo.AtualizarStatusAsync(
            ticket.Id, StatusTicket.EmPreparo,
            ticket.NomeCliente, ticket.EnviadoFilaEm,
            null, null, null, ct);

        ticket.Status = StatusTicket.EmPreparo;
        return ToResponse(ticket);
    }

    public async Task<TicketResponse> MarcarProntoAsync(int ticketId, CancellationToken ct = default)
    {
        var ticket = await ticketRepo.ObterPorIdAsync(ticketId, ct)
            ?? throw new KeyNotFoundException($"Ticket {ticketId} não encontrado.");

        if (ticket.Status != StatusTicket.EmPreparo)
            throw new InvalidOperationException("Ticket não está em preparo.");

        await ticketRepo.AtualizarStatusAsync(
            ticket.Id, StatusTicket.Pronto,
            ticket.NomeCliente, ticket.EnviadoFilaEm,
            DateTime.UtcNow, null, null, ct);

        ticket.Status = StatusTicket.Pronto;
        ticket.ProntoEm = DateTime.UtcNow;
        return ToResponse(ticket);
    }

    public async Task<IEnumerable<TicketResponse>> ConfirmarRetiradaAsync(ConfirmarRetiradaRequest dto, CancellationToken ct = default)
    {
        var resultados = new List<TicketResponse>();
        foreach (var codigo in dto.Codigos)
        {
            var ticket = await ticketRepo.ObterPorCodigoAsync(codigo, ct);
            if (ticket is null || ticket.Status != StatusTicket.Pronto) continue;

            await ticketRepo.AtualizarStatusAsync(
                ticket.Id, StatusTicket.Retirado,
                ticket.NomeCliente, ticket.EnviadoFilaEm,
                ticket.ProntoEm, dto.OperadorNome, DateTime.UtcNow, ct);

            ticket.Status = StatusTicket.Retirado;
            ticket.RetiradoPor = dto.OperadorNome;
            ticket.RetiradoEm = DateTime.UtcNow;
            resultados.Add(ToResponse(ticket));
        }
        return resultados;
    }

    private static TicketResponse ToResponse(Ticket t) => new(
        Id: t.Id,
        Codigo: t.Codigo,
        Status: t.Status.ToString(),
        NomeCliente: t.NomeCliente,
        NomeItem: t.ItemVenda?.Item?.Nome ?? "",
        EventoNome: t.ItemVenda?.Venda?.Evento?.Nome ?? "",
        RetiradoPor: t.RetiradoPor,
        RetiradoEm: t.RetiradoEm,
        EnviadoFilaEm: t.EnviadoFilaEm,
        ProntoEm: t.ProntoEm,
        CriadoEm: t.CriadoEm
    );
}