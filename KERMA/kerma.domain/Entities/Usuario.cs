using Kerma.Domain.Enums;

namespace Kerma.Domain.Entities;

public class Usuario : EntidadeBase
{
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Codigo { get; set; } = null!;        // ex: US-A1B2
    public string SenhaHash { get; set; } = null!;
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Operador;
    public bool Ativo { get; set; } = true;

    public ICollection<Evento> EventosCriados { get; set; } = [];
    public ICollection<Caixa> Caixas { get; set; } = [];
    public ICollection<Venda> Vendas { get; set; } = [];
    public ICollection<UsuarioEvento> EventosHabilitados { get; set; } = [];
}