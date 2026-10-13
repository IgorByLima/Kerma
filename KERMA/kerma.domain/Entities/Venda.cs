using Kerma.Domain.Enums;

namespace Kerma.Domain.Entities;

public class Venda : EntidadeBase
{
    public int EventoId { get; set; }
    public int CaixaId { get; set; }
    public int OperadorId { get; set; }
    public decimal Total { get; set; }
    public FormaPagamento FormaPagamento { get; set; }

    public StatusVenda Status { get; set; } = StatusVenda.Aberta;
    public OrigemVenda Origem { get; set; } = OrigemVenda.Caixa;
    public string? VoucherCodigo { get; set; }
    public DateTime? PagoEm { get; set; }

    public Evento Evento { get; set; } = null!;
    public Caixa Caixa { get; set; } = null!;
    public Usuario Operador { get; set; } = null!;
    public ICollection<ItemVenda> ItensVenda { get; set; } = [];
}