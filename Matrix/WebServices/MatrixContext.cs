using Microsoft.EntityFrameworkCore;
using Matrix.Data.Models;

namespace Matrix.WebServices;

public class MatrixContext : DbContext
{
    private ILogger<MatrixContext> _logger;
    
    public MatrixContext(DbContextOptions<MatrixContext> options) : base(options)
    {
        _logger = new LoggerFactory().CreateLogger<MatrixContext>();
    }
    
    public DbSet<ClockFace> ClockFace { get; init; }
    public DbSet<TextLine> TextLine { get; init; }
    public DbSet<MatrixColor> MatrixColor { get; init; }
    public DbSet<MatrixFont> MatrixFont { get; init; }
    public DbSet<TimePeriod> TimePeriod { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _logger.LogInformation("OnModelCreating");
    }
}