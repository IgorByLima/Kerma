using Kerma.Application.Services;
using Kerma.Domain.Enums;
using Kerma.Domain.Interfaces;
using Kerma.Infrastructure.Printing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kerma.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ImpressaoController(
    IImpressoraService impressoraService,
    ITicketRepositorio ticketRepo,
    IItemRepositorio itemRepo) : ControllerBase
{
    /// <summary>Imprime tickets e debita estoque</summary>
    [HttpPost("imprimir")]
    [AllowAnonymous]
    public async Task<IActionResult> Imprimir([FromBody] ImprimirRequest dto, CancellationToken ct)
    {
        try
        {
            // Debita estoque e cancela tickets não impressos
            foreach (var ticketDto in dto.Tickets)
            {
                var ticket = await ticketRepo.ObterPorCodigoAsync(ticketDto.Codigo, ct);
                if (ticket is null) continue;

                // Só debita se ainda não foi debitado (status Pendente)
                if (ticket.Status == StatusTicket.Pendente)
                {
                    var itemId = ticket.ItemVenda?.ItemId ?? 0;
                    if (itemId > 0)
                        await itemRepo.DecrementarEstoqueAsync(itemId, 1, ct);
                }
            }

            await impressoraService.ImprimirTicketsAsync(dto.Tickets);
            return Ok(new { mensagem = "Tickets impressos com sucesso." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = $"Erro ao imprimir: {ex.Message}" });
        }
    }
}

public record ImprimirRequest(
    IEnumerable<TicketImpressaoDto> Tickets
);