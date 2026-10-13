using Kerma.Application.DTOs.Request;
using Kerma.Application.DTOs.Response;
using Kerma.Application.Utils;
using Kerma.Domain.Entities;
using Kerma.Domain.Enums;
using Kerma.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Kerma.Application.Services;

// ─── Interface ────────────────────────────────────────────────────────────────
public interface IUsuarioService
{
    Task<CriarUsuarioResponse> CriarAsync(CriarUsuarioRequest dto, CancellationToken ct = default);
    Task<string> RedefinirSenhaAsync(int usuarioId, CancellationToken ct = default);
    Task<LoginResponse> LoginAsync(LoginRequest dto, CancellationToken ct = default);
    Task<UsuarioResponse?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<UsuarioResponse>> ListarAtivosAsync(CancellationToken ct = default);
    Task<UsuarioResponse> AtualizarAsync(int id, AtualizarUsuarioRequest dto, CancellationToken ct = default);
    Task DesativarAsync(int id, CancellationToken ct = default);
    Task HabilitarNoEventoAsync(HabilitarUsuarioEventoRequest dto, CancellationToken ct = default);
    Task<bool> ValidarSenhaAdminAsync(string senha, CancellationToken ct = default);
}

// ─── Implementação ────────────────────────────────────────────────────────────
public class UsuarioService(
    IUsuarioRepositorio usuarioRepo,
    IEventoRepositorio eventoRepo,
    IConfiguration config) : IUsuarioService
{
    public async Task<CriarUsuarioResponse> CriarAsync(CriarUsuarioRequest dto, CancellationToken ct = default)
    {
        var existente = await usuarioRepo.ListarAtivosAsync(ct);
        if (existente.Any(u => u.Email == dto.Email))
            throw new InvalidOperationException($"Já existe um usuário com o e-mail {dto.Email}.");

        var codigo = GeradorCodigo.GerarUsuario();
        var senha = GerarSenhaAleatoria();
        var perfil = Enum.Parse<PerfilUsuario>(dto.Perfil);

        var usuario = new Usuario
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Codigo = codigo,
            SenhaHash = HashSenha(senha),
            Perfil = perfil
        };

        await usuarioRepo.AdicionarAsync(usuario, ct);
        await usuarioRepo.SalvarAsync(ct);

        // Se veio EventoId, já habilita no evento
        if (dto.EventoId.HasValue)
            await usuarioRepo.HabilitarUsuarioNoEventoAsync(usuario.Id, dto.EventoId.Value, ct);

        return new CriarUsuarioResponse(
            usuario.Id, usuario.Nome, usuario.Email,
            usuario.Codigo, senha, usuario.Perfil.ToString());
    }
    public async Task<string> RedefinirSenhaAsync(int usuarioId, CancellationToken ct = default)
    {
        var usuario = await usuarioRepo.ObterPorIdAsync(usuarioId, ct)
            ?? throw new KeyNotFoundException($"Usuário {usuarioId} não encontrado.");

        // Gera senha aleatória
        var novaSenha = $"{GerarSenhaAleatoria()}@{new Random().Next(10, 99)}";
        usuario.SenhaHash = HashSenha(novaSenha);
        usuario.AtualizadoEm = DateTime.UtcNow;

        await usuarioRepo.AtualizarAsync(usuario, ct);
        await usuarioRepo.SalvarAsync(ct);

        return novaSenha;
    }

    

    public async Task<LoginResponse> LoginAsync(LoginRequest dto, CancellationToken ct = default)
    {
        // ── Separar EV-XXXX e US-XXXX do código combinado ────────────────────
        // Formato esperado: EV-XXXX-US-XXXX (11 caracteres: EV-XXXX = 7, - = 1, US-XXXX = 7 = 15 total... ajustado)
        // Exemplo real: EV-3S28-US-ADMI
        var partes = dto.Codigo.ToUpper().Split('-');
        if (partes.Length != 4 || partes[0] != "EV" || partes[2] != "US")
            throw new UnauthorizedAccessException("Formato de código inválido. Use EV-XXXX-US-XXXX.");

        var codigoEvento = $"EV-{partes[1]}";
        var codigoUsuario = $"US-{partes[3]}";

        // ── Validar evento ────────────────────────────────────────────────────
        var evento = await eventoRepo.ObterPorCodigoAsync(codigoEvento, ct)
            ?? throw new UnauthorizedAccessException("Evento não encontrado.");

        if (evento.Status != StatusEvento.Ativo)
            throw new UnauthorizedAccessException("Este evento não está ativo.");

        // ── Validar usuário ───────────────────────────────────────────────────
        var usuario = await usuarioRepo.ObterPorCodigoAsync(codigoUsuario, ct)
            ?? throw new UnauthorizedAccessException("Código ou senha inválidos.");

        if (!VerificarSenha(dto.Senha, usuario.SenhaHash))
            throw new UnauthorizedAccessException("Código ou senha inválidos.");

        if (!usuario.Ativo)
            throw new UnauthorizedAccessException("Usuário inativo.");

        // ── Validar vínculo usuário-evento (Admin pode logar em qualquer evento) ──
        if (usuario.Perfil != PerfilUsuario.Admin)
        {
            var habilitado = await usuarioRepo.UsuarioHabilitadoParaEventoAsync(usuario.Id, evento.Id, ct);
            if (!habilitado)
                throw new UnauthorizedAccessException("Você não está habilitado para este evento.");
        }

        // ── Gerar JWT ─────────────────────────────────────────────────────────
        var expiracao = DateTime.UtcNow.AddHours(double.Parse(config["Jwt:ExpiracaoHoras"]!));
        var token = GerarToken(usuario, evento, expiracao);

        return new LoginResponse(
            token, usuario.Nome, usuario.Perfil.ToString(),
            evento.Id, evento.Codigo, evento.Nome, expiracao);
    }

    public async Task<UsuarioResponse?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var u = await usuarioRepo.ObterPorIdAsync(id, ct);
        return u is null ? null : ToResponse(u);
    }

    public async Task<IEnumerable<UsuarioResponse>> ListarAtivosAsync(CancellationToken ct = default)
    {
        var lista = await usuarioRepo.ListarAtivosAsync(ct);
        return lista.Select(ToResponse);
    }

    public async Task<UsuarioResponse> AtualizarAsync(int id, AtualizarUsuarioRequest dto, CancellationToken ct = default)
    {
        var usuario = await usuarioRepo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Usuário {id} não encontrado.");

        usuario.Nome = dto.Nome;
        usuario.Email = dto.Email;
        usuario.Perfil = Enum.Parse<PerfilUsuario>(dto.Perfil);
        usuario.Ativo = dto.Ativo;
        usuario.AtualizadoEm = DateTime.UtcNow;

        await usuarioRepo.AtualizarAsync(usuario, ct);
        await usuarioRepo.SalvarAsync(ct);
        return ToResponse(usuario);
    }

    public async Task DesativarAsync(int id, CancellationToken ct = default)
    {
        var usuario = await usuarioRepo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Usuário {id} não encontrado.");

        usuario.Ativo = false;
        usuario.AtualizadoEm = DateTime.UtcNow;

        await usuarioRepo.AtualizarAsync(usuario, ct);
        await usuarioRepo.SalvarAsync(ct);
    }

    public async Task HabilitarNoEventoAsync(HabilitarUsuarioEventoRequest dto, CancellationToken ct = default)
    {
        var usuario = await usuarioRepo.ObterPorIdAsync(dto.UsuarioId, ct)
            ?? throw new KeyNotFoundException($"Usuário {dto.UsuarioId} não encontrado.");

        var evento = await eventoRepo.ObterPorIdAsync(dto.EventoId, ct)
            ?? throw new KeyNotFoundException($"Evento {dto.EventoId} não encontrado.");

        await usuarioRepo.HabilitarUsuarioNoEventoAsync(usuario.Id, evento.Id, ct);
    }

    // ─── Helpers privados ─────────────────────────────────────────────────────
    private static string GerarSenhaAleatoria()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        var bytes = RandomNumberGenerator.GetBytes(8);
        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }
    public async Task<bool> ValidarSenhaAdminAsync(string senha, CancellationToken ct = default)
    {
        var admins = await usuarioRepo.ListarAtivosAsync(ct);
        var admin = admins.FirstOrDefault(u => u.Perfil == PerfilUsuario.Admin && u.Ativo);
        if (admin is null) return false;
        return VerificarSenha(senha, admin.SenhaHash);
    }

    private static string HashSenha(string senha)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
        return Convert.ToHexString(bytes).ToLower();
    }

    private static bool VerificarSenha(string senha, string hash)
        => HashSenha(senha) == hash;

    private string GerarToken(Usuario usuario, Evento evento, DateTime expiracao)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Name,               usuario.Nome),
            new Claim(ClaimTypes.Role,               usuario.Perfil.ToString()),
            new Claim("codigo",                      usuario.Codigo),
            new Claim("eventoId",                    evento.Id.ToString()),
            new Claim("eventoCodigo",                evento.Codigo),
            new Claim("eventoNome",                  evento.Nome),
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expiracao,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UsuarioResponse ToResponse(Usuario u) => new(
        u.Id, u.Nome, u.Email, u.Codigo,
        u.Perfil.ToString(), u.Ativo, u.CriadoEm);
}