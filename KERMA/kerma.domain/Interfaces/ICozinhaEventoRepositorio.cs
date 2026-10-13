using Kerma.Domain.Entities;

namespace Kerma.Domain.Interfaces;

public interface ICozinhaEventoRepositorio : IRepositorio<CozinhaEvento>
{
    Task<IEnumerable<CozinhaEvento>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default);
}