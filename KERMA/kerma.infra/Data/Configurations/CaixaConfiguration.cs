using Kerma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kerma.Infrastructure.Data.Configurations;

public class CaixaConfiguration : IEntityTypeConfiguration<Caixa>
{
    public void Configure(EntityTypeBuilder<Caixa> builder)
    {
        builder.ToTable("caixa");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SaldoInicial).HasPrecision(10, 2).IsRequired();
        builder.Property(x => x.SaldoFinal).HasPrecision(10, 2);
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.Observacao).HasColumnType("text");
        builder.Property(x => x.AbertoEm).IsRequired();

        builder.HasOne(x => x.Evento)
               .WithMany(x => x.Caixas)
               .HasForeignKey(x => x.EventoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Operador)
               .WithMany(x => x.Caixas)
               .HasForeignKey(x => x.OperadorId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}