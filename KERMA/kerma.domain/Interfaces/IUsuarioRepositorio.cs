using Kerma.Domain.Entities;

namespace Kerma.Domain.Interfaces;

public interface IUsuarioRepositorio : IRepositorio<Usuario>
{
    Task<Usuario?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IEnumerable<Usuario>> ListarAtivosAsync(CancellationToken ct = default);
    Task<bool> UsuarioHabilitadoParaEventoAsync(int usuarioId, int eventoId, CancellationToken ct = default);
    Task HabilitarUsuarioNoEventoAsync(int usuarioId, int eventoId, CancellationToken ct = default);
    Task<IEnumerable<UsuarioEvento>> ListarUsuariosPorEventoAsync(int eventoId, CancellationToken ct = default);
}