using Kerma.Domain.Entities;
using Kerma.Domain.Enums;

namespace Kerma.Domain.Interfaces;

public interface IEventoRepositorio : IRepositorio<Evento>
{
    Task<Evento?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IEnumerable<Evento>> ListarPorStatusAsync(StatusEvento status, CancellationToken ct = default);
}