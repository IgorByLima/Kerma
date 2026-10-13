namespace Kerma.Domain.Entities;

public class CozinhaEvento : EntidadeBase
{
    public int EventoId { get; set; }
    public string Nome { get; set; } = null!;      // ex: "Cozinha Fria", "Bar"
    public bool Ativa { get; set; } = true;

    public Evento Evento { get; set; } = null!;
    public ICollection<Item> Itens { get; set; } = [];
}