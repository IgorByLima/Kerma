namespace Kerma.Application.DTOs.Request;

public record EnviarFilaRequest(
    IEnumerable<string> Codigos,
    string NomeCliente
);

public record ConfirmarRetiradaRequest(
    IEnumerable<string> Codigos,
    string OperadorNome
);