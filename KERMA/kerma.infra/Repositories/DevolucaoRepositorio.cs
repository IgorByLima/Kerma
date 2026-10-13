using Kerma.Domain.Entities;
using Kerma.Domain.Interfaces;
using Kerma.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kerma.Infrastructure.Repositories;

public class DevolucaoRepositorio(KermaDbContext db) : IDevolucaoRepositorio
{
    public async Task<Devolucao?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await db.Devolucoes.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IEnumerable<Devolucao>> ListarAsync(CancellationToken ct = default)
        => await db.Devolucoes.AsNoTracking().ToListAsync(ct);

    public async Task<IEnumerable<Devolucao>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default)
        => await db.Devolucoes
            .Include(d => d.Ticket)
                .ThenInclude(t => t.ItemVenda)
                    .ThenInclude(iv => iv.Item)
            .Include(d => d.NovoTicket)
            .AsNoTracking()
            .Where(d => d.Ticket.ItemVenda.Venda.EventoId == eventoId)
            .OrderByDescending(d => d.CriadoEm)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Devolucao entidade, CancellationToken ct = default)
        => await db.Devolucoes.AddAsync(entidade, ct);

    public Task AtualizarAsync(Devolucao entidade, CancellationToken ct = default)
    {
        db.Devolucoes.Update(entidade);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Devolucao entidade, CancellationToken ct = default)
    {
        db.Devolucoes.Remove(entidade);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}