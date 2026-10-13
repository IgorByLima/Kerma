using Kerma.Domain.Entities;

namespace Kerma.Domain.Interfaces;

public interface IVendaRepositorio : IRepositorio<Venda>
{
    Task<IEnumerable<Venda>> ListarPorCaixaAsync(int caixaId, CancellationToken ct = default);
    Task<IEnumerable<Venda>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default);
    Task<Venda?> ObterComItensAsync(int vendaId, CancellationToken ct = default);
    Task<Venda?> ObterPorVoucherAsync(string voucherCodigo, CancellationToken ct = default);
    Task AdicionarTicketAsync(Ticket ticket, CancellationToken ct = default);
}