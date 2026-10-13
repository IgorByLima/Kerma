using Kerma.Application.DTOs.Request;
using Kerma.Application.DTOs.Response;
using Kerma.Application.Utils;
using Kerma.Domain.Entities;
using Kerma.Domain.Enums;
using Kerma.Domain.Interfaces;

namespace Kerma.Application.Services;

public interface IDevolucaoService
{
    Task<DevolucaoResponse> CancelarTicketAsync(CancelarTicketRequest dto, CancellationToken ct = default);
    Task<DevolucaoResponse> TrocarTicketAsync(TrocarTicketRequest dto, CancellationToken ct = default);
    Task<IEnumerable<DevolucaoResponse>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default);
}

public class DevolucaoService(
    ITicketRepositorio ticketRepo,
    IItemRepositorio itemRepo,
    IDevolucaoRepositorio devolucaoRepo) : IDevolucaoService
{
    public async Task<DevolucaoResponse> CancelarTicketAsync(CancelarTicketRequest dto, CancellationToken ct = default)
    {
        var ticket = await ticketRepo.ObterPorCodigoAsync(dto.CodigoTicket, ct)
            ?? throw new KeyNotFoundException($"Ticket {dto.CodigoTicket} não encontrado.");

        if (ticket.Status == StatusTicket.Retirado)
            throw new InvalidOperationException("Ticket já foi retirado e não pode ser devolvido.");

        if (ticket.Status == StatusTicket.Cancelado)
            throw new InvalidOperationException("Ticket já foi cancelado.");

        // Cancelar ticket
        await ticketRepo.AtualizarStatusAsync(
            ticket.Id, StatusTicket.Cancelado,
            ticket.NomeCliente, ticket.EnviadoFilaEm,
            ticket.ProntoEm, null, null, ct);

        // Restaurar estoque
        var itemId = ticket.ItemVenda?.ItemId ?? 0;
        if (itemId > 0)
            await itemRepo.IncrementarEstoqueAsync(itemId, 1, ct);

        // Registrar devolução
        var devolucao = new Devolucao
        {
            TicketId = ticket.Id,
            Tipo = TipoDevolucao.Cancelamento,
            Motivo = dto.Motivo,
            AutorizadoPor = dto.AutorizadoPor,
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow,
        };

        await devolucaoRepo.AdicionarAsync(devolucao, ct);
        await devolucaoRepo.SalvarAsync(ct);

        return ToResponse(devolucao, ticket);
    }

    public async Task<DevolucaoResponse> TrocarTicketAsync(TrocarTicketRequest dto, CancellationToken ct = default)
    {
        var ticket = await ticketRepo.ObterPorCodigoAsync(dto.CodigoTicket, ct)
            ?? throw new KeyNotFoundException($"Ticket {dto.CodigoTicket} não encontrado.");

        if (ticket.Status == StatusTicket.Retirado)
            throw new InvalidOperationException("Ticket já foi retirado e não pode ser trocado.");

        if (ticket.Status == StatusTicket.Cancelado)
            throw new InvalidOperationException("Ticket já foi cancelado.");

        var novoItem = await itemRepo.ObterPorIdAsync(dto.NovoItemId, ct)
            ?? throw new KeyNotFoundException($"Item {dto.NovoItemId} não encontrado.");

        if (novoItem.EstoqueAtual < 1)
            throw new InvalidOperationException($"Item '{novoItem.Nome}' sem estoque.");

        // Cancelar ticket antigo
        await ticketRepo.AtualizarStatusAsync(
            ticket.Id, StatusTicket.Cancelado,
            ticket.NomeCliente, ticket.EnviadoFilaEm,
            ticket.ProntoEm, null, null, ct);

        // Restaurar estoque do item antigo
        var itemAntigoId = ticket.ItemVenda?.ItemId ?? 0;
        if (itemAntigoId > 0)
            await itemRepo.IncrementarEstoqueAsync(itemAntigoId, 1, ct);

        // Debitar estoque do novo item
        await itemRepo.DecrementarEstoqueAsync(dto.NovoItemId, 1, ct);

        // Gerar novo ticket
        var novoTicket = new Ticket
        {
            ItemVendaId = ticket.ItemVendaId,
            Codigo = GeradorCodigo.GerarTicket(),
            Barcode = GeradorCodigo.GerarTicket(),
            Status = StatusTicket.Pendente,
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow,
        };

        await ticketRepo.AdicionarAsync(novoTicket, ct);
        await ticketRepo.SalvarAsync(ct);

        // Registrar devolução
        var devolucao = new Devolucao
        {
            TicketId = ticket.Id,
            NovoTicketId = novoTicket.Id,
            Tipo = TipoDevolucao.Troca,
            Motivo = dto.Motivo,
            AutorizadoPor = dto.AutorizadoPor,
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow,
        };

        await devolucaoRepo.AdicionarAsync(devolucao, ct);
        await devolucaoRepo.SalvarAsync(ct);

        return ToResponse(devolucao, ticket, novoTicket);
    }

    public async Task<IEnumerable<DevolucaoResponse>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default)
    {
        var devolucoes = await devolucaoRepo.ListarPorEventoAsync(eventoId, ct);
        return devolucoes.Select(d => ToResponse(d, d.Ticket, d.NovoTicket));
    }

    private static DevolucaoResponse ToResponse(Devolucao d, Ticket? ticket, Ticket? novoTicket = null) => new(
        Id: d.Id,
        Tipo: d.Tipo.ToString(),
        Motivo: d.Motivo,
        AutorizadoPor: d.AutorizadoPor,
        CodigoTicket: ticket?.Codigo ?? "",
        NomeItem: ticket?.ItemVenda?.Item?.Nome ?? "",
        NovoCodigoTicket: novoTicket?.Codigo,
        CriadoEm: d.CriadoEm
    );
}