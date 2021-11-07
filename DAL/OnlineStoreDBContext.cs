using Microsoft.EntityFrameworkCore;
using System;
using Domain;
using DAL.Configurations;

namespace DAL
{
    public class OnlineStoreDBContext : DbContext
    {
        public DbSet <Order> Orders { get; set; }
        public DbSet <Product> Products { get; set; }
        public DbSet <Review> Reviews { get; set; }
        public DbSet <User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseCosmos(
                Environment.GetEnvironmentVariable("CosmosDBUrl"),
                Environment.GetEnvironmentVariable("ConnectionString"),
                databaseName: Environment.GetEnvironmentVariable("DatabaseName"));

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultContainer("Store");

            modelBuilder.ApplyConfiguration(new ReviewConfiguration());            
            modelBuilder.ApplyConfiguration(new ProductConfiguration());            
            modelBuilder.ApplyConfiguration(new OrderConfiguration());            
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
    }
}
