using Kerma.Domain.Entities;
using Kerma.Domain.Enums;

namespace Kerma.Domain.Interfaces;

public interface ITicketRepositorio : IRepositorio<Ticket>
{
    Task<IEnumerable<Ticket>> ListarFilaCozinhaPorCozinhaAsync(int eventoId, int cozinhaEventoId, CancellationToken ct = default);
    Task<Ticket?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IEnumerable<Ticket>> ListarPorItemVendaAsync(int itemVendaId, CancellationToken ct = default);

    Task<IEnumerable<Ticket>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default);
    Task<IEnumerable<Ticket>> ListarFilaCozinhaAsync(int eventoId, CancellationToken ct = default);
    Task<IEnumerable<Ticket>> ListarProntosPorEventoAsync(int eventoId, CancellationToken ct = default);
    Task AtualizarStatusAsync(int id, StatusTicket status, string? nomeCliente, DateTime? enviadoFilaEm, DateTime? prontoEm, string? retiradoPor, DateTime? retiradoEm, CancellationToken ct = default);
}