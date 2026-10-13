using Kerma.Domain.Entities;
using Kerma.Domain.Interfaces;
using Kerma.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kerma.Infrastructure.Repositories;

public class VendaRepositorio(KermaDbContext db) : IVendaRepositorio
{
    public async Task<Venda?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await db.Vendas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<Venda?> ObterComItensAsync(int vendaId, CancellationToken ct = default)
    => await db.Vendas
        .Include(v => v.ItensVenda)
            .ThenInclude(iv => iv.Item)
                .ThenInclude(i => i.CozinhaEvento)
        .FirstOrDefaultAsync(v => v.Id == vendaId, ct);

    public async Task<Venda?> ObterPorVoucherAsync(string voucherCodigo, CancellationToken ct = default)
        => await db.Vendas.AsNoTracking()
            .FirstOrDefaultAsync(v => v.VoucherCodigo == voucherCodigo, ct);

    public async Task<IEnumerable<Venda>> ListarAsync(CancellationToken ct = default)
        => await db.Vendas.AsNoTracking().ToListAsync(ct);

    public async Task<IEnumerable<Venda>> ListarPorCaixaAsync(int caixaId, CancellationToken ct = default)
        => await db.Vendas
            .Include(v => v.ItensVenda)
                .ThenInclude(iv => iv.Item)
            .Include(v => v.ItensVenda)
                .ThenInclude(iv => iv.Tickets)
            .AsNoTracking()
            .Where(v => v.CaixaId == caixaId)
            .ToListAsync(ct);

    public async Task<IEnumerable<Venda>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default)
        => await db.Vendas
            .Include(v => v.ItensVenda)
                .ThenInclude(iv => iv.Item)
            .Include(v => v.ItensVenda)
                .ThenInclude(iv => iv.Tickets)
            .AsNoTracking()
            .Where(v => v.EventoId == eventoId)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Venda entidade, CancellationToken ct = default)
        => await db.Vendas.AddAsync(entidade, ct);

    public Task AtualizarAsync(Venda entidade, CancellationToken ct = default)
    {
        db.Vendas.Update(entidade);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Venda entidade, CancellationToken ct = default)
    {
        db.Vendas.Remove(entidade);
        return Task.CompletedTask;
    }

    public async Task AdicionarTicketAsync(Ticket ticket, CancellationToken ct = default)
        => await db.Tickets.AddAsync(ticket, ct);

    public async Task<int> SalvarAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}