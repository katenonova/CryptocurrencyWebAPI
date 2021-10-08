using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Entities;
using WebAPI.Services;
using WebAPI.Helpers;

namespace WebAPI.Context
{
    public class APIDbContext : DbContext
    {
        public APIDbContext(DbContextOptions<APIDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        internal void Seed(APIDbContext dbContext)
        {
            if (!dbContext.Users.Any())
            {
                dbContext.Users.Add(new User
                {
                    FirstName = "Admin",
                    LastName = "",
                    Username = "admin",
                    Password = ExtensionMethods.ComputeSha256Hash("admin")
                }); ;
                dbContext.Users.Add(new User
                {
                    FirstName = "Angel",
                    LastName = "Angelov",
                    Username = "angel_angelov",
                    Password = ExtensionMethods.ComputeSha256Hash("angel")
                });

                dbContext.SaveChanges();
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
