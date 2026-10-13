using Kerma.Domain.Entities;
using Kerma.Domain.Enums;
using Kerma.Domain.Interfaces;
using Kerma.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kerma.Infrastructure.Repositories;

public class CaixaRepositorio(KermaDbContext db) : ICaixaRepositorio
{
    public async Task<Caixa?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await db.Caixas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IEnumerable<Caixa>> ListarAsync(CancellationToken ct = default)
        => await db.Caixas.AsNoTracking().ToListAsync(ct);

    public async Task<IEnumerable<Caixa>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default)
        => await db.Caixas
            .AsNoTracking()
            .Where(c => c.EventoId == eventoId)
            .ToListAsync(ct);

    public async Task<Caixa?> ObterCaixaAbertoPorOperadorAsync(int operadorId, int eventoId, CancellationToken ct = default)
        => await db.Caixas
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.OperadorId == operadorId &&
                c.EventoId == eventoId &&
                c.Status == StatusCaixa.Aberto, ct);

    public async Task AdicionarAsync(Caixa entidade, CancellationToken ct = default)
        => await db.Caixas.AddAsync(entidade, ct);

    public Task AtualizarAsync(Caixa entidade, CancellationToken ct = default)
    {
        db.Caixas.Update(entidade);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Caixa entidade, CancellationToken ct = default)
    {
        db.Caixas.Remove(entidade);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}