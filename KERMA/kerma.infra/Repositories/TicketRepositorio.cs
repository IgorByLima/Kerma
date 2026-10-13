using Kerma.Domain.Entities;
using Kerma.Domain.Enums;
using Kerma.Domain.Interfaces;
using Kerma.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kerma.Infrastructure.Repositories;

public class TicketRepositorio(KermaDbContext db) : ITicketRepositorio
{
    public async Task<Ticket?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await db.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Ticket?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default)
        => await db.Tickets
            .Include(t => t.ItemVenda)
                .ThenInclude(iv => iv.Item)
            .Include(t => t.ItemVenda)
                .ThenInclude(iv => iv.Venda)
                    .ThenInclude(v => v.Evento)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Codigo == codigo, ct);

    public async Task<IEnumerable<Ticket>> ListarAsync(CancellationToken ct = default)
        => await db.Tickets.AsNoTracking().ToListAsync(ct);

    public async Task<IEnumerable<Ticket>> ListarPorItemVendaAsync(int itemVendaId, CancellationToken ct = default)
        => await db.Tickets
            .AsNoTracking()
            .Where(t => t.ItemVendaId == itemVendaId)
            .ToListAsync(ct);

    public async Task<IEnumerable<Ticket>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default)
        => await db.Tickets
            .Include(t => t.ItemVenda)
                .ThenInclude(iv => iv.Item)
            .Include(t => t.ItemVenda)
                .ThenInclude(iv => iv.Venda)
            .AsNoTracking()
            .Where(t => t.ItemVenda.Venda.EventoId == eventoId)
            .ToListAsync(ct);

    public async Task<IEnumerable<Ticket>> ListarFilaCozinhaAsync(int eventoId, CancellationToken ct = default)
        => await db.Tickets
            .Include(t => t.ItemVenda)
                .ThenInclude(iv => iv.Item)
            .AsNoTracking()
            .Where(t =>
                t.ItemVenda.Venda.EventoId == eventoId &&
                (t.Status == StatusTicket.EmFila ||
                 t.Status == StatusTicket.EmPreparo ||
                 t.Status == StatusTicket.Pronto))
            .OrderBy(t => t.CriadoEm)
            .ToListAsync(ct);

    public async Task<IEnumerable<Ticket>> ListarProntosPorEventoAsync(int eventoId, CancellationToken ct = default)
        => await db.Tickets
            .Include(t => t.ItemVenda)
                .ThenInclude(iv => iv.Item)
            .AsNoTracking()
            .Where(t =>
                t.ItemVenda.Venda.EventoId == eventoId &&
                t.Status == StatusTicket.Pronto)
            .OrderBy(t => t.CriadoEm)
            .ToListAsync(ct);

    public async Task AtualizarStatusAsync(int id, StatusTicket status, string? nomeCliente, DateTime? enviadoFilaEm, DateTime? prontoEm, string? retiradoPor, DateTime? retiradoEm, CancellationToken ct = default)
    {
        var ticket = await db.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (ticket is null) return;

        ticket.Status = status;
        ticket.NomeCliente = nomeCliente;
        ticket.EnviadoFilaEm = enviadoFilaEm;
        ticket.ProntoEm = prontoEm;
        ticket.RetiradoPor = retiradoPor;
        ticket.RetiradoEm = retiradoEm;
        ticket.AtualizadoEm = DateTime.UtcNow;

        db.Tickets.Update(ticket);
        await db.SaveChangesAsync(ct);
    }
    public async Task<IEnumerable<Ticket>> ListarFilaCozinhaPorCozinhaAsync(int eventoId, int cozinhaEventoId, CancellationToken ct = default)
    => await db.Tickets
        .Include(t => t.ItemVenda)
            .ThenInclude(iv => iv.Item)
        .AsNoTracking()
        .Where(t =>
            t.ItemVenda.Venda.EventoId == eventoId &&
            t.CozinhaEventoId == cozinhaEventoId &&
            (t.Status == StatusTicket.EmFila ||
             t.Status == StatusTicket.EmPreparo ||
             t.Status == StatusTicket.Pronto))
        .OrderBy(t => t.CriadoEm)
        .ToListAsync(ct);
    public async Task AdicionarAsync(Ticket entidade, CancellationToken ct = default)
        => await db.Tickets.AddAsync(entidade, ct);

    public Task AtualizarAsync(Ticket entidade, CancellationToken ct = default)
    {
        db.Tickets.Update(entidade);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Ticket entidade, CancellationToken ct = default)
    {
        db.Tickets.Remove(entidade);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}