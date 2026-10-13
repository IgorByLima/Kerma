namespace Kerma.Application.DTOs.Response;

public record ItemResponse(
    int Id,
    int EventoId,
    string Nome,
    string? Descricao,
    decimal Custo,
    decimal PrecoVenda,
    decimal MargemAbsoluta,
    decimal MargemPercentual,
    int EstoqueInicial,
    int EstoqueAtual,
    int EstoqueMinimo,
    bool EstoqueCritico,
    string StatusEstoque,
    bool Ativo,
    DateTime CriadoEm,
    int? CozinhaEventoId,
    string? CozinhaNome
);