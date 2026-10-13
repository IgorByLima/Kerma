using Kerma.Application.DTOs.Request;
using Kerma.Application.DTOs.Response;
using Kerma.Domain.Entities;
using Kerma.Domain.Interfaces;

namespace Kerma.Application.Services;

// ─── Interface ───────────────────────────────────────────────────────────────
public interface IFornecedorService
{
    Task<FornecedorResponse> CriarAsync(CriarFornecedorRequest dto, int adminId, CancellationToken ct = default);
    Task<FornecedorResponse> AtualizarAsync(int id, AtualizarFornecedorRequest dto, CancellationToken ct = default);
    Task<FornecedorResponse?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<FornecedorResponse>> ListarAsync(CancellationToken ct = default);
    Task DesativarAsync(int id, CancellationToken ct = default);

    Task<FornecedorItemResponse> AdicionarCotacaoAsync(CriarFornecedorItemRequest dto, CancellationToken ct = default);
    Task<FornecedorItemResponse> AtualizarCotacaoAsync(int id, AtualizarFornecedorItemRequest dto, CancellationToken ct = default);
    Task<ComparativoItemResponse> ComparativoPorItemAsync(int itemId, CancellationToken ct = default);
    Task<IEnumerable<RankingFornecedorResponse>> RankingPorEventoAsync(int eventoId, CancellationToken ct = default);
}

