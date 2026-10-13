using Kerma.Domain.Enums;

namespace Kerma.Domain.Entities;

public class Ticket : EntidadeBase
{
    public int ItemVendaId { get; set; }
    public string Codigo { get; set; } = null!;
    public string Barcode { get; set; } = null!;
    public StatusTicket Status { get; set; } = StatusTicket.Pendente;
    public string? NomeCliente { get; set; }
    public string? RetiradoPor { get; set; }
    public DateTime? RetiradoEm { get; set; }
    public DateTime? EnviadoFilaEm { get; set; }
    public DateTime? ProntoEm { get; set; }

    public ItemVenda ItemVenda { get; set; } = null!;
    public ICollection<ImpressaoLog> ImpressaoLogs { get; set; } = [];
    public int? CozinhaEventoId { get; set; }
    public CozinhaEvento? CozinhaEvento { get; set; }
}