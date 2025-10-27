using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Server.Data
{
    public class RobotArmDbContextFactory : IDesignTimeDbContextFactory<RobotArmDb>
    {
        public RobotArmDb CreateDbContext(string[] args)
        {
            // Učitaj appsettings.* da ne hardcoduješ konekciju
            var basePath = Directory.GetCurrentDirectory();
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<RobotArmDb>();

            // sqlite
            var conn = config.GetConnectionString("DefaultConnection")
                      ?? "Data Source=robotarm.db";
            builder.UseSqlite(conn);

            return new RobotArmDb(builder.Options);
        }
    }
}
