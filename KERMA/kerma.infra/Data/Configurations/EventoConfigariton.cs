using Kerma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kerma.Infrastructure.Data.Configurations;

public class EventoConfiguration : IEntityTypeConfiguration<Evento>
{
    public void Configure(EntityTypeBuilder<Evento> builder)
    {
        builder.ToTable("evento");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Codigo).HasMaxLength(7).IsRequired();    // ex: EV-C3D4
        builder.Property(x => x.Descricao).HasColumnType("text");
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.DataInicio).IsRequired();
        builder.Property(x => x.DataFim).IsRequired();

        builder.HasIndex(x => x.Codigo).IsUnique();

        builder.HasOne(x => x.Criador)
               .WithMany(x => x.EventosCriados)
               .HasForeignKey(x => x.CriadoPor)
               .OnDelete(DeleteBehavior.Restrict);
    }
}