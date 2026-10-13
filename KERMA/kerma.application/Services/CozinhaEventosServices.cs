using Kerma.Domain.Entities;
using Kerma.Domain.Interfaces;

namespace Kerma.Application.Services;

public interface ICozinhaEventoService
{
    Task<IEnumerable<CozinhaEventoResponse>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default);
    Task<CozinhaEventoResponse> CriarAsync(int eventoId, string nome, CancellationToken ct = default);
}

public record CozinhaEventoResponse(int Id, string Nome, bool Ativa);

public class CozinhaEventoService(ICozinhaEventoRepositorio repo) : ICozinhaEventoService
{
    public async Task<IEnumerable<CozinhaEventoResponse>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default)
    {
        var lista = await repo.ListarPorEventoAsync(eventoId, ct);
        return lista.Select(c => new CozinhaEventoResponse(c.Id, c.Nome, c.Ativa));
    }

    public async Task<CozinhaEventoResponse> CriarAsync(int eventoId, string nome, CancellationToken ct = default)
    {
        var cozinha = new CozinhaEvento
        {
            EventoId = eventoId,
            Nome = nome,
            Ativa = true,
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow,
        };
        await repo.AdicionarAsync(cozinha, ct);
        await repo.SalvarAsync(ct);
        return new CozinhaEventoResponse(cozinha.Id, cozinha.Nome, cozinha.Ativa);
    }
}