using Kerma.Application.DTOs.Request;
using Kerma.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kerma.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VendasController(IVendaService service) : ControllerBase
{
    private int UsuarioAtualId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Cria uma nova venda em aberto</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Criar([FromBody] CriarVendaRequest dto, CancellationToken ct)
    {
        try
        {
            var venda = await service.CriarAsync(dto, UsuarioAtualId, ct);
            return CreatedAtAction(nameof(ObterPorId), new { id = venda.Id }, venda);
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

    /// <summary>Adiciona um item à venda</summary>
    [HttpPost("{id:int}/itens")]
    [Authorize]
    public async Task<IActionResult> AdicionarItem(int id, [FromBody] AdicionarItemVendaRequest dto, CancellationToken ct)
    {
        try
        {
            var venda = await service.AdicionarItemAsync(id, dto, ct);
            return Ok(venda);
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

    /// <summary>Confirma pagamento — debita estoque e gera tickets</summary>
    [HttpPatch("{id:int}/pagar")]
    [Authorize]
    public async Task<IActionResult> ConfirmarPagamento(int id, CancellationToken ct)
    {
        try
        {
            var resultado = await service.ConfirmarPagamentoAsync(id, ct);
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

    /// <summary>Cancela uma venda em aberto</summary>
    [HttpPatch("{id:int}/cancelar")]
    [Authorize]
    public async Task<IActionResult> Cancelar(int id, CancellationToken ct)
    {
        try
        {
            var venda = await service.CancelarAsync(id, ct);
            return Ok(venda);
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

    /// <summary>Obtém venda por ID</summary>
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var venda = await service.ObterPorIdAsync(id, ct);
        return venda is null ? NotFound() : Ok(venda);
    }

    /// <summary>Lista vendas de um caixa</summary>
    [HttpGet("caixa/{caixaId:int}")]
    [Authorize]
    public async Task<IActionResult> ListarPorCaixa(int caixaId, CancellationToken ct)
    {
        var lista = await service.ListarPorCaixaAsync(caixaId, ct);
        return Ok(lista);
    }

    /// <summary>Analytics do evento — dados para o dashboard</summary>
    [HttpGet("analytics")]
    [Authorize]
    public async Task<IActionResult> Analytics([FromQuery] int eventoId, [FromQuery] int? caixaId, CancellationToken ct)
    {
        var resultado = await service.ObterAnalyticsAsync(eventoId, caixaId, ct);
        return Ok(resultado);
    }
}