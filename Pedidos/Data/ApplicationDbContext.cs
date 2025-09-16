using Microsoft.EntityFrameworkCore;
using Pedidos.Models;

namespace Pedidos.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<ProductoModel> Productos { get; set; }
        public DbSet<OrdenModel> Ordenes { get; set; }
        public DbSet<OrdenItem> OrdenItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar relaciones
            modelBuilder.Entity<OrdenModel>()
                .HasOne(o => o.Cliente)
                .WithMany()
                .HasForeignKey(o => o.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenItem>()
                .HasOne(oi => oi.Orden)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrdenId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrdenItem>()
                .HasOne(oi => oi.Producto)
                .WithMany()
                .HasForeignKey(oi => oi.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar índices únicos
            modelBuilder.Entity<UserModel>()
                .HasIndex(u => u.Email)
                .IsUnique();
                
            // Datos semilla para roles
            modelBuilder.Entity<UserModel>().HasData(
                new UserModel
                {
                    Id = 1,
                    Nombre = "Administrador",
                    Email = "admin@pedidos.com",
                    Password = "admin123", // En producción esto debe estar hasheado
                    Rol = "admin"
                }
            );
        }
    }
}