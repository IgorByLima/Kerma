using Kerma.Domain.Entities;
using Kerma.Domain.Interfaces;
using Kerma.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kerma.Infrastructure.Repositories;

public class FornecedorRepositorio(KermaDbContext db) : IFornecedorRepositorio
{
    public async Task<Fornecedor?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await db.Fornecedores.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<Fornecedor?> ObterPorCNPJAsync(string cnpj, CancellationToken ct = default)
    {
        var normalizado = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");
        return await db.Fornecedores.AsNoTracking()
            .FirstOrDefaultAsync(f => f.CNPJ == normalizado, ct);
    }

    public async Task<IEnumerable<Fornecedor>> ListarAsync(CancellationToken ct = default)
        => await db.Fornecedores.AsNoTracking().ToListAsync(ct);

    public async Task<IEnumerable<Fornecedor>> ListarAtivosAsync(CancellationToken ct = default)
        => await db.Fornecedores.AsNoTracking().Where(f => f.Ativo).ToListAsync(ct);

    public async Task AdicionarAsync(Fornecedor entidade, CancellationToken ct = default)
        => await db.Fornecedores.AddAsync(entidade, ct);

    public Task AtualizarAsync(Fornecedor entidade, CancellationToken ct = default)
    {
        db.Fornecedores.Update(entidade);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Fornecedor entidade, CancellationToken ct = default)
    {
        db.Fornecedores.Remove(entidade);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}

public class FornecedorItemRepositorio(KermaDbContext db) : IFornecedorItemRepositorio
{
    public async Task<FornecedorItem?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await db.FornecedorItens
            .Include(fi => fi.Fornecedor)
            .Include(fi => fi.Item)
            .AsNoTracking()
            .FirstOrDefaultAsync(fi => fi.Id == id, ct);

    public async Task<IEnumerable<FornecedorItem>> ListarAsync(CancellationToken ct = default)
        => await db.FornecedorItens.AsNoTracking().ToListAsync(ct);

    public async Task<IEnumerable<FornecedorItem>> ListarPorItemAsync(int itemId, CancellationToken ct = default)
        => await db.FornecedorItens
            .Include(fi => fi.Fornecedor)
            .Where(fi => fi.ItemId == itemId && fi.Ativo)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IEnumerable<FornecedorItem>> ListarPorFornecedorAsync(int fornecedorId, CancellationToken ct = default)
        => await db.FornecedorItens
            .Include(fi => fi.Item)
            .Where(fi => fi.FornecedorId == fornecedorId && fi.Ativo)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IEnumerable<ComparativoFornecedorItem>> ComparativoPorItemAsync(int itemId, CancellationToken ct = default)
    {
        var hoje = DateTime.UtcNow.Date;
        var cotacoes = await db.FornecedorItens
            .Include(fi => fi.Fornecedor)
            .Include(fi => fi.Item)
            .Where(fi => fi.ItemId == itemId && fi.Ativo && fi.VigenteAte >= hoje && fi.Fornecedor.Ativo)
            .OrderBy(fi => fi.PrecoFornecedor)
            .AsNoTracking()
            .ToListAsync(ct);

        var menorPreco = cotacoes.FirstOrDefault()?.PrecoFornecedor ?? 0;

        return cotacoes.Select(fi => new ComparativoFornecedorItem(
            FornecedorId: fi.FornecedorId,
            NomeFornecedor: fi.Fornecedor.NomeFantasia ?? fi.Fornecedor.RazaoSocial,
            ItemId: fi.ItemId,
            NomeItem: fi.Item.Nome,
            PrecoFornecedor: fi.PrecoFornecedor,
            PrecoVendaItem: fi.Item.PrecoVenda,
            PrazoEntregaDias: fi.PrazoEntregaDias,
            QuantidadeMinima: fi.QuantidadeMinima,
            VigenteAte: fi.VigenteAte,
            MenorPreco: fi.PrecoFornecedor == menorPreco
        ));
    }

    public async Task<IEnumerable<RankingFornecedor>> RankingPorEventoAsync(int eventoId, CancellationToken ct = default)
    {
        var hoje = DateTime.UtcNow.Date;
        return await db.FornecedorItens
            .Include(fi => fi.Fornecedor)
            .Include(fi => fi.Item)
            .Where(fi =>
                fi.Item.EventoId == eventoId &&
                fi.Ativo &&
                fi.VigenteAte >= hoje &&
                fi.Fornecedor.Ativo)
            .GroupBy(fi => new { fi.FornecedorId, fi.Fornecedor.RazaoSocial, fi.Fornecedor.NomeFantasia })
            .Select(g => new RankingFornecedor(
    g.Key.FornecedorId,
    g.Key.NomeFantasia ?? g.Key.RazaoSocial,
    g.Count(),
    g.Sum(fi => fi.PrecoFornecedor * fi.Item.EstoqueInicial),
    g.Average(fi =>
        fi.Item.PrecoVenda > 0
            ? (fi.Item.PrecoVenda - fi.PrecoFornecedor) / fi.Item.PrecoVenda * 100
            : 0)
))

            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task AdicionarAsync(FornecedorItem entidade, CancellationToken ct = default)
        => await db.FornecedorItens.AddAsync(entidade, ct);

    public Task AtualizarAsync(FornecedorItem entidade, CancellationToken ct = default)
    {
        db.FornecedorItens.Update(entidade);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(FornecedorItem entidade, CancellationToken ct = default)
    {
        db.FornecedorItens.Remove(entidade);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}