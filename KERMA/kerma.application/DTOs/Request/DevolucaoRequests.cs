namespace Kerma.Application.DTOs.Request;

public record CancelarTicketRequest(
    string CodigoTicket,
    string Motivo,
    string AutorizadoPor
);

public record TrocarTicketRequest(
    string CodigoTicket,
    int NovoItemId,
    string Motivo,
    string AutorizadoPor
);