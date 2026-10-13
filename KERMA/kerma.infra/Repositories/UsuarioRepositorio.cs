using Kerma.Domain.Entities;
using Kerma.Domain.Interfaces;
using Kerma.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kerma.Infrastructure.Repositories;

public class UsuarioRepositorio(KermaDbContext db) : IUsuarioRepositorio
{
    public async Task<Usuario?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<Usuario?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default)
        => await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Codigo == codigo, ct);

    public async Task<IEnumerable<Usuario>> ListarAsync(CancellationToken ct = default)
        => await db.Usuarios.AsNoTracking().ToListAsync(ct);

    public async Task<IEnumerable<Usuario>> ListarAtivosAsync(CancellationToken ct = default)
        => await db.Usuarios.AsNoTracking().Where(u => u.Ativo).ToListAsync(ct);

    public async Task<bool> UsuarioHabilitadoParaEventoAsync(int usuarioId, int eventoId, CancellationToken ct = default)
        => await db.UsuarioEventos
            .AnyAsync(ue => ue.UsuarioId == usuarioId && ue.EventoId == eventoId && ue.Ativo, ct);

    public async Task HabilitarUsuarioNoEventoAsync(int usuarioId, int eventoId, CancellationToken ct = default)
    {
        var jaExiste = await db.UsuarioEventos
            .AnyAsync(ue => ue.UsuarioId == usuarioId && ue.EventoId == eventoId, ct);

        if (jaExiste) return;

        await db.UsuarioEventos.AddAsync(new UsuarioEvento
        {
            UsuarioId = usuarioId,
            EventoId = eventoId,
            Ativo = true
        }, ct);

        await db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<UsuarioEvento>> ListarUsuariosPorEventoAsync(int eventoId, CancellationToken ct = default)
        => await db.UsuarioEventos
            .Include(ue => ue.Usuario)
            .Where(ue => ue.EventoId == eventoId && ue.Ativo)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task AdicionarAsync(Usuario entidade, CancellationToken ct = default)
        => await db.Usuarios.AddAsync(entidade, ct);

    public Task AtualizarAsync(Usuario entidade, CancellationToken ct = default)
    {
        db.Usuarios.Update(entidade);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Usuario entidade, CancellationToken ct = default)
    {
        db.Usuarios.Remove(entidade);
        return Task.CompletedTask;
    }

    public async Task<int> SalvarAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}