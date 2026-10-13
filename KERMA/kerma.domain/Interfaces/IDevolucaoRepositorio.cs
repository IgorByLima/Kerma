using Kerma.Domain.Entities;

namespace Kerma.Domain.Interfaces;

public interface IDevolucaoRepositorio : IRepositorio<Devolucao>
{
    Task<IEnumerable<Devolucao>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default);
}