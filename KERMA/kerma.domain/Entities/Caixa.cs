using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kerma.Domain.Enums;

namespace Kerma.Domain.Entities;

public class Caixa : EntidadeBase
{
    public int EventoId { get; set; }
    public int OperadorId { get; set; }
    public decimal SaldoInicial { get; set; }
    public decimal? SaldoFinal { get; set; }
    public DateTime AbertoEm { get; set; } = DateTime.UtcNow;
    public DateTime? FechadoEm { get; set; }
    public StatusCaixa Status { get; set; } = StatusCaixa.Aberto;
    public string? Observacao { get; set; }

    public Evento Evento { get; set; } = null!;
    public Usuario Operador { get; set; } = null!;
    public ICollection<Venda> Vendas { get; set; } = [];
}