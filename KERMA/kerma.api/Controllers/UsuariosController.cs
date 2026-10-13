using Kerma.Application.DTOs.Request;
using Kerma.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kerma.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsuariosController(IUsuarioService service) : ControllerBase
{
    private int UsuarioAtualId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Login — retorna JWT com evento embutido</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto, CancellationToken ct)
    {
        try
        {
            var resultado = await service.LoginAsync(dto, ct);
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { mensagem = ex.Message });
        }
    }

    /// <summary>Lista todos os usuários ativos — somente Admin</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var lista = await service.ListarAtivosAsync(ct);
        return Ok(lista);
    }

    /// <summary>Obtém usuário por ID — somente Admin</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var usuario = await service.ObterPorIdAsync(id, ct);
        return usuario is null ? NotFound() : Ok(usuario);
    }

    /// <summary>Cadastra novo usuário — somente Admin</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Criar([FromBody] CriarUsuarioRequest dto, CancellationToken ct)
    {
        try
        {
            var criado = await service.CriarAsync(dto, ct);
            return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }

    /// <summary>Atualiza usuário existente — somente Admin</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarUsuarioRequest dto, CancellationToken ct)
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

    /// <summary>Desativa usuário (soft delete) — somente Admin</summary>
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
    /// <summary>Valida senha de admin — usado para autorizar devoluções</summary>
    [HttpPost("validar-admin")]
    [Authorize]
    public async Task<IActionResult> ValidarAdmin([FromBody] ValidarAdminRequest dto, CancellationToken ct)
    {
        try
        {
            var valido = await service.ValidarSenhaAdminAsync(dto.Senha, ct);
            return Ok(new { valido });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Habilita usuário para um evento — somente Admin</summary>
    /// <summary>Redefine a senha de um usuário</summary>
    [HttpPatch("{id:int}/redefinir-senha")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RedefinirSenha(int id, CancellationToken ct)
    {
        try
        {
            var novaSenha = await service.RedefinirSenhaAsync(id, ct);
            return Ok(new { novaSenha });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }
    [HttpPost("habilitar-evento")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> HabilitarNoEvento([FromBody] HabilitarUsuarioEventoRequest dto, CancellationToken ct)
    {
        try
        {
            await service.HabilitarNoEventoAsync(dto, ct);
            return Ok(new { mensagem = "Usuário habilitado para o evento com sucesso." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    /// <summary>Gera hash de senha — REMOVER ANTES DE IR PARA PRODUÇÃO</summary>
    [HttpGet("hash/{senha}")]
    [AllowAnonymous]
    public IActionResult GerarHash(string senha)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(senha));
        return Ok(Convert.ToHexString(bytes).ToLower());
    }
    public record ValidarAdminRequest(string Senha);

}