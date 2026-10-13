using Kerma.Application.DTOs.Request;
using Kerma.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kerma.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CaixasController(ICaixaService service) : ControllerBase
{
    private int UsuarioAtualId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Abre um novo caixa</summary>
    [HttpPost("abrir")]
    [Authorize]
    public async Task<IActionResult> Abrir([FromBody] AbrirCaixaRequest dto, CancellationToken ct)
    {
        try
        {
            var caixa = await service.AbrirAsync(dto, UsuarioAtualId, ct);
            return CreatedAtAction(nameof(ObterPorId), new { id = caixa.Id }, caixa);
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

    /// <summary>Fecha um caixa aberto</summary>
    [HttpPatch("{id:int}/fechar")]
    [Authorize]
    public async Task<IActionResult> Fechar(int id, [FromBody] FecharCaixaRequest dto, CancellationToken ct)
    {
        try
        {
            var caixa = await service.FecharAsync(id, dto, ct);
            return Ok(caixa);
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

    /// <summary>Obtém caixa por ID</summary>
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var caixa = await service.ObterPorIdAsync(id, ct);
        return caixa is null ? NotFound() : Ok(caixa);
    }

    /// <summary>Lista caixas de um evento</summary>
    [HttpGet("evento/{eventoId:int}")]
    [Authorize]
    public async Task<IActionResult> ListarPorEvento(int eventoId, CancellationToken ct)
    {
        var lista = await service.ListarPorEventoAsync(eventoId, ct);
        return Ok(lista);
    }

    /// <summary>Obtém o caixa aberto do operador logado num evento</summary>
    [HttpGet("aberto/evento/{eventoId:int}")]
    [Authorize]
    public async Task<IActionResult> ObterCaixaAberto(int eventoId, CancellationToken ct)
    {
        var caixa = await service.ObterCaixaAbertoDoOperadorAsync(UsuarioAtualId, eventoId, ct);
        return caixa is null ? NotFound() : Ok(caixa);
    }
}