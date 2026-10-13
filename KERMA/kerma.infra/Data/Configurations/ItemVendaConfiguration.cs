using Kerma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kerma.Infrastructure.Data.Configurations;

public class ItemVendaConfiguration : IEntityTypeConfiguration<ItemVenda>
{
    public void Configure(EntityTypeBuilder<ItemVenda> builder)
    {
        builder.ToTable("item_venda");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantidade).IsRequired();
        builder.Property(x => x.PrecoUnitario).HasPrecision(10, 2).IsRequired();
        builder.Property(x => x.CustoUnitario).HasPrecision(10, 2).IsRequired();
        builder.Property(x => x.Subtotal).HasPrecision(10, 2).IsRequired();

        builder.HasOne(x => x.Venda)
               .WithMany(x => x.ItensVenda)
               .HasForeignKey(x => x.VendaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Item)
               .WithMany(x => x.ItensVenda)
               .HasForeignKey(x => x.ItemId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}