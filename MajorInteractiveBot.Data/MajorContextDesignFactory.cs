using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace MajorInteractiveBot.Data
{
    public class MajorContextDesignFactory : IDesignTimeDbContextFactory<MajorContext>
    {
        public MajorContext CreateDbContext(string[] args)
        {
            var pathToAppSettings = Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(pathToAppSettings)
                .AddUserSecrets<MajorContext>()
                .AddEnvironmentVariables("MAJOR_")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<MajorContext>();

            var connectionString = configuration.GetConnectionString("MajorDb");

            optionsBuilder.UseNpgsql(connectionString);

            return new MajorContext(optionsBuilder.Options);
        }
    }
}
