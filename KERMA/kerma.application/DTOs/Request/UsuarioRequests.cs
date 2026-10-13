namespace Kerma.Application.DTOs.Request;

public record CriarUsuarioRequest(
    string Nome,
    string Email,
    string Perfil,
    int? EventoId        // opcional — habilita direto num evento ao criar
);

public record LoginRequest(
    string Codigo,       // formato: EV-XXXX-US-XXXX
    string Senha
);

public record AtualizarUsuarioRequest(
    string Nome,
    string Email,
    string Perfil,
    bool Ativo
);

public record HabilitarUsuarioEventoRequest(
    int UsuarioId,
    int EventoId
);