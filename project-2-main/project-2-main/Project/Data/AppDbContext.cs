using System;
using System.Data.SqlClient;
using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class AppDbContext : DbContext
    {
        private readonly string _connectionString;

        public AppDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        public static AppDbContext CreateDbContext()
        {
            var connectionString = 
                "Data Source=localhost,11433;" +
                "Initial Catalog=SDTrabalho2;" +
                "User ID=sa;" +
                "Password=francisco-teste-123!" +
                ";TrustServerCertificate=True;";
            return new AppDbContext(connectionString);
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Domicilio> Domicilios { get; set; }
        public DbSet<Modalidade> Modalidades { get; set; }
        public DbSet<Modalidades_Domicilio> Modalidades_Domicilios { get; set; }
        public DbSet<Reserva> Reserva { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
