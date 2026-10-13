namespace Kerma.Domain.Entities;

// Relacionamento N:N entre Fornecedor e Item com dados extras (preço, prazo etc.)
public class FornecedorItem : EntidadeBase
{
    public int FornecedorId { get; set; }
    public int ItemId { get; set; }
    public decimal PrecoFornecedor { get; set; }        // preço cotado por este fornecedor
    public int? PrazoEntregaDias { get; set; }
    public int? QuantidadeMinima { get; set; }
    public string? Observacao { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime VigenteAte { get; set; }            // validade da cotação

    public Fornecedor Fornecedor { get; set; } = null!;
    public Item Item { get; set; } = null!;
}