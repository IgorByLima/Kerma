using Kerma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kerma.Infrastructure.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("item");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Descricao).HasColumnType("text");
        builder.Property(x => x.Custo).HasPrecision(10, 2).IsRequired();
        builder.Property(x => x.PrecoVenda).HasPrecision(10, 2).IsRequired();
        builder.Property(x => x.EstoqueMinimo).HasDefaultValue(5);
        builder.Property(x => x.StatusEstoque).HasConversion<string>().IsRequired();

        builder.Ignore(x => x.MargemAbsoluta);
        builder.Ignore(x => x.MargemPercentual);
        builder.Ignore(x => x.EstoqueCritico);

        builder.HasOne(x => x.Evento)
               .WithMany(x => x.Itens)
               .HasForeignKey(x => x.EventoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}