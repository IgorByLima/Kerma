using Kerma.Domain.Enums;

namespace Kerma.Domain.Entities;

public class ImpressaoLog : EntidadeBase
{
    public int TicketId { get; set; }
    public string ImpressoraId { get; set; } = null!;
    public int Tentativas { get; set; } = 0;
    public StatusImpressao Status { get; set; } = StatusImpressao.Aguardando;
    public string? ErroMotivo { get; set; }
    public DateTime? ImprimeEm { get; set; }

    public Ticket Ticket { get; set; } = null!;
}