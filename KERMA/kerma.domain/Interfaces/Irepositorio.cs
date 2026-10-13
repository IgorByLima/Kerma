using Kerma.Domain.Entities;

namespace Kerma.Domain.Interfaces;

public interface IRepositorio<T> where T : EntidadeBase
{
    Task<T?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<T>> ListarAsync(CancellationToken ct = default);
    Task AdicionarAsync(T entidade, CancellationToken ct = default);
    Task AtualizarAsync(T entidade, CancellationToken ct = default);
    Task RemoverAsync(T entidade, CancellationToken ct = default);
    Task<int> SalvarAsync(CancellationToken ct = default);
}