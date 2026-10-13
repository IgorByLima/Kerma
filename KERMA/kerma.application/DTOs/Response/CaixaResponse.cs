namespace Kerma.Application.DTOs.Response;

public record CaixaResponse(
    int Id,
    int EventoId,
    int OperadorId,
    decimal SaldoInicial,
    decimal? SaldoFinal,
    DateTime AbertoEm,
    DateTime? FechadoEm,
    string Status,
    string? Observacao
);