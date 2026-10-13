using System.Linq;
using Kerma.Application.DTOs.Request;
using Kerma.Application.DTOs.Response;
using Kerma.Application.Utils;
using Kerma.Domain.Entities;
using Kerma.Domain.Enums;
using Kerma.Domain.Interfaces;

namespace Kerma.Application.Services;

public interface IVendaService

{
    Task<AnalyticsResponse> ObterAnalyticsAsync(int eventoId, int? caixaId, CancellationToken ct = default);
    Task<VendaResponse> CriarAsync(CriarVendaRequest dto, int operadorId, CancellationToken ct = default);
    Task<VendaResponse> AdicionarItemAsync(int vendaId, AdicionarItemVendaRequest dto, CancellationToken ct = default);
    Task<PagamentoResponse> ConfirmarPagamentoAsync(int vendaId, CancellationToken ct = default);
    Task<VendaResponse> CancelarAsync(int vendaId, CancellationToken ct = default);
    Task<VendaResponse?> ObterPorIdAsync(int vendaId, CancellationToken ct = default);
    Task<IEnumerable<VendaResponse>> ListarPorCaixaAsync(int caixaId, CancellationToken ct = default);
}

public class VendaService : IVendaService
{
    private readonly IVendaRepositorio vendaRepo;
    private readonly IItemRepositorio itemRepo;
    private readonly ICaixaRepositorio caixaRepo;
    private readonly IEstoqueService estoqueService;

    public VendaService(IVendaRepositorio vendaRepo, IItemRepositorio itemRepo, ICaixaRepositorio caixaRepo, IEstoqueService estoqueService)
    {
        this.vendaRepo = vendaRepo;
        this.itemRepo = itemRepo;
        this.caixaRepo = caixaRepo;
        this.estoqueService = estoqueService;
    }

    public async Task<VendaResponse> CriarAsync(CriarVendaRequest dto, int operadorId, CancellationToken ct = default)
    {
        var caixa = await caixaRepo.ObterPorIdAsync(dto.CaixaId, ct)
            ?? throw new KeyNotFoundException($"Caixa {dto.CaixaId} não encontrado.");

        if (caixa.Status != StatusCaixa.Aberto)
            throw new InvalidOperationException("Só é possível registrar vendas em caixas abertos.");

        if (caixa.OperadorId != operadorId)
            throw new InvalidOperationException("Este caixa pertence a outro operador.");

        var venda = new Venda
        {
            EventoId = dto.EventoId,
            CaixaId = dto.CaixaId,
            OperadorId = operadorId,
            FormaPagamento = Enum.Parse<FormaPagamento>(dto.FormaPagamento),
            Status = StatusVenda.Aberta,
            Origem = OrigemVenda.Caixa,
            Total = 0
        };

        await vendaRepo.AdicionarAsync(venda, ct);
        await vendaRepo.SalvarAsync(ct);
        return ToResponse(venda, Enumerable.Empty<ItemVenda>());

    }

