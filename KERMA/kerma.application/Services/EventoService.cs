using Kerma.Application.DTOs.Request;
using Kerma.Application.DTOs.Response;
using Kerma.Application.Utils;
using Kerma.Domain.Entities;
using Kerma.Domain.Enums;
using Kerma.Domain.Interfaces;

namespace Kerma.Application.Services;

// ─── Interface ────────────────────────────────────────────────────────────────
public interface IEventoService
{
    Task<EventoResponse> CriarAsync(CriarEventoRequest dto, int adminId, CancellationToken ct = default);
    Task<EventoResponse> AtualizarAsync(int id, AtualizarEventoRequest dto, CancellationToken ct = default);
    Task<EventoResponse?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<EventoResponse?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IEnumerable<EventoResponse>> ListarPorStatusAsync(StatusEvento status, CancellationToken ct = default);
    Task<IEnumerable<EventoResponse>> ListarTodosAsync(CancellationToken ct = default);
    Task EncerrarAsync(int id, CancellationToken ct = default);
}

// ─── Implementação ────────────────────────────────────────────────────────────
public class EventoService(IEventoRepositorio eventoRepo) : IEventoService
{
    public async Task<EventoResponse> CriarAsync(CriarEventoRequest dto, int adminId, CancellationToken ct = default)
    {
        if (dto.DataFim < dto.DataInicio)
            throw new InvalidOperationException("A data de fim não pode ser anterior à data de início.");

        var evento = new Evento
        {
            Nome = dto.Nome,
            Codigo = GeradorCodigo.GerarEvento(),
            Descricao = dto.Descricao,
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            Status = StatusEvento.Rascunho,
            CriadoPor = adminId
        };

        await eventoRepo.AdicionarAsync(evento, ct);
        await eventoRepo.SalvarAsync(ct);
        return ToResponse(evento);
    }

    public async Task<EventoResponse> AtualizarAsync(int id, AtualizarEventoRequest dto, CancellationToken ct = default)
    {
        var evento = await eventoRepo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Evento {id} não encontrado.");

        if (evento.Status == StatusEvento.Encerrado)
            throw new InvalidOperationException("Não é possível editar um evento encerrado.");

        if (dto.DataFim < dto.DataInicio)
            throw new InvalidOperationException("A data de fim não pode ser anterior à data de início.");

        evento.Nome = dto.Nome;
        evento.Descricao = dto.Descricao;
        evento.DataInicio = dto.DataInicio;
        evento.DataFim = dto.DataFim;
        evento.Status = Enum.Parse<StatusEvento>(dto.Status);
        evento.AtualizadoEm = DateTime.UtcNow;

        await eventoRepo.AtualizarAsync(evento, ct);
        await eventoRepo.SalvarAsync(ct);
        return ToResponse(evento);
    }

    public async Task<EventoResponse?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var e = await eventoRepo.ObterPorIdAsync(id, ct);
        return e is null ? null : ToResponse(e);
    }

    public async Task<EventoResponse?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default)
    {
        var e = await eventoRepo.ObterPorCodigoAsync(codigo, ct);
        return e is null ? null : ToResponse(e);
    }

    public async Task<IEnumerable<EventoResponse>> ListarPorStatusAsync(StatusEvento status, CancellationToken ct = default)
    {
        var lista = await eventoRepo.ListarPorStatusAsync(status, ct);
        return lista.Select(ToResponse);
    }

    public async Task<IEnumerable<EventoResponse>> ListarTodosAsync(CancellationToken ct = default)
    {
        var lista = await eventoRepo.ListarAsync(ct);
        return lista.Select(ToResponse);
    }

    public async Task EncerrarAsync(int id, CancellationToken ct = default)
    {
        var evento = await eventoRepo.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Evento {id} não encontrado.");

        if (evento.Status == StatusEvento.Encerrado)
            throw new InvalidOperationException("Evento já está encerrado.");

        evento.Status = StatusEvento.Encerrado;
        evento.AtualizadoEm = DateTime.UtcNow;

        await eventoRepo.AtualizarAsync(evento, ct);
        await eventoRepo.SalvarAsync(ct);
    }

    // ─── Mapper privado ───────────────────────────────────────────────────────
    private static EventoResponse ToResponse(Evento e) => new(
        e.Id, e.Nome, e.Codigo, e.Descricao,
        e.DataInicio, e.DataFim, e.Status.ToString(),
        e.CriadoPor, e.CriadoEm);
}