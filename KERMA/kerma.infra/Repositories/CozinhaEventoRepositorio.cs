using Kerma.Domain.Entities;
using Kerma.Domain.Interfaces;
using Kerma.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kerma.Infrastructure.Repositories;

public class CozinhaEventoRepositorio(KermaDbContext db) : ICozinhaEventoRepositorio
{
    public async Task<CozinhaEvento?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await db.CozinhasEvento.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IEnumerable<CozinhaEvento>> ListarAsync(CancellationToken ct = default)
        => await db.CozinhasEvento.AsNoTracking().ToListAsync(ct);

    public async Task<IEnumerable<CozinhaEvento>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default)
        => await db.CozinhasEvento
            .AsNoTracking()
            .Where(c => c.EventoId == eventoId && c.Ativa)
            .OrderBy(c => c.Nome)
            .ToListAsync(ct);

    public async Task AdicionarAsync(CozinhaEvento entidade, CancellationToken ct = default)
        => await db.CozinhasEvento.AddAsync(entidade, ct);

    public Task AtualizarAsync(CozinhaEvento entidade, CancellationToken ct = default)
    {
        db.CozinhasEvento.Update(entidade);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(CozinhaEvento entidade, CancellationToken ct = default)
    {
        db.CozinhasEvento.Remove(entidade);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}