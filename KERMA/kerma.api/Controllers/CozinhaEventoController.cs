using Kerma.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kerma.API.Controllers;

[ApiController]
[Route("api/cozinhas-evento")]
[Produces("application/json")]
public class CozinhasEventoController(ICozinhaEventoService service) : ControllerBase
{
    /// <summary>Lista cozinhas de um evento</summary>
    [HttpGet("evento/{eventoId:int}")]
    [Authorize]
    public async Task<IActionResult> ListarPorEvento(int eventoId, CancellationToken ct)
    {
        var lista = await service.ListarPorEventoAsync(eventoId, ct);
        return Ok(lista);
    }

    /// <summary>Cria uma nova cozinha para o evento</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Criar([FromBody] CriarCozinhaRequest dto, CancellationToken ct)
    {
        try
        {
            var cozinha = await service.CriarAsync(dto.EventoId, dto.Nome, ct);
            return Ok(cozinha);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}

public record CriarCozinhaRequest(int EventoId, string Nome);