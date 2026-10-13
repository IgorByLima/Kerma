using Kerma.Application.DTOs.Request;
using Kerma.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kerma.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FornecedoresController(IFornecedorService service) : ControllerBase
{
    private int AdminId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var lista = await service.ListarAsync(ct);
        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var fornecedor = await service.ObterPorIdAsync(id, ct);
        return fornecedor is null ? NotFound() : Ok(fornecedor);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Criar([FromBody] CriarFornecedorRequest dto, CancellationToken ct)
    {
        try
        {
            var criado = await service.CriarAsync(dto, AdminId, ct);
            return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarFornecedorRequest dto, CancellationToken ct)
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
    }

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

    [HttpPost("cotacoes")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdicionarCotacao([FromBody] CriarFornecedorItemRequest dto, CancellationToken ct)
    {
        var cotacao = await service.AdicionarCotacaoAsync(dto, ct);
        return Created($"api/fornecedores/cotacoes/{cotacao.Id}", cotacao);
    }

    [HttpPut("cotacoes/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AtualizarCotacao(int id, [FromBody] AtualizarFornecedorItemRequest dto, CancellationToken ct)
    {
        var atualizada = await service.AtualizarCotacaoAsync(id, dto, ct);
        return Ok(atualizada);
    }

    [HttpGet("comparativo/item/{itemId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ComparativoPorItem(int itemId, CancellationToken ct)
    {
        var comparativo = await service.ComparativoPorItemAsync(itemId, ct);
        return Ok(comparativo);
    }

    [HttpGet("ranking/evento/{eventoId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RankingPorEvento(int eventoId, CancellationToken ct)
    {
        var ranking = await service.RankingPorEventoAsync(eventoId, ct);
        return Ok(ranking);
    }
}