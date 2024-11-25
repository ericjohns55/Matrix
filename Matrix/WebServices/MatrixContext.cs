using Microsoft.EntityFrameworkCore;
using Matrix.DataModels;

namespace Matrix.WebServices;

public class MatrixContext : DbContext
{
    private ILogger<MatrixContext> _logger;
    
    public MatrixContext(DbContextOptions<MatrixContext> options) : base(options)
    {
        _logger = new LoggerFactory().CreateLogger<MatrixContext>();
    }
    
    public DbSet<TestData> TestData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _logger.LogInformation("OnModelCreating");
        modelBuilder.Entity<TestData>().ToTable("TestData");
    }
}