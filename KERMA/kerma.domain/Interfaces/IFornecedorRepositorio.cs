using Kerma.Domain.Entities;

namespace Kerma.Domain.Interfaces;

// ─── Fornecedor ──────────────────────────────────────────────────────────────
public interface IFornecedorRepositorio : IRepositorio<Fornecedor>
{
    Task<Fornecedor?> ObterPorCNPJAsync(string cnpj, CancellationToken ct = default);
    Task<IEnumerable<Fornecedor>> ListarAtivosAsync(CancellationToken ct = default);
}

// ─── FornecedorItem ──────────────────────────────────────────────────────────
public interface IFornecedorItemRepositorio : IRepositorio<FornecedorItem>
{
    /// <summary>Retorna todos os fornecedores com preço para um item específico</summary>
    Task<IEnumerable<FornecedorItem>> ListarPorItemAsync(int itemId, CancellationToken ct = default);

    /// <summary>Retorna todos os itens cotados por um fornecedor</summary>
    Task<IEnumerable<FornecedorItem>> ListarPorFornecedorAsync(int fornecedorId, CancellationToken ct = default);

    /// <summary>Comparativo: menor preço por item entre todos os fornecedores ativos e vigentes</summary>
    Task<IEnumerable<ComparativoFornecedorItem>> ComparativoPorItemAsync(int itemId, CancellationToken ct = default);

    /// <summary>Ranking de fornecedores por custo total do evento (soma de todos os itens)</summary>
    Task<IEnumerable<RankingFornecedor>> RankingPorEventoAsync(int eventoId, CancellationToken ct = default);
}

// ─── Projeções usadas nos relatórios ─────────────────────────────────────────
public record ComparativoFornecedorItem(
    int FornecedorId,
    string NomeFornecedor,
    int ItemId,
    string NomeItem,
    decimal PrecoFornecedor,
    decimal PrecoVendaItem,       // para calcular margem projetada
    int? PrazoEntregaDias,
    int? QuantidadeMinima,
    DateTime VigenteAte,
    bool MenorPreco               // true = este é o mais barato para este item
);

public record RankingFornecedor(
    int FornecedorId,
    string NomeFornecedor,
    int TotalItens,
    decimal CustoTotal,           // soma de PrecoFornecedor * EstoqueInicial de cada item
    decimal Margem                // margem média projetada se todos os itens vierem deste fornecedor
);