namespace Kerma.Application.DTOs.Request;

public record CriarEventoRequest(
    string Nome,
    string? Descricao,
    DateOnly DataInicio,
    DateOnly DataFim
);

public record AtualizarEventoRequest(
    string Nome,
    string? Descricao,
    DateOnly DataInicio,
    DateOnly DataFim,
    string Status    // "Rascunho", "Ativo", "Encerrado"
);