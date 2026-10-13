using Kerma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kerma.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Codigo).HasMaxLength(7).IsRequired();    // ex: US-A1B2
        builder.Property(x => x.SenhaHash).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Perfil).HasConversion<string>().IsRequired();

        builder.HasIndex(x => x.Codigo).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();
    }
}