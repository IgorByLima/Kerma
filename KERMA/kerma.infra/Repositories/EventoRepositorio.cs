using Kerma.Domain.Entities;
using Kerma.Domain.Enums;
using Kerma.Domain.Interfaces;
using Kerma.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kerma.Infrastructure.Repositories;

public class EventoRepositorio(KermaDbContext db) : IEventoRepositorio
{
    public async Task<Evento?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await db.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<Evento?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default)
        => await db.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.Codigo == codigo, ct);

    public async Task<IEnumerable<Evento>> ListarAsync(CancellationToken ct = default)
        => await db.Eventos.AsNoTracking().ToListAsync(ct);

    public async Task<IEnumerable<Evento>> ListarPorStatusAsync(StatusEvento status, CancellationToken ct = default)
        => await db.Eventos.AsNoTracking().Where(e => e.Status == status).ToListAsync(ct);

    public async Task AdicionarAsync(Evento entidade, CancellationToken ct = default)
        => await db.Eventos.AddAsync(entidade, ct);

    public Task AtualizarAsync(Evento entidade, CancellationToken ct = default)
    {
        db.Eventos.Update(entidade);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Evento entidade, CancellationToken ct = default)
    {
        db.Eventos.Remove(entidade);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}