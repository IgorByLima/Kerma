using Kerma.Domain.Entities;
using Kerma.Domain.Interfaces;
using Kerma.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kerma.Infrastructure.Repositories;

public class ItemRepositorio(KermaDbContext db) : IItemRepositorio
{
    public async Task<Item?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await db.Itens.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IEnumerable<Item>> ListarAsync(CancellationToken ct = default)
        => await db.Itens.AsNoTracking().ToListAsync(ct);

    public async Task<IEnumerable<Item>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default)
     => await db.Itens
         .Include(i => i.CozinhaEvento)
         .AsNoTracking()
         .Where(i => i.EventoId == eventoId && i.Ativo)
         .ToListAsync(ct);

    public async Task<IEnumerable<Item>> ListarComEstoqueCriticoAsync(int eventoId, CancellationToken ct = default)
        => await db.Itens
            .AsNoTracking()
            .Where(i => i.EventoId == eventoId && i.Ativo && i.EstoqueAtual <= i.EstoqueMinimo)
            .ToListAsync(ct);

    public async Task AtualizarStatusEstoqueAsync(int itemId, CancellationToken ct = default)
    {
        var item = await db.Itens.FindAsync([itemId], ct);
        if (item is null) return;

        item.StatusEstoque = item.EstoqueAtual switch
        {
            0 => Domain.Enums.StatusEstoque.Vermelho,
            _ when item.EstoqueAtual <= item.EstoqueMinimo => Domain.Enums.StatusEstoque.Amarelo,
            _ => Domain.Enums.StatusEstoque.Verde
        };

        item.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task IncrementarEstoqueAsync(int itemId, int quantidade, CancellationToken ct = default)
    {
        // ExecuteUpdateAsync atualiza diretamente no banco sem carregar a entidade
        await db.Itens
            .Where(i => i.Id == itemId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(i => i.EstoqueAtual, i => i.EstoqueAtual + quantidade)
                .SetProperty(i => i.AtualizadoEm, DateTime.UtcNow), ct);

        await AtualizarStatusEstoqueAsync(itemId, ct);
    }

    public async Task DecrementarEstoqueAsync(int itemId, int quantidade, CancellationToken ct = default)
    {
        await db.Itens
            .Where(i => i.Id == itemId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(i => i.EstoqueAtual, i => i.EstoqueAtual - quantidade)
                .SetProperty(i => i.AtualizadoEm, DateTime.UtcNow), ct);

        await AtualizarStatusEstoqueAsync(itemId, ct);
    }

    public async Task AdicionarAsync(Item entidade, CancellationToken ct = default)
        => await db.Itens.AddAsync(entidade, ct);

    public Task AtualizarAsync(Item entidade, CancellationToken ct = default)
    {
        db.Itens.Update(entidade);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Item entidade, CancellationToken ct = default)
    {
        db.Itens.Remove(entidade);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}