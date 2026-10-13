using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kerma.Domain.Enums;

public enum PerfilUsuario
{
    Admin,
    Operador,
    Visualizador
}

public enum StatusEvento
{
    Rascunho,
    Ativo,
    Encerrado
}

public enum StatusVenda
{
    Aberta,
    Paga,
    Cancelada
}

public enum OrigemVenda
{
    Caixa,
    AppExterno,
    Totem
}

public enum FormaPagamento
{
    Dinheiro,
    Pix,
    CartaoDebito,
    CartaoCredito
}

public enum StatusCaixa
{
    Aberto,
    Fechado
}

public enum StatusTicket {
    Pendente,
    EmFila,
    EmPreparo,
    Pronto, 
    Retirado,
    Cancelado
}

public enum StatusEstoque
{
    Verde,
    Amarelo,
    Vermelho
}

public enum TipoDevolucao
{
    Cancelamento,
    Troca
}

public enum StatusImpressao
{
    Aguardando,
    Impresso,
    Cancelado
}
