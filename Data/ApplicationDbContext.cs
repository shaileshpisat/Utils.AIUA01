using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Utils.AIUA01.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TableInfo> TableInfos { get; set; }
    public DbSet<ColumnInfo> ColumnInfos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mark TableInfo and ColumnInfo as keyless since they are used for raw SQL results
        modelBuilder.Entity<TableInfo>().HasNoKey();
        modelBuilder.Entity<ColumnInfo>().HasNoKey();
        
        base.OnModelCreating(modelBuilder);
    }
}
