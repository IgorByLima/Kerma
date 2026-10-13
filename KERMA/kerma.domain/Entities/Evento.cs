using Kerma.Domain.Enums;

namespace Kerma.Domain.Entities;

public class Evento : EntidadeBase
{
    public string Nome { get; set; } = null!;
    public string Codigo { get; set; } = null!;        // ex: EV-C3D4
    public string? Descricao { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
    public StatusEvento Status { get; set; } = StatusEvento.Rascunho;
    public int CriadoPor { get; set; }

    public Usuario Criador { get; set; } = null!;
    public ICollection<Item> Itens { get; set; } = [];
    public ICollection<Caixa> Caixas { get; set; } = [];
    public ICollection<Venda> Vendas { get; set; } = [];
    public ICollection<UsuarioEvento> UsuariosHabilitados { get; set; } = [];
}