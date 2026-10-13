namespace Kerma.Application.DTOs.Request;

public record AbrirCaixaRequest(
    int EventoId,
    decimal SaldoInicial
);

public record FecharCaixaRequest(
    decimal SaldoFinal,
    string? Observacao
);