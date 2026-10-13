using Kerma.Application.DTOs.Request;
using Kerma.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kerma.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DevolucoesController(IDevolucaoService service) : ControllerBase
{
    /// <summary>Cancela um ticket com autorização de admin</summary>
    [HttpPost("cancelar")]
    [Authorize]
    public async Task<IActionResult> Cancelar([FromBody] CancelarTicketRequest dto, CancellationToken ct)
    {
        try
        {
            var resultado = await service.CancelarTicketAsync(dto, ct);
            return Ok(resultado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Troca um ticket por outro item com autorização de admin</summary>
    [HttpPost("trocar")]
    [Authorize]
    public async Task<IActionResult> Trocar([FromBody] TrocarTicketRequest dto, CancellationToken ct)
    {
        try
        {
            var resultado = await service.TrocarTicketAsync(dto, ct);
            return Ok(resultado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Lista devoluções de um evento</summary>
    [HttpGet("evento/{eventoId:int}")]
    [Authorize]
    public async Task<IActionResult> ListarPorEvento(int eventoId, CancellationToken ct)
    {
        var devolucoes = await service.ListarPorEventoAsync(eventoId, ct);
        return Ok(devolucoes);
    }
}