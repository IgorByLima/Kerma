namespace Kerma.Application.DTOs.Response;

public record DevolucaoResponse(
    int Id,
    string Tipo,
    string Motivo,
    string AutorizadoPor,
    string CodigoTicket,
    string NomeItem,
    string? NovoCodigoTicket,
    DateTime CriadoEm
);