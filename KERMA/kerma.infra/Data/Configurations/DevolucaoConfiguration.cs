using Kerma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kerma.Infrastructure.Data.Configurations;

public class DevolucaoConfiguration : IEntityTypeConfiguration<Devolucao>
{
    public void Configure(EntityTypeBuilder<Devolucao> builder)
    {
        builder.ToTable("devolucao");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Motivo)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.AutorizadoPor)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(x => x.Tipo)
               .HasConversion<string>()
               .IsRequired();

        builder.HasOne(x => x.Ticket)
               .WithMany()
               .HasForeignKey(x => x.TicketId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.NovoTicket)
               .WithMany()
               .HasForeignKey(x => x.NovoTicketId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);
    }
}