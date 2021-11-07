using Microsoft.EntityFrameworkCore;
using System;
using Domain;
using DAL.Configurations;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DAL
{
    public class OnlineStoreDBContext : DbContext
    {
        public DbSet <Order> Orders { get; set; }
        public DbSet <Product> Products { get; set; }
        public DbSet <Review> Reviews { get; set; }
        public DbSet <User> Users { get; set; }
        public DbSet<RefreshToken> Tokens { get; set; }

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

            CreateImagesBlobContainer();
        }

        private static async void CreateImagesBlobContainer()
        {
            if (CloudStorageAccount.TryParse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), out CloudStorageAccount storageAccount))
            {
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("images");
                if (!await cloudBlobContainer.ExistsAsync())
                {
                    await cloudBlobContainer.CreateAsync();

                    BlobContainerPermissions permissions = new()
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    };
                    await cloudBlobContainer.SetPermissionsAsync(permissions);
                }
            }
            else
            {
                Console.WriteLine(
                    "A connection string has not been defined in the system environment variables. " +
                    "Add an environment variable named 'AZURE_STORAGE_CONNECTION_STRING' with your storage " +
                    "connection string as a value.");
            }
        }
    }
}
