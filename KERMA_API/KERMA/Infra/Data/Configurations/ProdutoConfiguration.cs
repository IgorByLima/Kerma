using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KERMA.Domain.Entities;

namespace RoomFlowApi.Infra.Data.Configurations
{
    public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
    {
        public void Configure(EntityTypeBuilder<Produto> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Nome)
                .IsRequired();
            builder.Property(p => p.Preco)
                .IsRequired();
            builder.Property(p => p.Custo)
                .IsRequired();

            builder.ToTable("TAB_Produto");
        }
    }
}

