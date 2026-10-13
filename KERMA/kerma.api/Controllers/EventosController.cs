using Kerma.Application.DTOs.Request;
using Kerma.Application.Services;
using Kerma.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kerma.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EventosController(IEventoService service) : ControllerBase
{
    private int UsuarioAtualId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Lista todos os eventos</summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var lista = await service.ListarTodosAsync(ct);
        return Ok(lista);
    }

    /// <summary>Lista eventos por status</summary>
    [HttpGet("status/{status}")]
    [Authorize]
    public async Task<IActionResult> ListarPorStatus(string status, CancellationToken ct)
    {
        if (!Enum.TryParse<StatusEvento>(status, out var statusEnum))
            return BadRequest(new { mensagem = $"Status inválido: {status}. Use Rascunho, Ativo ou Encerrado." });

        var lista = await service.ListarPorStatusAsync(statusEnum, ct);
        return Ok(lista);
    }

    /// <summary>Obtém evento por ID</summary>
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var evento = await service.ObterPorIdAsync(id, ct);
        return evento is null ? NotFound() : Ok(evento);
    }

    /// <summary>Obtém evento por código</summary>
    [HttpGet("codigo/{codigo}")]
    [Authorize]
    public async Task<IActionResult> ObterPorCodigo(string codigo, CancellationToken ct)
    {
        var evento = await service.ObterPorCodigoAsync(codigo, ct);
        return evento is null ? NotFound() : Ok(evento);
    }

    /// <summary>Cria novo evento — somente Admin</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Criar([FromBody] CriarEventoRequest dto, CancellationToken ct)
    {
        try
        {
            var criado = await service.CriarAsync(dto, UsuarioAtualId, ct);
            return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Atualiza evento existente — somente Admin</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarEventoRequest dto, CancellationToken ct)
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

    /// <summary>Encerra evento — somente Admin</summary>
    [HttpPatch("{id:int}/encerrar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Encerrar(int id, CancellationToken ct)
    {
        try
        {
            await service.EncerrarAsync(id, ct);
            return NoContent();
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
}