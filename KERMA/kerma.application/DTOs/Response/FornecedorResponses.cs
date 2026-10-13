namespace Kerma.Application.DTOs.Response;

public record FornecedorResponse(
    int Id,
    string RazaoSocial,
    string? NomeFantasia,
    string CNPJ,
    string? Telefone,
    string? Email,
    string? Observacao,
    bool Ativo,
    DateTime CriadoEm
);

public record FornecedorItemResponse(
    int Id,
    int FornecedorId,
    string NomeFornecedor,
    int ItemId,
    string NomeItem,
    decimal PrecoFornecedor,
    decimal PrecoVendaItem,
    decimal MargemProjetada,
    int? PrazoEntregaDias,
    int? QuantidadeMinima,
    string? Observacao,
    DateTime VigenteAte,
    bool Ativo,
    bool Vigente
);

public record CotacaoFornecedorResponse(
    int FornecedorId,
    string NomeFornecedor,
    decimal PrecoFornecedor,
    decimal MargemProjetada,
    int? PrazoEntregaDias,
    int? QuantidadeMinima,
    DateTime VigenteAte,
    bool MenorPreco
);

public record ComparativoItemResponse(
    int ItemId,
    string NomeItem,
    decimal PrecoVenda,
    IEnumerable<CotacaoFornecedorResponse> Cotacoes
);

public record RankingFornecedorResponse(
    int Posicao,
    int FornecedorId,
    string NomeFornecedor,
    int TotalItens,
    decimal CustoTotal,
    decimal MargemMedia
);