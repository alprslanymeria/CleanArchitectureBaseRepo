using Microsoft.EntityFrameworkCore;

namespace App.Persistence;

public class DbContextConfigurator
{
    public static void Configure(DbContextOptionsBuilder options, string connectionString, string migrationsAssembly)
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(migrationsAssembly);
        });
    }
}
