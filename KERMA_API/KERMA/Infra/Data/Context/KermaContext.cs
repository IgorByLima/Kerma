using Microsoft.EntityFrameworkCore;

namespace KERMA.Infra.Data.Context
{
    public class KermaContext : DbContext
    {
        // public DbSet<Produtos> ProdutoSet { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (optionsBuilder.IsConfigured) 
            {
              optionsBuilder.UseMySql("server=localhost;database=Kerma;port=3306;uid=root", ServerVersion.AutoDetect("server=localhost;database=Kerma;port=3306;uid=root"));

            }
        }
            


        
    }
}
