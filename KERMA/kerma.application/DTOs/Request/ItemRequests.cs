namespace Kerma.Application.DTOs.Request;

public record CriarItemRequest(
    int EventoId,
    string Nome,
    string? Descricao,
    decimal Custo,
    decimal PrecoVenda,
    int EstoqueInicial,
    int EstoqueMinimo,
    int? CozinhaEventoId
);

public record AtualizarItemRequest(
    string Nome,
    string? Descricao,
    decimal Custo,
    decimal PrecoVenda,
    int EstoqueMinimo,
    bool Ativo,
    int? CozinhaEventoId
);