    public async Task<VendaResponse> AdicionarItemAsync(int vendaId, AdicionarItemVendaRequest dto, CancellationToken ct = default)
    {
        var venda = await vendaRepo.ObterComItensAsync(vendaId, ct)
            ?? throw new KeyNotFoundException($"Venda {vendaId} não encontrada.");

        if (venda.Status != StatusVenda.Aberta)
            throw new InvalidOperationException("Só é possível adicionar itens a vendas em aberto.");

        var item = await itemRepo.ObterPorIdAsync(dto.ItemId, ct)
            ?? throw new KeyNotFoundException($"Item {dto.ItemId} não encontrado.");

        if (!item.Ativo)
            throw new InvalidOperationException("Este item está inativo.");

        if (item.EstoqueAtual < dto.Quantidade)
            throw new InvalidOperationException($"Estoque insuficiente. Disponível: {item.EstoqueAtual}.");

        var itemExistente = venda.ItensVenda.FirstOrDefault(iv => iv.ItemId == dto.ItemId);
        if (itemExistente is not null)
        {
            itemExistente.Quantidade += dto.Quantidade;
            itemExistente.Subtotal = itemExistente.Quantidade * itemExistente.PrecoUnitario;
        }
        else
        {
            venda.ItensVenda.Add(new ItemVenda
            {
                VendaId = vendaId,
                ItemId = dto.ItemId,
                Quantidade = dto.Quantidade,
                PrecoUnitario = item.PrecoVenda,
                CustoUnitario = item.Custo,
                Subtotal = dto.Quantidade * item.PrecoVenda
            });
        }

        venda.Total = venda.ItensVenda.Sum(iv => iv.Subtotal);
        venda.AtualizadoEm = DateTime.UtcNow;

        await vendaRepo.AtualizarAsync(venda, ct);
        await vendaRepo.SalvarAsync(ct);
        return ToResponse(venda, venda.ItensVenda);
    }
    public async Task<AnalyticsResponse> ObterAnalyticsAsync(int eventoId, int? caixaId, CancellationToken ct = default)
    {
        var vendas = caixaId.HasValue
            ? await vendaRepo.ListarPorCaixaAsync(caixaId.Value, ct)
            : await vendaRepo.ListarPorEventoAsync(eventoId, ct);

        var vendasPagas = vendas.Where(v => v.Status == StatusVenda.Paga).ToList();

        var totalVendido = vendasPagas.Sum(v => v.Total);
        var custoEstimado = vendasPagas.SelectMany(v => v.ItensVenda).Sum(iv => iv.CustoUnitario * iv.Quantidade);
        var lucroEstimado = totalVendido - custoEstimado;
        var margemPercent = totalVendido > 0 ? Math.Round(lucroEstimado / totalVendido * 100, 1) : 0;
        var ticketMedio = vendasPagas.Count > 0 ? Math.Round(totalVendido / vendasPagas.Count, 2) : 0;

        var todosTickets = vendasPagas.SelectMany(v => v.ItensVenda).SelectMany(iv => iv.Tickets).ToList();
        var retirados = todosTickets.Count(t => t.Status == StatusTicket.Retirado);
        var pendentes = todosTickets.Count(t =>
            t.Status == StatusTicket.Pendente ||
            t.Status == StatusTicket.EmFila ||
            t.Status == StatusTicket.EmPreparo ||
            t.Status == StatusTicket.Pronto);
        var devolucoes = todosTickets.Count(t => t.Status == StatusTicket.Cancelado);

        var vendasPorHora = vendasPagas
            .GroupBy(v => v.CriadoEm.Hour)
            .Select(g => new VendasPorHoraResponse(g.Key, g.Count(), g.Sum(v => v.Total)))
            .OrderBy(v => v.Hora)
            .ToList();

        var rankingItens = vendasPagas
            .SelectMany(v => v.ItensVenda)
            .GroupBy(iv => iv.Item?.Nome ?? "")
            .Select(g => new ItemRankingResponse(g.Key, g.Sum(iv => iv.Quantidade), g.Sum(iv => iv.PrecoUnitario * iv.Quantidade)))
            .OrderByDescending(r => r.Quantidade)
            .Take(10)
            .ToList();

        var totalGeral = vendasPagas.Count > 0 ? vendasPagas.Count : 1;
        var formasPagamento = vendasPagas
            .GroupBy(v => v.FormaPagamento.ToString())
            .Select(g => new FormaPagamentoResponse(
                g.Key,
                g.Count(),
                g.Sum(v => v.Total),
                Math.Round((decimal)g.Count() / totalGeral * 100, 1)))
            .OrderByDescending(f => f.Quantidade)
            .ToList();

        return new AnalyticsResponse(
            TotalVendido: totalVendido,
            CustoEstimado: custoEstimado,
            LucroEstimado: lucroEstimado,
            MargemPercent: margemPercent,
            TicketMedio: ticketMedio,
            TotalVendas: vendasPagas.Count,
            TotalTickets: todosTickets.Count,
            TicketsRetirados: retirados,
            TicketsPendentes: pendentes,
            TotalDevolucoes: devolucoes,
            VendasPorHora: vendasPorHora,
            RankingItens: rankingItens,
            FormasPagamento: formasPagamento
        );
    }


