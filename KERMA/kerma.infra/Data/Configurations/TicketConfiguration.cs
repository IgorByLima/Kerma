using Kerma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kerma.Infrastructure.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("ticket");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Codigo).HasMaxLength(7).IsRequired();    // ex: TK-X9K2
        builder.Property(x => x.Barcode).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.RetiradoPor).HasMaxLength(120);

        builder.HasIndex(x => x.Codigo).IsUnique();
        builder.HasIndex(x => x.Barcode).IsUnique();

        builder.HasOne(x => x.ItemVenda)
               .WithMany(x => x.Tickets)
               .HasForeignKey(x => x.ItemVendaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}