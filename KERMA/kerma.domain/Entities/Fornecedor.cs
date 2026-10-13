namespace Kerma.Domain.Entities;

public class Fornecedor : EntidadeBase
{
    public string RazaoSocial { get; set; } = null!;
    public string? NomeFantasia { get; set; }
    public string CNPJ { get; set; } = null!;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Observacao { get; set; }
    public bool Ativo { get; set; } = true;
    public int CadastradoPor { get; set; }              // só admin

    public Usuario Cadastrador { get; set; } = null!;
    public ICollection<FornecedorItem> FornecedorItens { get; set; } = [];
}