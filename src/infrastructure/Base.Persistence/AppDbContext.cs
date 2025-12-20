using Microsoft.EntityFrameworkCore;

namespace Base.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // DB SETS


    // ON MODEL CREATING
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // APPLY CONFIGURATIONS
        // BU ASSEMBLY İÇERİSİNDE "IEntityTypeConfiguration" INTERFACE'İNİ IMPLEMENT EDEN TÜM CLASS'LARI OTOMATİK OLARAK BULUP UYGULAR
        builder.ApplyConfigurationsFromAssembly(typeof(PersistenceAssembly).Assembly);

        base.OnModelCreating(builder);
    }
}
