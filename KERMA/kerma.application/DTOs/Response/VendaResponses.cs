namespace Kerma.Application.DTOs.Response;

public record ItemVendaResponse(
    int Id,
    int ItemId,
    string NomeItem,
    int Quantidade,
    decimal PrecoUnitario,
    decimal CustoUnitario,
    decimal Subtotal
);

public record VendaResponse(
    int Id,
    int EventoId,
    int CaixaId,
    int OperadorId,
    decimal Total,
    string FormaPagamento,
    string Status,
    string Origem,
    DateTime CriadoEm,
    DateTime? PagoEm,
    IEnumerable<ItemVendaResponse> Itens
);

public record TicketSimplificadoResponse(
    int Id,
    int ItemVendaId,
    string Codigo,
    string Barcode,
    string Status
);

public record PagamentoResponse(
    VendaResponse Venda,
    IEnumerable<TicketSimplificadoResponse> Tickets
);