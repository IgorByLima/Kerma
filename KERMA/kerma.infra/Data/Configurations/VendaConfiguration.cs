using Kerma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kerma.Infrastructure.Data.Configurations;

public class VendaConfiguration : IEntityTypeConfiguration<Venda>
{
    public void Configure(EntityTypeBuilder<Venda> builder)
    {
        builder.ToTable("venda");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Total).HasPrecision(10, 2).IsRequired();
        builder.Property(x => x.FormaPagamento).HasConversion<string>().IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.Origem).HasConversion<string>().IsRequired();
        builder.Property(x => x.VoucherCodigo).HasMaxLength(20);

        builder.HasIndex(x => x.VoucherCodigo).IsUnique();

        builder.HasOne(x => x.Evento)
               .WithMany(x => x.Vendas)
               .HasForeignKey(x => x.EventoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Caixa)
               .WithMany(x => x.Vendas)
               .HasForeignKey(x => x.CaixaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Operador)
               .WithMany(x => x.Vendas)
               .HasForeignKey(x => x.OperadorId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}