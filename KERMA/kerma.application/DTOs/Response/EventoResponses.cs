namespace Kerma.Application.DTOs.Response;

public record EventoResponse(
    int Id,
    string Nome,
    string Codigo,
    string? Descricao,
    DateOnly DataInicio,
    DateOnly DataFim,
    string Status,
    int CriadoPor,
    DateTime CriadoEm
);