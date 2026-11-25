using Microsoft.EntityFrameworkCore;
using MarcenariaVersatto.Models;

namespace MarcenariaVersatto.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<CategoriaMaterial> CategoriasMaterial { get; set; }
        public DbSet<Material> Materiais { get; set; }
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
        public DbSet<Orcamento> Orcamentos { get; set; }
        public DbSet<OrcamentoMaterial> OrcamentoMateriais { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>()
                .Property(c => c.CriadoEm)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.CpfCnpj)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .Property(u => u.CriadoEm)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Login)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.FuncionarioId)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Funcionario)
                .WithOne()
                .HasForeignKey<Usuario>(u => u.FuncionarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Funcionario>()
                .Property(f => f.DataAdmissao)
                .HasColumnType("date");

            modelBuilder.Entity<Funcionario>()
                .HasIndex(f => f.Cpf)
                .IsUnique();

            modelBuilder.Entity<Fornecedor>()
                .HasIndex(f => f.Cnpj)
                .IsUnique();

            modelBuilder.Entity<CategoriaMaterial>()
                .HasIndex(c => c.Nome)
                .IsUnique();

            modelBuilder.Entity<Material>()
                .HasOne(m => m.FornecedorPadrao)
                .WithMany(f => f.Materiais)
                .HasForeignKey(m => m.FornecedorPadraoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimentacaoEstoque>()
                .Property(m => m.DataMovimentacao)
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<MovimentacaoEstoque>()
                .HasOne(m => m.Fornecedor)
                .WithMany()
                .HasForeignKey(m => m.FornecedorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimentacaoEstoque>()
                .HasOne(m => m.Funcionario)
                .WithMany()
                .HasForeignKey(m => m.FuncionarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Orcamento>()
                .Property(o => o.ValorTotal)
                .HasComputedColumnSql(null, stored: true)
                .ValueGeneratedOnAddOrUpdate();

            modelBuilder.Entity<OrcamentoMaterial>()
                .Property(om => om.ValorTotal)
                .HasComputedColumnSql(null, stored: true)
                .ValueGeneratedOnAddOrUpdate();

            modelBuilder.Entity<OrcamentoMaterial>()
                .HasOne(om => om.Orcamento)
                .WithMany(o => o.Materiais)
                .HasForeignKey(om => om.OrcamentoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Pagamento>()
                .HasOne(p => p.Orcamento)
                .WithMany(o => o.Pagamentos)
                .HasForeignKey(p => p.OrcamentoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .Property(u => u.CriadoEm)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Usuario>()
                .Property(u => u.UltimoAcesso)
                .HasColumnType("timestamp without time zone");
            // Tipos das colunas exatamente como no banco (date / timestamp sem fuso)
            modelBuilder.Entity<Cliente>().Property(c => c.CriadoEm).HasColumnType("timestamp without time zone");
            modelBuilder.Entity<MovimentacaoEstoque>().Property(m => m.DataMovimentacao).HasColumnType("timestamp without time zone");
            modelBuilder.Entity<Orcamento>().Property(o => o.DataOrcamento).HasColumnType("date");
            modelBuilder.Entity<Pagamento>().Property(p => p.DataPagamento).HasColumnType("date");
        }
    }
}
