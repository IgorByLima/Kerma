namespace Kerma.Application.DTOs.Response;

public record TicketResponse(
    int Id,
    string Codigo,
    string Status,
    string? NomeCliente,
    string NomeItem,
    string EventoNome,
    string? RetiradoPor,
    DateTime? RetiradoEm,
    DateTime? EnviadoFilaEm,
    DateTime? ProntoEm,
    DateTime CriadoEm
);