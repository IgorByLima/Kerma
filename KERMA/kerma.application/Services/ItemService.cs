using Kerma.Application.DTOs.Request;
using Kerma.Application.DTOs.Response;
using Kerma.Domain.Entities;
using Kerma.Domain.Interfaces;

namespace Kerma.Application.Services;

// ─── Interface ────────────────────────────────────────────────────────────────
public interface IItemService
{
    Task<ItemResponse> CriarAsync(CriarItemRequest dto, CancellationToken ct = default);
    Task<ItemResponse> AtualizarAsync(int id, AtualizarItemRequest dto, CancellationToken ct = default);
    Task<ItemResponse?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ItemResponse>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default);
    Task<IEnumerable<ItemResponse>> ListarComEstoqueCriticoAsync(int eventoId, CancellationToken ct = default);
    Task DesativarAsync(int id, CancellationToken ct = default);
}

// ─── Implementação ────────────────────────────────────────────────────────────
public class ItemService(
    IItemRepositorio itemRepo,
    IEventoRepositorio eventoRepo) : IItemService
{
    public async Task<ItemResponse> CriarAsync(CriarItemRequest dto, CancellationToken ct = default)
    {
        var evento = await eventoRepo.ObterPorIdAsync(dto.EventoId, ct)
            ?? throw new KeyNotFoundException($"Evento {dto.EventoId} não encontrado.");

        if (evento.Status == Domain.Enums.StatusEvento.Encerrado)
            throw new InvalidOperationException("Não é possível adicionar itens a um evento encerrado.");

        if (dto.Custo < 0 || dto.PrecoVenda < 0)
            throw new InvalidOperationException("Custo e preço de venda não podem ser negativos.");

        if (dto.PrecoVenda < dto.Custo)
            throw new InvalidOperationException("Preço de venda não pode ser menor que o custo.");

        var item = new Item
        {
            EventoId = dto.EventoId,
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Custo = dto.Custo,
            PrecoVenda = dto.PrecoVenda,
            EstoqueInicial = dto.EstoqueInicial,
            EstoqueAtual = dto.EstoqueInicial,
            EstoqueMinimo = dto.EstoqueMinimo,
            CozinhaEventoId = dto.CozinhaEventoId,
        };

        await itemRepo.AdicionarAsync(item, ct);
        await itemRepo.SalvarAsync(ct);
        return ToResponse(item);
    }

    public async Task<ItemResponse> AtualizarAsync(int id, AtualizarItemRequest dto, CancellationToken ct = default)
    {
        var item = await itemRepo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Item {id} não encontrado.");

        if (dto.PrecoVenda < dto.Custo)
            throw new InvalidOperationException("Preço de venda não pode ser menor que o custo.");

        item.Nome = dto.Nome;
        item.Descricao = dto.Descricao;
        item.Custo = dto.Custo;
        item.PrecoVenda = dto.PrecoVenda;
        item.EstoqueMinimo = dto.EstoqueMinimo;
        item.Ativo = dto.Ativo;
        item.AtualizadoEm = DateTime.UtcNow;

        await itemRepo.AtualizarAsync(item, ct);
        await itemRepo.SalvarAsync(ct);
        return ToResponse(item);
    }

    public async Task<ItemResponse?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var item = await itemRepo.ObterPorIdAsync(id, ct);
        return item is null ? null : ToResponse(item);
    }

    public async Task<IEnumerable<ItemResponse>> ListarPorEventoAsync(int eventoId, CancellationToken ct = default)
    {
        var lista = await itemRepo.ListarPorEventoAsync(eventoId, ct);
        return lista.Select(ToResponse);
    }

    public async Task<IEnumerable<ItemResponse>> ListarComEstoqueCriticoAsync(int eventoId, CancellationToken ct = default)
    {
        var lista = await itemRepo.ListarComEstoqueCriticoAsync(eventoId, ct);
        return lista.Select(ToResponse);
    }

    public async Task DesativarAsync(int id, CancellationToken ct = default)
    {
        var item = await itemRepo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Item {id} não encontrado.");

        item.Ativo = false;
        item.AtualizadoEm = DateTime.UtcNow;

        await itemRepo.AtualizarAsync(item, ct);
        await itemRepo.SalvarAsync(ct);
    }

    // ─── Mapper privado ───────────────────────────────────────────────────────
    private static ItemResponse ToResponse(Item i) => new(
    i.Id,
    i.EventoId,
    i.Nome,
    i.Descricao,
    i.Custo,
    i.PrecoVenda,
    i.MargemAbsoluta,
    i.MargemPercentual,
    i.EstoqueInicial,
    i.EstoqueAtual,
    i.EstoqueMinimo,
    i.EstoqueCritico,
    i.StatusEstoque.ToString(),
    i.Ativo,
    i.CriadoEm,
    i.CozinhaEventoId,
    i.CozinhaEvento?.Nome);
}