// ─── Implementação ───────────────────────────────────────────────────────────
public class FornecedorService(
    IFornecedorRepositorio fornecedorRepo,
    IFornecedorItemRepositorio fornecedorItemRepo,
    IRepositorio<Item> itemRepo) : IFornecedorService
{
    public async Task<FornecedorResponse> CriarAsync(CriarFornecedorRequest dto, int adminId, CancellationToken ct = default)
    {
        var existente = await fornecedorRepo.ObterPorCNPJAsync(dto.CNPJ, ct);
        if (existente is not null)
            throw new InvalidOperationException($"Já existe um fornecedor com o CNPJ {dto.CNPJ}.");

        var fornecedor = new Fornecedor
        {
            RazaoSocial = dto.RazaoSocial,
            NomeFantasia = dto.NomeFantasia,
            CNPJ = dto.CNPJ.Replace(".", "").Replace("/", "").Replace("-", ""),
            Telefone = dto.Telefone,
            Email = dto.Email,
            Observacao = dto.Observacao,
            CadastradoPor = adminId
        };

        await fornecedorRepo.AdicionarAsync(fornecedor, ct);
        await fornecedorRepo.SalvarAsync(ct);
        return ToResponse(fornecedor);
    }

    public async Task<FornecedorResponse> AtualizarAsync(int id, AtualizarFornecedorRequest dto, CancellationToken ct = default)
    {
        var fornecedor = await fornecedorRepo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Fornecedor {id} não encontrado.");

        fornecedor.RazaoSocial = dto.RazaoSocial;
        fornecedor.NomeFantasia = dto.NomeFantasia;
        fornecedor.Telefone = dto.Telefone;
        fornecedor.Email = dto.Email;
        fornecedor.Observacao = dto.Observacao;
        fornecedor.Ativo = dto.Ativo;
        fornecedor.AtualizadoEm = DateTime.UtcNow;

        await fornecedorRepo.AtualizarAsync(fornecedor, ct);
        await fornecedorRepo.SalvarAsync(ct);
        return ToResponse(fornecedor);
    }

    public async Task<FornecedorResponse?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var f = await fornecedorRepo.ObterPorIdAsync(id, ct);
        return f is null ? null : ToResponse(f);
    }

    public async Task<IEnumerable<FornecedorResponse>> ListarAsync(CancellationToken ct = default)
    {
        var lista = await fornecedorRepo.ListarAtivosAsync(ct);
        return lista.Select(ToResponse);
    }

    public async Task DesativarAsync(int id, CancellationToken ct = default)
    {
        var fornecedor = await fornecedorRepo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Fornecedor {id} não encontrado.");
        fornecedor.Ativo = false;
        fornecedor.AtualizadoEm = DateTime.UtcNow;
        await fornecedorRepo.AtualizarAsync(fornecedor, ct);
        await fornecedorRepo.SalvarAsync(ct);
    }

    public async Task<FornecedorItemResponse> AdicionarCotacaoAsync(CriarFornecedorItemRequest dto, CancellationToken ct = default)
    {
        var fornecedor = await fornecedorRepo.ObterPorIdAsync(dto.FornecedorId, ct)
            ?? throw new KeyNotFoundException($"Fornecedor {dto.FornecedorId} não encontrado.");

        var item = await itemRepo.ObterPorIdAsync(dto.ItemId, ct)
            ?? throw new KeyNotFoundException($"Item {dto.ItemId} não encontrado.");

        var cotacao = new FornecedorItem
        {
            FornecedorId = dto.FornecedorId,
            ItemId = dto.ItemId,
            PrecoFornecedor = dto.PrecoFornecedor,
            PrazoEntregaDias = dto.PrazoEntregaDias,
            QuantidadeMinima = dto.QuantidadeMinima,
            Observacao = dto.Observacao,
            VigenteAte = dto.VigenteAte
        };

        await fornecedorItemRepo.AdicionarAsync(cotacao, ct);
        await fornecedorItemRepo.SalvarAsync(ct);
        return ToItemResponse(cotacao, fornecedor.NomeFantasia ?? fornecedor.RazaoSocial, item);
    }

    public async Task<FornecedorItemResponse> AtualizarCotacaoAsync(int id, AtualizarFornecedorItemRequest dto, CancellationToken ct = default)
    {
        var cotacao = await fornecedorItemRepo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Cotação {id} não encontrada.");

        cotacao.PrecoFornecedor = dto.PrecoFornecedor;
        cotacao.PrazoEntregaDias = dto.PrazoEntregaDias;
        cotacao.QuantidadeMinima = dto.QuantidadeMinima;
        cotacao.Observacao = dto.Observacao;
        cotacao.VigenteAte = dto.VigenteAte;
        cotacao.Ativo = dto.Ativo;
        cotacao.AtualizadoEm = DateTime.UtcNow;

        await fornecedorItemRepo.AtualizarAsync(cotacao, ct);
        await fornecedorItemRepo.SalvarAsync(ct);

        var nomeFornecedor = cotacao.Fornecedor?.NomeFantasia ?? cotacao.Fornecedor?.RazaoSocial ?? "";
        return ToItemResponse(cotacao, nomeFornecedor, cotacao.Item);
    }

    public async Task<ComparativoItemResponse> ComparativoPorItemAsync(int itemId, CancellationToken ct = default)
    {
        var item = await itemRepo.ObterPorIdAsync(itemId, ct)
            ?? throw new KeyNotFoundException($"Item {itemId} não encontrado.");

        var comparativo = await fornecedorItemRepo.ComparativoPorItemAsync(itemId, ct);
        var menorPreco = comparativo.MinBy(c => c.PrecoFornecedor)?.PrecoFornecedor ?? 0;

        var cotacoes = comparativo.Select(c => new CotacaoFornecedorResponse(
            c.FornecedorId,
            c.NomeFornecedor,
            c.PrecoFornecedor,
            item.PrecoVenda > 0
                ? Math.Round((item.PrecoVenda - c.PrecoFornecedor) / item.PrecoVenda * 100, 2)
                : 0,
            c.PrazoEntregaDias,
            c.QuantidadeMinima,
            c.VigenteAte,
            c.PrecoFornecedor == menorPreco
        ));

        return new ComparativoItemResponse(item.Id, item.Nome, item.PrecoVenda, cotacoes);
    }

    public async Task<IEnumerable<RankingFornecedorResponse>> RankingPorEventoAsync(int eventoId, CancellationToken ct = default)
    {
        var ranking = await fornecedorItemRepo.RankingPorEventoAsync(eventoId, ct);
        return ranking
            .OrderBy(r => r.CustoTotal)
            .Select((r, i) => new RankingFornecedorResponse(
                i + 1,
                r.FornecedorId,
                r.NomeFornecedor,
                r.TotalItens,
                r.CustoTotal,
                r.Margem
            ));
    }

    // ─── Mappers privados ─────────────────────────────────────────────────────
    private static FornecedorResponse ToResponse(Fornecedor f) => new(
        f.Id, f.RazaoSocial, f.NomeFantasia, f.CNPJ,
        f.Telefone, f.Email, f.Observacao, f.Ativo, f.CriadoEm);

    private static FornecedorItemResponse ToItemResponse(FornecedorItem fi, string nomeFornecedor, Item item) => new(
        fi.Id,
        fi.FornecedorId,
        nomeFornecedor,
        fi.ItemId,
        item.Nome,
        fi.PrecoFornecedor,
        item.PrecoVenda,
        item.PrecoVenda > 0
            ? Math.Round((item.PrecoVenda - fi.PrecoFornecedor) / item.PrecoVenda * 100, 2)
            : 0,
        fi.PrazoEntregaDias,
        fi.QuantidadeMinima,
        fi.Observacao,
        fi.VigenteAte,
        fi.Ativo,
        fi.VigenteAte >= DateTime.UtcNow.Date
    );
}