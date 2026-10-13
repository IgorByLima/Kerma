using Kerma.Domain.Entities;

namespace Kerma.Domain.Interfaces;

public interface ICaixaRepositorio : IRepositorio<Caixa>
{
    Task<IEnumerable<Caixa>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default);
    Task<Caixa?> ObterCaixaAbertoPorOperadorAsync(int operadorId, int eventoId, CancellationToken ct = default);
}