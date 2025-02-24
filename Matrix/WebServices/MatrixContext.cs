using Microsoft.EntityFrameworkCore;
using Matrix.Data.Models;
using Timer = Matrix.Data.Models.Timer;

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
    public DbSet<Timer> Timer { get; init; }
    public DbSet<PlainTextPayload> SavedPlainText { get; init; }
    public DbSet<ScrollingTextPayload> SavedScrollingText { get; init; }
    public DbSet<SavedImage> SavedImage { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var clockFaceBuilder = modelBuilder.Entity<ClockFace>();
        clockFaceBuilder.HasIndex(c => new { c.Name }).IsUnique();
        
        var matrixColorBuilder = modelBuilder.Entity<MatrixColor>();
        matrixColorBuilder.HasIndex(c => new { c.Name }).IsUnique();
        
        var matrixFontBuilder = modelBuilder.Entity<MatrixFont>();
        matrixFontBuilder.HasIndex(c => new { c.Name }).IsUnique();
        
        _logger.LogInformation("OnModelCreating");
    }
}