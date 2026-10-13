using Kerma.Domain.Entities;

namespace Kerma.Domain.Interfaces;

public interface IItemRepositorio : IRepositorio<Item>
{
    Task<IEnumerable<Item>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default);
    Task<IEnumerable<Item>> ListarComEstoqueCriticoAsync(int eventoId, CancellationToken ct = default);
    Task AtualizarStatusEstoqueAsync(int itemId, CancellationToken ct = default);
    Task IncrementarEstoqueAsync(int itemId, int quantidade, CancellationToken ct = default);
    Task DecrementarEstoqueAsync(int itemId, int quantidade, CancellationToken ct = default);
}