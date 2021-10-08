using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Migrations
{
    public class Configuration
    {
        protected void Seed(APIDbContext dbContext)
        {

            if (!dbContext.Users.Any())
            {
                dbContext.Users.Add(new User
                {
                    FirstName = "Admin",
                    LastName = "",
                    Username = "admin",
                    Password = this.ComputeSha256Hash("admin")
                }); ;
                dbContext.Users.Add(new User
                {
                    FirstName = "Angel",
                    LastName = "Angelov",
                    Username = "angel_angelov",
                    Password = this.ComputeSha256Hash("angel")
                });

                dbContext.SaveChanges();
            }

        }


        string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