    public async Task<PagamentoResponse> ConfirmarPagamentoAsync(int vendaId, CancellationToken ct = default)
    {
        var venda = await vendaRepo.ObterComItensAsync(vendaId, ct)
            ?? throw new KeyNotFoundException($"Venda {vendaId} não encontrada.");

        if (venda.Status != StatusVenda.Aberta)
            throw new InvalidOperationException("Esta venda não está em aberto.");

        if (!venda.ItensVenda.Any())
            throw new InvalidOperationException("Não é possível confirmar uma venda sem itens.");

        var tickets = new List<Ticket>();

        foreach (var itemVenda in venda.ItensVenda)
        {
            var item = itemVenda.Item
                ?? throw new KeyNotFoundException($"Item {itemVenda.ItemId} não encontrado.");

            // Verifica estoque mas NÃO debita ainda — só na impressão
            if (item.EstoqueAtual < itemVenda.Quantidade)
                throw new InvalidOperationException(
                    $"Estoque insuficiente para '{item.Nome}'. Disponível: {item.EstoqueAtual}.");

            for (int i = 0; i < itemVenda.Quantidade; i++)
            {
                var codigo = GeradorCodigo.GerarTicket();
                tickets.Add(new Ticket
                {
                    ItemVendaId = itemVenda.Id,
                    Codigo = codigo,
                    Barcode = codigo,
                    Status = StatusTicket.Pendente,
                    CozinhaEventoId = item.CozinhaEventoId,
                    CriadoEm = DateTime.UtcNow,
                    AtualizadoEm = DateTime.UtcNow,
                });
            }
        }

        // Salva os tickets
        foreach (var ticket in tickets)
            await vendaRepo.AdicionarTicketAsync(ticket, ct);

        // Marca venda como paga
        venda.Status = StatusVenda.Paga;
        venda.PagoEm = DateTime.UtcNow;
        venda.AtualizadoEm = DateTime.UtcNow;
        await vendaRepo.AtualizarAsync(venda, ct);
        await vendaRepo.SalvarAsync(ct);

        var ticketsResponse = tickets.Select(t => new TicketSimplificadoResponse(
            t.Id, t.ItemVendaId, t.Codigo, t.Barcode, t.Status.ToString()
        ));

        return new PagamentoResponse(
            ToResponse(venda, venda.ItensVenda),
            ticketsResponse
        );
    }

    public async Task<VendaResponse> CancelarAsync(int vendaId, CancellationToken ct = default)
    {
        var venda = await vendaRepo.ObterComItensAsync(vendaId, ct)
            ?? throw new KeyNotFoundException($"Venda {vendaId} não encontrada.");

        if (venda.Status == StatusVenda.Paga)
            throw new InvalidOperationException("Vendas pagas não podem ser canceladas diretamente. Use o módulo de devolução.");

        if (venda.Status == StatusVenda.Cancelada)
            throw new InvalidOperationException("Esta venda já está cancelada.");

        venda.Status = StatusVenda.Cancelada;
        venda.AtualizadoEm = DateTime.UtcNow;

        await vendaRepo.AtualizarAsync(venda, ct);
        await vendaRepo.SalvarAsync(ct);
        return ToResponse(venda, venda.ItensVenda);
    }

    public async Task<VendaResponse?> ObterPorIdAsync(int vendaId, CancellationToken ct = default)
    {
        var venda = await vendaRepo.ObterComItensAsync(vendaId, ct);
        return venda is null ? null : ToResponse(venda, venda.ItensVenda);
    }

    public async Task<IEnumerable<VendaResponse>> ListarPorCaixaAsync(int caixaId, CancellationToken ct = default)
    {
        var lista = await vendaRepo.ListarPorCaixaAsync(caixaId, ct);
        return lista.Select(v => ToResponse(v, v.ItensVenda));
    }
    
    private static VendaResponse ToResponse(Venda v, IEnumerable<ItemVenda> itens) => new(
        v.Id, v.EventoId, v.CaixaId, v.OperadorId,
        v.Total, v.FormaPagamento.ToString(),
        v.Status.ToString(), v.Origem.ToString(),
        v.CriadoEm, v.PagoEm,
        itens.Select(iv => new ItemVendaResponse(
            iv.Id, iv.ItemId, iv.Item?.Nome ?? "",
            iv.Quantidade, iv.PrecoUnitario,
            iv.CustoUnitario, iv.Subtotal)));
}