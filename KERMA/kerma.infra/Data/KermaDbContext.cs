using Kerma.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kerma.Infrastructure.Data;

public class KermaDbContext : DbContext
{
    public KermaDbContext(DbContextOptions<KermaDbContext> options) : base(options) { }

    // ─── DbSets (uma propriedade por entidade = uma tabela no banco) ──────────
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<CozinhaEvento> CozinhasEvento { get; set; }   
    public DbSet<Evento> Eventos { get; set; }
    public DbSet<Item> Itens { get; set; }
    public DbSet<Caixa> Caixas { get; set; }
    public DbSet<Venda> Vendas { get; set; }
    public DbSet<ItemVenda> ItensVenda { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Devolucao> Devolucoes { get; set; }
    public DbSet<ImpressaoLog> ImpressaoLogs { get; set; }
    public DbSet<Fornecedor> Fornecedores { get; set; }
    public DbSet<FornecedorItem> FornecedorItens { get; set; }
    public DbSet<UsuarioEvento> UsuarioEventos { get; set; }

    // ─── Aplicar todas as IEntityTypeConfiguration automaticamente ────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KermaDbContext).Assembly);
    }
}