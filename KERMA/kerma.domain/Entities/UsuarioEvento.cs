namespace Kerma.Domain.Entities;

public class UsuarioEvento : EntidadeBase
{
    public int UsuarioId { get; set; }
    public int EventoId { get; set; }
    public bool Ativo { get; set; } = true;

    public Usuario Usuario { get; set; } = null!;
    public Evento Evento { get; set; } = null!;
}