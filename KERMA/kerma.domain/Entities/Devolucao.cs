using Kerma.Domain.Enums;

namespace Kerma.Domain.Entities;

public class Devolucao : EntidadeBase
{
    public int TicketId { get; set; }
    public int? NovoTicketId { get; set; }
    public TipoDevolucao Tipo { get; set; }
    public string Motivo { get; set; } = null!;
    public string AutorizadoPor { get; set; } = null!;

    public Ticket Ticket { get; set; } = null!;
    public Ticket? NovoTicket { get; set; }
}