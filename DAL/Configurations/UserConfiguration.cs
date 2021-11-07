using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace DAL.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToContainer("Users");
            builder.HasData(new User()
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Password = "secret"
            });
        }
    }
}