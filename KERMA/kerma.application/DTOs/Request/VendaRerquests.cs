namespace Kerma.Application.DTOs.Request;

public record CriarVendaRequest(
    int EventoId,
    int CaixaId,
    string FormaPagamento    // "Dinheiro", "Pix", "CartaoDebito", "CartaoCredito"
);

public record AdicionarItemVendaRequest(
    int ItemId,
    int Quantidade
);