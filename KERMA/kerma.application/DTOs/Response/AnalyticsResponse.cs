namespace Kerma.Application.DTOs.Response;

public record VendasPorHoraResponse(int Hora, int Quantidade, decimal Total);
public record ItemRankingResponse(string NomeItem, int Quantidade, decimal Total);
public record FormaPagamentoResponse(string Forma, int Quantidade, decimal Total, decimal Percentual);

public record AnalyticsResponse(
    // Financeiro
    decimal TotalVendido,
    decimal CustoEstimado,
    decimal LucroEstimado,
    decimal MargemPercent,
    decimal TicketMedio,
    int TotalVendas,
    int TotalTickets,
    int TicketsRetirados,
    int TicketsPendentes,
    int TotalDevolucoes,

    // Gráficos
    IEnumerable<VendasPorHoraResponse> VendasPorHora,
    IEnumerable<ItemRankingResponse> RankingItens,
    IEnumerable<FormaPagamentoResponse> FormasPagamento
);