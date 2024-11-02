using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<MergeJob> MergeJobs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=mergejobs.db");
    }
}
