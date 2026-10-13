using Kerma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kerma.Infrastructure.Data.Configurations;

public class ImpressaoLogConfiguration : IEntityTypeConfiguration<ImpressaoLog>
{
    public void Configure(EntityTypeBuilder<ImpressaoLog> builder)
    {
        builder.ToTable("impressao_log");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImpressoraId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Tentativas).HasDefaultValue(0).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.ErroMotivo).HasColumnType("text");

        builder.HasOne(x => x.Ticket)
               .WithMany(x => x.ImpressaoLogs)
               .HasForeignKey(x => x.TicketId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}