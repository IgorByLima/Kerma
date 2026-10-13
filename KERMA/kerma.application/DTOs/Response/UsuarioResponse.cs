namespace Kerma.Application.DTOs.Response;

public record UsuarioResponse(
    int Id,
    string Nome,
    string Email,
    string Codigo,
    string Perfil,
    bool Ativo,
    DateTime CriadoEm
);

public record LoginResponse(
    string Token,
    string Nome,
    string Perfil,
    int EventoId,
    string EventoCodigo,
    string EventoNome,
    DateTime Expiracao
);

public record CriarUsuarioResponse(
    int Id,
    string Nome,
    string Email,
    string Codigo,
    string Senha,
    string Perfil
);