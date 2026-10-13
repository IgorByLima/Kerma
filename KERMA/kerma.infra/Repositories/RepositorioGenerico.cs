using Kerma.Domain.Entities;
using Kerma.Domain.Interfaces;
using Kerma.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kerma.Infrastructure.Repositories;

public class RepositorioGenerico<T>(KermaDbContext db) : IRepositorio<T> where T : EntidadeBase
{
    public async Task<T?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await db.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IEnumerable<T>> ListarAsync(CancellationToken ct = default)
        => await db.Set<T>().AsNoTracking().ToListAsync(ct);

    public async Task AdicionarAsync(T entidade, CancellationToken ct = default)
        => await db.Set<T>().AddAsync(entidade, ct);

    public Task AtualizarAsync(T entidade, CancellationToken ct = default)
    {
        db.Set<T>().Update(entidade);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(T entidade, CancellationToken ct = default)
    {
        db.Set<T>().Remove(entidade);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}