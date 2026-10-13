using Kerma.Application.DTOs.Request;
using Kerma.Application.DTOs.Response;
using Kerma.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Kerma.API.Hubs;

namespace Kerma.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TicketsController(
    ITicketService service,
    IHubContext<FilaCozinhaHub> hub) : ControllerBase
{
    /// <summary>Obtém ticket por código</summary>
    [HttpGet("codigo/{codigo}")]
    [Authorize]
    public async Task<IActionResult> ObterPorCodigo(string codigo, CancellationToken ct)
    {
        var ticket = await service.ObterPorCodigoAsync(codigo, ct);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    /// <summary>Lista todos os tickets de um evento</summary>
    [HttpGet("evento/{eventoId:int}")]
    [Authorize]
    public async Task<IActionResult> ListarPorEvento(int eventoId, CancellationToken ct)
    {
        var tickets = await service.ListarPorEventoAsync(eventoId, ct);
        return Ok(tickets);
    }

    /// <summary>Lista fila da cozinha com filtro opcional por cozinha</summary>
    [HttpGet("fila/{eventoId:int}")]
    [Authorize]
    public async Task<IActionResult> ListarFila(int eventoId, [FromQuery] int? cozinhaEventoId, CancellationToken ct)
    {
        IEnumerable<TicketResponse> tickets;
        if (cozinhaEventoId.HasValue)
            tickets = await service.ListarFilaCozinhaPorCozinhaAsync(eventoId, cozinhaEventoId.Value, ct);
        else
            tickets = await service.ListarFilaCozinhaAsync(eventoId, ct);
        return Ok(tickets);
    }

    /// <summary>Lista tickets prontos para retirada</summary>
    [HttpGet("prontos/{eventoId:int}")]
    [Authorize]
    public async Task<IActionResult> ListarProntos(int eventoId, CancellationToken ct)
    {
        var tickets = await service.ListarProntosPorEventoAsync(eventoId, ct);
        return Ok(tickets);
    }

    /// <summary>Envia tickets para a fila da cozinha</summary>
    [HttpPost("fila")]
    [Authorize]
    public async Task<IActionResult> EnviarParaFila([FromBody] EnviarFilaRequest dto, CancellationToken ct)
    {
        try
        {
            var tickets = await service.EnviarParaFilaAsync(dto, ct);
            await hub.Clients.All.SendAsync("FilaAtualizada", tickets, ct);
            return Ok(tickets);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Inicia preparo de um ticket</summary>
    [HttpPatch("{id:int}/iniciar-preparo")]
    [Authorize]
    public async Task<IActionResult> IniciarPreparo(int id, CancellationToken ct)
    {
        try
        {
            var ticket = await service.IniciarPreparoAsync(id, ct);
            await hub.Clients.All.SendAsync("TicketAtualizado", ticket, ct);
            return Ok(ticket);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { mensagem = ex.Message }); }
    }

    /// <summary>Marca ticket como pronto</summary>
    [HttpPatch("{id:int}/pronto")]
    [Authorize]
    public async Task<IActionResult> MarcarPronto(int id, CancellationToken ct)
    {
        try
        {
            var ticket = await service.MarcarProntoAsync(id, ct);
            await hub.Clients.All.SendAsync("TicketAtualizado", ticket, ct);
            return Ok(ticket);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { mensagem = ex.Message }); }
    }

    /// <summary>Confirma retirada dos tickets</summary>
    [HttpPost("retirar")]
    [Authorize]
    public async Task<IActionResult> ConfirmarRetirada([FromBody] ConfirmarRetiradaRequest dto, CancellationToken ct)
    {
        try
        {
            var tickets = await service.ConfirmarRetiradaAsync(dto, ct);
            await hub.Clients.All.SendAsync("FilaAtualizada", tickets, ct);
            return Ok(tickets);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}