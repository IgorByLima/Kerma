using Kerma.Application.DTOs.Request;
using Kerma.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kerma.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ItensController(IItemService service) : ControllerBase
{
    /// <summary>Lista itens de um evento</summary>
    [HttpGet("evento/{eventoId:int}")]
    [Authorize]
    public async Task<IActionResult> ListarPorEvento(int eventoId, CancellationToken ct)
    {
        var lista = await service.ListarPorEventoAsync(eventoId, ct);
        return Ok(lista);
    }

    /// <summary>Lista itens com estoque crítico de um evento</summary>
    [HttpGet("evento/{eventoId:int}/criticos")]
    [Authorize]
    public async Task<IActionResult> ListarCriticos(int eventoId, CancellationToken ct)
    {
        var lista = await service.ListarComEstoqueCriticoAsync(eventoId, ct);
        return Ok(lista);
    }

    /// <summary>Obtém item por ID</summary>
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var item = await service.ObterPorIdAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Cria novo item — somente Admin</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Criar([FromBody] CriarItemRequest dto, CancellationToken ct)
    {
        try
        {
            var criado = await service.CriarAsync(dto, ct);
            return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
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

    /// <summary>Atualiza item existente — somente Admin</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarItemRequest dto, CancellationToken ct)
    {
        try
        {
            var atualizado = await service.AtualizarAsync(id, dto, ct);
            return Ok(atualizado);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Desativa item (soft delete) — somente Admin</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Desativar(int id, CancellationToken ct)
    {
        try
        {
            await service.DesativarAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}