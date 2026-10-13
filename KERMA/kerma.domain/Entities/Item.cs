using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kerma.Domain.Enums;

namespace Kerma.Domain.Entities;

public class Item : EntidadeBase
{
    public int EventoId { get; set; }
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
    public decimal Custo { get; set; }
    public decimal PrecoVenda { get; set; }
    public int EstoqueInicial { get; set; }
    public int EstoqueAtual { get; set; }
    public int EstoqueMinimo { get; set; } = 5;
    public StatusEstoque StatusEstoque { get; set; } = StatusEstoque.Verde;
    public bool Ativo { get; set; } = true;
    public int? CozinhaEventoId { get; set; }
    public CozinhaEvento? CozinhaEvento { get; set; }

    // Propriedades calculadas — nunca persistidas
    public decimal MargemAbsoluta => PrecoVenda - Custo;
    public decimal MargemPercentual => PrecoVenda > 0 ? Math.Round((PrecoVenda - Custo) / PrecoVenda * 100, 2) : 0;
    public bool EstoqueCritico => EstoqueAtual <= EstoqueMinimo;

    public Evento Evento { get; set; } = null!;
    public ICollection<ItemVenda> ItensVenda { get; set; } = [];
}