using Kerma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kerma.Infrastructure.Data.Configurations;

public class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("fornecedor");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RazaoSocial).HasMaxLength(180).IsRequired();
        builder.Property(x => x.NomeFantasia).HasMaxLength(180);
        builder.Property(x => x.CNPJ).HasMaxLength(14).IsRequired();
        builder.Property(x => x.Telefone).HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(180);
        builder.Property(x => x.Observacao).HasColumnType("text");

        builder.HasIndex(x => x.CNPJ).IsUnique();

        builder.HasOne(x => x.Cadastrador)
               .WithMany()
               .HasForeignKey(x => x.CadastradoPor)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class FornecedorItemConfiguration : IEntityTypeConfiguration<FornecedorItem>
{
    public void Configure(EntityTypeBuilder<FornecedorItem> builder)
    {
        builder.ToTable("fornecedor_item");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PrecoFornecedor).HasPrecision(10, 2).IsRequired();
        builder.Property(x => x.Observacao).HasMaxLength(255);

        // Índice único: um fornecedor não pode ter dois preços ativos para o mesmo item
        builder.HasIndex(x => new { x.FornecedorId, x.ItemId })
               .HasFilter("ativo = 1")
               .IsUnique();

        builder.HasOne(x => x.Fornecedor)
               .WithMany(x => x.FornecedorItens)
               .HasForeignKey(x => x.FornecedorId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Item)
               .WithMany()
               .HasForeignKey(x => x.ItemId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}