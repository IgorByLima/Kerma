using System.Net.Sockets;

namespace Kerma.Domain.Entities;

public class ItemVenda : EntidadeBase
{
    public int VendaId { get; set; }
    public int ItemId { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }          // fotografia
    public decimal CustoUnitario { get; set; }          // fotografia
    public decimal Subtotal { get; set; }

    public Venda Venda { get; set; } = null!;
    public Item Item { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = [];
}