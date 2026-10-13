using Kerma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kerma.Infrastructure.Data.Configurations;

public class UsuarioEventoConfiguration : IEntityTypeConfiguration<UsuarioEvento>
{
    public void Configure(EntityTypeBuilder<UsuarioEvento> builder)
    {
        builder.ToTable("usuario_evento");
        builder.HasKey(x => x.Id);

        // Índice único: um usuário não pode ser habilitado duas vezes no mesmo evento
        builder.HasIndex(x => new { x.UsuarioId, x.EventoId })
               .IsUnique();

        builder.HasOne(x => x.Usuario)
               .WithMany(x => x.EventosHabilitados)
               .HasForeignKey(x => x.UsuarioId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Evento)
               .WithMany(x => x.UsuariosHabilitados)
               .HasForeignKey(x => x.EventoId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}