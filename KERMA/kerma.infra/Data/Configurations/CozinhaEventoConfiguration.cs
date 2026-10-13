using Kerma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kerma.Infrastructure.Data.Configurations;

public class CozinhaEventoConfiguration : IEntityTypeConfiguration<CozinhaEvento>
{
    public void Configure(EntityTypeBuilder<CozinhaEvento> builder)
    {
        builder.ToTable("cozinha_evento");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome)
               .HasMaxLength(100)
               .IsRequired();

        builder.HasOne(x => x.Evento)
               .WithMany()
               .HasForeignKey(x => x.EventoId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Itens)
               .WithOne(x => x.CozinhaEvento)
               .HasForeignKey(x => x.CozinhaEventoId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}