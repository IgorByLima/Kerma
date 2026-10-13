namespace Kerma.Application.DTOs.Request;

public record CriarFornecedorRequest(
    string RazaoSocial,
    string? NomeFantasia,
    string CNPJ,
    string? Telefone,
    string? Email,
    string? Observacao
);

public record AtualizarFornecedorRequest(
    string RazaoSocial,
    string? NomeFantasia,
    string? Telefone,
    string? Email,
    string? Observacao,
    bool Ativo
);

public record CriarFornecedorItemRequest(
    int FornecedorId,
    int ItemId,
    decimal PrecoFornecedor,
    int? PrazoEntregaDias,
    int? QuantidadeMinima,
    string? Observacao,
    DateTime VigenteAte
);

public record AtualizarFornecedorItemRequest(
    decimal PrecoFornecedor,
    int? PrazoEntregaDias,
    int? QuantidadeMinima,
    string? Observacao,
    DateTime VigenteAte,
    bool Ativo
);